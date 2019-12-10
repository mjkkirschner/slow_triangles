using System;
using System.Collections.Generic;
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

        public ppmImage(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            var dataStrings = new List<String>();

            var index = 0;
            var withinHeader = true;

            while (withinHeader)
            {
                //keep building up a string until we reach whitepace.
                var currentBytes = bytes.Skip(index).TakeWhile((b) =>
                {

                    //walk the bytes taking bytes to constuct a line until we find whitespace OR if the current start char is # until we find an end of line char.

                    return !Char.IsWhiteSpace(Encoding.ASCII.GetString(new byte[1] { b }, 0, 1).ToCharArray().FirstOrDefault())
|| (Convert.ToChar(bytes.Skip(index).FirstOrDefault()) == '#' && !Char.IsControl(Convert.ToChar(b)));


                });

                //convert current bytes to string
                var currentLine = Encoding.ASCII.GetString(currentBytes.ToArray());
                //increment index
                index += currentBytes.Count() + 1;
                //get rid of comments
                var split = currentLine.Split('#');
                var remainingString = split.FirstOrDefault();
                //filter out whitespace or nulls from splitting
                dataStrings.Add(remainingString);
                dataStrings = dataStrings.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                //we're looking for
                //P6
                //width
                //height
                //max color val
                if (dataStrings.Count == 4)
                {
                    withinHeader = false;
                }
                dataStrings.ForEach(x => Console.WriteLine(x));
            }

            //index now represents start of data.
            var colorData = new List<Color>();
            for (; index < bytes.Length; index = index + 3)
            {
                colorData.Add(Color.FromArgb(bytes[index], bytes[index + 1], bytes[index + 2]));
            }
            this.width = int.Parse(dataStrings[1]);
            this.height = int.Parse(dataStrings[2]);
            this.maxColorValue = ushort.Parse(dataStrings[3]);
            this.colors = colorData.ToArray();

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