using System;
using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
    private const string _bufferName = "Shadows";

    private CommandBuffer buffer = new CommandBuffer()
    {
        name = _bufferName
    };

    private ScriptableRenderContext _context;

    private CullingResults _cullingResults;

    private ShadowSettings _shadowSettings;

    private const int _maxShadowDirLightCount = 4; //

    private int _shadowedDirLightCount; //light with shadows (enabled, have str and not cull)

    private static int _dirShadowAtlasId = Shader.PropertyToID("_DirShadowAtlas");
    private static int _dirShadowMatricesId = Shader.PropertyToID("_DirShadowMatrices");

    private static Matrix4x4[] _dirShadowMatrices = new Matrix4x4[_maxShadowDirLightCount];


    struct ShadowedDirLight
    {
        public int VisibleLightIndex;
    }

    private ShadowedDirLight[] ShadowedDirLights = new ShadowedDirLight[_maxShadowDirLightCount];


    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        this._context = context;
        this._cullingResults = cullingResults;
        this._shadowSettings = shadowSettings;
        _shadowedDirLightCount = 0;
    }

    void ExecuteBuffer()
    {
        _context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

//return strength and index
    public Vector2 ReserveDirShadows(Light light, int visibleLightIndex)
    {
        if (_shadowedDirLightCount < _maxShadowDirLightCount
            && light.shadows != LightShadows.None
            && light.shadowStrength > 0f
            && _cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
        {
            ShadowedDirLights[_shadowedDirLightCount] = new ShadowedDirLight()
            {
                VisibleLightIndex = visibleLightIndex
            };
            return new Vector2(light.shadowStrength, _shadowedDirLightCount++);
        }

        return Vector2.zero;
    }

    public void Render()
    {
        if (_shadowedDirLightCount > 0)
        {
            RenderDirShadows();
        }
        else
        {
            //избегание ошибки 
            FictiveRenderDirShadow();
        }
    }


    private void RenderDirShadows()
    {
        int atlasSize = (int) _shadowSettings.directional.AtlasSize;
       
        buffer.GetTemporaryRT(_dirShadowAtlasId, atlasSize, atlasSize, 16, FilterMode.Bilinear,
            RenderTextureFormat.Shadowmap); //32
        buffer.SetRenderTarget(_dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        buffer.ClearRenderTarget(true, false, Color.clear);
       
        
        //шлем матрицы
        buffer.SetGlobalMatrixArray(_dirShadowMatricesId, _dirShadowMatrices);
        buffer.BeginSample(_bufferName);
        ExecuteBuffer();

        //count of split atlas + and calc tileSize
        int split = _shadowedDirLightCount <= 1 ? 1 : 2;
        int tileSize = atlasSize / split;


        for (int i = 0; i < _shadowedDirLightCount; i++)
        {
            //RenderDirShadows(i, atlasSize); //один источник
            RenderDirShadows(i, split, tileSize); //????
        }

        //шлем массив матриц
        //buffer.SetGlobalMatrixArray(_dirShadowMatricesId, _dirShadowMatrices);
        buffer.EndSample(_bufferName);
        ExecuteBuffer();
    }

    void RenderDirShadows(int index, int split, int tileSize)
    {
        ShadowedDirLight light = ShadowedDirLights[index];
        var shadowSettings = new ShadowDrawingSettings(_cullingResults, light.VisibleLightIndex);
       
        //calculate matrices and etc
        _cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
            light.VisibleLightIndex,
            0,
            1,
            Vector3.zero,
            tileSize,
            0,
            out Matrix4x4 viewMatrix,
            out Matrix4x4 projMatrix,
            out ShadowSplitData splitData
        );
        shadowSettings.splitData = splitData;
       
        //set place in atlas
        SetTileViewport(index, split, tileSize);

        //create VP
        Matrix4x4 VPm = projMatrix * viewMatrix;
        Vector2 offset = GetTileOffset(index, split);
        _dirShadowMatrices[index] = ConvertToAtlasMatrix(VPm, offset, split);

        buffer.SetViewProjectionMatrices(viewMatrix, projMatrix);
        ExecuteBuffer();

        _context.DrawShadows(ref shadowSettings);
    }

    Vector2 GetTileOffset(int index, int split)
    {
        return new Vector2(index % split, (int) (index / split)); //0-0, 1-0, 0-0, 1-1
    }

    //place tile in atlas
    void SetTileViewport(int index, int split, float tileSize)
    {
        Vector2 offset = GetTileOffset(index, split);
        buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
    }

    //recalculate coord 
    Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
    {
        if (SystemInfo.usesReversedZBuffer)
        {
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;
        }

        //uv 0-1 to frustrum -1-1 + 0.5 and atlas scale
        float scale = 1f / split;

        m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
        m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
        m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
        m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;

        m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
        m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
        m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
        m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;

        m.m20 = 0.5f * (m.m20 + m.m30);
        m.m21 = 0.5f * (m.m21 + m.m31);
        m.m22 = 0.5f * (m.m22 + m.m32);
        m.m23 = 0.5f * (m.m23 + m.m33);

        return m;
    }

    //try no avoid mistake
    private void FictiveRenderDirShadow()
    {
        buffer.GetTemporaryRT(_dirShadowAtlasId, 1, 1, 16, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
    }

    public void Cleanup()
    {
        buffer.ReleaseTemporaryRT(_dirShadowAtlasId);
        ExecuteBuffer();
    }
}