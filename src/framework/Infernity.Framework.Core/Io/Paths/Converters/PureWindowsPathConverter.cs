using System.ComponentModel;

namespace Infernity.Framework.Core.Io.Paths.Converters
{
    /// <summary>
    /// Adds type conversion support from strings to paths.
    /// </summary>
    public class PureWindowsPathConverter : TypeConverter
    {
        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            if (sourceType == typeof (string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc/>
        public override object? ConvertFrom(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value)
        {
            if (value is string path)
            {
                return new PureWindowsPath(path);
            }
            return base.ConvertFrom(context, culture, value);
        }

        /// <inheritdoc/>
        public override bool IsValid(ITypeDescriptorContext? context, object? value)
        {
            if (value is string strValue)
            {
                return PureWindowsPath.TryParse(strValue, out _);
            }
            return base.IsValid(context, value);
        }
    }
}
