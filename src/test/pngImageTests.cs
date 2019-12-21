using NUnit.Framework;
using renderer.core;
using System.Linq;
namespace Tests
{
    public class PNGImageTests
    {
        [SetUp]
        public void Setup()
        {
        }



        [Test]
        public void LoadPNGImage()
        {
            //load a ppm
            var png = PNGImage.readPngFromFile("../../../../../textures/testTexture.png");

            var ppm = new ppmImage(512, 512, 255);
            ppm.colors = png.Colors;

            var savePath = "../../../../../testTexture1.ppm";
            System.IO.File.WriteAllBytes(savePath, ppm.toByteArray());

        }
    }
}
