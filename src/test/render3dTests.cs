using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using NUnit.Framework;
using renderer.core;
using System.Linq;
using renderer.dataStructures;

using renderer.utilities;
using renderer._3d;
using renderer.materials;
using renderer.shaders;
using System.IO;
using renderer_core.dataStructures;

namespace Tests
{
    public class Render3dTests
    {
        string root = new DirectoryInfo("../../../../../").FullName;


        [SetUp]
        public void Setup()
        {
        }


        [Test]
        public void MeshRenderPerspProjectionShader()
        {

            var cameraPos = new Vector3(0, 0, 4);
            var target = new Vector3(0, 0, 0);
            var view = Matrix4x4.CreateLookAt(cameraPos, target, Vector3.UnitY);
            var proj = Matrix4x4.CreatePerspective(4, 3, 1, 10);
            var viewport = MatrixExtensions.CreateViewPortMatrix(0, 0, 255, 640, 480);

            var mesh = ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo("../../../../../geometry_models/knot3/knot3.obj"));

            var renderable = new Renderable<Mesh>(
                new Material()
                {
                    Shader = new Base3dShader(view, proj, viewport)
                },
                mesh);
            var renderer = new Renderer3dGeneric<Mesh>(640, 480, Color.White,
                new List<IEnumerable<Renderable<Mesh>>> { new List<Renderable<Mesh>> { renderable } });

            var image = new ppmImage(640, 480, 255);
            image.Colors = renderer.Render();
            System.IO.File.WriteAllBytes("../../../perspectiveTest1.ppm", image.toByteArray());
            Assert.AreEqual(300249, image.Colors.Where(x => x == Color.White).Count());


        }

        [Test]
        public void Prespective_Tex_Map()
        {

            var cameraPos = new Vector3(0, 0, 2);
            var target = new Vector3(0, 0, 0);
            var width = 1024;
            var height = 1024;
            var view = Matrix4x4.CreateLookAt(cameraPos, target, Vector3.UnitY);
            var proj = Matrix4x4.CreatePerspective(4, 4, 1, 10);
            var viewport = MatrixExtensions.CreateViewPortMatrix(0, 0, 255, width, height);

            var mesh = ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo("../../../../../geometry_models/knot3/knot3.obj"));
            var diffuseTex = new ppmImage("../../../../../textures/testTexture2.ppm");

            var renderable = new Renderable<Mesh>(
                new DiffuseMaterial()
                {
                    Shader = new Unlit_TextureShader(view, proj, viewport),
                    DiffuseTexture = new Texture2d(diffuseTex.Width, diffuseTex.Height, diffuseTex.Colors)
                },
                mesh);
            var renderer = new Renderer3dGeneric<Mesh>(width, height, Color.White,
                new List<IEnumerable<Renderable<Mesh>>> { new List<Renderable<Mesh>> { renderable } });

            var image = new ppmImage(width, height, 255);
            image.Colors = renderer.Render();
            System.IO.File.WriteAllBytes("../../../perspectiveTestDiffuse.ppm", image.toByteArray());
            Assert.AreEqual(847497, image.Colors.Where(x => x == Color.White).Count());


        }

        [Test]
        public void Prespective_Normal_Map()
        {

            var cameraPos = new Vector3(0, 2, 2);
            var target = new Vector3(0, 0, 0);
            var width = 1024;
            var height = 1024;
            var view = Matrix4x4.CreateLookAt(cameraPos, target, Vector3.UnitY);
            var proj = Matrix4x4.CreatePerspective(4, 4, 1, 10);
            var viewport = MatrixExtensions.CreateViewPortMatrix(0, 0, 255, width, height);

            var mesh = ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo("../../../../../geometry_models/knot3/knot3.obj"));
            var diffuseTex = new ppmImage("../../../../../textures/testTexture2.ppm");
            var normalMap = PNGImage.LoadPNGFromPath("../../../../../textures/testMaps/gridnormalmap.png");

            var renderable = new Renderable<Mesh>(
                new NormalMaterial()
                {
                    Shader = new Lit_TextureShader(view, proj, viewport)
                    {
                        uniform_ambient = .5f,
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
            System.IO.File.WriteAllBytes("../../../perspectiveTestNormal.ppm", image.toByteArray());
            Assert.AreEqual(1002520, image.Colors.Where(x => x == Color.White).Count());


        }

        //note - model in this test is taken directly from:
        //https://github.com/ssloy/tinyrenderer/tree/master/obj/african_head
        //to compare the output to sample rendering.
        [Test]
        public void Prespective_Tex_Map_TinyRenderReference()
        {

            var cameraPos = new Vector3(-1, 0, 1);
            var target = new Vector3(0, 0, 0);
            var width = 2048;
            var height = 2048;
            var view = Matrix4x4.CreateLookAt(cameraPos, target, Vector3.UnitY);
            var proj = Matrix4x4.CreatePerspective(4, 4, 1, 10);
            var viewport = MatrixExtensions.CreateViewPortMatrix(0, 0, 255, width, height);

            var mesh = ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo("../../../../../geometry_models/tiny_renderer_sample_models/african_head.obj"));
            var diffuseTex = PNGImage.LoadPNGFromPath("../../../../../geometry_models/tiny_renderer_sample_models/african_head_diffuse2.png");

            var renderable = new Renderable<Mesh>(
                new DiffuseMaterial()
                {
                    Shader = new Lit_NormalShader(view, proj, viewport)
                    {
                        uniform_ambient = 10,
                        uniform_light_array = new ILight[]
                            { new DirectionalLight(new Vector3(0, 0, 1), false, Color.Red) },
                    },

                    DiffuseTexture = new Texture2d(diffuseTex.Width, diffuseTex.Height, diffuseTex.Colors),
                },
                mesh);
            var renderer = new Renderer3dGeneric<Mesh>(width, height, Color.White,
                new List<IEnumerable<Renderable<Mesh>>> { new List<Renderable<Mesh>> { renderable } });

            var image = new ppmImage(width, height, 255);
            image.Colors = renderer.Render();
            System.IO.File.WriteAllBytes("../../../perspectiveTestTexHead.ppm", image.toByteArray());
            Assert.AreEqual(3856227, image.Colors.Where(x => x == Color.White).Count());


        }

        //note - model in this test is taken directly from:
        //https://github.com/ssloy/tinyrenderer/tree/master/obj/african_head
        //to compare the output to sample rendering.
        [Test]
        public void Prespective_Normal_Map_TinyRenderReference()
        {

            var cameraPos = new Vector3(-.6f, .6f, 3);
            var target = new Vector3(0, 0, 0);
            var width = 2048;
            var height = 2048;
            var view = Matrix4x4.CreateLookAt(cameraPos, target, Vector3.UnitY);
            var proj = Matrix4x4.CreatePerspective(1, 1, 1, 10);
            var viewport = MatrixExtensions.CreateViewPortMatrix(0, 0, 255, width, height);

            var mesh = ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo("../../../../../geometry_models/tiny_renderer_sample_models/african_head.obj"));
            var diffuseTex = PNGImage.LoadPNGFromPath("../../../../../geometry_models/tiny_renderer_sample_models/african_head_diffuse2.png");
            var normalMap = PNGImage.LoadPNGFromPath("../../../../../geometry_models/tiny_renderer_sample_models/african_head_nm_tangent.png");

            var renderable = new Renderable<Mesh>(
                new NormalMaterial()
                {
                    Shader = new Lit_NormalShader(view, proj, viewport)
                    {
                        uniform_ambient = 10,
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
            System.IO.File.WriteAllBytes("../../../perspectiveTestNormalHead.ppm", image.toByteArray());
            Assert.AreEqual(3362439, image.Colors.Where(x => x == Color.White).Count());


        }

        [Test]
        public void RenderIsFlippedCorrectly()
        {

            var cameraPos = new Vector3(-.6f, .6f, 5);
            var target = new Vector3(0, 0, 0);
            var width = 2048;
            var height = 2048;
            var view = Matrix4x4.CreateLookAt(cameraPos, target, Vector3.UnitY);
            var proj = Matrix4x4.CreatePerspective(1, 1, 1, 10);
            var viewport = MatrixExtensions.CreateViewPortMatrix(0, 0, 255, width, height);

            var mesh = ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo(System.IO.Path.Combine(root, "geometry_models/head_asymmetric/head.OBJ")));
            var diffuseTex = PNGImage.LoadPNGFromPath(System.IO.Path.Combine(root, "geometry_models/head_asymmetric/head.png"));

            var renderable = new Renderable<Mesh>(
                new DiffuseMaterial()
                {
                    Shader = new Lit_NormalShader(view, proj, viewport)
                    {
                        uniform_light_array = new ILight[]
                            { new DirectionalLight(new Vector3(0, 0, 1), false, Color.Red) },
                    },
                    DiffuseTexture = new Texture2d(diffuseTex.Width, diffuseTex.Height, diffuseTex.Colors),
                },
                mesh);
            var renderer = new Renderer3dGeneric<Mesh>(width, height, Color.White,
                new List<IEnumerable<Renderable<Mesh>>> { new List<Renderable<Mesh>> { renderable } });

            var image = new ppmImage(width, height, 255);
            image.Colors = renderer.Render();
            image.Flip();
            System.IO.File.WriteAllBytes("../../../asymmetricHead.ppm", image.toByteArray());
            Assert.AreEqual(3528584, image.Colors.Where(x => x == Color.White).Count());
        }

    }
}