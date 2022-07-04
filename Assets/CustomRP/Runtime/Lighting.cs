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

    private static int _dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
    private static int _dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
    private static int _dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");
    private static int _dirLightShadowDataId = Shader.PropertyToID("_DirLightShadowData");
    
    private static Vector4[] _dirLightColors = new Vector4[_maxDirLightCount];
    private static Vector4[] _dirLightDirections = new Vector4[_maxDirLightCount];
    private static Vector4[] _dirLightShadowData = new Vector4[_maxDirLightCount];

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

        buffer.SetGlobalInt(_dirLightCountId, visibleLights.Length);
        buffer.SetGlobalVectorArray(_dirLightColorsId, _dirLightColors);
        buffer.SetGlobalVectorArray(_dirLightDirectionsId, _dirLightDirections);
        buffer.SetGlobalVectorArray(_dirLightShadowDataId, _dirLightShadowData);
    }

    private void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
    {
        _dirLightColors[index] = visibleLight.finalColor;
        _dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
        _dirLightShadowData[index] = _shadows.ReserveDirShadows(visibleLight.light, index);
        
    }

    public void Cleanup()
    {
        _shadows.Cleanup();
    }
    
}