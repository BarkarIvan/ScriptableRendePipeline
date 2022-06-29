
using UnityEngine.Rendering;
using UnityEngine;

public class CustomRenderPipeline : RenderPipeline
{
    
    private CameraRenderer _renderer = new CameraRenderer();
    private ShadowSettings _shadowSettings;
    private bool useDynamicBatching, useGPUInstancing;

    public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatching, ShadowSettings shadowSettings)
    {
        //shadows
        this._shadowSettings = shadowSettings;
        //batching settings
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
        //SRP batching
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatching;
        GraphicsSettings.lightsUseLinearIntensity = true;//???
    }
   
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        BeginFrameRendering(context,cameras);   //event
       
        foreach (var camera in cameras)
        {
           BeginCameraRendering(context,camera); //event
           _renderer.Render(context, camera, useDynamicBatching, useGPUInstancing,_shadowSettings);
        }
    }
}
