
using UnityEngine.Rendering;
using UnityEngine;

public class CustomRenderPipeline : RenderPipeline
{
    
    private CameraRenderer _renderer = new CameraRenderer();


    public CustomRenderPipeline()
    {
        //SRP batching
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
    }
   
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            _renderer.Render(context, camera);
        }
    }
}
