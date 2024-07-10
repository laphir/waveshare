using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Waveshare
{
    public abstract class DisplayCommand : IDisplayCommand
    {
        public abstract string Name { get; }

        public static DisplayCommand[] Parse(params string[] lines)
        {
            return Parse(lines.AsEnumerable());
        }

        public static DisplayCommand[] Parse(IEnumerable<string> lines)
        {
            var commands = new List<DisplayCommand>();
            foreach (var line in lines)
            {
                var parts = line.Split("\n\r ,".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                {
                    continue;
                }

                var commandName = parts[0];
                switch (commandName)
                {
                    case "SetGpio":
                        commands.Add(new SetGpioCommand
                        {
                            PinName = Enum.Parse<GpioPinName>(parts[1], true),
                            Value = ParsePinValue(parts[2]),
                        });
                        break;
                    case "Sleep":
                        commands.Add(new SleepCommand
                        {
                            DurationMilliseconds = int.Parse(parts[1]),
                        });
                        break;
                    case "SendCommand":
                        commands.Add(new SendCommandCommand
                        {
                            Command = byte.Parse(parts[1]),
                        });
                        break;
                    case "SendData":
                        commands.Add(new SendDataCommand
                        {
                            SingleData = byte.Parse(parts[1]),
                        });
                        break;
                    case "ReadData":
                        commands.Add(new ReadDataCommand
                        {
                            Command = byte.Parse(parts[1]),
                            Cookie = uint.Parse(parts[2]),
                        });
                        break;
                    case "WaitForIdle":
                        commands.Add(new WaitForIdleCommand());
                        break;
                    default:
                        throw new NotImplementedException($"Unknown command: {commandName}");
                }
            }

            return commands.ToArray();
        }

        private static bool ParsePinValue(string value)
        {
            if (value == "1" || value.ToLowerInvariant() == "high")
            {
                return true;
            }
            else if (value == "0" || value.ToLowerInvariant() == "low")
            {
                return false;
            }

            return bool.Parse(value);
        }
    }

    /// <summary>
    /// Print debug level messages on STD output.
    /// </summary>
    class DebugMessageCommand : DisplayCommand
    {
        public override string Name => "DebugMessage";
        public string Message { get; set; } = "";

        public override string ToString()
        {
            return $"{Name} {Message}";
        }
    }

    /// <summary>
    /// Sleeps few milliseconds.
    /// </summary>
    class SleepCommand : DisplayCommand
    {
        public override string Name => "Sleep";
        public int DurationMilliseconds { get; set; }

        public override string ToString()
        {
            return $"{Name} {DurationMilliseconds}";
        }
    }

    /// <summary>
    /// All of my Waveshare devices control only these pins.
    /// </summary>
    enum GpioPinName
    {
        Reset,
        DC,
        CS,
    }

    /// <summary>
    /// Control the GPIO pin. Setting High or Low value.
    /// </summary>
    class SetGpioCommand : DisplayCommand
    {
        public override string Name => "SetGpio";
        public GpioPinName PinName { get; set; }

        /// <summary>
        /// true: high value, false: low value
        /// </summary>
        public bool Value { get; set; }

        public override string ToString()
        {
            var value = Value ? "1" : "0";
            return $"{Name} {PinName} {value}";
        }
    }

    /// <summary>
    /// Send 1 byte command using SPI
    /// </summary>
    class SendCommandCommand : DisplayCommand
    {
        public override string Name => "SendCommand";
        public byte Command { get; set; }

        public override string ToString()
        {
            return $"{Name} 0x{Command.ToString("X2")}";
        }
    }

    /// <summary>
    /// Send 1 byte or multiple bytes of data using SPI.
    /// </summary>
    class SendDataCommand : DisplayCommand
    {
        public override string Name => "SendData";

        public bool IsSingleByte => MultipleData == null;
        public byte SingleData { get; set; }
        public byte[]? MultipleData { get; set; } = null;

        public override string ToString()
        {
            if (IsSingleByte)
            {
                // Single byte mode
                return $"{Name} 0x{SingleData.ToString("X2")}";
            }
            else
            {
                // Multiple byte mode
                var data = string.Join(", ", MultipleData!.Select(b => $"0x{b.ToString("X2")}"));
                return $"{Name} {data}";
            }
        }
    }

    /// <summary>
    /// Read 1 byte from SPI. This was testing purpose, currently not using this in normal operation.
    /// </summary>
    class ReadDataCommand : DisplayCommand
    {
        public override string Name => "ReadData";
        public uint Cookie { get; set; }
        public byte Command { get; set; }
        public byte? Value { get; set; }

        public override string ToString()
        {
            if (Value.HasValue)
            {
                return $"{Name} {Cookie} 0x{Value.Value.ToString("X2")}";
            }
            else
            {
                return $"{Name} {Cookie} NotRead";
            }
        }
    }

    /// <summary>
    /// Waiting for the device gets idle.
    /// GPIO idle pin becomes High or Low voltage when it becomes idle.
    /// Note that 2.13bc waits for Low, but 2.7HATv2 waits for High.
    /// Check the documentation for your waveshare device.
    /// </summary>
    class WaitForIdleCommand : DisplayCommand
    {
        public override string Name => "WaitForIdle";

        public override string ToString()
        {
            return Name;
        }
    }
}
