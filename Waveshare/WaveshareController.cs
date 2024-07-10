using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waveshare.Helper;

namespace Waveshare
{
    internal class WaveshareController : IDisposable, IDisplayController
    {
        private GpioController? _controller;
        private SpiDevice? _spiDevice;

        // For raspberry PI gpio pin numbers
        public const int ResetPinNumber = 17;
        public const int DcPinNumber = 25;
        public const int CsPinNumber = 8;
        public const int PowerPinNumber = 18;
        public const int BusyPinNumber = 24;

        public WaveshareController()
        {
            _controller = new GpioController();

            InitializeGpio();

            // See DEV_Module_Init() from WaveShare's sample.
            var settings = new SpiConnectionSettings(0, 0);
            settings.DataFlow = DataFlow.MsbFirst;
            settings.Mode = SpiMode.Mode0;
            // WaveShare sample is using BCM2835_SPI_CLOCK_DIVIDER_128, It's 2 MHz ~ 3 MHz.
            // C#'s default is 500 kHz.
            settings.ClockFrequency = 2_000_000;
            settings.ChipSelectLineActiveState = PinValue.Low;
            _spiDevice = SpiDevice.Create(settings);
        }

        private void InitializeGpio()
        {
            Debug.WriteLine("WaveShareDisplay.Initialize()");

            // See DEV_GPIO_Init() from WaveShare's sample.
            _controller?.OpenPin(ResetPinNumber);
            _controller?.OpenPin(DcPinNumber);
            _controller?.OpenPin(CsPinNumber);
            _controller?.OpenPin(BusyPinNumber);
            _controller?.OpenPin(PowerPinNumber);

            _controller?.SetPinMode(ResetPinNumber, PinMode.Output);
            _controller?.SetPinMode(DcPinNumber, PinMode.Output);
            _controller?.SetPinMode(CsPinNumber, PinMode.Output);
            _controller?.SetPinMode(BusyPinNumber, PinMode.Input);
            _controller?.SetPinMode(PowerPinNumber, PinMode.Output);

            _controller?.Write(CsPinNumber, PinValue.High);
            _controller?.Write(PowerPinNumber, PinValue.High);
        }

        private void ShutdownGpio()
        {
            Debug.WriteLine("WaveShareDisplay.Shutdown()");

            // See DEV_Module_Exit() from WaveShare's sample.
            _controller?.Write(CsPinNumber, PinValue.Low);
            _controller?.Write(DcPinNumber, PinValue.Low);
            _controller?.Write(PowerPinNumber, PinValue.Low);
            _controller?.Write(ResetPinNumber, PinValue.Low);

            _controller?.ClosePin(ResetPinNumber);
            _controller?.ClosePin(DcPinNumber);
            _controller?.ClosePin(CsPinNumber);
            _controller?.ClosePin(BusyPinNumber);
            _controller?.ClosePin(PowerPinNumber);
        }

        #region IDisposable implementation
        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ShutdownGpio();

                    _spiDevice?.Dispose();
                    _spiDevice = null;

                    // dispose managed state (managed objects)
                    _controller?.Dispose();
                    _controller = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~WaveShareDisplay()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

        public PinValue DeviceBusyValue { get; set; } = PinValue.High;

        public void SpiWrite(byte data)
        {
            SpiWrite(new byte[] { data });
        }

        public void SpiWrite(byte[] data)
        {
            _spiDevice?.Write(data);
        }

        public byte? SpiRead()
        {
            return _spiDevice?.ReadByte();
        }

        /// <summary>
        /// Display reset pin
        /// </summary>
        public PinValue ResetValue
        {
            get => _controller?.Read(ResetPinNumber) ?? PinValue.Low;
            set => _controller?.Write(ResetPinNumber, value);
        }

        public PinValue SpiDcValue
        {
            get => _controller?.Read(DcPinNumber) ?? PinValue.Low;
            set => _controller?.Write(DcPinNumber, value);
        }

        public PinValue SpiCsValue
        {
            get => _controller?.Read(CsPinNumber) ?? PinValue.Low;
            set => _controller?.Write(CsPinNumber, value);
        }

        public PinValue BusyValue => _controller?.Read(BusyPinNumber) ?? PinValue.Low;

        public void ExecuteCommands(IEnumerable<DisplayCommand> commands)
        {
            foreach (var command in commands)
            {
                switch (command)
                {
                    case DebugMessageCommand debugMessageCommand:
                        Log.WriteLine(LogLevel.Debug, debugMessageCommand.Message);
                        break;

                    case WaitForIdleCommand _:
                        while (BusyValue == DeviceBusyValue)
                        {
                            Task.Delay(100).Wait();
                        }
                        break;

                    case SetGpioCommand setGpioCommand:
                        switch (setGpioCommand.PinName)
                        {
                            case GpioPinName.Reset:
                                ResetValue = setGpioCommand.Value ? PinValue.High : PinValue.Low;
                                break;
                            case GpioPinName.DC:
                                SpiDcValue = setGpioCommand.Value ? PinValue.High : PinValue.Low;
                                break;
                            case GpioPinName.CS:
                                SpiCsValue = setGpioCommand.Value ? PinValue.High : PinValue.Low;
                                break;
                            default:
                                throw new NotImplementedException($"Pin {setGpioCommand.PinName} is not implemented.");
                        }
                        break;

                    case SendCommandCommand sendCommandCommand:
                        SpiDcValue = PinValue.Low;
                        SpiCsValue = PinValue.Low;
                        SpiWrite(sendCommandCommand.Command);
                        SpiCsValue = PinValue.High;
                        break;

                    case SendDataCommand sendDataCommand:
                        SpiDcValue = PinValue.High;
                        SpiCsValue = PinValue.Low;
                        if (sendDataCommand.IsSingleByte)
                        {
                            SpiWrite(sendDataCommand.SingleData);
                        }
                        else
                        {
                            SpiWrite(sendDataCommand.MultipleData!);
                        }
                        SpiCsValue = PinValue.High;
                        break;

                    case ReadDataCommand readData:
                        SpiDcValue = PinValue.Low;
                        SpiCsValue = PinValue.Low;
                        SpiWrite(readData.Command);
                        SpiDcValue = PinValue.High;
                        readData.Value = SpiRead();
                        SpiCsValue = PinValue.High;
                        break;

                    case SleepCommand sleepCommand:
                        Task.Delay(sleepCommand.DurationMilliseconds).Wait();
                        break;

                    default:
                        throw new NotSupportedException($"Command {command} is not supported.");
                }
            }
        }
    }
}
