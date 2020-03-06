using renderer.dataStructures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// This project exists so that we can reference System.Drawing (from .net framework) and safely import
/// convert Texture2d from a .netStandard project to System.Drawing bitmaps - using System.Drawing from System.Drawing.Common
/// causes ZT import errors in Dynamo because of the types with the same name.
/// </summary>

namespace netbitmapbridge
{
   
    public static class ImageUtilities
    {
        /// <summary>
        /// Convert to a Texture2d to a Bitmap image.
        /// </summary>
        /// <param name="tex"></param>
        /// <returns></returns>
        public static Bitmap Texture2dToBitmap(Texture2d tex)
        {

            var data = tex.ColorData.Cast<Color>().ToArray();
            var width = tex.Width;
            var height = tex.Height;
            Bitmap Bmp = new Bitmap(width, height);
            for (int ii = 0; ii < (width * height); ii++)
            {
                var ypos = ii / width;
                var xpos = ii % width;
                Bmp.SetPixel(xpos, ypos, Color.FromArgb(data[ii].A, data[ii].R, data[ii].G, data[ii].B));
            }
            return Bmp;
        }

        /// <summary>
        /// Converts a bitmap image to a texture2d for use in renderer nodes.
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Texture2d BitmapToTexture2d(Bitmap bmp)
        {

            var colorData = new List<Color>(); ;
            for (var x = 0; x < bmp.Width; x++)
            {
                for (var y = 0; y < bmp.Height; y++)
                {
                    colorData.Add(bmp.GetPixel(x, y));
                }
            }
            return new Texture2d(bmp.Width, bmp.Height, colorData);
        }
    }
}
