using System;
using System.IO;
using System.Linq;
using System.Text;

//based on code from
//https://github.com/EliotJones/BigGustave
namespace renderer.core
{
    [Flags]
    public enum ColorType
    {
        GREY = 0, INDEX = 1, COLOR = 2, ALPHA = 4
    }

    public enum CompressionType : byte
    {
        Defalate = 0
    }

    public enum FilterMethod
    {
        ADAPTIVE = 0
    }

    public class pngHeader
    {
        int Width;
        int Height;
        int BitDepth;
        ColorType ColorType;

        CompressionType Compression;

        FilterMethod Filter;
        bool Interlaced;

        bool IncludesAlpha;

    }

    class ChunkHeader
    {
        public int datalength;
        public string name;
        bool IsCritical => char.IsUpper(name[0]);

        public ChunkHeader(byte[] Header)
        {
            //data is reversed since PNG data is big endian and intel is little endian.
            datalength = BitConverter.ToInt32(Header.Take(4).Reverse().ToArray(), 0);
            name = Encoding.ASCII.GetString(Header, 4, 4);
        }
    }

    public class ImageHeader
    {
        public int Width;
        public int Height;
        public byte BitDeph;
        public ColorType ColorType;
        public CompressionType CompressionType;
        public FilterMethod FilterMethod;
        public bool Interlaced;

        public ImageHeader(int width, int height, byte bitDepth,
                     ColorType colorType, CompressionType compression,
                            FilterMethod filterMethod, bool interlaced)
        {


            this.Width = width;
            this.Height = height;
            this.BitDeph = bitDepth;
            this.ColorType = colorType;
            this.CompressionType = compression;
            this.FilterMethod = filterMethod;
            this.Interlaced = interlaced;
        }
    }

    public class PNGImage
    {
        pngHeader header;
        public PNGImage()
        {

        }

        public static PNGImage readPngFromFile(string path)
        {
            //read the image header first and run some assertions on it.
            //TODO - once it works, use streams or spans...

            var bytes = System.IO.File.ReadAllBytes(path);
            var imageHeader = GetImageHeader(bytes);


            //keep reading chunk headers from the stream or bytes...
            //read bytes = to length specified by header.
            //depending on header type:
            //do stuff...
            //ignore CRC for chunk.
            return new PNGImage();

        }

        private static ImageHeader GetImageHeader(byte[] bytes)
        {
            var currentPos = 0;
            var firstHeader = Encoding.ASCII.GetString(bytes.Take(8).ToArray());
            currentPos += 8;
            if (!firstHeader.Contains("PNG"))
            {
                throw new Exception("invalid header");
            }

            var IHDRHEADER = bytes.Skip(currentPos).Take(8).ToArray();
            currentPos += 8;
            var imageHeader = new ChunkHeader(IHDRHEADER);
            if (imageHeader.datalength != 13)
            {
                throw new Exception("chunk header did not specify 13 byte len");
            }
            if (imageHeader.name != "IHDR")
            {
                throw new Exception("chunk header not IHDR");
            }

            var width = BitConverter.ToInt32(bytes.Skip(currentPos).Take(4).Reverse().ToArray(), 0);
            currentPos += 4;
            var height = BitConverter.ToInt32(bytes.Skip(currentPos).Take(4).Reverse().ToArray(), 0);
            currentPos += 4;
            var bitDepth = bytes.Skip(currentPos).Take(1).FirstOrDefault();
            currentPos += 1;
            var ColorType = bytes.Skip(currentPos).Take(1).FirstOrDefault();
            currentPos += 1;
            var CompresionMethod = bytes.Skip(currentPos).Take(1).FirstOrDefault();
            currentPos += 1;
            var filter = bytes.Skip(currentPos).Take(1).FirstOrDefault();
            currentPos += 1;
            var interlaced = BitConverter.ToBoolean(bytes.Skip(currentPos).Take(1).ToArray(), 0);
            currentPos += 1;

            return new ImageHeader(width, height, bitDepth, (ColorType)ColorType,
           (CompressionType)CompresionMethod, (FilterMethod)filter, interlaced);
        }


    }
}
