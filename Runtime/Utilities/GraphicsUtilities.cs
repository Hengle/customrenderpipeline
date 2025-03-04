﻿using UnityEngine;

namespace Arycama.CustomRenderPipeline
{
    public static class GraphicsUtilities
    {
        public static Vector4 ThreadIdScaleOffset(int width, int height)
        {
            return new Vector4((float)(1.0 / width), (float)(1.0 / height), (float)(0.5 / width), (float)(0.5 / height));
        }

        /// <summary>
        /// Calculates a scale and offset for remapping a UV from a 0-1 range to a halfTexel to (1-halfTexel) range
        /// </summary>
        public static Vector2 HalfTexelRemap(float width)
        {
            var invWidth = 1f / width;
            return new Vector2(1f - invWidth, 0.5f * invWidth);
        }

        /// <summary>
        /// Calculates a scale and offset for remapping a UV from a 0-1 range to a halfTexel to (1-halfTexel) range
        /// </summary>
        public static Vector4 HalfTexelRemap(float width, float height)
        {
            var invWidth = 1f / width;
            var invHeight = 1f / height;
            return new Vector4(1f - invWidth, 1f - invHeight, 0.5f * invWidth, 0.5f * invHeight);
        }

        /// <summary>
        /// Calculates ScaleOffset to Remap a CS thread to UV coordinate that stretches from 0:1. (No half-texel offset)
        /// </summary>
        public static Vector3 ThreadIdScaleOffset01(int width, int height, int depth)
        {
            return new Vector3(1f / (width - 1), 1f / (height - 1), 1f / (depth - 1));
        }

        /// <summary>
        /// Calculates ScaleOffset to Remap a CS thread to UV coordinate that stretches from 0:1. (No half-texel offset)
        /// </summary>
        public static Vector2 ThreadIdScaleOffset01(int width, int height)
        {
            return new Vector2(1f / (width - 1), 1f / (height - 1));
        }
    }
}