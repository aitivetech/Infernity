using System.ComponentModel;

using Infernity.Framework.Core.Io.Paths.Factories;

namespace Infernity.Framework.Core.Io.Paths.Converters
{
    /// <summary>
    /// Adds type conversion support from strings to paths depending on the
    /// platform.
    /// </summary>
    internal class PathFactoryConverter : TypeConverter
    {
        private readonly PathFactory _factory = new PathFactory();

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
            var path = value as string;
            if (path != null)
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
