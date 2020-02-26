using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using renderer._3d;
using renderer.core;
using renderer.dataStructures;
using renderer.materials;
using renderer.shaders;
using renderer.utilities;

namespace slow_triangles
{


    class Program
    {
        static IntPtr BufferPointer;

        //native functions from swift we want to use to open a window and 
        //draw an RGBA texture.
        [DllImport("libimageViewer.dylib")]
        private static extern void start_render_view(int width, int height);

        [DllImport("libimageViewer.dylib")]


        private static extern void update_tex(IntPtr data);
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Renderer");
            //lets start up a render

            //just a test for now.

            var cameraPos = new Vector3(0, 2, 2);
            var target = new Vector3(0, 0, 0);
            var width = 1024;
            var height = 1024;
            var view = Matrix4x4.CreateLookAt(cameraPos, target, Vector3.UnitY);
            var proj = Matrix4x4.CreatePerspective(4, 4, 1, 10);
            var viewport = MatrixExtensions.CreateViewPortMatrix(0, 0, 255, width, height);

            var mesh = ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo("../../geometry_models/knot3/knot3.obj"));
            var diffuseTex = new ppmImage("../../textures/testTexture2.ppm");
            var normalMap = PNGImage.LoadPNGFromPath("../../textures/testMaps/gridnormalmap.png");

            var renderable = new Renderable<Mesh>(
                new NormalMaterial()
                {
                    Shader = new NormalShader(view, proj, viewport) { ambientCoef = 10, LightDirection = new Vector3(0, 0, 1) },
                    DiffuseTexture = new Texture2d(diffuseTex.Width, diffuseTex.Height, diffuseTex.Colors),
                    NormalMap = new Texture2d(normalMap.Width, normalMap.Height, normalMap.Colors)
                },
                mesh);
            var renderer = new Renderer3dGeneric<Mesh>(width, height, Color.White,
                new List<IEnumerable<Renderable<Mesh>>> { new List<Renderable<Mesh>> { renderable } });

            var handle = GCHandle.Alloc(renderer.previewBuffer, GCHandleType.Pinned);
            BufferPointer = handle.AddrOfPinnedObject();

            Task.Run(() =>
            {
                var stopwatch = new Stopwatch();
                var mat = Matrix4x4.CreateRotationY(0.174533f);
                while (true)
                {
                    stopwatch.Start();

                    var transformedLightDir = Vector3.Transform(renderable.material.Shader.LightDirection, Matrix4x4.Transpose(mat));
                    renderable.material.Shader.LightDirection = transformedLightDir;
                    renderer.Render();
                    Console.WriteLine($"rendering took:{stopwatch.ElapsedMilliseconds}");
                    stopwatch.Reset();
                }

            });
            Task.Run(() =>
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(1000);
                    update_tex(BufferPointer);
                }
            });

            start_render_view(width, height);
        }
    }
}
