using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Waveshare.Helper;

namespace Waveshare
{

    /// <summary>
    /// This is an enum listing the colors supported by the Waveshare e-ink display.
    /// 
    /// The StringName attribute is used for parsing arguments in the console, 
    /// and RGBColor is used to convert colors expressed in RGB24 format (i.e., a loaded PNG image) to colors for the e-ink display.
    /// 
    /// StringName must be unique within this enum. Since there is a test case in 
    /// the unit tests that checks for uniqueness, make sure to run the tests if you add a new value.
    /// </summary>
    public enum PixelColor
    {
        [StringNames("black"), RGBColor("#000000")]
        Black = 0,
        [StringNames("white"), RGBColor("#FFFFFF")]
        White = 1,
        [StringNames("gray2"), RGBColor("#AAAAAA")]
        DarkGray = 2,
        [StringNames("gray1"), RGBColor("#555555")]
        LightGray = 3,
        [StringNames("red"), RGBColor("#FF0000")]
        Red = 4,
    }

    /// <summary>
    /// This is an enum listing the types of colors supported by the Waveshare e-ink display.
    /// Some types of displays can support multiple styles of colors.
    /// For example, a display that supports 4 gray levels can also support black & white.
    /// 
    /// This enum is sorted by the number of supported colors.
    /// Please keep the sorting order as it is used for internal optimization.
    /// Since there is a test case in the unit tests that checks the sorting order, make sure to run the tests if you modify this enum.
    /// </summary>
    public enum DisplayColorStyle
    {
        // 2 colors
        [PixelColors(PixelColor.Black, PixelColor.White)]
        BlackAndWhite,

        // 3 colors
        [PixelColors(PixelColor.Black, PixelColor.White, PixelColor.Red)]
        BlackAndWhiteAndRed,

        // 4 colors
        [PixelColors(PixelColor.Black, PixelColor.White, PixelColor.LightGray, PixelColor.DarkGray)]
        FourGray,

        // and more.
    }

    public enum RefreshMode
    {
        Full,
        Partial,
    }

    /// <summary>
    /// This is an enum listing the types of Waveshare e-ink displays.
    /// 
    /// StringName must be unique within this enum. Since there is a test case in 
    /// the unit tests that checks for uniqueness, make sure to run the tests if you add a new value.
    /// </summary>
    public enum DisplayKind
    {
        [StringNames("2.13bc")]
        Waveshare2_13bc,

        [StringNames("2.7v2")]
        Waveshare2_7HAT,

        [StringNames("7.5v2")]
        Waveshare7_5v2,
    }

    public enum RotationKind
    {
        [StringNames("none")]
        None,

        [StringNames("90", "cw90")]
        Rotate90,

        [StringNames("180")]
        Rotate180,

        [StringNames("270", "-90", "ccw90")]
        Rotate270,
    }

    /// <summary>
    /// This interface encapsulates the communication with Waveshare devices via GPIO and SPI.
    /// </summary>
    public interface IDisplayController
    {
        System.Device.Gpio.PinValue DeviceBusyValue { get; set; }

        void ExecuteCommands(IEnumerable<DisplayCommand> commands);
    }

    /// <summary>
    /// This interface is used to define the commands that can be sent to the Waveshare e-ink display.
    /// </summary>
    public interface IDisplayCommand
    {
        string Name { get; }
    }

    /// <summary>
    /// All device specific configurations are handled here.
    /// </summary>
    public interface IDisplayConfig
    {
        /// <summary>
        /// Human readable name of the display device.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Display width and height in pixels.
        /// To get this value, see "e-Paper" folder for your display in Waveshare's sample code or documentation.
        /// </summary>
        int DisplayWidth { get; }
        int DisplayHeight { get; }

        /// <summary>
        /// To get this value, see "e-Paper" folder for your display in Waveshare's sample code or documentation.
        /// The function name looks like EPD_WaitUntilIdle() or EPD_2IN13BC_ReadBusy()
        /// Note: each device seems have a different value for this.
        /// </summary>
        System.Device.Gpio.PinValue BusyValue { get; }

        bool CanPartialRefresh { get; }

        /// <summary>
        /// Some devices have buttons. e.g. 2.7 HAT does.
        /// Return null if there is no buttons.
        /// </summary>
        int[]? ButtonPins { get; }


        /// <summary>
        /// Color styles supported by the display device.
        /// This list is sorted by it's supported colors.
        /// The first one is black and white, and the last one is the most colorful one.
        /// </summary>
        DisplayColorStyle[] ColorStyles { get; }

        /// <summary>
        /// Generating commands for initializing display for full refresh.
        /// Some devices need to use different commands for different color styles.
        /// </summary>
        DisplayCommand[] Initialize(DisplayColorStyle style);

        /// <summary>
        /// Generating commands for initializing display for partial refresh.
        /// Some devices doesn't support this feature.
        /// </summary>
        DisplayCommand[] InitializePartial(DisplayColorStyle style, int x, int y, int width, int height);

        /// <summary>
        /// Commands for sending image data
        /// Some devices need to initialize again to send a different color style.
        /// </summary>
        /// <param name="style"></param>
        /// <param name="image">Sequence is [height, width]</param>
        /// <returns></returns>
        DisplayCommand[] SendDisplayData(DisplayColorStyle style, PixelColor[,] image);

        /// <summary>
        /// Commands for refreshing screen.
        /// </summary>
        /// <param name="colorStyle">Some devices need to use different commands for different color styles.</param>
        /// <param name="refreshMode">Only a few devices supports partial refresh</param>
        DisplayCommand[] Refresh(DisplayColorStyle colorStyle, RefreshMode refreshMode);

        /// <summary>
        /// Commands for shutting down the display device. Actually it is deep sleep mode.
        /// Need to Initialize again to wake up.
        /// </summary>
        DisplayCommand[] TurnOff();
    }

}
