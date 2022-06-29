using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    private ScriptableRenderContext context;
    private Camera camera;

    private const string _bufferName = "CRP Render Camera";

    private CommandBuffer buffer = new CommandBuffer()
    {
        name = _bufferName
    };

    private CullingResults _cullingResults;

    private static ShaderTagId unlitShaderTagId = new ShaderTagId("CustomRPUnlit");
    private static ShaderTagId litShaderTagId = new ShaderTagId("CustomRPLit");

    private Lighting _lighting = new Lighting();
    
    public void Render(ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useGPUInstancing, ShadowSettings shadowSettings)
    {
        this.context = context;
        this.camera = camera;

        PrepareBuffer();
        PrepareForSceneWindow();
        
        if (!Cull(shadowSettings.MaxDistance))
        {
            return;
        }
        
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
        _lighting.Setup(context, _cullingResults, shadowSettings);
        buffer.EndSample(SampleName);
        
        Setup();
        
        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
        DrawUnsupportedShaders();
        DrawGizmos();
        _lighting.Cleanup();
        Submit();
    }

    private void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        //opaque
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing
        };
        drawingSettings.SetShaderPassName(1, litShaderTagId);
       
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);

        //skybox
        context.DrawSkybox(camera);

        //transparent
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;

        context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
    }


    bool Cull(float maxShadowDistance)
    {
        if (!camera.TryGetCullingParameters(out ScriptableCullingParameters p)) return false;
        p.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
        _cullingResults = context.Cull(ref p);
        return true;
    }

    private void Setup()
    {
        context.SetupCameraProperties(camera);
        CameraClearFlags flags = camera.clearFlags;
        buffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth, 
            flags == CameraClearFlags.Color, 
            flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear
            );
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
    }

    private void Submit()
    {
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }

    private void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }
}