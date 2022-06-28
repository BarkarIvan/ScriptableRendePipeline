
using UnityEngine.Rendering;
using UnityEngine;

public class CustomRenderPipeline : RenderPipeline
{
    
    private CameraRenderer _renderer = new CameraRenderer();
    private bool useDynamicBatching, useGPUInstancing;

    public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatching)
    {

        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
        //SRP batching
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatching;
    }
   
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            _renderer.Render(context, camera, useDynamicBatching, useGPUInstancing);
        }
    }
}
