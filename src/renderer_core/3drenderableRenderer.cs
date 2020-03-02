using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using renderer.dataStructures;
using renderer.interfaces;
using renderer.utilities;

namespace renderer._3d
{
    public class Renderer3dGeneric<T> : IRenderer<Renderable<T>> where T : Mesh
    {
        public IEnumerable<IEnumerable<Renderable<T>>> Scene { get; set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Color[] ImageBuffer { get; private set; }

        /// <summary>
        /// buffer for previews raw bytes for RGBA image.null 8 bit per channel.
        /// </summary>
        /// <value></value>
        public byte[] previewBuffer { get; }
        public double[] DepthBuffer { get; private set; }
        private Color fillColor = Color.Black;
        //  private Color[] defaultBuffer;
        //  private double[] defaultDepthBuffer;

        public Renderer3dGeneric(int width, int height, Color fillColor, IEnumerable<IEnumerable<Renderable<T>>> renderData)
        {
            this.Width = width;
            this.Height = height;
            this.Scene = renderData;
            this.fillColor = fillColor;
            ImageBuffer = Enumerable.Repeat(fillColor, Width * Height).ToArray();
            DepthBuffer = Enumerable.Repeat(10000.0, Width * Height).ToArray();
            previewBuffer = ImageBuffer.SelectMany(x => new byte[4] { x.R, x.G, x.B, x.A }).ToArray();

        }

        public Color[] Render()
        {
            /*           if (defaultBuffer is null)
                      {
                          defaultBuffer = Enumerable.Repeat(fillColor, Width * Height).ToArray();
                      }
                      if (defaultDepthBuffer is null)
                      {
                          defaultDepthBuffer = Enumerable.Repeat(10000.0, Width * Height).ToArray();
                      } */
            //cleanup from previous renders
            ImageBuffer = new Color[Width*Height]; //Enumerable.Repeat(fillColor, Width * Height).ToArray();


            /*  for (var i = 0; i < previewBuffer.Length; i = i + 4)
              {
                  previewBuffer[i] = fillColor.R;
                  previewBuffer[i + 1] = fillColor.G;
                  previewBuffer[i + 2] = fillColor.B;
                  previewBuffer[i + 3] = fillColor.A;
              }
  */
            DepthBuffer = Enumerable.Repeat(10000.0, Width * Height).ToArray();

            this.Scene.ToList().ForEach(group =>
            {
                group.ToList().ForEach(renderable =>
                {
                    //for now only one shader.
                    var material = renderable.material;

                    //render

                    Parallel.ForEach(renderable.RenderableObject.Triangles, (triFace, state, triIndex) =>
                     {
                         //this slows down the render so we can see it.
                         //System.Threading.Thread.Sleep(3);
                         // Console.WriteLine($"{triIndex} out of {renderable.RenderableObject.Triangles.Count}");
                         //transform verts to screenspace
                         var screenCoords = new List<Vector3>();
                         var localVertIndex = 0;

                         foreach (var meshVertIndex in triFace.vertIndexList)
                         {
                             var vect = material.Shader.VertexToFragment(renderable.RenderableObject, (int)triIndex, localVertIndex);


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

                             TriangleExtensions.drawTriangle((int)triIndex, screenCoords.ToArray(), material, DepthBuffer, ImageBuffer, Width, previewBuffer);

                         }
                     });
                });
            });


           
                for (var i = 0; i < ImageBuffer.Length; i = i + 1)
                {
                    previewBuffer[i * 4] = ImageBuffer[i].R;
                    previewBuffer[i * 4 + 1] = ImageBuffer[i].G;
                    previewBuffer[i * 4 + 2] = ImageBuffer[i].B;
                    previewBuffer[i * 4 + 3] = ImageBuffer[i].A;
                }
                return ImageBuffer;
            
        }
    }

}