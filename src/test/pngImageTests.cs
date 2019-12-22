using NUnit.Framework;
using renderer.core;
using System.Drawing;
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
            var png = PNGImage.LoadPNGFromPath("../../../../../textures/testTexture.png");

            var ppm = new ppmImage(512, 512, 255);
            ppm.colors = png.Colors;
            Assert.AreEqual(png.Colors.Where(x => x == Color.FromArgb(255, 216, 216, 216)).Count(), 71608);
            var savePath = "../../../../../testTexture1.ppm";
            System.IO.File.WriteAllBytes(savePath, ppm.toByteArray());

        }

        [Test]
        public void LoadPNGImageWithAlpha()
        {
            //load a ppm
            var png = PNGImage.LoadPNGFromPath("../../../../../textures/testTextureWithSomeAlpha.png");

            var ppm = new ppmImage(512, 512, 255);
            ppm.colors = png.Colors;
            Assert.AreEqual(png.Colors.Where(x => x == Color.FromArgb(255, 216, 216, 216)).Count(), 11016);
            var savePath = "../../../../../testTextureWithSomeAlpha.ppm";
            System.IO.File.WriteAllBytes(savePath, ppm.toByteArray());

        }
    }
}
