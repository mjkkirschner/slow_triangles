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
using renderer.materials;
using renderer.shaders;
using renderer.tests;

namespace Tests
{
    public class Render2dTests
    {
        [SetUp]
        public void Setup()
        {
        }



        [Test]
        public void MeshRendererCanRenderKnot_WithSimpleShaders()
        {

            var mesh = ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo("../../../../../geometry_models/knot3/knot3.obj"));
            var tris = mesh.Triangles;

            mesh.VertexData = mesh.VertexData.Select(x => Vector4.Multiply(x, new Vector4(1.0f, 1.0f, 1.0f, 1.0f)))
            //scale and offset.
            .Select(x => Vector4.Multiply(Vector4.Add(x, new Vector4(5, 5, 0, 0)), 120.0f)).ToArray();


            var renderable = new Renderable<Mesh>(new Material() { Shader = new Shader() }, mesh);
            var renderer = new Renderer2dGeneric<Mesh>(1024, 768, Color.Black, new List<IEnumerable<Renderable<Mesh>>> { new List<Renderable<Mesh>> { renderable } });

            var image = new ppmImage(1024, 768, 255);
            image.Colors = renderer.Render();
            Assert.AreEqual(736261, image.Colors.Where(x => x == Color.Black).Count());

            System.IO.File.WriteAllBytes("../../../ShaderRender1.ppm", image.toByteArray());
        }

        [Test]
        public void MeshRenderUVsAndTexture()
        {

            var mesh = ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo("../../../../../geometry_models/knot3/knot3.obj"));
            var tris = mesh.Triangles;

            mesh.VertexData = mesh.VertexData.Select(x => Vector4.Multiply(x, new Vector4(1.0f, 1.0f, 1.0f, 1.0f)))
            //scale and offset.
            .Select(x => Vector4.Multiply(Vector4.Add(x, new Vector4(5, 5, 0, 0)), 120.0f)).ToArray();

            var ppm = new ppmImage("../../../../../textures/testTexture2.ppm");
            var renderable = new Renderable<Mesh>(
                new DiffuseMaterial()
                {
                    Shader = new Unlit_TextureShader(Matrix4x4.Identity,Matrix4x4.Identity,Matrix4x4.Identity),
                    DiffuseTexture = new Texture2d(ppm.Width, ppm.Height, ppm.Colors)
                },
                mesh);
            var renderer = new Renderer2dGeneric<Mesh>(1024, 768, Color.Black, new List<IEnumerable<Renderable<Mesh>>> { new List<Renderable<Mesh>> { renderable } });

            var image = new ppmImage(1024, 768, 255);
            image.Colors = renderer.Render();

            System.IO.File.WriteAllBytes("../../../ShaderRender2.ppm", image.toByteArray());
             //how many pixels are bluish...
            Assert.AreEqual(1755, image.Colors.Where(x => Utilities.ComputeSimpleColorDistance(x, Color.FromArgb(31, 159, 254)) < 5).Count());

        }
    }
}