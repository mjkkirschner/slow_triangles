

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using renderer._3d;
using renderer.core;
using renderer.dataStructures;
using renderer.materials;
using renderer.shaders;
using renderer.utilities;
using renderer_core.dataStructures;

namespace Tests
{
    public class Render3dAnimationTests
    {
        string root = new DirectoryInfo("../../../../../").FullName;


        //note - model in this test is taken directly from:
        //https://github.com/ssloy/tinyrenderer/tree/master/obj/african_head
        //to compare the output to sample rendering.
        [Test]
        public void Prespective_Normal_Map_TinyRenderReferenceAnimation()
        {

            var cameraPos = new Vector3(-.6f, .6f, 3);
            var target = new Vector3(0, 0, 0);
            var width = 480;
            var height = 480;
            var view = Matrix4x4.CreateLookAt(cameraPos, target, Vector3.UnitY);
            var proj = Matrix4x4.CreatePerspective(1, 1, 1, 10);
            var viewport = MatrixExtensions.CreateViewPortMatrix(0, 0, 255, width, height);

            var mesh = ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo(Path.Combine(root, "geometry_models/tiny_renderer_sample_models/african_head.obj")));
            var diffuseTex = PNGImage.LoadPNGFromPath(Path.Combine(root, "geometry_models/tiny_renderer_sample_models/african_head_diffuse2.png"));
            var normalMap = PNGImage.LoadPNGFromPath(Path.Combine(root, "geometry_models/tiny_renderer_sample_models/african_head_nm_tangent.png"));

            var lightvals = Enumerable.Range(0, 11).Select(x => (x / 5.0) - 1.0).ToList();

            var renderable = new Renderable<Mesh>(
                new NormalMaterial()
                {
                    Shader = new Lit_TextureShader(view, proj, viewport) { uniform_ambient = .5f, 
                    uniform_light_array = new ILight[]
                            { new DirectionalLight(new Vector3(0, 0, 1), false, Color.White) }
                    },
                    DiffuseTexture = new Texture2d(diffuseTex.Width, diffuseTex.Height, diffuseTex.Colors),
                    NormalMap = new Texture2d(normalMap.Width, normalMap.Height, normalMap.Colors)
                },
                mesh);
            var renderer = new Renderer3dGeneric<Mesh>(width, height, Color.White,
                new List<IEnumerable<Renderable<Mesh>>> { new List<Renderable<Mesh>> { renderable } });

            for (var i = 0; i < lightvals.Count; i++)
            {
                var mat = Matrix4x4.CreateRotationY(0.174533f * 3f);
                var transformedLightDir = Vector3.Transform(((renderable.material.Shader as Lit_TextureShader).uniform_light_array[0] as DirectionalLight).Direction, Matrix4x4.Transpose(mat));
                ((renderable.material.Shader as Lit_TextureShader).uniform_light_array[0] as DirectionalLight).Direction = transformedLightDir;
                var image = new ppmImage(width, height, 255);
                image.Colors = renderer.Render();
                image.Flip();

                var path = $"../../../animation1/perspectiveTestNormalHead{i}.ppm";
                System.IO.FileInfo file = new System.IO.FileInfo(path);
                file.Directory.Create();
                System.IO.File.WriteAllBytes(path, image.toByteArray());
            }
        }
    }
}