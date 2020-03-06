using Autodesk.DesignScript.Runtime;
using renderer.dataStructures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace slow_triangles.DynamoNodes.Materials
{

    /// <summary>
    /// An interface for material wrappers.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class Material
    {
        public renderer.dataStructures.Shader Shader { get; protected set; }
        public renderer.dataStructures.IMaterial InternalMaterial { get; protected set; }
    }

    /// <summary>
    /// A diffuse material.
    /// </summary>
    public class DiffuseMaterial : Material
    {
        public static DiffuseMaterial ByColorArray(DSCore.Color[][] pixels, [DefaultArgument("null")] object shader)
        {
            renderer.dataStructures.Shader realShader;
            if (shader == null || !(shader is renderer.dataStructures.Shader))
            {
                //TODO these parameters of the shader need to be set at render time based on the camera.
                //use defaults for now.
                realShader = new renderer.shaders.Unlit_TextureShader(Matrix4x4.Identity, Matrix4x4.Identity, Matrix4x4.Identity);
            }
            else
            {
                realShader = shader as renderer.dataStructures.Shader;
            }

            var height = pixels.Length;
            var width = pixels[0].Length;
            //TODO may neeed to transpose first.
            var colorsFlat = pixels.SelectMany(i => i);
            var texture = new Texture2d(width, height, colorsFlat.Select(x => Color.FromArgb(x.Alpha, x.Red, x.Green, x.Blue)));

            var difMaterial = new DiffuseMaterial()
            {
                InternalMaterial = new renderer.materials.DiffuseMaterial()
                {
                    DiffuseTexture = texture,
                    Shader = realShader
                }
            };
            return difMaterial;
        }

        public static DiffuseMaterial ByTexture(Texture2d diffuseMap, [DefaultArgument("null")] object shader)
        {
            //ugly. and slow.
            var colorData = meshHelpers.Split<Color>(diffuseMap.ColorData.ToList(), diffuseMap.Width).Select(x => x.Select(y => DSCore.Color.ByARGB(y.A, y.R, y.G, y.B)).ToArray()).ToArray();
            return DiffuseMaterial.ByColorArray(colorData, shader);
        }
        //TODO add constructor with images

        //TODO consider how to set shader on material.
    }

    public class NormalMaterial : DiffuseMaterial
    {
        public static NormalMaterial ByTextures(Texture2d diffuseMap, Texture2d normalMap, [DefaultArgument("null")] object shader)
        {
            renderer.dataStructures.Shader realShader;
            if (shader == null || !(shader is renderer.dataStructures.Shader))
            {
                //TODO these parameters of the shader need to be set at render time based on the camera.
                //use defaults for now.
                realShader = new renderer.shaders.Single_DirLight_NormalShader(Matrix4x4.Identity, Matrix4x4.Identity, Matrix4x4.Identity);
            }
            else
            {
                realShader = shader as renderer.dataStructures.Shader;
            }

            return new NormalMaterial()
            {

                InternalMaterial = new renderer.materials.NormalMaterial()
                {
                    DiffuseTexture = diffuseMap,
                    NormalMap = normalMap,
                    Shader = realShader
                }
            };

        }
    }

    public class SpecularMaterial : DiffuseMaterial
    {
        public static SpecularMaterial ByTexturesAndCoef(Texture2d diffuseMap, float shininess, float specPower, float diffusePower, [DefaultArgument("null")] object shader)
        {
            renderer.dataStructures.Shader realShader;
            if (shader == null || !(shader is renderer.dataStructures.Shader))
            {
                //TODO these parameters of the shader need to be set at render time based on the camera.
                //use defaults for now.
                realShader = new renderer.shaders.Lit_SpecularTextureShader(Matrix4x4.Identity, Matrix4x4.Identity, Matrix4x4.Identity);
            }
            else
            {
                realShader = shader as renderer.dataStructures.Shader;
            }

            return new SpecularMaterial()
            {

                InternalMaterial = new renderer.materials.SpecularMaterial()
                {
                    DiffuseTexture = diffuseMap,
                    Kd = diffusePower,
                    Ks = specPower,
                    Shininess = shininess,
                    Shader = realShader
                }
            };

        }
    }

}
