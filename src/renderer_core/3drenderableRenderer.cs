using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using renderer.dataStructures;
using renderer.interfaces;
using renderer.shaders;
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
        public Color[] ShadowMap { get; private set; }
        public List<ILight> Lights { get; set; }

        private Color fillColor = Color.Black;
        private bool enableShadowPass = false;

        public Renderer3dGeneric(int width, int height, Color fillColor, IEnumerable<IEnumerable<Renderable<T>>> renderData, bool enableShadowPass = true)
        {
            this.Width = width;
            this.Height = height;
            this.Scene = renderData;
            this.fillColor = fillColor;
            ImageBuffer = Enumerable.Repeat(fillColor, Width * Height).ToArray();
            DepthBuffer = Enumerable.Repeat(10000.0, Width * Height).ToArray();
            ShadowMap = Enumerable.Repeat(10000.0, Width * Height).ToArray();
            this.enableShadowPass = enableShadowPass;

        }

        public Color[] Render()
        {
            //cleanup from previous renders
            ImageBuffer = Enumerable.Repeat(fillColor, Width * Height).ToArray();
            DepthBuffer = Enumerable.Repeat(10000.0, Width * Height).ToArray();
            ShadowMap = Enumerable.Repeat(10000.0, Width * Height).ToArray();

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

                           //we will render each of our objects for each light to a shadow map that is used inside the 
                           //regular frag shader pass.
                           if (enableShadowPass)
                           {
                               //TODO
                               //lets just assume if have a single light for now.
                               //TODO need to think more about this - do we want to render all our shadow maps first and then access them later
                               //when rendering?

                               var light = (material.Shader as Single_DirLight_TextureShader).uniform_dir_light;

                               var view = Matrix4x4.CreateLookAt(light.Position, light.Position + light.Direction, Vector3.UnitY);
                               var proj = light.ShadowProjectionMatrix;
                               var viewport = MatrixExtensions.CreateViewPortMatrix(0, 0, 255, Width, Height);
                               var shadowShader = new ShadowMapGenShader(view, proj, viewport);

                               var vect_shadowPass = shadowShader.VertexToFragment(renderable.RenderableObject, triIndex, localVertIndex);
                               screenCoords.Add(new Vector3(vect.X, vect.Y, vect.Z));

                               TriangleExtensions.drawTriangle(triIndex, screenCoords.ToArray(), material, DepthBuffer, ShadowMap, Width);
                           }



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