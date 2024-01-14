﻿using UnityEngine;
using UnityEngine.Rendering;

namespace Arycama.CustomRenderPipeline
{
    public class FullscreenRenderPass : RenderPass
    {
        private readonly MaterialPropertyBlock propertyBlock;
        private readonly Material material;
        private readonly int passIndex;

        public FullscreenRenderPass(Material material, int passIndex = 0)
        {
            propertyBlock = new MaterialPropertyBlock();
            this.material = material;
            this.passIndex = passIndex;
        }

        public override void SetTexture(CommandBuffer command, string propertyName, Texture texture)
        {
            propertyBlock.SetTexture(propertyName, texture);
        }

        public override void SetBuffer(CommandBuffer command, string propertyName, GraphicsBuffer buffer)
        {
            propertyBlock.SetBuffer(propertyName, buffer);
        }

        public override void SetVector(CommandBuffer command, string propertyName, Vector4 value)
        {
            propertyBlock.SetVector(propertyName, value);
        }

        public override void SetFloat(CommandBuffer command, string propertyName, float value)
        {
            propertyBlock.SetFloat(propertyName, value);
        }

        public override void SetInt(CommandBuffer command, string propertyName, int value)
        {
            propertyBlock.SetInt(propertyName, value);
        }

        public override void Execute(CommandBuffer command)
        {
            command.DrawProcedural(Matrix4x4.identity, material, passIndex, MeshTopology.Triangles, 3, 1, propertyBlock);
        }
    }
}