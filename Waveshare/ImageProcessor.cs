using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waveshare.Helper;

namespace Waveshare
{
    public class ImageProcessor : IDisposable
    {
        #region IDisposable Support
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                    bitmap?.Dispose();
                    bitmap = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ImageProcessor()
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

        SKBitmap? bitmap;
        public int Width => bitmap?.Width ?? 0;
        public int Height => bitmap?.Height ?? 0;

        public static ImageProcessor Load(string filename)
        {
            return new ImageProcessor(SKBitmap.Decode(filename));
        }

        public ImageProcessor(SKBitmap bitmap)
        {
            this.bitmap = bitmap;
        }

        public void Rotate(RotationKind rotationKind)
        {
            if (bitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            if (rotationKind == RotationKind.None)
            {
                return;
            }

            // Rotate image
            // Read pixel by pixel and rotate the image
            SKBitmap? rotated = null;
            try
            {
                if (rotationKind == RotationKind.Rotate90 || rotationKind == RotationKind.Rotate270)
                {
                    rotated = new SKBitmap(Height, Width, bitmap.ColorType, bitmap.AlphaType);
                }
                else
                {
                    rotated = new SKBitmap(Width, Height, bitmap.ColorType, bitmap.AlphaType);
                }

                // I'm not using SKMatrix for rotation, because this operation should be pixel perfect.
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        var color = bitmap.GetPixel(x, y);
                        switch (rotationKind)
                        {
                            case RotationKind.Rotate90:
                                rotated.SetPixel(Height - y - 1, x, color);
                                break;
                            case RotationKind.Rotate270:
                                rotated.SetPixel(y, Width - x - 1, color);
                                break;
                            case RotationKind.Rotate180:
                                rotated.SetPixel(Width - x - 1, Height - y - 1, color);
                                break;
                        }
                    }
                }

                bitmap.Dispose();
                bitmap = rotated;
                rotated = null;
            }
            finally
            {
                // Make sure rotated is disposed when there is an exception
                if (rotated != null)
                {
                    rotated.Dispose();
                }
            }
        }

        /// <summary>
        /// Expand the canvas to fit into given width and height.
        /// TODO: Support alignment.
        /// </summary>
        public void Expand(int width, int height, PixelColor fillColor)
        {
            if (bitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            if (width < Width || height < Height)
            {
                throw new ArgumentOutOfRangeException("Width and height must be greater than the original image.");
            }

            SKBitmap? expanded = null;
            try
            {
                expanded = new SKBitmap(width, height, bitmap.ColorType, bitmap.AlphaType);
                using (var canvas = new SKCanvas(expanded))
                {
                    canvas.Clear(fillColor.ToSKColor());

                    // TODO: Check if DrawBitmap is pixel perfect
                    int dx = (width - Width) / 2;
                    int dy = (height - Height) / 2;
                    canvas.DrawBitmap(bitmap, dx, dy);
                }

                bitmap.Dispose();
                bitmap = expanded;
                expanded = null;
            }
            finally
            {
                if (expanded != null)
                {
                    expanded.Dispose();
                }
            }
        }


        // TODO: RGB to GrayScale
        // public void ConvertGrayScale()

        // TODO: RGB / Grayscale to 1bpp
        // public void ConvertMonochrome()

        public PixelColor[,] ToPixels(DisplayColorStyle style)
        {
            if (bitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            var width = Width;
            var height = Height;
            var result = new PixelColor[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    if (color == SKColors.White)
                    {
                        result[y, x] = PixelColor.White;
                    }
                    else
                    {
                        result[y, x] = color.ToPixelColor(style);
                    }
                }
            }

            return result;
        }

        public void Save(string savePath)
        {
            if (bitmap == null)
            {
                throw new InvalidOperationException("No image loaded.");
            }

            using (var image = SKImage.FromBitmap(bitmap))
            using (var data = image.Encode())
            using (var stream = System.IO.File.OpenWrite(savePath))
            {
                data.SaveTo(stream);
            }
        }
    }
}
