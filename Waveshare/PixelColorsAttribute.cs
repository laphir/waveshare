using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Waveshare
{
    /// <summary>
    /// For mapping DisplayColorStyle to specific device colors
    /// </summary>
    internal class PixelColorsAttribute : Attribute
    {
        public PixelColor[] Colors { get; }

        public PixelColorsAttribute(params PixelColor[] colors)
        {
            Colors = colors;
        }
    }
}
