using NUnit.Framework;
using renderer.core;
using System.Linq;
namespace Tests
{
    public class PPMImageTests
    {
        [SetUp]
        public void Setup()
        {
        }



        [Test]
        public void LoadPPMImage()
        {
            //load a ppm
            var ppm = new ppmImage("../../../../../textures/testTexture2.ppm");
            //write it back out
            var savePath = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllBytes(savePath, ppm.toByteArray());
            //load it back
            var ppm2 = new ppmImage(savePath);

            Assert.AreEqual(ppm.width, ppm2.width);
            Assert.AreEqual(ppm.height, ppm2.height);
            Assert.AreEqual(ppm.maxColorValue, ppm2.maxColorValue);
            Assert.IsTrue(ppm.colors.SequenceEqual(ppm2.colors));

            //cleanup
            System.IO.File.Delete(savePath);

        }
    }
}
