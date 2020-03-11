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
            ShadowMap = Enumerable.Repeat(Color.White, Width * Height).ToArray();
            this.enableShadowPass = enableShadowPass;

        }

        /// <summary>
        /// Performs a render pass to the specified output buffer
        /// </summary>
        /// <param name="renderable"></param>
        /// <param name="material"></param>
        /// <param name="shader">This shader is the one that is used, it takes precedence over the shader on the material - so its best to get the shader from the material and use it here.</param>
        /// <param name="outputBuffer"></param>
        /// <returns></returns>
        private Color[] RenderPass(Renderable<T> renderable, IMaterial material, Shader shader, Color[] outputBuffer)
        {
            var triIndex = 0;
            foreach (var triFace in renderable.RenderableObject.Triangles)
            {
                var screenCoords = new List<Vector3>();
                var localVertIndex = 0;

                foreach (var meshVertInde in triFace.vertIndexList)
                {
                    var vect = shader.VertexToFragment(renderable.RenderableObject, triIndex, localVertIndex);
                    //TODO do some culling or clipping!
                    screenCoords.Add(new Vector3(vect.X, vect.Y, vect.Z));
                    localVertIndex++;
                }
                //TODO
                //use clip information to decide if we should render this tri or skip it:
                TriangleExtensions.drawTriangle(triIndex, screenCoords.ToArray(), material, shader, DepthBuffer, outputBuffer, Width);
                triIndex = triIndex + 1;
            }
            return outputBuffer.ToArray();
        }

        public Color[] Render()
        {
            //cleanup from previous renders
            ImageBuffer = Enumerable.Repeat(fillColor, Width * Height).ToArray();
            DepthBuffer = Enumerable.Repeat(10000.0, Width * Height).ToArray();
            ShadowMap = Enumerable.Repeat(Color.White, Width * Height).ToArray();


            this.Scene.ToList().ForEach(group =>
               group.ToList().ForEach(renderable =>
               {


                   //before rendering the actual camera view, do shadow map pass:
                   var light = (renderable.material.Shader as Single_DirLight_TextureShader).uniform_dir_light;

                   var view = Matrix4x4.CreateLookAt(light.Direction, Vector3.Zero, Vector3.UnitY);
                   var proj = light.ShadowProjectionMatrix;
                   //we need to align this viewport to bound the object and center on the lights position...
                   var viewport = MatrixExtensions.CreateViewPortMatrix(0,0, 255, Width,Height );
                   var shadowShader = new ShadowMapGenShader(view, proj, viewport);

                   var mvp = Matrix4x4.Transpose(Matrix4x4.Multiply(view, proj));
                   var finalShadowMatrix = (Matrix4x4.Multiply(viewport, mvp));


                   RenderPass(renderable, renderable.material, shadowShader, ShadowMap);
                   //reset depth buffer bettween passes
                   DepthBuffer = Enumerable.Repeat(10000.0, Width * Height).ToArray();
                   //shadow map should now have a depth image in it.
                   //a hack! :)

                   //TODO OH MY _ WHY DO I NEED TO DO THIS.
                   var shadowTex = new Texture2d(Width, Height, ShadowMap);
                   //shadowTex.Flip();
                   (renderable.material.Shader as Single_DirLight_TextureShader).uniform_shadow_map =shadowTex ;
                   (renderable.material.Shader as Single_DirLight_TextureShader).uniform_shadow_light_matrix = finalShadowMatrix;
                  //standard pass.
                  RenderPass(renderable, renderable.material, renderable.material.Shader, ImageBuffer);


               }));

            return ImageBuffer.ToArray();
        }
    }

}