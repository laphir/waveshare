using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waveshare.Config
{
    /// <summary>
    /// https://www.waveshare.com/wiki/7.5inch_e-Paper_HAT
    /// </summary>
    internal class WaveshareConfig_7_5v2 : IDisplayConfig
    {
        public string Name => "Waveshare 7.5 inch v2";

        // From EPD_7IN5_V2_WIDTH
        public int DisplayWidth => 800;
        // From EPD_7IN5_V2_HEIGHT
        public int DisplayHeight => 480;

        /// <summary>
        /// Low means busy.
        /// </summary>
        public System.Device.Gpio.PinValue BusyValue => System.Device.Gpio.PinValue.Low;

        public bool CanPartialRefresh => true;

        // No buttons on this display
        public int[]? ButtonPins => null;

        public DisplayColorStyle[] ColorStyles => new[] { DisplayColorStyle.BlackAndWhite };

        enum CommandKind
        {
            PanelSetting = 0x00,
            PowerSetting = 0x01,
            PowerOff = 0x02,
            PowerOn = 0x04,
            BoosterSoftStart = 0x06,
            DeepSleep = 0x07,
            DataStartTransmission1 = 0x10,
            DisplayRefresh = 0x12,
            DataStartTransmission2 = 0x13,
            DualSpiMode = 0x15,
            VComAndDataIntervalSetting = 0x50,
            TConSetting = 0x60,
            ResolutionSetting = 0x61,
        }

        public DisplayCommand[] Initialize(DisplayColorStyle style)
        {
            if (style != DisplayColorStyle.BlackAndWhite)
            {
                throw new NotSupportedException($"Unsupported color style: {style}");
            }

            // EPD_2IN13BC_Init
            List<DisplayCommand> commands = new List<DisplayCommand>();
            commands.Add(new DebugMessageCommand { Message = "Initialize screen" });

            commands.AddRange(Reset);

            commands.AddRange(SendCommandByte(CommandKind.PowerSetting));
            commands.AddRange(SendDataByte(0x07));
            commands.AddRange(SendDataByte(0x07));      //VGH=20V,VGL=-20V
            commands.AddRange(SendDataByte(0x3f));      //VDH=15V
            commands.AddRange(SendDataByte(0x3f));      //VDL=-15V

            //Enhanced display drive(Add 0x06 command)
            commands.AddRange(SendCommandByte(CommandKind.BoosterSoftStart));
            commands.AddRange(SendDataByte(0x17));
            commands.AddRange(SendDataByte(0x17));
            commands.AddRange(SendDataByte(0x28));
            commands.AddRange(SendDataByte(0x17));

            commands.AddRange(SendCommandByte(CommandKind.PowerOn));
            commands.AddRange(WaitForIdle(100));

            commands.AddRange(SendCommandByte(CommandKind.PanelSetting));
            commands.AddRange(SendDataByte(0x1F));      //KW-3f   KWR-2F   BWROTP 0f    BWOTP 1f

            commands.AddRange(SendCommandByte(CommandKind.ResolutionSetting));
            commands.AddRange(SendDataByte(0x03));      // width 800
            commands.AddRange(SendDataByte(0x20));
            commands.AddRange(SendDataByte(0x01));      // height 480
            commands.AddRange(SendDataByte(0xE0));

            commands.AddRange(SendCommandByte(CommandKind.DualSpiMode));
            commands.AddRange(SendDataByte(0x00));

            commands.AddRange(SendCommandByte(CommandKind.VComAndDataIntervalSetting));
            commands.AddRange(SendDataByte(0x10));
            commands.AddRange(SendDataByte(0x07));

            commands.AddRange(SendCommandByte(CommandKind.TConSetting));
            commands.AddRange(SendDataByte(0x22));

            return commands.ToArray();
        }

        public DisplayCommand[] InitializePartial(DisplayColorStyle style, int x, int y, int width, int height)
        {
            throw new NotImplementedException("Partial refresh is not implemented");
        }

        public DisplayCommand[] Refresh(DisplayColorStyle style, RefreshMode refreshMode)
        {
            if (refreshMode != RefreshMode.Full)
            {
                throw new NotImplementedException($"Not implemented refresh mode: {refreshMode}");
            }

            if (style != DisplayColorStyle.BlackAndWhite)
            {
                throw new NotSupportedException($"Unsupported color style: {style}");
            }

            List<DisplayCommand> commands = new List<DisplayCommand>();
            commands.AddRange(SendCommandByte(CommandKind.DisplayRefresh));
            commands.AddRange(WaitForIdle(100));  //!!!The delay here is necessary, 200uS at least!!!
            return commands.ToArray();
        }

        public DisplayCommand[] SendDisplayData(DisplayColorStyle style, PixelColor[,] image)
        {
            if (style != DisplayColorStyle.BlackAndWhite)
            {
                throw new NotSupportedException($"Unsupported color style: {style}");
            }

            List<DisplayCommand> commands = new List<DisplayCommand>();

            // Send old data - just white.
            byte[] oldRow = new byte[(DisplayWidth + 7) / 8];
            commands.AddRange(SendCommandByte(CommandKind.DataStartTransmission1));
            for (int i = 0; i < DisplayHeight; i++)
            {
                commands.AddRange(SendDataBytes(oldRow));
            }

            // Send new data
            // white is 0, black is 1
            commands.AddRange(SendCommandByte(CommandKind.DataStartTransmission2));
            for (int i = 0; i < DisplayHeight; i++)
            {
                byte[] newRow = new byte[(DisplayWidth + 7) / 8];

                for (int j = 0; j < DisplayWidth; j++)
                {
                    if (image[i, j] == PixelColor.Black)
                    {
                        newRow[j / 8] |= (byte)(0x80 >> j % 8);
                    }
                }

                commands.AddRange(SendDataBytes(newRow));
            }

            return commands.ToArray();
        }

        public DisplayCommand[] TurnOff()
        {
            List<DisplayCommand> commands = new List<DisplayCommand>();
            commands.AddRange(SendCommandByte(CommandKind.PowerOff));
            commands.AddRange(WaitForIdle());
            commands.AddRange(SendCommandByte(CommandKind.DeepSleep));
            commands.AddRange(SendDataByte(0xA5));  // This is a check code.
            return commands.ToArray();
        }

        /// <summary>
        /// Reset - after deep sleeping the display, need to call this to wake it up.
        /// See EPD_Reset()
        /// </summary>
        public DisplayCommand[] Reset => DisplayCommand.Parse(
            "SetGpio reset, 1",
            "Sleep 20",
            "SetGpio reset, 0",
            "Sleep 2",
            "SetGpio reset, 1",
            "Sleep 20");

        /// <summary>
        /// See EPD_SendCommand()
        /// </summary>
        private DisplayCommand[] SendCommandByte(CommandKind command) =>
            DisplayCommand.Parse($"SendCommand {(byte)command}");

        // See EPD_SendData
        public DisplayCommand[] SendDataByte(byte data) =>
            DisplayCommand.Parse($"SendData {data}");

        // See EPD_SendData2
        public DisplayCommand[] SendDataBytes(byte[] data)
        {
            return new DisplayCommand[]
            {
                new SendDataCommand{ MultipleData = data, }
            };
        }

        public DisplayCommand[] WaitForIdle(uint milliseconds = 0) =>
            milliseconds > 0 ?
                DisplayCommand.Parse($"Sleep {milliseconds}", "WaitForIdle") :
                DisplayCommand.Parse("WaitForIdle");
    }
}
