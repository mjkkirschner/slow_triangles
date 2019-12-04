using System.Drawing;
using renderer.interfaces;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System;
using renderer.utilities;
using renderer.dataStructures;

namespace renderer._2d
{

    /// <summary>
    /// Renders 2d triangles which are index groups into a collection of vector 2.
    /// Vectors must be set before calling render.
    /// </summary>
    public class Triangle2dRenderer : IRenderer<TriangleFace>
    {
        public IEnumerable<IEnumerable<TriangleFace>> Scene { get; set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Vector3[] VertexData { get; set; }
        private Color[] imageBuffer;
        private double[] depthBuffer;


        public Triangle2dRenderer(int width, int height, IEnumerable<IEnumerable<TriangleFace>> triangleIndexData)
        {
            this.Width = width;
            this.Height = height;
            this.Scene = triangleIndexData;
            imageBuffer = Enumerable.Repeat(Color.Red, Width * Height).ToArray();
            depthBuffer = Enumerable.Repeat(10000.0, Width * Height).ToArray();

        }

        public Color[] Render()
        {
            if (VertexData == null)
            {
                throw new Exception($"{nameof(VertexData)} must be set before calling render");
            }

            //cache conversion of all verts to 2d - 
            var Verts2d = VertexData.Select(x => x.ToVector2()).ToArray();
            //fill depth buffer

            this.Scene.ToList().ForEach(trigroup =>
               {
                   trigroup.ToList().ForEach(tri =>
                   {
                       Console.WriteLine(tri);
                       //all position data will be unchanged and unscaled.
                       //TODO except maybe invert the y?
                       var A = VertexData[tri.vertIndexList[0] - 1];
                       var B = VertexData[tri.vertIndexList[1] - 1];
                       var C = VertexData[tri.vertIndexList[2] - 1];

                       var verts = new List<Vector3>() { A, B, C };
                       //calculate bounding box and iterate all pixels within.
                       var minx = verts.Select(x => x.X).Min();
                       var miny = verts.Select(x => x.Y).Min();
                       var maxx = verts.Select(x => x.X).Max();
                       var maxy = verts.Select(x => x.Y).Max();

                       Console.WriteLine($"{minx},{miny}    {maxx},{maxy}");

                       var random = new Random();
                       var randomint = random.Next(255);
                       var randomint2 = random.Next(255);
                       var randomint3= random.Next(255);

                       Enumerable.Range((int)minx, (int)(maxx - minx) + 2).ToList().ForEach(x =>
                         {

                             Enumerable.Range((int)miny, (int)(maxy - miny) + 2).ToList().ForEach(y =>
                             {
                                 var IsInsideTriangle = pixelIsInsideTriangle(x, y, tri, VertexData);

                                 var bary = TriangleExtensions.BaryCoordinates2(x, y, tri, Verts2d);
                                 //compute the depth of current pixel.
                                 var z = bary.X * A.Z + bary.Y * B.Z + bary.Z * C.Z;
                                

                                 var AB = Vector3.Subtract(A, B);
                                 var AC = Vector3.Subtract(A, C);
                                 var ABXAC = Vector3.Normalize(Vector3.Cross(AB, AC));


                                 if (IsInsideTriangle)
                                 {
                                     var flatIndex = Width * (int)y + (int)x;
                                     //don't draw unless we are within bounds
                                     //don't draw if something is already in the depth buffer for this pixel.
                                     if (flatIndex <= imageBuffer.Length && flatIndex > -1 /*&& z < depthBuffer[flatIndex]*/)
                                     {
                                         //only draw if nothing else is closer in the depth buffer
                                         if (z < depthBuffer[flatIndex])
                                         {
                                             //adjust color here.
                                             var normalDotLightVector = Vector3.Dot(ABXAC, new Vector3(1.0f, 0f, 0f));
                                             //if (normalDotLightVector < 0)
                                             //{
                                             //    return;
                                             //}
                                             var diffuseCoef = (float)(Math.Max(normalDotLightVector, 0.3));
                                             //don't draw pixels which are facing directly away from camera.


                                             imageBuffer[flatIndex] = Color.FromArgb((int)(diffuseCoef * randomint), (int)(diffuseCoef * randomint2), (int)(diffuseCoef * randomint3));
                                             depthBuffer[flatIndex] = z;
                                         }
                                     }
                                 }
                             });
                         });

                   });
               });

            return imageBuffer;
        }

        private static bool pixelIsInsideTriangle(int x, int y, TriangleFace triangle, Vector3[] vectors)
        {
            var pt1 = vectors[triangle.vertIndexList[0] - 1];
            var pt2 = vectors[triangle.vertIndexList[1] - 1];
            var pt3 = vectors[triangle.vertIndexList[2] - 1];

            var barycenter = TriangleExtensions.BaryCoordinates2(x, y, pt1.ToVector2(), pt2.ToVector2(), pt3.ToVector2());
            //only in the triangle if coefs are all positive.
            if (barycenter.X < 0 || barycenter.Y < 0 || barycenter.Z < 0)
            {
                return false;
            }

            return true;
        }



    }

}