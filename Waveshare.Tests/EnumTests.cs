using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waveshare.Helper;

namespace Waveshare.Tests
{
    public class EnumTests
    {
        [Fact]
        public void TestStringNameAttribute()
        {
            var a = EnumHelpers.GetValueFromStringName<DisplayKind>("2.13bc");
            Assert.Equal(DisplayKind.Waveshare2_13bc, a);
        }

        [Fact]
        public void TestDisplayKindHasStringNames()
        {
            var kindType = typeof(DisplayKind);
            var fields = kindType.GetFields();
            HashSet<string> abbreviations = new();

            foreach (var field in fields)
            {
                if (field.FieldType != kindType)
                {
                    continue;
                }

                var option = field.GetCustomAttribute<StringNamesAttribute>();
                Assert.NotNull(option);
                Assert.NotEmpty(option.Names);

                // Check if abbreviation is unique
                foreach (var abbreviation in option.Names)
                {
                    Assert.DoesNotContain(abbreviation, abbreviations);
                    abbreviations.Add(abbreviation);
                }
            }
        }

        [Fact]
        public void TestPixelColorHasStringName()
        {
            var colorsType = typeof(PixelColor);
            var fields = colorsType.GetFields();
            HashSet<string> abbreviations = new();

            foreach (var field in fields)
            {
                if (field.FieldType != colorsType)
                {
                    continue;
                }

                var option = field.GetCustomAttribute<StringNamesAttribute>();
                Assert.NotNull(option);
                Assert.NotEmpty(option.Names);

                // Check if abbreviation is unique
                foreach (var abbreviation in option.Names)
                {
                    Assert.DoesNotContain(abbreviation, abbreviations);
                    abbreviations.Add(abbreviation);
                }
            }
        }

        [Fact]
        public void TestPixelColorHasRGBColor()
        {
            var colorsType = typeof(PixelColor);
            var fields = colorsType.GetFields();

            foreach (var field in fields)
            {
                if (field.FieldType != colorsType)
                {
                    continue;
                }

                var option = field.GetCustomAttribute<RGBColorAttribute>();
                Assert.NotNull(option);
            }
        }

        [Fact]
        public void TestDisplayColorStyleIsSorted()
        {
            // The number of supported colors of DisplayColorStyle should be sorted.

            var list = Enum.GetValues<DisplayColorStyle>().Select(a => a.SupportedColors().Length).ToList();

            // Check if list is sorted.
            for (int i = 1; i < list.Count; i++)
            {
                Assert.True(list[i - 1] <= list[i]);
            }
        }
    }
}
