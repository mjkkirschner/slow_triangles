using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using renderer.core;
using renderer._2d;
using renderer.interfaces;

namespace Tests
{
    public class TriangleRendering
    {
        [SetUp]
        public void Setup()
        {
        }



        [Test]
        public void TriangleRenderTeapot()
        {            //scale and offset all verts.
            var verts = ObjFileLoader.LoadVertsFromObjAtPath(new System.IO.FileInfo("../../../../../geometry_models/teapot.obj"))
            //flip verts since image top is 0,0
            .Select(x => Vector4.Multiply(x, new Vector4(1.0f, -1.0f, 1.0f, 1.0f)))
            //scale and offset.
                .Select(x => Vector4.Multiply(Vector4.Add(x, new Vector4(5, 5, 0, 0)), 100)).ToArray();

            var tris = ObjFileLoader.LoadTrisFromObjAtPath(new System.IO.FileInfo("../../../../../geometry_models/teapot.obj"));



            var renderer = new Triangle2dRenderer(1024, 768, new List<IEnumerable<TriangleFace>> { tris });
            renderer.VertexData = verts.Select(x => new Vector3(x.X, x.Y, x.Z)).ToArray();

            var image = new ppmImage(1024, 768, 255);
            image.colors = renderer.Render();
            //Assert.AreEqual(18702, image.colors.Where(x => x == Color.White).Count());

            System.IO.File.WriteAllBytes("../../../triangleTest1_teapot.ppm", image.toByteArray());
        }
    }
}