﻿using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace Arycama.CustomRenderPipeline
{
    public class ObjectRenderer : RenderFeature
    {
        private RenderQueueRange renderQueueRange;
        private readonly SortingCriteria sortingCriteria;
        private readonly bool excludeObjectMotionVectors;
        private readonly PerObjectData perObjectData;
        private readonly string passName, profilerTag;

        public ObjectRenderer(RenderQueueRange renderQueueRange, SortingCriteria sortingCriteria, bool excludeObjectMotionVectors, PerObjectData perObjectData, string passName, RenderGraph renderGraph) : base(renderGraph)
        {
            this.renderQueueRange = renderQueueRange;
            this.sortingCriteria = sortingCriteria;
            this.excludeObjectMotionVectors = excludeObjectMotionVectors;
            this.passName = passName;
            this.perObjectData = perObjectData;

            profilerTag = $"Render Objects ({passName})";
        }

        public void Render(CullingResults cullingResults, Camera camera)
        {
            var pass = renderGraph.AddRenderPass<GlobalRenderPass>();
            pass.SetRenderFunction((command, context) =>
            {
                using var profilerScope = command.BeginScopedSample(profilerTag);

                var srpDefaultUnlitShaderPassName = new ShaderTagId(passName);
                var rendererListDesc = new RendererListDesc(srpDefaultUnlitShaderPassName, cullingResults, camera)
                {
                    renderQueueRange = renderQueueRange,
                    sortingCriteria = sortingCriteria,
                    excludeObjectMotionVectors = excludeObjectMotionVectors,
                    rendererConfiguration = perObjectData
                };

                var rendererList = context.CreateRendererList(rendererListDesc);
                command.DrawRendererList(rendererList);
            });
        }
    }
}