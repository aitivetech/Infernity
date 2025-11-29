using System.ComponentModel;
using System.Globalization;

using PathLib;

namespace Infernity.Framework.Core.Io.Paths.Posix
{
    /// <summary>
    /// Turn a string into a Windows path.
    /// </summary>
    public class PosixPathConverter : TypeConverter
    {
        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc/>
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string path)
            {
                return new PosixPath(path);
            }
            return base.ConvertFrom(context, culture, value);
        }

        /// <inheritdoc/>
        public override bool IsValid(ITypeDescriptorContext? context, object? value)
        {
            if (value is string strValue)
            {
                return PurePosixPath.TryParse(strValue, out _);
            }
            return base.IsValid(context, value);
        }
    }
}
