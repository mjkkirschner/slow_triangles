

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
    public class Render3dShadowMapTests
    {
        string root = new DirectoryInfo("../../../../../").FullName;


        [Test]
        public void Prespective_Tex_Map()
        {

            var cameraPos = new Vector3(-2, 5, 1);
            var target = new Vector3(0, 0, 0); 
            var width = 1024;
            var height = 1024;
            var view = Matrix4x4.CreateLookAt(cameraPos, target, Vector3.UnitY);
            var proj = Matrix4x4.CreatePerspective(4, 4, 2f, 10);
            var viewport = MatrixExtensions.CreateViewPortMatrix(0, 0, 255, width, height);

            var mesh = ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo("../../../../../geometry_models/shadowmapscene/simplescene2.obj"));
            var diffuseTex = PNGImage.LoadPNGFromPath("../../../../../textures/testTexture2.png");

            var renderable = new Renderable<Mesh>(
                new DiffuseMaterial()
                {
                    Shader = new Single_DirLight_TextureShader(view, proj, viewport)
                    {
                        uniform_ambient = .5f,
                        uniform_dir_light = new DirectionalLight(new Vector3(1, .2f, 0), true, Color.Red,1f),
                    },
                    DiffuseTexture = new Texture2d(diffuseTex.Width, diffuseTex.Height, diffuseTex.Colors)
                },
                mesh);
            var renderer = new Renderer3dGeneric<Mesh>(width, height, Color.White,
                new List<IEnumerable<Renderable<Mesh>>> { new List<Renderable<Mesh>> { renderable } });

            var image = new ppmImage(width, height, 255);
            image.Colors = renderer.Render();
            var shadowMapImage = new ppmImage(width, height, 255);
            shadowMapImage.Colors = renderer.ShadowMap;
             
            image.Flip();
            shadowMapImage.Flip();

            var path = "../../../ShadowMapTest/";
            System.IO.FileInfo file = new System.IO.FileInfo(path);
            file.Directory.Create();

            System.IO.File.WriteAllBytes("../../../ShadowMapTest/knotShadow1.ppm", image.toByteArray());
            System.IO.File.WriteAllBytes("../../../ShadowMapTest/knotShadow1_shadowMap.ppm", shadowMapImage.toByteArray());
            Assert.AreEqual(847497, image.Colors.Where(x => x == Color.White).Count());

        }
    }
}