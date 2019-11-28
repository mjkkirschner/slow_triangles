using System;
using System.Drawing;
using System.Linq;
using renderer.core;

namespace slow_triangles
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var image = new ppmImage(640, 480, 255);
            Enumerable.Range(0, 640).ToList().ForEach(x =>
            {
                image.setPixel(x, 0, Color.Red);

            });
            System.IO.File.WriteAllBytes("../image.ppm",image.toByteArray());

        }
    }
}
