using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using renderer.dataStructures;
using renderer.interfaces;
using renderer.utilities;

namespace renderer._2d
{
    public class Renderer2dGeneric<T> : IRenderer<Renderable<T>> where T : Mesh
    {
        public IEnumerable<IEnumerable<Renderable<T>>> Scene { get; set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        private Color[] imageBuffer;
        private double[] depthBuffer;

        public Renderer2dGeneric(int width, int height, Color fillColor, IEnumerable<IEnumerable<Renderable<T>>> renderData)
        {
            this.Width = width;
            this.Height = height;
            this.Scene = renderData;
            imageBuffer = Enumerable.Repeat(fillColor, Width * Height).ToArray();
            depthBuffer = Enumerable.Repeat(10000.0, Width * Height).ToArray();

        }

        public Color[] Render()
        {
            this.Scene.ToList().ForEach(group =>
               group.ToList().ForEach(renderable =>
               {
                   //for now only one shader.
                   var material = renderable.material;

                   //render
                   var triIndex = 0;
                   foreach (var triFace in renderable.RenderableObject.Triangles)
                   {

                       //transform verts to screenspace
                       var screenCoords = new List<Vector3>();
                       var localVertIndex = 0;
                       foreach (var meshVertInde in triFace.vertIndexList)
                       {
                           screenCoords.Add(material.Shader.VertexToFragment(renderable.RenderableObject, triIndex, localVertIndex));
                           localVertIndex++;
                       }

                       TriangleExtensions.drawTriangle(triIndex, screenCoords.ToArray(), material, depthBuffer, imageBuffer, Width);
                       triIndex = triIndex + 1;
                   }
               })
               );

            return imageBuffer.ToArray();
        }
    }

}