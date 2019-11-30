using System.Collections.Generic;
using System.Drawing;
using renderer.core;
using renderer.interfaces;
using System.Numerics;
using System;
using System.Linq;

namespace renderer._2d
{

    /// <summary>
    /// A test renderer for rendering 2d lines, data should be in vector2 pairs (start,end)
    // TODO perf implications of value tuple?
    /// </summary>
    public class LineRenderer2d : IRenderer<(Vector2 start, Vector2 end, Color lineColor)>
    {
        public IEnumerable<IEnumerable<(Vector2 start, Vector2 end, Color lineColor)>> Scene { get; set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        private Color[] imageBuffer;

        public LineRenderer2d(int width, int height, IEnumerable<IEnumerable<(Vector2, Vector2, Color)>> data)
        {
            Scene = data;
            Width = width;
            Height = height;
            imageBuffer = new Color[width * height];
        }

        public Color[] Render()
        {
            //march through our input lines and render each of them - 
            //modifying the output image as we go.
            foreach (var item in Scene)
            {
                foreach (var line in item)
                {
                    var color = line.lineColor;
                    var start = line.start;
                    var end = line.end;
                    drawLine(start, end, color, this.imageBuffer);

                }
            }
            return imageBuffer;
        }

        //this is not efficient. It may write the same pixel twice - It may not land exactly on the end pixel.
        //but it's simple to understand.
        private void drawLine(Vector2 start, Vector2 end, Color lineColor, Color[] imageBuffer)
        {
            //walk from start to end by 1(px)

            var movementVec = Vector2.Subtract(end, start);
            var movementVecNorm = Vector2.Normalize(movementVec);
            var dist = movementVec.Length();
            var x = start.X;
            var y = start.Y;
            while (dist > 0.00001)
            {
                //break out if we're trying to draw lines outside the image bounds.
                var flatIndex = Width * (int)y + (int)x;
                if (flatIndex > imageBuffer.Length || flatIndex < 0)
                {
                    break;
                }
                imageBuffer[flatIndex] = lineColor;
                x = x + movementVecNorm.X;
                y = y + movementVecNorm.Y;
                dist = dist - 1.0f;
            }

        }

    }
}
