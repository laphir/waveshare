using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waveshare.Helper;

namespace Waveshare
{

    public class DisplayManager
    {
        public DisplayKind Kind { get; private set; }
        public IDisplayConfig? Config { get; private set; }
        public IDisplayController? Device { get; private set; }

        public int Width => Config!.DisplayWidth;
        public int Height => Config!.DisplayHeight;

        public DisplayColorStyle MaximumColorStyle => Config!.ColorStyles.Last();

        // Internal variables - store last used configuration
        private DisplayColorStyle? colorStyle;
        private RefreshMode? refreshMode;
        private int deferCount = 0;
        private List<DisplayCommand>? deferredCommands = null;

        public static IDisplayConfig CreateConfig(DisplayKind kind)
        {
            return kind switch
            {
                DisplayKind.Waveshare2_7HAT => new Config.WaveshareConfig_2_7HATv2(),
                DisplayKind.Waveshare2_13bc => new Config.WaveshareConfig_2_13bc(),
                DisplayKind.Waveshare7_5v2 => new Config.WaveshareConfig_7_5v2(),
                _ => throw new NotImplementedException($"Unknown display kind: {kind}"),
            };
        }

        public static DisplayManager CreateWaveshare(DisplayKind kind)
        {
            var controller = new DisplayManager();
            controller.Kind = kind;

            controller.Config = CreateConfig(kind);
            controller.Device = new WaveshareController();

            // Copy device configurations
            controller.Device.DeviceBusyValue = controller.Config.BusyValue;
            return controller;
        }

        // Asking configs
        public bool CanDisplayColor(PixelColor color)
        {
            // given color is displayable on the device.
            return Config!.ColorStyles.Any(style => style.SupportedColors().Contains(color));
        }

        /// <summary>
        /// Start accumulating screen updates.
        /// </summary>
        public void BeginDeferring()
        {
            if (++deferCount == 1)
            {
                deferredCommands = new List<DisplayCommand>();
                colorStyle = null;
                refreshMode = null;
            }
        }

        /// <summary>
        /// Stop accumulating screen updates and paint them on screen.
        /// Screen will be turned off after all deferred updates are executed.
        /// </summary>
        public void EndDeferring()
        {
            if (--deferCount == 0)
            {
                if (deferredCommands?.Count > 0)
                {
                    deferredCommands.AddRange(Config!.TurnOff());
                }

                ExecuteDeferredUpdates();

                // Clear all internal states
                deferredCommands = null;
                colorStyle = null;
                refreshMode = null;
            }
        }

        /// <summary>
        /// Flush all deferred updates to the display.
        /// </summary>
        public void ExecuteDeferredUpdates()
        {
            Device!.ExecuteCommands(deferredCommands!);
            deferredCommands?.Clear();
        }

        /// <summary>
        /// Clear screen with a given color
        /// </summary>
        /// <exception cref="ArgumentException">When color is not supported on the device</exception>
        public void ClearScreen(PixelColor color)
        {
            if (!CanDisplayColor(color))
            {
                throw new ArgumentException($"Color {color} is not supported by this display");
            }

            var requiredStyle = Config!.ColorStyles.Where(style => style.SupportedColors().Contains(color)).First();
            const RefreshMode requiredRefreshMode = RefreshMode.Full;

            BeginDeferring();

            // Initialize the display only when it is different from the last time.
            if (colorStyle != requiredStyle || refreshMode != requiredRefreshMode)
            {
                deferredCommands!.AddRange(Config!.Initialize(requiredStyle));
                colorStyle = requiredStyle;
                refreshMode = requiredRefreshMode;
            }
            var image = ImageHelper.FillColor(Config!.DisplayWidth, Config!.DisplayHeight, color);
            deferredCommands!.AddRange(Config!.SendDisplayData(requiredStyle, image));
            deferredCommands!.AddRange(Config!.Refresh(requiredStyle, requiredRefreshMode));
            EndDeferring();
        }

        public void DrawFullScreen(PixelColor[,] image, DisplayColorStyle requiredStyle)
        {
            const RefreshMode requiredRefreshMode = RefreshMode.Full;

            BeginDeferring();

            // Initialize the display only when it is different from the last time.
            if (colorStyle != requiredStyle || refreshMode != requiredRefreshMode)
            {
                deferredCommands!.AddRange(Config!.Initialize(requiredStyle));
                colorStyle = requiredStyle;
                refreshMode = requiredRefreshMode;
            }

            deferredCommands!.AddRange(Config!.SendDisplayData(requiredStyle, image));
            deferredCommands!.AddRange(Config!.Refresh(requiredStyle, requiredRefreshMode));
            EndDeferring();
        }
    }
}
