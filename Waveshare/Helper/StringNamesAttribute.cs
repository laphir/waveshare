using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Waveshare.Helper
{
    /// <summary>
    /// It is an attribute for giving short names when specifying command line options.
    /// </summary>
    internal class StringNamesAttribute : Attribute
    {
        public string[] Names { get; }

        public StringNamesAttribute(params string[] names)
        {
            Names = names;
        }
    }
}
