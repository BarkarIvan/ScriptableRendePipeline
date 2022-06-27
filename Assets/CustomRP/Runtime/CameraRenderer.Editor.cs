using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    partial void DrawUnsupportedShaders();
    partial void DrawGizmos();
    partial void PrepareForSceneWindow();
    partial void PrepareBuffer();

#if UNITY_EDITOR

    private string SampleName { get; set; }

    private static ShaderTagId[] legacyShadeTagIds =
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMGBM"),
        new ShaderTagId("VertexLM")
    };

    private static Material _errorMaterial;

    partial void DrawGizmos()
    {
        if (Handles.ShouldRenderGizmos())
        {
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }
    }


    partial void DrawUnsupportedShaders()
    {
        if (_errorMaterial == null)
        {
            _errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }

        var drawingSettings = new DrawingSettings(legacyShadeTagIds[0], new SortingSettings(camera))
        {
            overrideMaterial = _errorMaterial
        };

        for (int i = 1; i < legacyShadeTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, legacyShadeTagIds[i]);
        }

        var filteringSettings = FilteringSettings.defaultValue;
        context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
    }

    //render ui in scene wiew
    partial void PrepareForSceneWindow()
    {
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }
    }

    partial void PrepareBuffer()
    {
        Profiler.BeginSample("Editor only");
        buffer.name = SampleName = camera.name;
        Profiler.EndSample();
    }
#else
    const string SampleName => bufferName;
#endif
}