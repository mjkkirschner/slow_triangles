

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
    public class Render3dSpecularShaderTest
    {
        string root = new DirectoryInfo("../../../../../").FullName;

        [Test]
        public void specular_Sphere()
        {
            var cameraPos = new Vector3(0, 0, 3);
            var target = new Vector3(0, 0, 0);
            var width = 480;
            var height = 480;
            var view = Matrix4x4.CreateLookAt(cameraPos, target, Vector3.UnitY);
            var proj = Matrix4x4.CreatePerspective(1, 1, 1, 10);
            var viewport = MatrixExtensions.CreateViewPortMatrix(0, 0, 255, width, height);

            var mesh = ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo(Path.Combine(root, "geometry_models/sphere/sphere1.obj")));
            var diffuseTex = PNGImage.LoadPNGFromPath(Path.Combine(root, "geometry_models/sphere/spherediff.png"));

            var renderable = new Renderable<Mesh>(
                new NormalMaterial()
                {
                    Shader = new Lit_TextureShader(view, proj, viewport)
                    {
                        uniform_ambient = 0f,
                        uniform_light_array = new ILight[]
                            { new DirectionalLight(new Vector3(0, 0, 1), false, Color.White) }
                    },
                    DiffuseTexture = new Texture2d(diffuseTex.Width, diffuseTex.Height, diffuseTex.Colors),
                },
                mesh);
            var renderer = new Renderer3dGeneric<Mesh>(width, height, Color.White,
                new List<IEnumerable<Renderable<Mesh>>> { new List<Renderable<Mesh>> { renderable } });

            (renderable.material.Shader as Lit_TextureShader).uniform_cam_world_pos = cameraPos;
            var image = new ppmImage(width, height, 255);
            image.Colors = renderer.Render();
            image.Flip();

            var path = $"../../../specular/spherespec.ppm";
            System.IO.FileInfo file = new System.IO.FileInfo(path);
            file.Directory.Create();
            System.IO.File.WriteAllBytes(path, image.toByteArray());

        }
    }
}