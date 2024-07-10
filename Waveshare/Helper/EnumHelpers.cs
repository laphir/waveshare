using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waveshare.Helper
{
    public static class EnumHelpers
    {
        static Lazy<Dictionary<string, DisplayKind>> stringToDisplayKinds =
            new Lazy<Dictionary<string, DisplayKind>>(() => MapStringNamesToValue<DisplayKind>());
        static Lazy<Dictionary<string, PixelColor>> stringToPixelColors =
            new Lazy<Dictionary<string, PixelColor>>(() => MapStringNamesToValue<PixelColor>());
        static Lazy<Dictionary<string, RotationKind>> stringToRotationKind =
            new Lazy<Dictionary<string, RotationKind>>(() => MapStringNamesToValue<RotationKind>());

        public static Dictionary<string, TEnum> MapStringNamesToValue<TEnum>()
            where TEnum : struct, Enum
        {
            var enumType = typeof(TEnum);
            var dict = new Dictionary<string, TEnum>();

            foreach (var value in Enum.GetValues<TEnum>())
            {
                var member = enumType.GetMember(value.ToString());
                var option = member[0].GetCustomAttributes(typeof(StringNamesAttribute), false);
                if (option.Length > 0)
                {
                    var attribute = (StringNamesAttribute)option[0];
                    foreach (var name in attribute.Names)
                    {
                        dict.Add(name, value);
                    }
                }
            }

            return dict;
        }

        public static string[] GetAllNamesForDisplayKind()
        {
            return stringToDisplayKinds.Value.Keys.ToArray();
        }

        public static string[] GetAllNamesForPixelColor()
        {
            return stringToPixelColors.Value.Keys.ToArray();
        }

        public static string[] GetAllNamesForRotationKind()
        {
            return stringToRotationKind.Value.Keys.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEnum">Enum that has StringNameAttribute</typeparam>
        /// <returns></returns>
        public static TEnum GetValueFromStringName<TEnum>(string name)
            where TEnum : struct, Enum
        {
            var enumType = typeof(TEnum);

            // Use cached dictionary for performance
            if (enumType == typeof(DisplayKind))
            {
                if (stringToDisplayKinds.Value.TryGetValue(name, out var value))
                {
                    // Object is required for casting to TEnum!
                    return (TEnum)(object)value;
                }
            }
            else if (enumType == typeof(PixelColor))
            {
                if (stringToPixelColors.Value.TryGetValue(name, out var value))
                {
                    // Object is required for casting to TEnum!
                    return (TEnum)(object)value;
                }
            }
            else if (enumType == typeof(RotationKind))
            {
                if (stringToRotationKind.Value.TryGetValue(name, out var value))
                {
                    // Object is required for casting to TEnum!
                    return (TEnum)(object)value;
                }
            }

            // Enumerate all values of enumType
            foreach (var value in Enum.GetValues<TEnum>())
            {
                var member = enumType.GetMember(value.ToString());
                var option = member[0].GetCustomAttributes(typeof(StringNamesAttribute), false);
                if (option.Length > 0)
                {
                    var attribute = (StringNamesAttribute)option[0];
                    if (attribute.Names.Contains(name))
                    {
                        return value;
                    }
                }
            }

            throw new ArgumentException($"{name} is not defined for type {enumType.Name}");
        }


        static Lazy<Dictionary<PixelColor, SKColor>> pixelColorToSKColors =
            new Lazy<Dictionary<PixelColor, SKColor>>(() => MapPixelColorToSKColor());

        public static Dictionary<PixelColor, SKColor> MapPixelColorToSKColor()
        {
            var enumType = typeof(PixelColor);
            var dict = new Dictionary<PixelColor, SKColor>();

            foreach (var value in Enum.GetValues<PixelColor>())
            {
                var member = enumType.GetMember(value.ToString());
                var option = member[0].GetCustomAttributes(typeof(RGBColorAttribute), false);
                if (option.Length > 0)
                {
                    var attribute = (RGBColorAttribute)option[0];
                    dict.Add(value, attribute.Color);
                }
            }

            return dict;
        }

        public static SKColor ToSKColor(this PixelColor color)
        {
            if (pixelColorToSKColors.Value.TryGetValue(color, out var skColor))
            {
                return skColor;
            }

            throw new ArgumentException($"{color} is not defined for type {typeof(PixelColor).Name}");
        }

        static Lazy<Dictionary<DisplayColorStyle, PixelColor[]>> displayColorStyleToPixelColors = 
            new Lazy<Dictionary<DisplayColorStyle, PixelColor[]>>(() => MapDisplayColorStyleToPixelColors());

        private static Dictionary<DisplayColorStyle, PixelColor[]> MapDisplayColorStyleToPixelColors()
        {
            var enumType = typeof(DisplayColorStyle);
            var dict = new Dictionary<DisplayColorStyle, PixelColor[]>();

            foreach (var value in Enum.GetValues<DisplayColorStyle>())
            {
                var member = enumType.GetMember(value.ToString());
                var option = member[0].GetCustomAttributes(typeof(PixelColorsAttribute), false);
                if (option.Length > 0)
                {
                    var attribute = (PixelColorsAttribute)option[0];
                    dict.Add(value, attribute.Colors);
                }
            }

            return dict;
        }

        public static PixelColor[] SupportedColors(this DisplayColorStyle style)
        {
            if (displayColorStyleToPixelColors.Value.TryGetValue(style, out var colors))
            {
                return colors;
            }

            throw new ArgumentException($"{style} is not defined for type {typeof(DisplayColorStyle).Name}");
        }
    }
}
