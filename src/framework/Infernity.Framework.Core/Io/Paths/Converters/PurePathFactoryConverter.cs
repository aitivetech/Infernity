using System.ComponentModel;
using System.Globalization;

using Infernity.Framework.Core.Io.Paths.Factories;

namespace Infernity.Framework.Core.Io.Paths.Converters
{
    /// <summary>
    /// Adds type conversion support from strings to paths depending on the
    /// platform.
    /// </summary>
    internal class PurePathFactoryConverter : TypeConverter
    {
        private readonly PurePathFactory _factory = new PurePathFactory();

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
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string path)
            {
                return _factory.Create(path);
            }
            return base.ConvertFrom(context, culture, value);
        }

        /// <inheritdoc/>
        public override bool IsValid(ITypeDescriptorContext? context, object? value)
        {
            if (value is string strValue)
            {
                return _factory.TryCreate(strValue, out _);
            }

            return base.IsValid(context, value);
        }
    }
}
