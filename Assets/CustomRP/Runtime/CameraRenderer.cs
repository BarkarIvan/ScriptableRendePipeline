
using UnityEngine;
using UnityEngine.Rendering;

public class CameraRenderer
{
    private ScriptableRenderContext context;
    private Camera camera;

    private const string _bufferName = "CRP Render Camera";

    private CommandBuffer buffer = new CommandBuffer()
    {
        name = _bufferName
    };

    private CullingResults _cullingResults;
    private static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    
    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;

        if (!Cull())
        {
            return;
        }
        
        Setup();
        DrawVisibleGeometry();
        Submit();
    }

    private void DrawVisibleGeometry()
    {
        var sortingSettings = new SortingSettings();
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);
        var filteringSettings = new FilteringSettings(RenderQueueRange.all);
        
        context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
        
        context.DrawSkybox(camera);
    }

    bool Cull()
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            _cullingResults = context.Cull(ref p);
            return true;
        }

        return false;
    }
    
    private void Setup()
    {
        context.SetupCameraProperties(camera);
        buffer.ClearRenderTarget(true,true,Color.clear);
        buffer.BeginSample(_bufferName);
        ExecuteBuffer();
    }

    private void Submit()
    {
        buffer.EndSample(_bufferName);
        ExecuteBuffer();
        context.Submit();
    }

    private void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

}
