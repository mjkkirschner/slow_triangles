

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
    public class Render3dIBLTests
    {
        string root = new DirectoryInfo("../../../../../").FullName;


        [Test]
        public void Prespective_Tex_Map()
        {

            var cameraPos = new Vector3(0, 0, 10);
            var target = new Vector3(0, 0, 0);
            var width = 1024;
            var height = 1024;
            var view = Matrix4x4.CreateLookAt(cameraPos, target, Vector3.UnitY);
            var proj = Matrix4x4.CreatePerspective(4, 4, 2f, 10);
            var viewport = MatrixExtensions.CreateViewPortMatrix(0, 0, 255, width, height);

            var mesh = ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo("../../../../../geometry_models/sphere/sphere1.obj"));
            var skybox = ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo("../../../../../geometry_models/largesphere/largesphere.obj"));
            var diffuseTex = PNGImage.LoadPNGFromPath("../../../../../geometry_models/sphere/spherediff.png");
            var lighTex = PNGImage.LoadPNGFromPath("../../../../../textures/testMaps/IBLtests/small_hangar_02_2kblur.png");
            var skytex = PNGImage.LoadPNGFromPath("../../../../../textures/testMaps/IBLtests/small_hangar_02_2k.png");

            var renderable = new Renderable<Mesh>(
                new ImageBasedLightMaterial()
                {
                    Shader = new ImageBasedLightingShader(view, proj, viewport)
                    {

                        uniform_cam_world_pos = cameraPos
                    },
                    DiffuseTexture = new Texture2d(diffuseTex.Width, diffuseTex.Height, diffuseTex.Colors),
                    LightTexture = new Texture2d(lighTex.Width, lighTex.Height, lighTex.Colors)
                },
                mesh);

            var skyboxrenderable = new Renderable<Mesh>(
           new SkyBoxMaterial()
           {
               Shader = new SkyBoxShader(view, proj, viewport)
               {
                   uniform_cam_world_pos = cameraPos
               },
               LightTexture = new Texture2d(skytex.Width, skytex.Height, skytex.Colors)
           },
           skybox);
            var renderer = new Renderer3dGeneric<Mesh>(width, height, Color.White,
                new List<IEnumerable<Renderable<Mesh>>> { new List<Renderable<Mesh>> { renderable, skyboxrenderable } }, enableShadowPass: false);

            var image = new ppmImage(width, height, 255);
            image.Colors = renderer.Render();
            var shadowMapImage = new ppmImage(width, height, 255);

            var path = "../../../IBLtest/";
            System.IO.FileInfo file = new System.IO.FileInfo(path);
            file.Directory.Create();
            image.Flip();
            System.IO.File.WriteAllBytes("../../../IBLtest/knotShadow1.ppm", image.toByteArray());
            Assert.AreEqual(847497, image.Colors.Where(x => x == Color.White).Count());

        }
    }
}