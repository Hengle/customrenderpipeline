using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

public class AmbientOcclusion
{
    [Serializable]
    public class Settings
    {
        [SerializeField, Range(0.0f, 8.0f)] private float strength = 1.0f;
        [SerializeField] private Color tint = Color.black;
        [SerializeField] private float radius = 5.0f;
        [SerializeField, Range(0f, 1f)] private float falloff = 0.75f;
        [SerializeField, Range(1, 8)] private int directionCount = 1;
        [SerializeField, Range(1, 32)] private int sampleCount = 8;

        public float Strength => strength;
        public Color Tint => tint;
        public float Radius => radius;
        public float Falloff => falloff;
        public int DirectionCount => directionCount;
        public int SampleCount => sampleCount;
    }

    private Settings settings;
    private Material material;
    private ComputeShader computeShader;
    private CustomSampler sampler, csSampler;

    public AmbientOcclusion(Settings settings)
    {
        this.settings = settings;
        material = new Material(Shader.Find("Hidden/Ambient Occlusion")) { hideFlags = HideFlags.HideAndDontSave };
        computeShader = Resources.Load<ComputeShader>("PostProcessing/NormalsFromDepth");

        sampler = CustomSampler.Create("Ambient Occlusion", true);
        csSampler = CustomSampler.Create("Ambient Occlusion CS", true);
    }

    public void Render(CommandBuffer command, Camera camera, RenderTargetIdentifier depth, RenderTargetIdentifier scene)
    {
        if (settings.Strength == 0.0f)
            return;


        var normals = Shader.PropertyToID("_ViewNormals");
        command.GetTemporaryRT(normals, new RenderTextureDescriptor(camera.pixelWidth, camera.pixelHeight, RenderTextureFormat.ARGB2101010) { enableRandomWrite = true });

        var viewDepth = Shader.PropertyToID("_ViewDepth");
        command.GetTemporaryRT(viewDepth, new RenderTextureDescriptor(camera.pixelWidth, camera.pixelHeight, RenderTextureFormat.RHalf) { enableRandomWrite = true });

        command.SetComputeTextureParam(computeShader, 0, "_CameraDepth", depth);
        command.SetComputeTextureParam(computeShader, 0, "DepthResult", viewDepth);
        command.SetComputeTextureParam(computeShader, 0, "NormalResult", normals);

        command.SetComputeIntParam(computeShader, "Width", camera.pixelWidth);
        command.SetComputeIntParam(computeShader, "Height", camera.pixelHeight);

        computeShader.GetKernelThreadGroupSizes(0, out var x, out var y, out var z);

        var threadGroupsX = (int)((camera.pixelWidth - 1) / x) + 1;
        var threadGroupsY = (int)((camera.pixelHeight - 1) / y) + 1;

        command.SetComputeIntParam(computeShader, "DispatchSizeX", threadGroupsX);
        command.SetComputeIntParam(computeShader, "DispatchSizeY", threadGroupsY);

        //command.BeginSample(csSampler);
        //command.DispatchCompute(computeShader, 0, threadGroupsX, threadGroupsY, 1);
        //command.EndSample(csSampler);

        command.SetGlobalVector("ScaleOffset", new Vector2(1.0f / camera.pixelWidth, 1.0f / camera.pixelHeight));

        command.SetRenderTarget(new RenderTargetBinding(
            new [] { new RenderTargetIdentifier(normals), new RenderTargetIdentifier(viewDepth) }, 
            new [] { RenderBufferLoadAction.DontCare, RenderBufferLoadAction.DontCare },
            new [] { RenderBufferStoreAction.Store, RenderBufferStoreAction.Store },
            depth, RenderBufferLoadAction.Load, RenderBufferStoreAction.DontCare) { flags = RenderTargetFlags.ReadOnlyDepthStencil });

        //command.BeginSample(sampler);
        command.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Triangles, 3);
       // command.EndSample(sampler);

       // Debug.Log($"{csSampler.GetRecorder().gpuElapsedNanoseconds}, {sampler.GetRecorder().gpuElapsedNanoseconds}");

        var tanHalfFovY = Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f);
        var tanHalfFovX = tanHalfFovY * camera.aspect;
        command.SetGlobalVector("_UvToView", new Vector4(tanHalfFovX * 2f, tanHalfFovY * 2f, -tanHalfFovX, -tanHalfFovY));

        command.SetGlobalVector("_Tint", settings.Tint.linear);
        command.SetGlobalFloat("_Radius", settings.Radius * camera.pixelHeight / tanHalfFovY * 0.5f);
        command.SetGlobalFloat("_AoStrength", settings.Strength);
        command.SetGlobalFloat("_FalloffScale", settings.Falloff == 1f ? 0f : 1f / (settings.Radius * settings.Falloff - settings.Radius));
        command.SetGlobalFloat("_FalloffBias", settings.Falloff == 1f ? 1f : 1f / (1f - settings.Falloff));
        command.SetGlobalInt("_DirectionCount", settings.DirectionCount);
        command.SetGlobalInt("_SampleCount", settings.SampleCount);

        command.SetGlobalTexture("_ViewDepth", viewDepth);
        command.SetGlobalTexture("_ViewNormals", normals);
        command.SetGlobalTexture("_CameraDepth", depth);
        command.SetRenderTarget(new RenderTargetBinding(scene, RenderBufferLoadAction.Load, RenderBufferStoreAction.DontCare, depth, RenderBufferLoadAction.Load, RenderBufferStoreAction.DontCare) { flags = RenderTargetFlags.ReadOnlyDepthStencil });
        command.DrawProcedural(Matrix4x4.identity, material, 1, MeshTopology.Triangles, 3);

        if(RenderSettings.fog)
            command.DrawProcedural(Matrix4x4.identity, material, 2, MeshTopology.Triangles, 3);


    }
}
