using System.Reflection;

namespace Infernity.Framework.Core.Reflection;

public static class TypeExtensions
{
    extension(Type type)
    {
        public bool IsDefaultConstructibleClass()
        {
            if (type is { IsClass: true, IsAbstract: false })
            {
                var constructors = type.GetConstructors(
                    BindingFlags.Public | BindingFlags.Instance);

                if (constructors.Any(c => c.GetParameters().Length == 0))
                {
                    return true;
                }
            }

            return false;
        }

        public void EnsureStaticConstructorExecution()
        {
            Type?  currentType = type;

            while (currentType != null && currentType != typeof(object))
            {
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(currentType.TypeHandle);
                currentType = currentType.BaseType;
            }
        }

        public bool IsOfGenericType(Type genericType)
        {
            if (!genericType.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Not a generic type definition", nameof(genericType));
            }
        
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();

                return genericTypeDefinition == genericType;
            }

            return false;
        }
    }
}