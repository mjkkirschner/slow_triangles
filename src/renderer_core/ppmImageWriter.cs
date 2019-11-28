using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace renderer.core
{
    /// <summary>
    /// I am too lazy to install libgdiplus or to use imagesharp.
    /// </summary>
    public class ppmImage
    {
        public int width;
        public int height;
        public UInt16 maxColorValue;
        public Color[] colors;
        public ppmImage(int width, int height, UInt16 maxColorValue)
        {
            this.height = height;
            this.width = width;
            this.maxColorValue = maxColorValue;
            this.colors = new Color[width * height];
        }

        public void setPixel(int x, int y, Color color)
        {
            this.colors[(y * width) + x] = color;
        }

        public byte[] toByteArray()
        {
            var header = new string[] { "P6", Environment.NewLine, width.ToString(), Environment.NewLine, height.ToString(), Environment.NewLine, maxColorValue.ToString(), Environment.NewLine };
            var data = this.colors.SelectMany(x => new byte[3] { x.R, x.G, x.B });

            var headerAsBytes = Encoding.ASCII.GetBytes(string.Join("", header));

            return (headerAsBytes.Concat(data)).ToArray();
        }

    }

}