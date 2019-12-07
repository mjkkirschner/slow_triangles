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
    public class GenericRenderer
    {
        [SetUp]
        public void Setup()
        {
        }



        [Test]
        public void MeshRendererCanRenderKnot_WithSimpleShaders()
        {

            var mesh = ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo("../../../../../geometry_models/knot/knot.obj"));
            var tris = mesh.Triangles;

            mesh.VertexData = mesh.VertexData.Select(x => Vector4.Multiply(x, new Vector4(1.0f, 1.0f, 1.0f, 1.0f)))
            //scale and offset.
            .Select(x => Vector4.Multiply(Vector4.Add(x, new Vector4(5, 5, 0, 0)), 120.0f)).ToArray();


            var renderable = new Renderable<Mesh>(new Material() { Shader = new Shader() }, mesh);
            var renderer = new Renderer2dGeneric<Mesh>(1024, 768, Color.Black, new List<IEnumerable<Renderable<Mesh>>> { new List<Renderable<Mesh>> { renderable } });

            var image = new ppmImage(1024, 768, 255);
            image.colors = renderer.Render();
            //Assert.AreEqual(18702, image.colors.Where(x => x == Color.White).Count());

            System.IO.File.WriteAllBytes("../../../ShaderRender1.ppm", image.toByteArray());
        }



    }
}