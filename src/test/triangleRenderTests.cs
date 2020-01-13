using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using renderer.core;
using renderer._2d;
using renderer.interfaces;
using renderer.dataStructures;

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
            image.Colors = renderer.Render();
            Assert.AreEqual(677784, image.Colors.Where(x => x == Color.Red).Count());

            System.IO.File.WriteAllBytes("../../../triangleTest1_teapot.ppm", image.toByteArray());
        }

        [Test]
        public void TriangleRenderKnot1()
        {            //scale and offset all verts.

            //flip verts since image top is 0,0


            var mesh = ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo("../../../../../geometry_models/knot/knot.obj"));
            var tris = mesh.Triangles;

            mesh.VertexData = mesh.VertexData.Select(x => Vector4.Multiply(x, new Vector4(1.0f, 1.0f, 1.0f, 1.0f)))
            //scale and offset.
            .Select(x => Vector4.Multiply(Vector4.Add(x, new Vector4(5, 5, 0, 0)), 120.0f)).ToArray();

            var renderer = new Triangle2dRenderer(1024, 768, new List<IEnumerable<TriangleFace>> { tris });
            renderer.VertexData = mesh.VertexData.Select(x => new Vector3(x.X, x.Y, x.Z)).ToArray();

            var image = new ppmImage(1024, 768, 255);
            image.Colors = renderer.Render();
            Assert.AreEqual(741639, image.Colors.Where(x => x == Color.Red).Count());

            System.IO.File.WriteAllBytes("../../../knotTest1.ppm", image.toByteArray());
        }

        [Test]
        public void TriangleRenderKnot1WithVertsDirect()
        {
            var verts = ObjFileLoader.LoadVertsFromObjAtPath(new System.IO.FileInfo("../../../../../geometry_models/knot/knot.obj"))
          //flip verts since image top is 0,0
          .Select(x => Vector4.Multiply(x, new Vector4(1.0f, 1.0f, 1.0f, 1.0f)))
              //scale and offset.
              .Select(x => Vector4.Multiply(Vector4.Add(x, new Vector4(5, 5, 0, 0)), 120)).ToArray();

            var tris = ObjFileLoader.LoadTrisFromObjAtPath(new System.IO.FileInfo("../../../../../geometry_models/knot/knot.obj"));

            var renderer = new Triangle2dRenderer(1024, 768, new List<IEnumerable<TriangleFace>> { tris });
            renderer.VertexData = verts.Select(x => new Vector3(x.X, x.Y, x.Z)).ToArray();

            var image = new ppmImage(1024, 768, 255);
            image.Colors = renderer.Render();
            Assert.AreEqual(741639, image.Colors.Where(x => x == Color.Red).Count());

            System.IO.File.WriteAllBytes("../../../knotTestVertsDirect.ppm", image.toByteArray());
        }

        [Test]
        public void TriangleRenderKnot3()
        {            //scale and offset all verts.

            //flip verts since image top is 0,0


            var mesh = ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo("../../../../../geometry_models/knot3/knot3.obj"));
            var tris = mesh.Triangles;

            mesh.VertexData = mesh.VertexData.Select(x => Vector4.Multiply(x, new Vector4(1.0f, 1.0f, 1.0f, 1.0f)))
            //scale and offset.
            .Select(x => Vector4.Multiply(Vector4.Add(x, new Vector4(5, 5, 0, 0)), 120)).ToArray();

            var renderer = new Triangle2dRenderer(1024, 768, new List<IEnumerable<TriangleFace>> { tris });
            renderer.VertexData = mesh.VertexData.Select(x => new Vector3(x.X, x.Y, x.Z)).ToArray();

            var image = new ppmImage(1024, 768, 255);
            image.Colors = renderer.Render();
            Assert.AreEqual(736261, image.Colors.Where(x => x == Color.Red).Count());

            System.IO.File.WriteAllBytes("../../../knotTest3.ppm", image.toByteArray());
        }

        [Test]
        public void TriangleRenderTeapot2_usingMeshLoadOBJ()
        {

            var mesh = ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo("../../../../../geometry_models/teapot.obj"));
            var tris = mesh.Triangles;

            mesh.VertexData = mesh.VertexData.Select(x => Vector4.Multiply(x, new Vector4(1.0f, -1.0f, 1.0f, 1.0f)))
            //scale and offset.
            .Select(x => Vector4.Multiply(Vector4.Add(x, new Vector4(5, 5, 0, 0)), 100)).ToArray();

            var renderer = new Triangle2dRenderer(1024, 768, new List<IEnumerable<TriangleFace>> { tris });
            renderer.VertexData = mesh.VertexData.Select(x => new Vector3(x.X, x.Y, x.Z)).ToArray();

            var image = new ppmImage(1024, 768, 255);
            image.Colors = renderer.Render();
            Assert.AreEqual(677784, image.Colors.Where(x => x == Color.Red).Count());

            System.IO.File.WriteAllBytes("../../../teapotTest2.ppm", image.toByteArray());
        }
    }
}