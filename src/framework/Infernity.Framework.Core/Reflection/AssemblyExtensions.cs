using System.Reflection;

namespace Infernity.Framework.Core.Reflection;

public static class AssemblyExtensions
{
    extension(Assembly assembly)
    {
        public IEnumerable<T> CreateInstancesImplementing<T>()
        {
            return assembly.CreateInstancesImplementing<T>(new HashSet<Type>());
        }

        public IEnumerable<T> CreateInstancesImplementing<T>(IReadOnlySet<Type> except)
        {
            foreach (var type in assembly.GetTypes().Where(t =>
                         t.ImplementsInterface<T>() && t.IsDefaultConstructibleClass() && !except.Contains(t)))
            {
                yield return (T)Activator.CreateInstance(type)!;
            }
        }
    }
}