using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using renderer.dataStructures;
using renderer.interfaces;
using renderer.utilities;
using renderer_core.dataStructures;

namespace renderer._3d
{
    public class Renderer3dGeneric<T> : IRenderer<Renderable<T>> where T : Mesh
    {
        public IEnumerable<IEnumerable<Renderable<T>>> Scene { get; set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Color[] ImageBuffer { get; private set; }
        public double[] DepthBuffer { get; private set; }
        public List<ILight> Lights { get; set; }

        private Color fillColor = Color.Black;


        public Renderer3dGeneric(int width, int height, Color fillColor, IEnumerable<IEnumerable<Renderable<T>>> renderData)
        {
            this.Width = width;
            this.Height = height;
            this.Scene = renderData;
            this.fillColor = fillColor;
            ImageBuffer = Enumerable.Repeat(fillColor, Width * Height).ToArray();
            DepthBuffer = Enumerable.Repeat(10000.0, Width * Height).ToArray();


        }

        public Color[] Render()
        {
            //cleanup from previous renders
            ImageBuffer = Enumerable.Repeat(fillColor, Width * Height).ToArray();
            DepthBuffer = Enumerable.Repeat(10000.0, Width * Height).ToArray();

            this.Scene.ToList().ForEach(group =>
               group.ToList().ForEach(renderable =>
               {
                   //for now only one shader.
                   var material = renderable.material;

                   //render
                   var triIndex = 0;
                   foreach (var triFace in renderable.RenderableObject.Triangles)
                   {
                      // Console.WriteLine($"{triIndex} out of {renderable.RenderableObject.Triangles.Count}");
                       //transform verts to screenspace
                       var screenCoords = new List<Vector3>();
                       var localVertIndex = 0;
                       foreach (var meshVertInde in triFace.vertIndexList)
                       {
                           var vect = material.Shader.VertexToFragment(renderable.RenderableObject, triIndex, localVertIndex);

                           //if outside clip bounds, we will mark the vert NAN.
                           /* if (scaledX < 0 || scaledX > Width || scaledY < 0 || scaledY > Height)
                            {
                                screenCoords.Add(new Vector3(float.NaN, float.NaN, float.NaN));
                            }
                            else
                          */
                           {
                               screenCoords.Add(new Vector3(vect.X, vect.Y, vect.Z));

                           }

                           localVertIndex++;

                       }
                       //draw if not nan.
                       //todo - logic is a bit backward.
                       //  if (screenCoords.All(x => !float.IsNaN(x.X)))
                       {
                           TriangleExtensions.drawTriangle(triIndex, screenCoords.ToArray(), material, DepthBuffer, ImageBuffer, Width);

                       }
                       triIndex = triIndex + 1;
                   }
               })
               );

            return ImageBuffer.ToArray();
        }
    }

}