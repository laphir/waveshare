using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waveshare.Config;

namespace Waveshare.Tests
{
    public class WaveshareConfig_2_13bc_Tests
    {
        WaveshareConfig_2_13bc DisplayConfig { get; } = new WaveshareConfig_2_13bc();

        [Fact]
        public void TestHardwareReset()
        {
            var commands = DisplayConfig.HardwareReset.Select(a => a.ToString());
            var expected = new string[]
            {
                "SetGpio Reset 1",
                "Sleep 200",
                "SetGpio Reset 0",
                "Sleep 2",
                "SetGpio Reset 1",
                "Sleep 200",
                "WaitForIdle",
            };

            Assert.Equal(expected, commands);
        }

        [Fact]
        public void TestInitialize()
        {
            var commands = DisplayConfig.Initialize(DisplayColorStyle.BlackAndWhiteAndRed)
                .Where(a => a is not DebugMessageCommand)
                .Select(a => a.ToString());
            var expected = new string[]
            {
                // Reset
                "SetGpio Reset 1",
                "Sleep 200",
                "SetGpio Reset 0",
                "Sleep 2",
                "SetGpio Reset 1",
                "Sleep 200",
                "WaitForIdle",

                // Initialize
                "SendCommand 0x06",
                "SendData 0x17",
                "SendData 0x17",
                "SendData 0x17",
                "SendCommand 0x04",
                "Sleep 10",
                "WaitForIdle",
                "SendCommand 0x00",
                "SendData 0x8F",
                "SendCommand 0x50",
                "SendData 0x70",
                "SendCommand 0x61",
                "SendData 0x68",
                "SendData 0x00",
                "SendData 0xD4"
            };

            Assert.Equal(expected, commands);
        }

        [Fact]
        public void TestRefresh()
        {
            var commands = DisplayConfig.Refresh(DisplayColorStyle.BlackAndWhiteAndRed, RefreshMode.Full)
                .Where(a => a is not DebugMessageCommand)
                .Select(a => a.ToString());

            var expected = new string[]
            {
                "SendCommand 0x12",
                "Sleep 10",
                "WaitForIdle"
            };

            Assert.Equal(expected, commands);
        }

        [Fact]
        public void TestTurnOff()
        {
            var commands = DisplayConfig.TurnOff()
                .Where(a => a is not DebugMessageCommand)
                .Select(a => a.ToString());

            var expected = new string[]
            {
                "SendCommand 0x02",
                "WaitForIdle",
                "SendCommand 0x07",
                "SendData 0xA5"
            };

            Assert.Equal(expected, commands);
        }
    }
}
