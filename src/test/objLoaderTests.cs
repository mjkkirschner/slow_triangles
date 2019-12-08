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
    public class OBJLoaderTests
    {
        [SetUp]
        public void Setup()
        {
        }



        [Test]
        public void LoadComplexOBJTestAsMesh()
        {            //scale and offset all verts.

            //flip verts since image top is 0,0


            var mesh = ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo("../../../../../geometry_models/knot/knot.obj"));
            var tris = mesh.Triangles;

            mesh.VertexData = mesh.VertexData.Select(x => Vector4.Multiply(x, new Vector4(1.0f, 1.0f, 1.0f, 1.0f)))
            //scale and offset.
            .Select(x => Vector4.Multiply(Vector4.Add(x, new Vector4(5, 5, 0, 0)), 120)).ToArray();

            var verts = mesh.VertexData;
            var lines = new List<List<(Vector2, Vector2, Color)>>();
            var trilines = tris.SelectMany(x =>

                         new List<(Vector2, Vector2, Color)>(){
                        (new Vector2(verts[x.vertIndexList[0]-1].X, verts[x.vertIndexList[0]-1].Y),
                       new Vector2(verts[x.vertIndexList[1]-1].X, verts[x.vertIndexList[1]-1].Y),Color.White),

                        (new Vector2(verts[x.vertIndexList[1]-1].X, verts[x.vertIndexList[1]-1].Y),
                       new Vector2(verts[x.vertIndexList[2]-1].X, verts[x.vertIndexList[2]-1].Y),Color.White),

                       (new Vector2(verts[x.vertIndexList[2]-1].X, verts[x.vertIndexList[2]-1].Y),
                      new Vector2(verts[x.vertIndexList[0]-1].X, verts[x.vertIndexList[0]-1].Y),Color.White),
                         }
                      );

            lines.Add(trilines.ToList());

            var renderer = new LineRenderer2d(1024, 768,lines );

            var image = new ppmImage(1024, 768, 255);
            image.colors = renderer.Render();
            Assert.AreEqual(23672, image.colors.Where(x => x == Color.White).Count());

            System.IO.File.WriteAllBytes("../../../objLoaderTest1.ppm", image.toByteArray());
        }

         [Test]
        public void LoadComplexOBJTestAsMesh2()
        {            //scale and offset all verts.

            //flip verts since image top is 0,0


            var mesh = ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo("../../../../../geometry_models/knot3/knot3.obj"));
            var tris = mesh.Triangles;

            mesh.VertexData = mesh.VertexData.Select(x => Vector4.Multiply(x, new Vector4(1.0f, 1.0f, 1.0f, 1.0f)))
            //scale and offset.
            .Select(x => Vector4.Multiply(Vector4.Add(x, new Vector4(5, 5, 0, 0)), 120)).ToArray();

            var verts = mesh.VertexData;
            var lines = new List<List<(Vector2, Vector2, Color)>>();
            var trilines = tris.SelectMany(x =>

                         new List<(Vector2, Vector2, Color)>(){
                        (new Vector2(verts[x.vertIndexList[0]-1].X, verts[x.vertIndexList[0]-1].Y),
                       new Vector2(verts[x.vertIndexList[1]-1].X, verts[x.vertIndexList[1]-1].Y),Color.White),

                        (new Vector2(verts[x.vertIndexList[1]-1].X, verts[x.vertIndexList[1]-1].Y),
                       new Vector2(verts[x.vertIndexList[2]-1].X, verts[x.vertIndexList[2]-1].Y),Color.White),

                       (new Vector2(verts[x.vertIndexList[2]-1].X, verts[x.vertIndexList[2]-1].Y),
                      new Vector2(verts[x.vertIndexList[0]-1].X, verts[x.vertIndexList[0]-1].Y),Color.White),
                         }
                      );

            lines.Add(trilines.ToList());

            var renderer = new LineRenderer2d(1024, 768,lines );

            var image = new ppmImage(1024, 768, 255);
            image.colors = renderer.Render();
            Assert.AreEqual(38703, image.colors.Where(x => x == Color.White).Count());

            System.IO.File.WriteAllBytes("../../../objLoaderTest2.ppm", image.toByteArray());
        }
    }
}