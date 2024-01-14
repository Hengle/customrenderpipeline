using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Arycama.CustomRenderPipeline
{
    public class AmbientOcclusion : RenderFeature
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

        private readonly Settings settings;
        private readonly Material material;

        public AmbientOcclusion(Settings settings, RenderGraph renderGraph) : base(renderGraph)
        {
            this.settings = settings;
            material = new Material(Shader.Find("Hidden/Ambient Occlusion")) { hideFlags = HideFlags.HideAndDontSave };
        }

        public void Render(Camera camera, RTHandle depth, RTHandle scene, float scale)
        {
            if (settings.Strength == 0.0f)
                return;

            var scaledWidth = (int)(camera.pixelWidth * scale);
            var scaledHeight = (int)(camera.pixelHeight * scale);
            var normals = renderGraph.GetTexture(scaledWidth, scaledHeight, GraphicsFormat.A2B10G10R10_UNormPack32);
            var viewDepth = renderGraph.GetTexture(scaledWidth, scaledHeight, GraphicsFormat.R16_SFloat);

            var pass0 = renderGraph.AddRenderPass(new FullscreenRenderPass(material, 0));
            pass0.ReadTexture("_CameraDepth", depth);

            pass0.SetRenderFunction((command, context) =>
            {
                pass0.SetVector(command, "ScaleOffset", new Vector2(1.0f / scaledWidth, 1.0f / scaledHeight));

                command.SetRenderTarget(new RenderTargetBinding(
                    new[] { new RenderTargetIdentifier(normals), new RenderTargetIdentifier(viewDepth) },
                    new[] { RenderBufferLoadAction.DontCare, RenderBufferLoadAction.DontCare },
                    new[] { RenderBufferStoreAction.Store, RenderBufferStoreAction.Store },
                    depth, RenderBufferLoadAction.Load, RenderBufferStoreAction.DontCare)
                { flags = RenderTargetFlags.ReadOnlyDepthStencil });

                pass0.Execute(command);
            });

            var pass1 = renderGraph.AddRenderPass(new FullscreenRenderPass(material, 1));
            pass1.ReadTexture("_ViewDepth", viewDepth);
            pass1.ReadTexture("_ViewNormals", normals);
            pass1.ReadTexture("_CameraDepth", depth);

            pass1.SetRenderFunction((command, context) =>
            {
                pass1.SetVector(command, "ScaleOffset", new Vector2(1.0f / scaledWidth, 1.0f / scaledHeight));
                var tanHalfFovY = Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f);
                var tanHalfFovX = tanHalfFovY * camera.aspect;

                pass1.SetVector(command, "_UvToView", new Vector4(tanHalfFovX * 2f, tanHalfFovY * 2f, -tanHalfFovX, -tanHalfFovY));
                pass1.SetVector(command, "_Tint", settings.Tint.linear);
                pass1.SetFloat(command, "_Radius", settings.Radius * scaledHeight / tanHalfFovY * 0.5f);
                pass1.SetFloat(command, "_AoStrength", settings.Strength);
                pass1.SetFloat(command, "_FalloffScale", settings.Falloff == 1f ? 0f : 1f / (settings.Radius * settings.Falloff - settings.Radius));
                pass1.SetFloat(command, "_FalloffBias", settings.Falloff == 1f ? 1f : 1f / (1f - settings.Falloff));
                pass1.SetInt(command, "_DirectionCount", settings.DirectionCount);
                pass1.SetInt(command, "_SampleCount", settings.SampleCount);

                command.SetRenderTarget(new RenderTargetBinding(scene, RenderBufferLoadAction.Load, RenderBufferStoreAction.DontCare, depth, RenderBufferLoadAction.Load, RenderBufferStoreAction.DontCare) { flags = RenderTargetFlags.ReadOnlyDepthStencil });
                pass1.Execute(command);
            });

            if (RenderSettings.fog)
            {
                var pass2 = renderGraph.AddRenderPass(new FullscreenRenderPass(material, 2));
                pass2.SetRenderFunction((command, context) =>
                {
                    pass2.Execute(command);
                });
            }
        }
    }
}