using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    private const string _bufferName = "Lighting";

    private CommandBuffer buffer = new CommandBuffer()
    {
        name = _bufferName
    };

    private CullingResults _cullingResult;
    private const int _maxDirLightCount = 4;

    private static int dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");

    private static int dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");

    private static int dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");

    private static Vector4[] dirLightColors = new Vector4[_maxDirLightCount];

    private static Vector4[] dirLightDirections = new Vector4[_maxDirLightCount];

    private Shadows _shadows = new Shadows();


    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        this._cullingResult = cullingResults;
        buffer.BeginSample(_bufferName);
       
        //shadows
        _shadows.Setup(context, cullingResults, shadowSettings);
        
        SetupLights();
        
        //render shadows
        _shadows.Render();
        
        buffer.EndSample(_bufferName);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    private void SetupLights()
    {
        NativeArray<VisibleLight> visibleLights = _cullingResult.visibleLights;
        int dirLightCount = 0;
        for (int i = 0; i < visibleLights.Length; i++)
        {
            VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType != LightType.Directional) continue;
            SetupDirectionalLight(dirLightCount++, ref visibleLight);
            if (dirLightCount >= _maxDirLightCount)
            {
                break;
            }
        }

        buffer.SetGlobalInt(dirLightCountId, visibleLights.Length);
        buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
        buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
    }

    private void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
    {
        dirLightColors[index] = visibleLight.finalColor;
        dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
        _shadows.ReserveDirShadows(visibleLight.light, index);
    }

    public void Cleanup()
    {
        _shadows.Cleanup();
    }
    
}