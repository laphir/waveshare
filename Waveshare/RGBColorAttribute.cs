using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Waveshare
{
    internal class RGBColorAttribute : Attribute
    {
        public SKColor Color { get; }

        public RGBColorAttribute(string hex)
        {
            Color = SKColor.Parse(hex);
        }
    }
}
