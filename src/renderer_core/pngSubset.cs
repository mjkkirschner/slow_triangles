using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using renderer.utilities;

//based on code from
//https://github.com/EliotJones/BigGustave
//https://www.w3.org/TR/2003/REC-PNG-20031110/#9FtIntro
namespace renderer.core
{
    [Flags]
    enum ColorType
    {
        GREY = 0, INDEX = 1, COLOR = 2, ALPHA = 4
    }

    enum CompressionType : byte
    {
        Defalate = 0
    }

    enum FilterMethod
    {
        ADAPTIVE = 0
    }

    /*
      0	None	Filt(x) = Orig(x)	Recon(x) = Filt(x)
      1	Sub	Filt(x) = Orig(x) - Orig(a)	Recon(x) = Filt(x) + Recon(a)
      2	Up	Filt(x) = Orig(x) - Orig(b)	Recon(x) = Filt(x) + Recon(b)
      3	Average	Filt(x) = Orig(x) - floor((Orig(a) + Orig(b)) / 2)	Recon(x) = Filt(x) + floor((Recon(a) + Recon(b)) / 2)
      4	Paeth	Filt(x) = Orig(x) - PaethPredictor(Orig(a), Orig(b), Orig(c))	Recon(x) = Filt(x) + PaethPredictor(Recon(a), Recon(b), Recon(c))
*/

    /// <summary>
    /// Filter type describes which filter algorithm is used to encode a specific scanline.
    /// </summary>
    enum FilterType
    {
        None = 0,
        Sub_Filter = 1,
        Up_Filter = 2,
        Average_Filter = 3,
        PaethFilter = 4
    }

    /// <summary>
    /// Describe a chunk.
    /// </summary>
    internal class ChunkHeader
    {
        public int Datalength { get; }
        public string Name { get; }
        bool IsCritical => char.IsUpper(Name[0]);

        public ChunkHeader(byte[] Header)
        {
            //data is reversed since PNG data is big endian and intel is little endian.
            Datalength = BitConverter.ToInt32(Header.Take(4).Reverse().ToArray(), 0);
            Name = Encoding.ASCII.GetString(Header, 4, 4);
        }
    }

    /// <summary>
    /// specific chunk and data that represents image header.
    /// </summary>
    internal class ImageHeader
    {
        public int Width { get; }
        public int Height { get; }
        public byte BitDepth { get; }
        public ColorType ColorType { get; }
        public CompressionType CompressionType { get; }
        public FilterMethod FilterMethod { get; }
        public bool Interlaced { get; }

        public ImageHeader(int width, int height, byte bitDepth,
                     ColorType colorType, CompressionType compression,
                            FilterMethod filterMethod, bool interlaced)
        {
            this.Width = width;
            this.Height = height;
            this.BitDepth = bitDepth;
            this.ColorType = colorType;
            this.CompressionType = compression;
            this.FilterMethod = filterMethod;
            this.Interlaced = interlaced;
        }
    }


    /// <summary>
    /// small subset of png decode functionality.
    /// </summary>
    public class PNGImage
    {
        private ImageHeader Header;
        public byte[] data;
        public int bytesPerPixel;

        public Color[] Colors
        {
            get
            {
                return ListExtensions.Split<Byte>(data.ToList(), (uint)bytesPerPixel).Select(pixel => Color.FromArgb(pixel[3], pixel[0], pixel[1], pixel[2])).ToArray();
            }
        }

        private PNGImage(ImageHeader header, byte[] data, int bytesPerPixel)
        {
            this.Header = header;
            this.data = data;
            this.bytesPerPixel = bytesPerPixel;
        }

        public static PNGImage LoadPNGFromPath(string path)
        {
            //read the image header first and run some assertions on it.
            //TODO - once it works, use streams or spans...

            var bytes = System.IO.File.ReadAllBytes(path);
            var position = 0;
            var imageHeader = GetImageHeader(bytes, out position);
            var errors = false;

            //keep reading chunk headers from the stream or bytes...
            //read bytes = to length specified by header.
            //depending on header type:
            //do stuff...
            //ignore CRC for chunk.
            var dataBytesOnly = new MemoryStream();
            while (position < bytes.Length && !errors)
            {
                try
                {
                    var chunkHeader = new ChunkHeader(bytes.Skip(position).Take(8).ToArray());
                    position += 8;
                    //read some more bytes given the chunks length.
                    var currentData = bytes.Skip(position).Take(chunkHeader.Datalength).ToArray();
                    //add 4 bytes for CRC.... TODO maybe check it...
                    position += chunkHeader.Datalength + 4;
                    switch (chunkHeader.Name)
                    {
                        case "IEND":
                            Console.WriteLine("this is the end of the png.");
                            break;
                        case "IDAT":
                            Console.WriteLine("found data chunk");
                            dataBytesOnly.Write(currentData, 0, currentData.Length);
                            break;
                        case "PLTE":
                            //TODO create palette?
                            Console.WriteLine("found index color palette chunk");
                            break;
                        default:
                            Console.WriteLine($"unknown chunk type {chunkHeader.Name}");
                            break;
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    errors = true;
                }
            }
            //reset stream pos or will not decompress.
            dataBytesOnly.Seek(2, SeekOrigin.Begin);
            var outputStream = new MemoryStream();

            //done collecting data - decompress it:
            using (var deflateStream = new DeflateStream(dataBytesOnly, CompressionMode.Decompress))
            {
                deflateStream.CopyTo(outputStream);
            }

            //dispose our read stream

            var decompressedBytes = outputStream.ToArray();
            dataBytesOnly.Dispose();
            outputStream.Dispose();

            //we now have decompressed data, but it's still encoded via the filter -
            //need to reconstruct and decode it.
            decompressedBytes = DecodeDecompressedData(decompressedBytes, imageHeader);

            return new PNGImage(imageHeader, decompressedBytes, SamplesPerPixel(imageHeader));

        }

        private static byte[] DecodeDecompressedData(byte[] decompressedData, ImageHeader pngHeader)
        {
            if (pngHeader.Interlaced)
            {
                throw new Exception("this decoder does not support interlaced pngs");
            }

            var samplePerPixel = SamplesPerPixel(pngHeader);
            var bytesPerScanLine = BytesPerScanLine(pngHeader, samplePerPixel);

            var rows = utilities.ListExtensions.Split(decompressedData.ToList(), (uint)(bytesPerScanLine + 1)).Select(x => x.ToArray()).ToArray();
            var rowIndex = 0;
            foreach (var row in rows)
            {

                var filterType = (FilterType)row[0];
                //don't go negative
                //TODO what do we do for the first row....
                byte[] previousRow;
                if (rowIndex - 1 >= 0)
                {
                    previousRow = rows[rowIndex - 1];
                }
                else
                {
                    previousRow = Enumerable.Repeat<byte>(0, row.Length).ToArray();
                }

                for (var byteIndex = 1; byteIndex < row.Count(); byteIndex++)
                {
                    //we might try indexing into a row that doesn't exist - 
                    //catch

                    Inverse_Filter(row, previousRow, filterType, byteIndex, (pngHeader.BitDepth / 8) * samplePerPixel);

                }
                rowIndex++;
            }
            return rows.SelectMany(x => x.Skip(1)).ToArray();
        }

        /// <summary>
        /// Inverses the filter of a byte, writes directly into the current row.
        /// </summary>
        /// <param name="currentRow"></param>
        /// <param name="previousRow"></param>
        /// <param name="type"></param>
        /// <param name="bytesPerPixel"></param>
        private static void Inverse_Filter(byte[] currentRow, byte[] previousRow, FilterType type, int currentByteIndex, int bytesPerPixel)
        {
            // in the below index checks we check against index of 1 instead of 0 because the filter type byte is index 0 and we don't
            // want to include this inside any of the filter computations.

            //Console.WriteLine(Enum.GetName(typeof(FilterType), type));
            if (type == FilterType.Up_Filter)
            {

                var dataInRowAbove = previousRow[currentByteIndex];
                currentRow[currentByteIndex] += dataInRowAbove;
            }

            else if (type == FilterType.Sub_Filter)
            {
                byte dataInPreviousPixel = 0;
                if (currentByteIndex - bytesPerPixel >= 1)
                {
                    dataInPreviousPixel = currentRow[currentByteIndex - bytesPerPixel];
                }

                currentRow[currentByteIndex] += dataInPreviousPixel;
            }

            else if (type == FilterType.None)
            {

            }
            else if (type == FilterType.Average_Filter)
            {
                var dataInRowAbove = previousRow[currentByteIndex];
                byte dataInPreviousPixel = 0;
                if (currentByteIndex - bytesPerPixel >= 1)
                {
                    dataInPreviousPixel = currentRow[currentByteIndex - bytesPerPixel];
                }
                currentRow[currentByteIndex] += (byte)((dataInPreviousPixel + dataInRowAbove) / 2);
            }
            else if (type == FilterType.PaethFilter)
            {
                var dataInRowAbove = previousRow[currentByteIndex];

                byte dataInPreviousPixel = 0;
                byte dataInPreviousPixelAbove = 0;
                if (currentByteIndex - bytesPerPixel >= 1)
                {
                    dataInPreviousPixel = currentRow[currentByteIndex - bytesPerPixel];
                }

                if (currentByteIndex - bytesPerPixel >= 1)
                {
                    dataInPreviousPixelAbove = previousRow[currentByteIndex - bytesPerPixel];
                }

                currentRow[currentByteIndex] += paethFilter(dataInPreviousPixel, dataInRowAbove, dataInPreviousPixelAbove);
            }



        }

        private static byte paethFilter(byte a, byte b, byte c)
        {
            var p = a + b - c;
            var pa = Math.Abs(p - a);
            var pb = Math.Abs(p - b);
            var pc = Math.Abs(p - c);

            if (pa <= pb && pa <= pc)
            {
                return a;
            }

            return pb <= pc ? b : c;
        }

        private static int BytesPerScanLine(ImageHeader pngHeader, byte samplePerPixel)
        {
            //number of pixels per line * number of channels per line * number of bytes per channel.
            return pngHeader.Width * samplePerPixel * (pngHeader.BitDepth / 8);
        }
        private static byte SamplesPerPixel(ImageHeader header)
        {
            switch (header.ColorType)
            {
                case ColorType.GREY:
                    return 1;
                case ColorType.INDEX:
                    return 1;
                case ColorType.COLOR:
                    return 3;
                case ColorType.ALPHA:
                    return 2;
                case ColorType.COLOR | ColorType.ALPHA:
                    return 4;
                default:
                    return 0;
            }
        }

        private static ImageHeader GetImageHeader(byte[] bytes, out int position)
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
            if (imageHeader.Datalength != 13)
            {
                throw new Exception("chunk header did not specify 13 byte len");
                /*
                Width	4 bytes
                Height	4 bytes
                Bit depth	1 byte
                Colour type	1 byte
                Compression method	1 byte
                Filter method	1 byte
                Interlace method	1 byte
                */
            }
            if (imageHeader.Name != "IHDR")
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

            //crc 4 bytes
            currentPos += 4;

            position = currentPos;

            return new ImageHeader(width, height, bitDepth, (ColorType)ColorType,
           (CompressionType)CompresionMethod, (FilterMethod)filter, interlaced);
        }


    }
}
