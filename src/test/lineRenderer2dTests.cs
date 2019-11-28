using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using renderer.core;
using renderer.line;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void lineRender2dsimpleLines()
        {
            var lines = new List<List<(Vector2, Vector2, Color)>>()
            {
                new List<(Vector2,Vector2,Color)>{
                    (new Vector2(13, 20), new Vector2(80, 40),Color.White),
                    (new Vector2(20, 13), new Vector2(40, 80),Color.White),
                    (new Vector2(80, 40), new Vector2(80, 480),Color.White),
                    (new Vector2(0, 0), new Vector2(320, 240),Color.White)
                 }
            };

            var renderer = new LineRenderer2d(640, 480, lines);

            var image = new ppmImage(640, 480, 255);
            image.colors = renderer.Render();
            Assert.AreEqual(930, image.colors.Where(x => x == Color.White).Count());
            System.IO.File.WriteAllBytes("../../../linetest.ppm", image.toByteArray());
        }

        [Test]
        public void lineRender2dArrayedCircle()
        {
            var lines = new List<List<(Vector2, Vector2, Color)>>();


            var center = new Vector2(100, 300);
            var points = Enumerable.Range(0, 360).Select(x =>
            new Vector2(
                    (float)(center.X + 100 * Math.Cos(x * (Math.PI / 180.0))),
                    (float)(center.Y + 100 * Math.Sin(x * (Math.PI / 180.0))))
            );
            var pairs = points.Select(x => (center, x, Color.White));
            lines.Add(pairs.ToList());

            var renderer = new LineRenderer2d(640, 480, lines);

            var image = new ppmImage(640, 480, 255);
            image.colors = renderer.Render();
            Assert.AreEqual(24390, image.colors.Where(x => x == Color.White).Count());

            System.IO.File.WriteAllBytes("../../../linetest_circle.ppm", image.toByteArray());
        }

        [Test]
        public void lineRenderTeapot()
        {
            var lines = new List<List<(Vector2, Vector2, Color)>>();
            var verts = ObjFileLoader.LoadVertsFromObjAtPath(new System.IO.FileInfo("../../../../../geometry_models/teapot.obj"))
                .Select(x => Vector4.Add(x, new Vector4(5, 5, 0, 0))).ToArray();
            var tris = ObjFileLoader.LoadTrisFromObjAtPath(new System.IO.FileInfo("../../../../../geometry_models/teapot.obj"));

            var trilines = tris.SelectMany(x =>

                          new List<(Vector2, Vector2, Color)>(){
                        (new Vector2(verts[x.indexList[0]-1].X*50, verts[x.indexList[0]-1].Y*50),
                       new Vector2(verts[x.indexList[1]-1].X*50, verts[x.indexList[1]-1].Y*50),Color.White),

                        (new Vector2(verts[x.indexList[1]-1].X*50, verts[x.indexList[1]-1].Y*50),
                       new Vector2(verts[x.indexList[2]-1].X*50, verts[x.indexList[2]-1].Y*50),Color.White),

                       (new Vector2(verts[x.indexList[2]-1].X*50, verts[x.indexList[2]-1].Y*50),
                      new Vector2(verts[x.indexList[0]-1].X*50, verts[x.indexList[0]-1].Y*50),Color.White),
                          }
                       );

            lines.Add(trilines.ToList());

            var renderer = new LineRenderer2d(640, 480, lines);

            var image = new ppmImage(640, 480, 255);
            image.colors = renderer.Render();
            Assert.AreEqual(18702, image.colors.Where(x => x == Color.White).Count());

            System.IO.File.WriteAllBytes("../../../linetest_teapot.ppm", image.toByteArray());
        }
    }
}