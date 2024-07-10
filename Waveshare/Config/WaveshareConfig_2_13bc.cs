using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waveshare.Config
{
    /// <summary>
    /// Waveshare 2.13 inch BC config.
    /// Black, White, Red
    /// </summary>
    internal class WaveshareConfig_2_13bc : IDisplayConfig
    {
        public string Name => "Waveshare 2.13bc";

        public int DisplayWidth => 104;
        public int DisplayHeight => 212;

        // EPD_2IN13BC_ReadBusy
        public PinValue BusyValue => PinValue.Low;

        public bool CanPartialRefresh => false;

        /// <summary>
        /// 2.13bc does not have buttons.
        /// </summary>
        public int[]? ButtonPins => null;

        public DisplayColorStyle[] ColorStyles => new[] { DisplayColorStyle.BlackAndWhite, DisplayColorStyle.BlackAndWhiteAndRed, };

        // See Waveshare's documentation
        enum CommandKind : byte
        {
            PanelSetting = 0x00,
            PowerOff = 0x02,
            PowerOn = 0x04,
            BoosterSoftStart = 0x06,
            DeepSleep = 0x07,
            DataStartTransmission1 = 0x10,
            DataStartTransmission2 = 0x13,
            DisplayRefresh = 0x12,
            VcomAndDataIntervalSetting = 0x50,
            ResolutionSetting = 0x61,
            PartialOut = 0x92,

            // From EPD_2in13bc.c
            //  #define PANEL_SETTING                               0x00
            //  #define POWER_SETTING                               0x01
            //  #define POWER_OFF                                   0x02
            //  #define POWER_OFF_SEQUENCE_SETTING                  0x03
            //  #define POWER_ON                                    0x04
            //  #define POWER_ON_MEASURE                            0x05
            //  #define BOOSTER_SOFT_START                          0x06
            //  #define DEEP_SLEEP                                  0x07
            //  #define DATA_START_TRANSMISSION_1                   0x10
            //  #define DATA_STOP                                   0x11
            //  #define DISPLAY_REFRESH                             0x12
            //  #define DATA_START_TRANSMISSION_2                   0x13
            //  #define VCOM_LUT                                    0x20
            //  #define W2W_LUT                                     0x21
            //  #define B2W_LUT                                     0x22
            //  #define W2B_LUT                                     0x23
            //  #define B2B_LUT                                     0x24
            //  #define PLL_CONTROL                                 0x30
            //  #define TEMPERATURE_SENSOR_CALIBRATION              0x40
            //  #define TEMPERATURE_SENSOR_SELECTION                0x41
            //  #define TEMPERATURE_SENSOR_WRITE                    0x42
            //  #define TEMPERATURE_SENSOR_READ                     0x43
            //  #define VCOM_AND_DATA_INTERVAL_SETTING              0x50
            //  #define LOW_POWER_DETECTION                         0x51
            //  #define TCON_SETTING                                0x60
            //  #define RESOLUTION_SETTING                          0x61
            //  #define GET_STATUS                                  0x71
            //  #define AUTO_MEASURE_VCOM                           0x80
            //  #define READ_VCOM_VALUE                             0x81
            //  #define VCM_DC_SETTING                              0x82
            //  #define PARTIAL_WINDOW                              0x90
            //  #define PARTIAL_IN                                  0x91
            //  #define PARTIAL_OUT                                 0x92
            //  #define PROGRAM_MODE                                0xA0
            //  #define ACTIVE_PROGRAM                              0xA1
            //  #define READ_OTP_DATA                               0xA2
            //  #define POWER_SAVING                                0xE3
        }

        public DisplayCommand[] Initialize(DisplayColorStyle style)
        {
            if (style == DisplayColorStyle.BlackAndWhite || style == DisplayColorStyle.BlackAndWhiteAndRed)
            {
            }
            else
            {
                throw new NotSupportedException($"Unsupported color style: {style}");
            }

            // EPD_2IN13BC_Init
            List<DisplayCommand> commands = new List<DisplayCommand>();
            commands.Add(new DebugMessageCommand { Message = "Initialize screen" });

            commands.AddRange(HardwareReset);

            commands.AddRange(SendCommandByte(CommandKind.BoosterSoftStart));
            commands.AddRange(SendDataByte(0x17));
            commands.AddRange(SendDataByte(0x17));
            commands.AddRange(SendDataByte(0x17));

            commands.AddRange(SendCommandByte(CommandKind.PowerOn));
            commands.AddRange(WaitForIdle(10));

            commands.AddRange(SendCommandByte(CommandKind.PanelSetting));
            commands.AddRange(SendDataByte(0x8F));

            commands.AddRange(SendCommandByte(CommandKind.VcomAndDataIntervalSetting));
            commands.AddRange(SendDataByte(0x70));

            commands.AddRange(SendCommandByte(CommandKind.ResolutionSetting));
            commands.AddRange(SendDataByte((byte)DisplayWidth));
            commands.AddRange(SendDataByte((byte)(DisplayHeight >> 8)));
            commands.AddRange(SendDataByte((byte)(DisplayHeight & 0xFF)));
            return commands.ToArray();
        }

        public DisplayCommand[] InitializePartial(DisplayColorStyle style, int x, int y, int width, int height)
        {
            throw new NotSupportedException("Partial refresh is not supported");
        }

        public DisplayCommand[] Refresh(DisplayColorStyle style, RefreshMode refreshMode)
        {
            if (refreshMode != RefreshMode.Full)
            {
                throw new NotSupportedException($"Unsupported refresh mode: {refreshMode}");
            }

            if (style == DisplayColorStyle.BlackAndWhite || style == DisplayColorStyle.BlackAndWhiteAndRed)
            {
            }
            else
            {
                throw new NotSupportedException($"Unsupported color style: {style}");
            }

            // EPD_2IN13BC_TurnOnDisplay
            List<DisplayCommand> commands = new List<DisplayCommand>();
            commands.Add(new DebugMessageCommand { Message = "Refresh screen" });
            commands.AddRange(SendCommandByte(CommandKind.DisplayRefresh));
            commands.AddRange(WaitForIdle(10));
            return commands.ToArray();
        }

        public DisplayCommand[] SendDisplayData(DisplayColorStyle style, PixelColor[,] image)
        {
            if (style == DisplayColorStyle.BlackAndWhite || style == DisplayColorStyle.BlackAndWhiteAndRed)
            {
            }
            else
            {
                throw new NotSupportedException($"Unsupported color style: {style}");
            }

            if (image.GetLength(0) != DisplayHeight || image.GetLength(1) != DisplayWidth)
            {
                throw new ArgumentException($"Image size must be [{DisplayHeight}, {DisplayWidth}]");
            }

            // Split image 
            byte[,] black = new byte[DisplayHeight, (DisplayWidth + 7) / 8];
            byte[,] red = new byte[DisplayHeight, (DisplayWidth + 7) / 8];

            for (int y = 0; y < DisplayHeight; y++)
            {
                for (int x = 0; x < DisplayWidth; x++)
                {
                    int i = x / 8;
                    int j = x % 8;
                    var pixel = image[y, x];
                    switch (pixel)
                    {
                        case PixelColor.Black:
                            black[y, i] |= (byte)(0x80 >> j);
                            red[y, i] |= (byte)(0x80 >> j);
                            break;
                        case PixelColor.White:
                            red[y, i] |= (byte)(0x80 >> j);
                            break;
                        case PixelColor.Red:
                            break;
                        default:
                            throw new NotSupportedException($"Unsupported color: {pixel}");
                    }
                }
            }

            // EPD_2IN13BC_Display
            List<DisplayCommand> commands = new List<DisplayCommand>();
            commands.Add(new DebugMessageCommand { Message = "Send display data" });

            commands.AddRange(SendCommandByte(CommandKind.DataStartTransmission1));
            for (int y = 0; y < DisplayHeight; y++)
            {
                for (int x = 0; x < (DisplayWidth + 7) / 8; x++)
                {
                    commands.AddRange(SendDataByte(black[y, x]));
                }
            }
            commands.AddRange(SendCommandByte(CommandKind.PartialOut));

            commands.AddRange(SendCommandByte(CommandKind.DataStartTransmission2));
            for (int y = 0; y < DisplayHeight; y++)
            {
                for (int x = 0; x < (DisplayWidth + 7) / 8; x++)
                {
                    commands.AddRange(SendDataByte(red[y, x]));
                }
            }
            commands.AddRange(SendCommandByte(CommandKind.PartialOut));
            return commands.ToArray();
        }

        public DisplayCommand[] TurnOff()
        {
            // EPD_2IN13BC_Sleep
            List<DisplayCommand> commands = new List<DisplayCommand>();
            commands.Add(new DebugMessageCommand { Message = "Deep sleep" });
            commands.AddRange(SendCommandByte(CommandKind.PowerOff));
            commands.AddRange(WaitForIdle());
            commands.AddRange(SendCommandByte(CommandKind.DeepSleep));
            commands.AddRange(SendDataByte(0xA5));
            return commands.ToArray();
        }

        /// <summary>
        /// Sending a command by its name.
        /// </summary>
        /// <exception cref="ArgumentException">when the command is unknown</exception>
        public DisplayCommand[] SendCommandByte(string commandName)
        {
            if (Enum.TryParse(commandName, true, out CommandKind command))
            {
                return SendCommandByte(command);
            }
            else
            {
                throw new ArgumentException($"Unknown command: {commandName}");
            }
        }

        /// <summary>
        /// See EPD_2IN13BC_SendCommand()
        /// </summary>
        private DisplayCommand[] SendCommandByte(CommandKind command) =>
            DisplayCommand.Parse($"SendCommand {(byte)command}");

        // See EPD_2IN13BC_SendData
        public DisplayCommand[] SendDataByte(byte data) =>
            DisplayCommand.Parse($"SendData {data}");

        public DisplayCommand[] WaitForIdle(uint milliseconds = 0) =>
            milliseconds > 0 ?
                DisplayCommand.Parse($"Sleep {milliseconds}", "WaitForIdle") :
                DisplayCommand.Parse("WaitForIdle");

        /// <summary>
        /// Hardware reset - after deep sleeping the display, need to call this to wake it up.
        /// See EPD_2IN13BC_Reset()
        /// </summary>
        public DisplayCommand[] HardwareReset => DisplayCommand.Parse(
            "SetGpio reset, 1",
            "Sleep 200",
            "SetGpio reset, 0",
            "Sleep 2",
            "SetGpio reset, 1",
            "Sleep 200",
            "WaitForIdle");
    }
}
