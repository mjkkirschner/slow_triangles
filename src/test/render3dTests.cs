using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using NUnit.Framework;
using renderer.core;
using System.Linq;
using renderer.dataStructures;

using renderer.utilities;
using renderer._3d;

namespace Tests
{
    public class Render3dTests
    {
        [SetUp]
        public void Setup()
        {
        }


        [Test]
        public void MeshRenderUVsAndTexture()
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
            image.colors = renderer.Render();
            System.IO.File.WriteAllBytes("../../../perspectiveTest1.ppm", image.toByteArray());
            Assert.AreEqual(300115, image.colors.Where(x => x == Color.White).Count());


        }





    }
}