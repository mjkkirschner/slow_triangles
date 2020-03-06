using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace slow_triangles.DynamoNodes.Shaders
{

        public static class SingleDirLightNormalShader
        {
            public static renderer.shaders.Single_DirLight_NormalShader ByUniforms(float ambient, bool debugNormals = false)
            {
                var shader = new renderer.shaders.Single_DirLight_NormalShader(Matrix4x4.Identity, Matrix4x4.Identity, Matrix4x4.Identity)
                {
                    uniform_ambient = ambient,
                    uniform_debug_normal = debugNormals,
                };
                return shader;
            }
        }

    public static class SingleDirLightDiffuseShader
    {
        public static renderer.shaders.Single_DirLight_TextureShader ByUniforms(float ambient)
        {
            var shader = new renderer.shaders.Single_DirLight_TextureShader(Matrix4x4.Identity, Matrix4x4.Identity, Matrix4x4.Identity)
            {
                uniform_ambient = ambient,
            };
            return shader;
        }
    }


    public static class UnlitDiffuseTextureShader
        {
            public static renderer.shaders.Unlit_TextureShader ByUniforms(float ambient, bool debugNormals = false)
            {
                var shader = new renderer.shaders.Unlit_TextureShader(Matrix4x4.Identity, Matrix4x4.Identity, Matrix4x4.Identity)
                {
                    uniform_ambient = ambient,
                };
                return shader;
            }
        }

        public static class LitSpecularShader
        {
            public static renderer.shaders.Lit_SpecularTextureShader ByUniforms(float ambient)
            {
                var shader = new renderer.shaders.Lit_SpecularTextureShader(Matrix4x4.Identity, Matrix4x4.Identity, Matrix4x4.Identity)
                {
                    uniform_ambient = ambient,
                };
                return shader;
            }
        }

    }
