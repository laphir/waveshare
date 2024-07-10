using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waveshare.Tests
{
    public class ImageProcessorTests
    {
        [Fact]
        public void TestBlackAndWhite()
        {
            using var bitmap = new SKBitmap(2, 1, true);
            bitmap.SetPixel(0, 0, SKColors.Black);
            bitmap.SetPixel(1, 0, SKColors.White);

            using var processor = new ImageProcessor(bitmap);
            var pixels = processor.ToPixels(DisplayColorStyle.BlackAndWhite);

            Assert.Equal(1, pixels.GetLength(0));
            Assert.Equal(2, pixels.GetLength(1));
            Assert.Equal(PixelColor.Black, pixels[0, 0]);
            Assert.Equal(PixelColor.White, pixels[0, 1]);
        }

        //[Fact]
        //public void TestRealIamge()
        //{
        //    using var bitmap = SKBitmap.Decode("C:\\Users\\yoongki\\Desktop\\asuka-100.png");
        //    using var processor = new ImageProcessor(bitmap);
        //    var pixels = processor.ToPixels(DisplayColorStyle.FourGray);

        //    int darkGrays = 0;
        //    int blacks = 0;
        //    foreach (var pixel in pixels)
        //    {
        //        if (pixel == PixelColor.DarkGray)
        //        {
        //            darkGrays++;
        //        }
        //        else if (pixel == PixelColor.Black)
        //        {
        //            blacks++;
        //        }
        //    }
        //}

        [Fact]
        public void Test4Gray()
        {
            using var bitmap = new SKBitmap(256, 1, true);
            for (int i = 0; i < 256; i++)
            {
                var hex = i.ToString("X2");
                bitmap.SetPixel(i, 0, SKColor.Parse($"#{hex}{hex}{hex}"));
            }

            using var processor = new ImageProcessor(bitmap);
            var pixels = processor.ToPixels(DisplayColorStyle.FourGray);

            // Code to generate test code
            //for (int i = 0; i < 256; i++)
            //{
            //    System.Diagnostics.Debug.WriteLine($"Assert.Equal(PixelColor.{pixels[0, i]}, pixels[0,{i}]);");
            //}

            Assert.Equal(PixelColor.Black, pixels[0, 0]);
            Assert.Equal(PixelColor.Black, pixels[0, 1]);
            Assert.Equal(PixelColor.Black, pixels[0, 2]);
            Assert.Equal(PixelColor.Black, pixels[0, 3]);
            Assert.Equal(PixelColor.Black, pixels[0, 4]);
            Assert.Equal(PixelColor.Black, pixels[0, 5]);
            Assert.Equal(PixelColor.Black, pixels[0, 6]);
            Assert.Equal(PixelColor.Black, pixels[0, 7]);
            Assert.Equal(PixelColor.Black, pixels[0, 8]);
            Assert.Equal(PixelColor.Black, pixels[0, 9]);
            Assert.Equal(PixelColor.Black, pixels[0, 10]);
            Assert.Equal(PixelColor.Black, pixels[0, 11]);
            Assert.Equal(PixelColor.Black, pixels[0, 12]);
            Assert.Equal(PixelColor.Black, pixels[0, 13]);
            Assert.Equal(PixelColor.Black, pixels[0, 14]);
            Assert.Equal(PixelColor.Black, pixels[0, 15]);
            Assert.Equal(PixelColor.Black, pixels[0, 16]);
            Assert.Equal(PixelColor.Black, pixels[0, 17]);
            Assert.Equal(PixelColor.Black, pixels[0, 18]);
            Assert.Equal(PixelColor.Black, pixels[0, 19]);
            Assert.Equal(PixelColor.Black, pixels[0, 20]);
            Assert.Equal(PixelColor.Black, pixels[0, 21]);
            Assert.Equal(PixelColor.Black, pixels[0, 22]);
            Assert.Equal(PixelColor.Black, pixels[0, 23]);
            Assert.Equal(PixelColor.Black, pixels[0, 24]);
            Assert.Equal(PixelColor.Black, pixels[0, 25]);
            Assert.Equal(PixelColor.Black, pixels[0, 26]);
            Assert.Equal(PixelColor.Black, pixels[0, 27]);
            Assert.Equal(PixelColor.Black, pixels[0, 28]);
            Assert.Equal(PixelColor.Black, pixels[0, 29]);
            Assert.Equal(PixelColor.Black, pixels[0, 30]);
            Assert.Equal(PixelColor.Black, pixels[0, 31]);
            Assert.Equal(PixelColor.Black, pixels[0, 32]);
            Assert.Equal(PixelColor.Black, pixels[0, 33]);
            Assert.Equal(PixelColor.Black, pixels[0, 34]);
            Assert.Equal(PixelColor.Black, pixels[0, 35]);
            Assert.Equal(PixelColor.Black, pixels[0, 36]);
            Assert.Equal(PixelColor.Black, pixels[0, 37]);
            Assert.Equal(PixelColor.Black, pixels[0, 38]);
            Assert.Equal(PixelColor.Black, pixels[0, 39]);
            Assert.Equal(PixelColor.Black, pixels[0, 40]);
            Assert.Equal(PixelColor.Black, pixels[0, 41]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 42]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 43]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 44]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 45]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 46]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 47]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 48]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 49]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 50]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 51]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 52]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 53]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 54]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 55]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 56]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 57]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 58]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 59]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 60]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 61]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 62]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 63]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 64]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 65]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 66]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 67]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 68]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 69]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 70]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 71]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 72]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 73]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 74]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 75]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 76]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 77]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 78]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 79]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 80]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 81]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 82]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 83]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 84]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 85]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 86]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 87]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 88]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 89]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 90]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 91]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 92]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 93]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 94]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 95]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 96]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 97]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 98]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 99]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 100]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 101]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 102]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 103]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 104]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 105]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 106]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 107]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 108]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 109]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 110]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 111]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 112]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 113]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 114]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 115]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 116]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 117]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 118]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 119]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 120]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 121]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 122]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 123]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 124]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 125]);
            Assert.Equal(PixelColor.DarkGray, pixels[0, 126]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 127]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 128]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 129]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 130]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 131]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 132]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 133]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 134]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 135]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 136]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 137]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 138]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 139]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 140]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 141]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 142]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 143]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 144]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 145]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 146]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 147]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 148]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 149]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 150]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 151]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 152]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 153]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 154]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 155]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 156]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 157]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 158]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 159]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 160]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 161]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 162]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 163]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 164]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 165]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 166]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 167]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 168]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 169]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 170]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 171]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 172]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 173]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 174]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 175]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 176]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 177]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 178]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 179]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 180]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 181]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 182]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 183]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 184]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 185]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 186]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 187]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 188]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 189]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 190]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 191]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 192]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 193]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 194]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 195]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 196]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 197]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 198]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 199]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 200]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 201]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 202]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 203]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 204]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 205]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 206]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 207]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 208]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 209]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 210]);
            Assert.Equal(PixelColor.LightGray, pixels[0, 211]);
            Assert.Equal(PixelColor.White, pixels[0, 212]);
            Assert.Equal(PixelColor.White, pixels[0, 213]);
            Assert.Equal(PixelColor.White, pixels[0, 214]);
            Assert.Equal(PixelColor.White, pixels[0, 215]);
            Assert.Equal(PixelColor.White, pixels[0, 216]);
            Assert.Equal(PixelColor.White, pixels[0, 217]);
            Assert.Equal(PixelColor.White, pixels[0, 218]);
            Assert.Equal(PixelColor.White, pixels[0, 219]);
            Assert.Equal(PixelColor.White, pixels[0, 220]);
            Assert.Equal(PixelColor.White, pixels[0, 221]);
            Assert.Equal(PixelColor.White, pixels[0, 222]);
            Assert.Equal(PixelColor.White, pixels[0, 223]);
            Assert.Equal(PixelColor.White, pixels[0, 224]);
            Assert.Equal(PixelColor.White, pixels[0, 225]);
            Assert.Equal(PixelColor.White, pixels[0, 226]);
            Assert.Equal(PixelColor.White, pixels[0, 227]);
            Assert.Equal(PixelColor.White, pixels[0, 228]);
            Assert.Equal(PixelColor.White, pixels[0, 229]);
            Assert.Equal(PixelColor.White, pixels[0, 230]);
            Assert.Equal(PixelColor.White, pixels[0, 231]);
            Assert.Equal(PixelColor.White, pixels[0, 232]);
            Assert.Equal(PixelColor.White, pixels[0, 233]);
            Assert.Equal(PixelColor.White, pixels[0, 234]);
            Assert.Equal(PixelColor.White, pixels[0, 235]);
            Assert.Equal(PixelColor.White, pixels[0, 236]);
            Assert.Equal(PixelColor.White, pixels[0, 237]);
            Assert.Equal(PixelColor.White, pixels[0, 238]);
            Assert.Equal(PixelColor.White, pixels[0, 239]);
            Assert.Equal(PixelColor.White, pixels[0, 240]);
            Assert.Equal(PixelColor.White, pixels[0, 241]);
            Assert.Equal(PixelColor.White, pixels[0, 242]);
            Assert.Equal(PixelColor.White, pixels[0, 243]);
            Assert.Equal(PixelColor.White, pixels[0, 244]);
            Assert.Equal(PixelColor.White, pixels[0, 245]);
            Assert.Equal(PixelColor.White, pixels[0, 246]);
            Assert.Equal(PixelColor.White, pixels[0, 247]);
            Assert.Equal(PixelColor.White, pixels[0, 248]);
            Assert.Equal(PixelColor.White, pixels[0, 249]);
            Assert.Equal(PixelColor.White, pixels[0, 250]);
            Assert.Equal(PixelColor.White, pixels[0, 251]);
            Assert.Equal(PixelColor.White, pixels[0, 252]);
            Assert.Equal(PixelColor.White, pixels[0, 253]);
            Assert.Equal(PixelColor.White, pixels[0, 254]);
            Assert.Equal(PixelColor.White, pixels[0, 255]);

        }
    }
}
