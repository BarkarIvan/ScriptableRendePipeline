using UnityEditor;
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

    private const int _maxShadowDirLightCount = 1;

    private int _shadowedDirLightCount;

    private static int _dirShadowAtlasId = Shader.PropertyToID("_DirShadowAtlas");


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

    struct ShadowedDirLight
    {
        public int VisibleLightIndex;
    }

    private ShadowedDirLight[] ShadowedDirLights = new ShadowedDirLight[_maxShadowDirLightCount];


    public void ReserveDirShadows(Light light, int visibleLightIndex)
    {
        if (_shadowedDirLightCount < _maxShadowDirLightCount
            && light.shadows != LightShadows.None
            && light.shadowStrength > 0f
            && _cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
        {
            ShadowedDirLights[_shadowedDirLightCount++] = new ShadowedDirLight()
            {
                VisibleLightIndex = visibleLightIndex
            };
        }
    }

    public void Render()
    {
        if (_shadowedDirLightCount > 0)
        {
            RenderDirShadows();
        }
        else
        {
            FictiveRenderDirShadow();
        }
    }


    private void RenderDirShadows()
    {
        int atlasSize = (int) _shadowSettings.directional.AtlasSize;
        buffer.GetTemporaryRT(_dirShadowAtlasId, atlasSize, atlasSize, 16, FilterMode.Bilinear,
            RenderTextureFormat.Shadowmap); //32
        buffer.SetRenderTarget(_dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        buffer.ClearRenderTarget(true,false,Color.clear);
    }


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