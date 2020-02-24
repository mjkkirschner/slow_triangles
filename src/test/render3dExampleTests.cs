using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using NUnit.Framework;
using renderer._3d;
using renderer.core;
using renderer.dataStructures;
using renderer.materials;
using renderer.shaders;
using renderer.utilities;
using System.Linq;
using renderer_core.dataStructures;

namespace Tests
{
    public class Render3dExamples
    {
        string root = new DirectoryInfo("../../../../../").FullName;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Prespective_Normal_Map_ZbrushBust1()
        {

            var cameraPos = new Vector3(-5f, .5f, 4f);
            var target = new Vector3(0, 0, 0);
            var width = 2048;
            var height = 2048;
            var view = Matrix4x4.CreateLookAt(cameraPos, target, Vector3.UnitY);
            var proj = Matrix4x4.CreatePerspective(1, 1, 1, 10);
            var viewport = MatrixExtensions.CreateViewPortMatrix(0, 0, 255, width, height);

            var mesh = ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo(Path.Combine(root, "geometry_models/bust1/villianhead2.OBJ")));
            var diffuseTex = PNGImage.LoadPNGFromPath(Path.Combine(root, "geometry_models/bust1/diffuse2.png"));
            var normalMap = PNGImage.LoadPNGFromPath(Path.Combine(root, "geometry_models/bust1/normalmap2.png"));

            var renderable = new Renderable<Mesh>(
                new NormalMaterial()
                {
                    Shader = new Lit_NormalShader(view, proj, viewport) { uniform_ambient = 10, 
                     uniform_light_array = new ILight[]
                            { new DirectionalLight(new Vector3(0, 0, 1), false, Color.Red) },
                    },

                    DiffuseTexture = new Texture2d(diffuseTex.Width, diffuseTex.Height, diffuseTex.Colors),
                    NormalMap = new Texture2d(normalMap.Width, normalMap.Height, normalMap.Colors)
                },
                mesh);
            var renderer = new Renderer3dGeneric<Mesh>(width, height, Color.White,
                new List<IEnumerable<Renderable<Mesh>>> { new List<Renderable<Mesh>> { renderable } });

            var image = new ppmImage(width, height, 255);
            image.Colors = renderer.Render();
            image.Flip();
            System.IO.File.WriteAllBytes("../../../perspectiveTestNormalVillian.ppm", image.toByteArray());
            Assert.AreEqual(3680167, image.Colors.Where(x => x == Color.White).Count());


        }

        [Test]
        public void Prespective_Normal_Map_ZbrushMando1()
        {

            var cameraPos = new Vector3(-2f, 1, 4f);
            var target = new Vector3(0, 0, 0);
            var width = 2048;
            var height = 2048;
            var view = Matrix4x4.CreateLookAt(cameraPos, target, Vector3.UnitY);
            var proj = Matrix4x4.CreatePerspective(1, 1, 1, 10);
            var viewport = MatrixExtensions.CreateViewPortMatrix(0, 0, 255, width, height);

            var mesh = ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo(Path.Combine(root, "geometry_models/mandohead/mandoclean.OBJ")));
            var diffuseTex = PNGImage.LoadPNGFromPath(Path.Combine(root, "geometry_models/mandohead/mandocleandiffuse6.png"));
            var normalMap = PNGImage.LoadPNGFromPath(Path.Combine(root, "geometry_models/mandohead/mandocleannormal1.png"));

            var renderable = new Renderable<Mesh>(
                new NormalMaterial()
                {
                    Shader = new Lit_NormalShader(view, proj, viewport) { uniform_ambient = 10, 
                      uniform_light_array = new ILight[]
                            { new DirectionalLight(new Vector3(0, 0, 1), false, Color.Red) },
                    },

                    DiffuseTexture = new Texture2d(diffuseTex.Width, diffuseTex.Height, diffuseTex.Colors),
                    NormalMap = new Texture2d(normalMap.Width, normalMap.Height, normalMap.Colors)
                },
                mesh);
            var renderer = new Renderer3dGeneric<Mesh>(width, height, Color.White,
                new List<IEnumerable<Renderable<Mesh>>> { new List<Renderable<Mesh>> { renderable } });

            var image = new ppmImage(width, height, 255);
            image.Colors = renderer.Render();
            image.Flip();
            System.IO.File.WriteAllBytes("../../../perspectiveTestNormalMandoonecolorfront.ppm", image.toByteArray());
            Assert.AreEqual(3248415, image.Colors.Where(x => x == Color.White).Count());

        }
    }
}
