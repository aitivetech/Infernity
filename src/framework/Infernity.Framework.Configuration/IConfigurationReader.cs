using System.Reflection;
using System.Text.Json.Nodes;

using Infernity.Framework.Core.Functional;

namespace Infernity.Framework.Configuration;

public interface IConfigurationReader
{
    Optional<JsonObject> ReadData(string sectionId);

    Optional<object> Read(string sectionId,
        Type type);
}

public static class ConfigurationReaderExtensions
{
    extension(IConfigurationReader reader)
    {
        public JsonObject ReadRequiredData(string sectionId)
        {
            return reader.ReadData(sectionId).OrThrow(() =>
                new ConfigurationException($"Configuration section not found: {sectionId}"));
        }

        public Optional<object> Read(Type type)
        {
            return reader.Read(SectionId(type),type);
        }
        
        public object ReadRequired(
            string sectionId,
            Type type)
        {
            return reader.Read(sectionId,
                type).OrThrow(() => new ConfigurationException($"Configuration section not found: {sectionId}"));
        }
        
        public object ReadRequired(Type type)
        {
            return reader.ReadRequired(SectionId(type), type);
        }

        public Optional<T> Read<T>()
            where T : class
        {
            return reader.Read<T>(SectionId(typeof(T)));
        }

        public Optional<T> Read<T>(string sectionId)
            where T : class
        {
            return reader.Read(sectionId,
                typeof(T)).OfType<T>();
        }

        public Optional<T> ReadRequired<T>()
            where T : class
        {
            return reader.ReadRequired<T>(SectionId(typeof(T)));
        }

        public Optional<T> ReadRequired<T>(string sectionId)
            where T : class
        {
            return reader.Read(sectionId,
                typeof(T)).OfType<T>().OrThrow(() => new ConfigurationException($"Configuration section not found: {sectionId}"));
        }

        private static string SectionId(Type type)
        {
            var attribute = type.GetCustomAttribute<ConfigurationSectionAttribute>(true);

            if (attribute == null)
            {
                throw new ConfigurationException($"Configuration section type candidate is not marked with {nameof(ConfigurationSectionAttribute)}.");
            }
            
            return attribute.Id;
        }
    }
}