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
            var png = PNGImage.readPngFromFile("../../../../../testTexture.png");


        }
    }
}
