using System.Collections.Concurrent;

namespace Infernity.Framework.Core.Patterns;

public static class GlobalsRegistry
{
    private static readonly ConcurrentDictionary<Type, object> _globals = new();

    public static void Register<T>(T instance)
        where T : notnull
    {
        _globals[typeof(T)] = instance;
    }

    public static T Resolve<T>()
        where T : notnull
    {
        if (_globals.TryGetValue(typeof(T), out var value) && value is T typedValue)
        {
            return typedValue;
        }

        if (
            _globals.TryGetValue(typeof(IServiceProvider), out var serviceProviderObject)
            && serviceProviderObject is IServiceProvider serviceProvider
        )
        {
            var instance = serviceProvider.GetService(typeof(T));

            if (instance != null)
            {
                if(_globals.TryAdd(typeof(T), instance))
                {
                    return (T)instance;
                }

                return Resolve<T>();
            }
        }

        throw new InvalidOperationException($"No instance of {typeof(T)} registered globally");
    }
}
