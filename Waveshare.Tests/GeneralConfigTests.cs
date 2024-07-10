using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waveshare.Helper;

namespace Waveshare.Tests
{
    /// <summary>
    /// Tests on integrity of configs
    /// </summary>
    public class GeneralConfigTests
    {
        [Fact]
        public void TestWidthAndHeightAreNotZero()
        {
            foreach (var kind in Enum.GetValues<DisplayKind>())
            {
                var config = DisplayManager.CreateConfig(kind);
                Assert.True(config.DisplayWidth > 0, $"{kind} width is not positive");
                Assert.True(config.DisplayHeight > 0, $"{kind} height is not positive");
            }
        }

        [Fact]
        public void TestNameIsNotEmpty()
        {
            foreach (var kind in Enum.GetValues<DisplayKind>())
            {
                var config = DisplayManager.CreateConfig(kind);
                Assert.False(string.IsNullOrWhiteSpace(config.Name), $"{kind} name is empty");
            }
        }

        [Fact]
        public void TestColorStylesAreNotEmpty()
        {
            foreach (var kind in Enum.GetValues<DisplayKind>())
            {
                var config = DisplayManager.CreateConfig(kind);
                Assert.NotEmpty(config.ColorStyles);
            }
        }

        [Fact]
        public void TestColorStylesAreUnique()
        {
            foreach (var kind in Enum.GetValues<DisplayKind>())
            {
                var config = DisplayManager.CreateConfig(kind);
                Assert.True(config.ColorStyles.Distinct().Count() == config.ColorStyles.Length);
            }
        }

        [Fact]
        public void TestColorStylesAreSortedByNumberOfColors()
        {
            foreach (var kind in Enum.GetValues<DisplayKind>())
            {
                var config = DisplayManager.CreateConfig(kind);
                var sorted = config.ColorStyles.OrderBy(x => x.SupportedColors().Length).ToArray();
                Assert.Equal(sorted, config.ColorStyles);
            }
        }

        [Fact]
        public void TestInitializeCommandsAreNotEmpty()
        {
            foreach (var kind in Enum.GetValues<DisplayKind>())
            {
                var config = DisplayManager.CreateConfig(kind);
                foreach (var style in config.ColorStyles)
                {
                    var commands = config.Initialize(style);
                    Assert.NotEmpty(commands);
                }
            }
        }

        [Fact]
        public void TestSendDisplayDataCommandsAreNotEmpty()
        {
            foreach (var kind in Enum.GetValues<DisplayKind>())
            {
                var config = DisplayManager.CreateConfig(kind);
                foreach (var style in config.ColorStyles)
                {
                    var commands = config.SendDisplayData(style, ImageHelper.FillColor(config.DisplayWidth, config.DisplayHeight, PixelColor.White));
                    Assert.NotEmpty(commands);
                }
            }
        }

        [Fact]
        public void TestRefreshCommandsAreNotEmpty()
        {
            foreach (var kind in Enum.GetValues<DisplayKind>())
            {
                var config = DisplayManager.CreateConfig(kind);
                foreach (var style in config.ColorStyles)
                {
                    var commands = config.Refresh(style, RefreshMode.Full);
                    Assert.NotEmpty(commands);
                }
            }
        }

        [Fact]
        public void TestTurnOffCommandsAreNotEmpty()
        {
            foreach (var kind in Enum.GetValues<DisplayKind>())
            {
                var config = DisplayManager.CreateConfig(kind);
                var commands = config.TurnOff();
                Assert.NotEmpty(commands);
            }
        }
    }
}
