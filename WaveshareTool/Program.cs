using SkiaSharp;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.ComponentModel;
using Waveshare;
using Waveshare.Helper;

namespace WaveshareTool;
/// <summary>
/// How to add a new device:
///  * Add WaveshareConfig_* class - Note that there are at least 3 kinds of display controller. Please check command is same as the new display.
///  * Add DisplayKind enum, and add new abbreviation for the display as CmdOption
///  * Update DisplayManager.CreateWaveshare method
/// </summary>

class Program
{
    public static async Task Main(string[] args)
    {
        var rootCommand = new RootCommand("Waveshare refreshing app");
        var deviceOption = new Option<string>("--device", "Specify Waveshare device")
            .FromAmong(EnumHelpers.GetAllNamesForDisplayKind());
        deviceOption.AddAlias("-d");
        rootCommand.AddGlobalOption(deviceOption);

        var verboseOption = new Option<bool>("--verbose", "Show verbose output");
        verboseOption.AddAlias("-v");
        rootCommand.AddGlobalOption(verboseOption);

        var clearCommand = new Command("clear", "Clear screen");
        var colorOption = new Option<string>("--color", () => "white", "Specify color to clear the screen")
            .FromAmong(EnumHelpers.GetAllNamesForPixelColor());
        clearCommand.AddOption(colorOption);
        clearCommand.SetHandler(
            (device, color, verbose) =>
            {
                ShowVerbose = verbose;
                ClearScreen(device, color);
            },
            deviceOption, colorOption, verboseOption);
        rootCommand.AddCommand(clearCommand);

        var drawCommand = new Command("draw", "Draw image on screen");
        var imageArgument = new Argument<string>("image", "Image file to draw");
        var saveOption = new Option<string>("--save", "Save image to file for debugging");
        var rotateOption = new Option<string>("--rotate", "Rotate image. CW=ClockWise, CCW=CounterClockWise")
            .FromAmong(EnumHelpers.GetAllNamesForRotationKind());
        drawCommand.AddArgument(imageArgument);
        drawCommand.AddOption(colorOption);
        drawCommand.AddOption(saveOption);
        drawCommand.AddOption(rotateOption);
        drawCommand.SetHandler(
            (imageFile, device, fillColor, savePath, rotation, verbose) =>
            {
                ShowVerbose = verbose;
                DrawImage(imageFile, device, fillColor, savePath, rotation);
            },
            imageArgument, deviceOption, colorOption, saveOption, rotateOption, verboseOption);
        rootCommand.AddCommand(drawCommand);

        var testCommand = new Command("test");
        testCommand.SetHandler( () => Test());
        rootCommand.AddCommand(testCommand);

        await rootCommand.InvokeAsync(args);
    }

    static bool ShowVerbose { get; set; } = false;

    internal static void LogLine(LogLevel level, string message)
    {
        Console.WriteLine(message);
    }

    private static Task DrawImage(string imageFile, string device, string fillColor, string savePath, string rotation)
    {
        if (!File.Exists(imageFile))
        {
            throw new ArgumentException($"File {imageFile} does not exist");
        }

        var backgroundColor = EnumHelpers.GetValueFromStringName<PixelColor>(fillColor);
        var displayKind = EnumHelpers.GetValueFromStringName<DisplayKind>(device);
        var rotationKind = EnumHelpers.GetValueFromStringName<RotationKind>(rotation);

        var display = DisplayManager.CreateWaveshare(displayKind);

        if (!display.CanDisplayColor(backgroundColor))
        {
            throw new ArgumentException($"Color {backgroundColor} is not supported by this display");
        }

        Log.WriteLine(LogLevel.Info, $"Loading image from {imageFile}");
        var processor = ImageProcessor.Load(imageFile);
        Log.WriteLine(LogLevel.Debug, $"Image is {processor.Width} x {processor.Height}");

        if (rotationKind != RotationKind.None)
        {
            Log.WriteLine(LogLevel.Debug, $"Rotating image {rotationKind}");
            processor.Rotate(rotationKind);
            Log.WriteLine(LogLevel.Debug, $"Image is now {processor.Width} x {processor.Height}");
        }

        if (processor.Width == display.Width && processor.Height == display.Height)
        {
            // noop
        }
        else if (processor.Width > display.Width || processor.Height > display.Height)
        {
            throw new NotSupportedException($"Image size {processor.Width}x{processor.Height} is too large for the display {display.Width}x{display.Height}");
        }
        else if (processor.Width <= display.Width || processor.Height <= display.Height)
        {
            // expand the image.
            Log.WriteLine(LogLevel.Debug, $"Expanding to fill screen {display.Width} x {display.Height}");
            processor.Expand(display.Width, display.Height, backgroundColor);
        }

        if (!string.IsNullOrEmpty(savePath))
        {
            Log.WriteLine(LogLevel.Debug, $"Saving image to {savePath}");
            processor.Save(savePath);
        }

        // Convert bitmap to pixel color array
        var image = processor.ToPixels(display.MaximumColorStyle);

        // Get the minimum required color style for the image
        // This is because some displays support faster drawing when we choose minimum color style.
        // E.g. 2.7 HAT v2 is 4 gray display, but if image is black and white, it draws twice faster.
        var minimumStyle = image.DetectMinimumColorStyle();
        Log.WriteLine(LogLevel.Debug, $"Detected color style of image is {minimumStyle}");

        display.DrawFullScreen(image, minimumStyle);
        return Task.CompletedTask;
    }

    private static Task ClearScreen(string device, string color)
    {
        var displayColor = EnumHelpers.GetValueFromStringName<PixelColor>(color);
        var displayKind = EnumHelpers.GetValueFromStringName<DisplayKind>(device);

        var display = DisplayManager.CreateWaveshare(displayKind);

        if (!display.CanDisplayColor(displayColor))
        {
            throw new ArgumentException($"Color {displayColor} is not supported by this display");
        }

        display.ClearScreen(displayColor);

        return Task.CompletedTask;
    }

    private static Task Test()
    {
        using SKBitmap sKBitmap = new SKBitmap(400, 200);
        using (SKCanvas sKCanvas = new SKCanvas(sKBitmap))
        {
            sKCanvas.Clear(SKColors.White);

            var paint = new SKPaint
            {
                Typeface = SKTypeface.FromFamilyName("LCD"),
                Color = SKColors.Black,
                IsAntialias = false,
                //LcdRenderText = true,
                //IsAutohinted = true,
                HintingLevel = SKPaintHinting.NoHinting,
                TextSize = 40f
            };

            sKCanvas.DrawText("0123456789 ABCDEFH", 100f, 100f, paint);

        }
        using var sKFileWStream = new SKFileWStream("test.png");
        sKBitmap.Encode(sKFileWStream, SKEncodedImageFormat.Png, 100);
        return Task.CompletedTask;
    }
}
