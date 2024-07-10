using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SkiaSharp;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Waveshare.Helper
{
    public static class ImageHelper
    {
        /// <summary>
        /// This function is used to generate a temporary image to clear the screen.
        /// </summary>
        /// <returns>[height, width]</returns>
        public static PixelColor[,] FillColor(int width, int height, PixelColor color)
        {
            var image = new PixelColor[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    image[y, x] = color;
                }
            }
            return image;
        }

        /// <summary>
        /// This function was used to generate a test pattern.
        /// </summary>
        /// <returns>[height, width]</returns>
        public static PixelColor[,] GenerateChessboardPattern(int width, int height, int patternSize, PixelColor[] colors)
        {
            var image = new PixelColor[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var fill = colors[(x / patternSize + y / patternSize) % colors.Length];
                    image[y, x] = fill;
                }
            }

            return image;
        }

        /// <summary>
        /// The number of bits required to represent black and white is 1 bit.
        /// To reduce the amount of transmitted data, Waveshare displays require packing 8 pixels into 1 byte for transmission.
        /// This function packs one row of an image into a byte array.
        /// </summary>
        /// <param name="image">[height, width]</param>
        /// <param name="y">The row index of image to process</param>
        /// <returns>packed byte array</returns>
        /// <exception cref="NotSupportedException">When the pixel has other than black and white</exception>
        public static byte[] PackRowAsBlackAndWhite(PixelColor[,] image, int y)
        {
            var height = image.GetLength(0);
            if (y >= height || y < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }

            var width = image.GetLength(1);
            var packed = new byte[(width + 7) / 8];

            // White is 1, Black is 0.
            for (int x = 0; x < width; x++)
            {
                var color = image[y, x];
                if (color == PixelColor.White)
                {
                    packed[x / 8] |= (byte)(1 << 7 - x % 8);
                }
                else if (color == PixelColor.Black)
                {
                    // no op
                }
                else
                {
                    throw new NotSupportedException($"Color {color} is not supported.");
                }
            }

            return packed;
        }


        /// <summary>
        /// The minimum number of bits required to represent 4 gray levels is 2 bits per pixel.
        /// This function separates the high bit and low bit of each pixel and packs them into a byte array.
        /// The Waveshare 2.7 HAT v2 display requires each bit to be sent separately with different commands.
        /// </summary>
        /// <param name="image">[height, width]</param>
        /// <param name="y">The row index of image to process</param>
        /// <returns>(lowBits, highBits)</returns>
        /// <exception cref="NotSupportedException">When the pixel has other than 4gray</exception>
        public static Tuple<byte[], byte[]> PackRowAs4Gray(PixelColor[,] image, int y)
        {
            // Surprise!
            // Unlike black and white images, now white is 00, and black is 11.
            // 
            // Input color expectation:
            //      White Gray1 Gray2 Black
            // 
            // Output uses a different color code. Basically bit-value is negated.
            //      White Gray1 Gray2 Black
            //      00    01    10    11
            //
            // Then store high bit and low bit in a different stream.
            //      0x24 data - low bit
            //      0x26 data - high bit

            var height = image.GetLength(0);
            if (y >= height || y < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }

            var width = image.GetLength(1);
            var lowBits = new byte[(width + 7) / 8];
            var highBits = new byte[(width + 7) / 8];

            for (int x = 0; x < width; x++)
            {
                var color = image[y, x];
                if (color == PixelColor.White)
                {
                    // Set 0 to both bits, but array is initialized as 0, so no op.
                }
                else if (color == PixelColor.Black)
                {
                    // Set 1 to both bits.
                    lowBits[x / 8] |= (byte)(1 << 7 - x % 8);
                    highBits[x / 8] |= (byte)(1 << 7 - x % 8);
                }
                else if (color == PixelColor.LightGray)
                {
                    // Set 01. High remains 0, low set to 1.
                    lowBits[x / 8] |= (byte)(1 << 7 - x % 8);
                }
                else if (color == PixelColor.DarkGray)
                {
                    // Set 10. Low remains 0, high set to 1.
                    highBits[x / 8] |= (byte)(1 << 7 - x % 8);
                }
                else
                {
                    throw new NotSupportedException($"Color {color} is not supported.");
                }
            }

            return Tuple.Create(lowBits, highBits);
        }

        /// <summary>
        /// Choose all possible color styles from given colors.
        /// </summary>
        public static IEnumerable<DisplayColorStyle> ChooseStyleFromColors(IEnumerable<PixelColor> colors)
        {
            foreach (var color in Enum.GetValues<DisplayColorStyle>())
            {
                var supported = color.SupportedColors();
                if (colors.All(c => supported.Contains(c)))
                {
                    yield return color;
                }
            }
        }

        public static IEnumerable<DisplayColorStyle> ChooseStyleFromColors(params PixelColor[] colors)
        {
            return ChooseStyleFromColors(colors as IEnumerable<PixelColor>);
        }

        public static DisplayColorStyle DetectMinimumColorStyle(this PixelColor[,] image)
        {
            var usedColors = new HashSet<PixelColor>();
            foreach (var color in image)
            {
                usedColors.Add(color);
            }

            var compatibleStyles = ChooseStyleFromColors(usedColors)
                .OrderBy(s => s.SupportedColors().Length)               // sort by supported colors. First one is minimum color style.
                .ToArray();
            return compatibleStyles[0];
        }

        public static PixelColor ToPixelColor(this SKColor color, DisplayColorStyle style)
        {
            // Obvious cases.
            bool hasRed = style == DisplayColorStyle.BlackAndWhiteAndRed;
            if (hasRed && color.Red == 255 && color.Green == 0 && color.Blue == 0)
            {
                return PixelColor.Red;
            }

            byte grayscale = color.Red;

            // Quick check for grayscale
            bool isGrayScale = color.Red == color.Green && color.Green == color.Blue;
            if (!isGrayScale)
            {
                grayscale = (byte)((uint)color.Red * 307 + (uint)color.Green * 604 + (uint)color.Blue * 113 >> 10);
            }

            // 4 gray scale - map to nearest color
            if (style == DisplayColorStyle.FourGray)
            {
                const int oneThird = 255 / 3;
                switch (grayscale)
                {
                    case byte n when n < oneThird / 2:
                        return PixelColor.Black;
                    case byte n when n < oneThird + oneThird / 2:
                        return PixelColor.DarkGray;
                    case byte n when n < oneThird * 2 + oneThird / 2:
                        return PixelColor.LightGray;
                    default:
                        return PixelColor.White;
                }
            }

            return grayscale < 128 ? PixelColor.Black : PixelColor.White;
        }
    }
}
