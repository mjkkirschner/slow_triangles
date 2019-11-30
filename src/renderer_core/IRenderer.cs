using System.Collections.Generic;
using System.Drawing;
using renderer.core;

namespace renderer.interfaces
{

    public struct TriangleFace
    {
        public int[] indexList;

        public TriangleFace(int[] inds)
        {
            this.indexList = inds;
        }
    }
    public interface IRenderer<T>
    {
        /// <summary>
        /// Geometry which is to be rendered.
        /// TODO add meshtype.
        /// </summary>
        /// <value></value>
        IEnumerable<IEnumerable<T>> Scene { get; set; }

        int Width { get; }

        int Height { get; }

        //TODO - bytes?
        Color[] Render();



    }
}