using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waveshare.Helper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Waveshare.Config
{
    /// <summary>
    /// Waveshare 2.7 inch HAT v2 display configuration.
    /// </summary>
    internal class WaveshareConfig_2_7HATv2 : IDisplayConfig
    {
        public string Name => "Waveshare 2.7 inch HAT v2";

        // This is referenced as X in the Waveshare documentation.
        public int DisplayWidth => 176;
        // This is referenced as Y in the Waveshare documentation.
        public int DisplayHeight => 264;

        public System.Device.Gpio.PinValue BusyValue => System.Device.Gpio.PinValue.High;

        public bool CanPartialRefresh => true;

        // TODO: update this value
        public int[]? ButtonPins => null;

        public DisplayColorStyle[] ColorStyles => new[] { DisplayColorStyle.BlackAndWhite, DisplayColorStyle.FourGray };


        // TODO: 4 buttons on the board
        // P5, P6, P13, P19

        // See Waveshare's documentation
        enum CommandKind : byte
        {
            DriverOutputControl = 0x01,         // then, write 3 bytes of data
            GateDrivingVoltageControl = 0x03,   // then, write 1 byte of data
            SourceDrivingVoltageControl = 0x04, // then, write 3 bytes of data
            InitialCodeSetting = 0x08,          // no additional data
            WriteRegisterForInitialCode = 0x09, // then, write 4 bytes of data
            ReadRegisterForInitialCode = 0x0A,  // no additional data
            BoosterSoftStartControl = 0x0C,     // then, write 4 bytes of data
            DeepSleepMode = 0x10,               // then, write 1 byte of data
            DataEntryModeSetting = 0x11,        // then, write 1 byte of data
            SwReset = 0x12,                     // no additional data
            HVReadyDetection = 0x14,            // then, write 1 byte of data
            VCIDetection = 0x15,                // then, write 1 byte of data
            TemperatureSensorSelection = 0x18,  // then, write 1 byte of data
            TemperatureSensorWrite = 0x1A,      // then, write 1 byte of data
            TemperatureSensorRead = 0x1B,       // then, read 1 byte of data
            TemperatureSensorWriteExternal = 0x1C, // then, write 3 bytes of data
            ICRevisionRead = 0x1F,              // then, read 1 byte of data
            MasterActivation = 0x20,            // no additional data
            DisplayUpdateControl1 = 0x21,       // then, write 2 bytes of data
            DisplayUpdateControl2 = 0x22,       // then, write 1 byte of data
            WriteRamWhite = 0x24,               // then, write multiple bytes of data, until another command is sent. White = 1, Black = 0.
            WriteRamRed = 0x26,                 // then, write multiple bytes of data, until another command is sent. Red = 1, non red = 0.
            ReadRam = 0x27,                     // then, read multiple bytes of data, until another command is sent.
            VcomSense = 0x28,                   // no additional data
            VcomSenseDuration = 0x29,           // then, write 1 byte of data
            ProgramVcomOtp = 0x2A,              // no additional data
            WriteVcomRegister = 0x2c,           // then, write 1 byte of data
            OtpRegisterRead = 0x2D,             // then, read 11 bytes of data
            UserIDRead = 0x2E,                  // then, read 10 bytes of data
            StatusBitRead = 0x2F,               // then, read 1 byte of data
            ProgramWSOtp = 0x30,                // no additional data
            LoadWSOtp = 0x31,                   // no additional data
            WriteLutRegister = 0x32,            // then, write 227 bytes. See document 6.7
            CRCCalculation = 0x34,              // no additional data
            CRCStatusRead = 0x35,               // then, read 2 bytes of data
            ProgramOTPSelection = 0x36,         // no additional data
            WriteRegisterForDisplay = 0x37,     // then, write 10 bytes of data
            WriteRegisterForUserID = 0x38,      // then, write 10 bytes of data
            OTPProgramMode = 0x39,              // then, write 1 byte of data
            BorderWaveformControl = 0x3C,       // then, write 1 byte of data
            EndOption = 0x3F,                   // then, write 1 byte of data
            ReadRamOption = 0x41,               // then, write 1 byte of data
            SetRamXAddress = 0x44,              // then, write 2 bytes of data
            SetRamYAddress = 0x45,              // then, write 4 bytes of data
            AutoWriteRedRam = 0x46,             // then, write 1 byte of data
            AutoWriteBlackRam = 0x47,           // then, write 1 byte of data
            SetRamXCounter = 0x4E,              // then, write 1 byte of data
            SetRamYCounter = 0x4F,              // then, write 2 bytes of data
            Nop = 0x7F,                         // no additional data
        }

        #region Raw commands
        /// <summary>
        /// Hardware reset - after deep sleeping the display, need to call this to wake it up.
        /// See EPD_2IN7_V2_Reset()
        /// </summary>
        public DisplayCommand[] HardwareReset => DisplayCommand.Parse(
            "SetGpio reset, 1",
            "Sleep 20",
            "SetGpio reset, 0",
            "Sleep 2",
            "SetGpio reset, 1",
            "Sleep 20",
            "WaitForIdle");

        /// <summary>
        /// Waveshare documentation says we need to send SWReset command after hardware reset.
        /// Waveshare documentation, chapter 14.1 requires Sleep 10 msec, after sending command.
        /// </summary>
        public DisplayCommand[] SoftwareReset =>
            SendCommandByte("SwReset")
            .Concat(DisplayCommand.Parse("Sleep 10", "WaitForIdle")).ToArray();

        public DisplayCommand[] DriverOutputControl =>
            SendCommandByte("DriverOutputControl")
            .Concat(SendDataByte(0x07))     // MUX Gate lines setting - 0x107 == 263 == DeviceHeight - 1
            .Concat(SendDataByte(0x01))
            .Concat(SendDataByte(0x00)).ToArray();

        /// <summary>
        /// Sending a byte command.
        /// See also: SendCommandByte()
        /// </summary>
        public DisplayCommand[] SendCommandRawByte(byte command)
        {
            return SendCommandByte(command);
        }

        /// <summary>
        /// Sending a command by its name.
        /// </summary>
        /// <exception cref="ArgumentException">when the command is unknown</exception>
        public DisplayCommand[] SendCommandByte(string commandName)
        {
            if (Enum.TryParse(commandName, true, out CommandKind command))
            {
                return SendCommandByte((byte)command);
            }
            else
            {
                throw new ArgumentException($"Unknown command: {commandName}");
            }
        }


        /// <summary>
        /// See EPD_2IN7_V2_SendCommand()
        /// </summary>
        private DisplayCommand[] SendCommandByte(byte command) =>
            DisplayCommand.Parse($"SendCommand {command}");

        // See EPD_2IN7_V2_SendData
        public DisplayCommand[] SendDataByte(byte data) =>
            DisplayCommand.Parse($"SendData {data}");

        public DisplayCommand[] SetRamXAddressAsFullWidth => SetRamXAddress(0, DisplayWidth);
        public DisplayCommand[] SetRamXAddress(int start, int end)
        {
            if (start < 0)
            {
                throw new ArgumentException($"start({start}) must be greater than or equal to 0.");
            }
            if (end > DisplayWidth)
            {
                throw new ArgumentException($"end({end}) must be less than DisplayWidth({DisplayWidth}).");
            }
            if (start > end)
            {
                throw new ArgumentException($"start({start}) must be less than or equal to end({end}).");
            }

            // if end was 176, we need to send 21.
            start /= 8;
            end = (end + 7) / 8;
            if (end > 0)
            {
                end--;
            }

            return SendCommandByte("SetRamXAddress")
                .Concat(SendDataByte((byte)(start & 0xFF)))
                .Concat(SendDataByte((byte)(end & 0xFF))).ToArray();
        }

        public DisplayCommand[] SetRamYAddressAsFullHeight => SetRamYAddress(0, DisplayHeight);
        public DisplayCommand[] SetRamYAddress(int start, int end)
        {
            if (start < 0)
            {
                throw new ArgumentException($"start({start}) must be greater than or equal to 0.");
            }
            if (end > DisplayHeight)
            {
                throw new ArgumentException($"end({end}) must be less than DisplayHeight({DisplayHeight}).");
            }
            if (start > end)
            {
                throw new ArgumentException($"start({start}) must be less than or equal to end({end}).");
            }

            if (end > 0)
            {
                end--;
            }

            return SendCommandByte("SetRamYAddress")
                .Concat(SendDataByte((byte)(start & 0xFF)))
                .Concat(SendDataByte((byte)(start >> 8 & 0x01)))
                .Concat(SendDataByte((byte)(end & 0xFF)))
                .Concat(SendDataByte((byte)(end >> 8 & 0x01))).ToArray();
        }

        public DisplayCommand[] SetRamXCounter(int position)
        {
            if (position < 0)
            {
                throw new ArgumentException($"position({position}) must be greater than or equal to 0.");
            }
            if (position > DisplayWidth)
            {
                throw new ArgumentException($"position({position}) must be less than DisplayWidth({DisplayWidth}).");
            }

            position /= 8;
            return SendCommandByte("SetRamXCounter")
                .Concat(SendDataByte((byte)(position & 0xFF))).ToArray();
        }

        public DisplayCommand[] SetRamYCounter(int position)
        {
            if (position < 0)
            {
                throw new ArgumentException($"position({position}) must be greater than or equal to 0.");
            }
            if (position > DisplayHeight)
            {
                throw new ArgumentException($"position({position}) must be less than DisplayHeight({DisplayHeight}).");
            }

            return SendCommandByte("SetRamYCounter")
                .Concat(SendDataByte((byte)(position & 0xFF)))
                .Concat(SendDataByte((byte)(position >> 8 & 0x01))).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xIncrement">true: increase in X direction. Usually true</param>
        /// <param name="yIncrement">true: increase in Y direction. Usually true</param>
        /// <param name="followY">false: follow X axis, true: follow Y axis. Usually false</param>
        /// <returns></returns>
        public DisplayCommand[] SetDataEntryMode(bool xIncrement = true, bool yIncrement = true, bool followY = false)
        {
            byte data = 0;
            if (xIncrement)
            {
                data |= 0x01;
            }
            if (yIncrement)
            {
                data |= 0x02;
            }
            if (followY)
            {
                data |= 0x04;
            }

            return SendCommandByte("DataEntryModeSetting")
                .Concat(SendDataByte(data)).ToArray();
        }

        /// <summary>
        /// Seems we need to send LUT after this command.
        /// </summary>
        public DisplayCommand[] SetBorderWaveForm(byte value) =>
            SendCommandByte("BorderWaveformControl")
            .Concat(SendDataByte(value)).ToArray();

        public DisplayCommand[] SetVComVoltage(byte value) =>
            SendCommandByte("WriteVcomRegister")
            .Concat(SendDataByte(value)).ToArray();

        public DisplayCommand[] SetGateDrivingVoltage(byte value) =>
            SendCommandByte("GateDrivingVoltageControl")
            .Concat(SendDataByte(value)).ToArray();

        public DisplayCommand[] SetSourceDrivingVoltage(byte a, byte b, byte c) =>
            SendCommandByte("SourceDrivingVoltageControl")
            .Concat(SendDataByte(a))
            .Concat(SendDataByte(b))
            .Concat(SendDataByte(c)).ToArray();

        public DisplayCommand[] SetEndOption(byte value) =>
            SendCommandByte("EndOption")
            .Concat(SendDataByte(value)).ToArray();

        public DisplayCommand[] SendLUT()
        {
            List<DisplayCommand> commands = new List<DisplayCommand>();
            commands.AddRange(SendCommandByte("WriteLutRegister"));

            // This table is from LUT_DATA_4Gray
            byte[] lut = new byte[]
            {
                0x40,   0x48,   0x80,   0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
                0x8,    0x48,   0x10,   0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
                0x2,    0x48,   0x4,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
                0x20,   0x48,   0x1,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
                0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
                0xA,    0x19,   0x0,    0x3,    0x8,    0x0,    0x0,
                0x14,   0x1,    0x0,    0x14,   0x1,    0x0,    0x3,
                0xA,    0x3,    0x0,    0x8,    0x19,   0x0,    0x0,
                0x1,    0x0,    0x0,    0x0,    0x0,    0x0,    0x1,
                0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
                0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
                0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
                0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
                0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
                0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
                0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
                0x0,    0x0,    0x0,    0x0,    0x0,    0x0,    0x0,
                0x22,   0x22,   0x22,   0x22,   0x22,   0x22,   0x0,    0x0,    0x0,
            };

            foreach (var b in lut)
            {
                commands.Add(new SendDataCommand { SingleData = b });
            }

            return commands.ToArray();
        }

        public DisplayCommand[] RefreshScreen_B1 =>
            SendCommandByte("DisplayUpdateControl2")
            .Concat(SendDataByte(0xB1))
            .Concat(SendCommandByte("MasterActivation"))
            .Concat(DisplayCommand.Parse("WaitForIdle")).ToArray();

        /// <summary>
        /// Use this for refreshing 4 gray image
        /// </summary>
        public DisplayCommand[] RefreshScreen_C7 =>
            SendCommandByte("DisplayUpdateControl2")
            .Concat(SendDataByte(0xC7))
            .Concat(SendCommandByte("MasterActivation"))
            .Concat(DisplayCommand.Parse("WaitForIdle")).ToArray();

        /// <summary>
        /// Use this for refreshing black and white image, full refresh
        /// </summary>
        public DisplayCommand[] RefreshScreen_F7 =>
            SendCommandByte("DisplayUpdateControl2")
            .Concat(SendDataByte(0xF7))
            .Concat(SendCommandByte("MasterActivation"))
            .Concat(DisplayCommand.Parse("WaitForIdle")).ToArray();

        /// <summary>
        /// Use this for refreshing black and white image, partial refresh
        /// </summary>
        public DisplayCommand[] RefreshScreen_FF =>
            SendCommandByte("DisplayUpdateControl2")
            .Concat(SendDataByte(0xFF))
            .Concat(SendCommandByte("MasterActivation"))
            .Concat(DisplayCommand.Parse("WaitForIdle")).ToArray();

        /// <summary>
        /// Select temperature sensor.
        /// </summary>
        /// <param name="isExternal">true: external, false: internal</param>
        public DisplayCommand[] SelectTemperatureSensor(bool isExternal) =>
            SendCommandByte("TemperatureSensorSelection")
            .Concat(SendDataByte(isExternal ? (byte)0x48 : (byte)0x80)).ToArray();

        /// <summary>
        /// Tried but always returning 0x00. Not working??
        /// </summary>
        public DisplayCommand[] ReadTemperatureSensor(uint cookie) =>
            DisplayCommand.Parse($"ReadData {(int)CommandKind.TemperatureSensorRead} {cookie}");
        #endregion

        public DisplayCommand[] Initialize(DisplayColorStyle style)
        {
            if (style == DisplayColorStyle.FourGray)
            {
                return Initialize4Gray();
            }
            else if (style == DisplayColorStyle.BlackAndWhite)
            {
                return InitializeBlackAndWhite();
            }
            else
            {
                throw new NotSupportedException($"Color style {style} is not supported on {Name}.");
            }
        }

        private DisplayCommand[] InitializeBlackAndWhite()
        {
            // See EPD_2IN7_V2_Init()
            List<DisplayCommand> commands = new List<DisplayCommand>();
            commands.Add(new DebugMessageCommand { Message = "Resetting device" });
            commands.AddRange(HardwareReset);
            commands.AddRange(SoftwareReset);

            commands.Add(new DebugMessageCommand { Message = "Initializing device for black and white" });
            commands.AddRange(SetRamXAddressAsFullWidth);   // 0x44
            commands.AddRange(SetRamYAddressAsFullHeight);  // 0x45
            commands.AddRange(SetDataEntryMode());          // 0x11
            commands.AddRange(SetRamXCounter(0));           // 0x4E
            commands.AddRange(SetRamYCounter(0));           // 0x4F
            return commands.ToArray();
        }

        private DisplayCommand[] Initialize4Gray()
        {
            // See EPD_2IN7_V2_Init_4GRAY()
            List<DisplayCommand> commands = new List<DisplayCommand>();
            commands.Add(new DebugMessageCommand { Message = "Resetting device" });
            commands.AddRange(HardwareReset);
            commands.AddRange(SoftwareReset);

            commands.Add(new DebugMessageCommand { Message = "Initializing device for 4gray" });
            // These are undocumented commands, so commented out.
            // 0x74 - set analog block control       
            // 0x7E - set digital block control          

            commands.AddRange(DriverOutputControl);         // 0x01
            commands.AddRange(SetDataEntryMode());          // 0x11

            commands.AddRange(SetRamXAddressAsFullWidth);   // 0x44
            commands.AddRange(SetRamYAddressAsFullHeight);  // 0x45
            commands.AddRange(SetDataEntryMode());          // 0x11

            commands.AddRange(SetBorderWaveForm(0));        // 0x3C - 0x00: LUT0

            commands.AddRange(SetVComVoltage(0x1C));        // 0x2C - 0x1C: -0.7V
            commands.AddRange(SetEndOption(0x22));          // 0x3F - 0x22: Normal
            commands.AddRange(SetGateDrivingVoltage(0x17)); // 0x03 - 0x17: 20V
            commands.AddRange(SetSourceDrivingVoltage(0x41, 0x00, 0x32));

            commands.AddRange(SendLUT());
            commands.AddRange(SetRamXCounter(0));           // 0x4E
            commands.AddRange(SetRamYCounter(0));           // 0x4F
            return commands.ToArray();
        }

        public DisplayCommand[] InitializePartial(DisplayColorStyle style, int x, int y, int width, int height)
        {
            // See EPD_2IN7_V2_Display_Partial

            // Adjust x, y, width and height to be multiple of 8
            if (x % 8 != 0)
            {
                width += x % 8;
                x -= x % 8;
            }
            if (width % 8 != 0)
            {
                width += 8 - (width % 8);
            }

            // argument check
            if (x < 0)
            {
                throw new ArgumentException($"x({x}) must be greater than or equal to 0.");
            }
            if (y < 0)
            {
                throw new ArgumentException($"y({y}) must be greater than or equal to 0.");
            }
            if (x + width > DisplayWidth)
            {
                throw new ArgumentException($"x({x}) + width({width}) must be less than DisplayWidth({DisplayWidth}).");
            }
            if (y + height > DisplayHeight)
            {
                throw new ArgumentException($"y({y}) + height({height}) must be less than DisplayHeight({DisplayHeight}).");
            }

            List<DisplayCommand> commands = new List<DisplayCommand>();
            commands.Add(new DebugMessageCommand { Message = "Resetting device" });
            commands.AddRange(HardwareReset);
            commands.AddRange(SoftwareReset);

            commands.Add(new DebugMessageCommand { Message = "Initializing device for black and white partial refresh" });
            commands.AddRange(SetBorderWaveForm(0x80));

            commands.AddRange(SetRamXAddress(x, x + width));   // 0x44
            commands.AddRange(SetRamYAddress(y, y + height));  // 0x45
            commands.AddRange(SetRamXCounter(x));           // 0x4E
            commands.AddRange(SetRamYCounter(y));           // 0x4F
            return commands.ToArray();
        }

        public DisplayCommand[] SendDisplayData(DisplayColorStyle style, PixelColor[,] image)
        {
            if (image.GetLength(0) != DisplayHeight || image.GetLength(1) != DisplayWidth)
            {
                throw new ArgumentException($"Image size must be [{DisplayHeight}, {DisplayWidth}]");
            }

            // Change DisplayColors to packed bytes and transmit to device.
            if (style == DisplayColorStyle.FourGray)
            {
                List<DisplayCommand> lowChannel = new List<DisplayCommand>();
                List<DisplayCommand> highChannel = new List<DisplayCommand>();
                lowChannel.AddRange(SendCommandByte("WriteRamWhite"));
                lowChannel.Add(new DebugMessageCommand { Message = "Uploading low bits" });
                highChannel.AddRange(SendCommandByte("WriteRamRed"));
                lowChannel.Add(new DebugMessageCommand { Message = "Uploading high bits" });

                for (int y = 0; y < DisplayHeight; y++)
                {
                    var (low, high) = ImageHelper.PackRowAs4Gray(image, y);
                    foreach (var b in low)
                    {
                        lowChannel.Add(new SendDataCommand { SingleData = b });
                    }
                    foreach (var b in high)
                    {
                        highChannel.Add(new SendDataCommand { SingleData = b });
                    }
                }

                lowChannel.AddRange(highChannel);
                return lowChannel.ToArray();
            }
            else if (style == DisplayColorStyle.BlackAndWhite)
            {
                List<DisplayCommand> commands = new List<DisplayCommand>();
                commands.AddRange(SendCommandByte("WriteRamWhite"));
                commands.Add(new DebugMessageCommand { Message = "Uploading image data" });

                for (int y = 0; y < DisplayHeight; y++)
                {
                    var packedRow = ImageHelper.PackRowAsBlackAndWhite(image, y);
                    foreach (var b in packedRow)
                    {
                        commands.Add(new SendDataCommand { SingleData = b });
                    }
                }
                return commands.ToArray();
            }
            else
            {
                throw new NotSupportedException($"Color style {style} is not supported on {Name}.");
            }
        }

        public DisplayCommand[] Refresh(DisplayColorStyle style, RefreshMode refreshMode)
        {
            List<DisplayCommand> commands = new List<DisplayCommand>();

            if (style == DisplayColorStyle.FourGray)
            {
                commands.Add(new DebugMessageCommand { Message = "Refreshing screen for 4gray" });
                commands.AddRange(RefreshScreen_C7);

                if (refreshMode != RefreshMode.Full)
                {
                    throw new NotImplementedException($"Partial refresh is not implemented on {Name}.");
                }
            }
            else if (style == DisplayColorStyle.BlackAndWhite)
            {
                switch (refreshMode)
                {
                    case RefreshMode.Full:
                        commands.Add(new DebugMessageCommand { Message = "Fully refreshing screen for black and white" });
                        commands.AddRange(RefreshScreen_F7);
                        break;
                    case RefreshMode.Partial:
                        commands.Add(new DebugMessageCommand { Message = "Partial refreshing screen for black and white" });
                        commands.AddRange(RefreshScreen_FF);
                        break;
                    default:
                        throw new NotSupportedException($"Refresh mode {refreshMode} is not supported on {Name}.");
                }
            }
            else
            {
                throw new NotSupportedException($"Color style {style} is not supported on {Name}.");
            }

            return commands.ToArray();
        }

        /// <summary>
        /// 2.7 makes the device go into deep sleep mode.
        /// </summary>
        public DisplayCommand[] TurnOff()
        {
            List<DisplayCommand> commands = new List<DisplayCommand>();
            commands.Add(new DebugMessageCommand { Message = "Deep sleep" });
            commands.AddRange(SendCommandByte("DeepSleepMode"));
            commands.AddRange(SendDataByte(0x01));
            return commands.ToArray();
        }
    }
}
