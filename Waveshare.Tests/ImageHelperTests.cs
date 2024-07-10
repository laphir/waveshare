using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waveshare.Helper;

namespace Waveshare.Tests
{
    public class ImageHelperTests
    {
        [Fact]
        public void TestSupportedColorsAreDefinedForAllStyles()
        {
            foreach (var color in Enum.GetValues<DisplayColorStyle>())
            {
                var colors = color.SupportedColors();
                Assert.NotEmpty(colors);
            }
        }

        [Fact]
        public void TestChooseStyleFromBlack()
        {
            var styles = ImageHelper.ChooseStyleFromColors(PixelColor.Black);
            Assert.Contains(DisplayColorStyle.BlackAndWhite, styles);
            Assert.Contains(DisplayColorStyle.FourGray, styles);
            Assert.Contains(DisplayColorStyle.BlackAndWhiteAndRed, styles);
        }

        [Fact]
        public void TestChooseStyleFromRedAndBlack()
        {
            var styles = ImageHelper.ChooseStyleFromColors(PixelColor.Black, PixelColor.Red);
            Assert.DoesNotContain(DisplayColorStyle.BlackAndWhite, styles);
            Assert.DoesNotContain(DisplayColorStyle.FourGray, styles);
            Assert.Contains(DisplayColorStyle.BlackAndWhiteAndRed, styles);
        }

        [Fact]
        public void TestDetectMinimumColorStyle_BlackAndWhite()
        {
            var image = new PixelColor[,]
            {
                { PixelColor.Black, PixelColor.White },
                { PixelColor.White, PixelColor.Black },
            };

            var style = ImageHelper.DetectMinimumColorStyle(image);
            Assert.Equal(DisplayColorStyle.BlackAndWhite, style);
        }

        [Fact]
        public void TestDetectMinimumColorStyle_Gray()
        {
            var image = new PixelColor[,]
            {
                { PixelColor.Black, PixelColor.DarkGray },
                { PixelColor.White, PixelColor.Black },
            };

            var style = ImageHelper.DetectMinimumColorStyle(image);
            Assert.Equal(DisplayColorStyle.FourGray, style);
        }

        [Fact]
        public void TestDetectMinimumColorStyle_BWR()
        {
            var image = new PixelColor[,]
            {
                { PixelColor.Black, PixelColor.Red },
                { PixelColor.White, PixelColor.Black },
            };

            var style = ImageHelper.DetectMinimumColorStyle(image);
            Assert.Equal(DisplayColorStyle.BlackAndWhiteAndRed, style);
        }

        [Fact]
        public void TestToPixelColor_BlackAndWhite()
        {
            var style = DisplayColorStyle.BlackAndWhite;

            {
                var color = PixelColor.Black;
                var skColor = SKColor.Parse("#000000");
                var result = skColor.ToPixelColor(style);
                Assert.Equal(color, result);
            }
            {
                var color = PixelColor.White;
                var skColor = SKColor.Parse("#FFFFFF");
                var result = skColor.ToPixelColor(style);
                Assert.Equal(color, result);
            }
        }

        [Fact]
        public void TestToPixelColor_4Gray()
        {
            var style = DisplayColorStyle.FourGray;
            string[] whites = "#FFFFFF,#EEEEEE,#DDDDDD".Split(',');
            string[] lGrays = "#CCCCCC,#BBBBBB,#AAAAAA,#999999,#888888".Split(',');
            string[] dGrays = "#777777,#666666,#555555,#444444,#333333".Split(',');
            string[] blacks = "#222222,#111111,#000000".Split(',');

            foreach (var skColor in whites.Select(SKColor.Parse))
            {
                var color = PixelColor.White;
                var result = skColor.ToPixelColor(style);
                Assert.Equal(color, result);
            }

            foreach (var skColor in lGrays.Select(SKColor.Parse))
            {
                var color = PixelColor.LightGray;
                var result = skColor.ToPixelColor(style);
                Assert.Equal(color, result);
            }

            foreach (var skColor in dGrays.Select(SKColor.Parse))
            {
                var color = PixelColor.DarkGray;
                var result = skColor.ToPixelColor(style);
                Assert.Equal(color, result);
            }

            foreach (var skColor in blacks.Select(SKColor.Parse))
            {
                var color = PixelColor.Black;
                var result = skColor.ToPixelColor(style);
                Assert.Equal(color, result);
            }
        }
    }
}
