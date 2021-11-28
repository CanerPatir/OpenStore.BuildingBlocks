using System.Collections.Concurrent;
using System.Reflection;
using OpenStore.Domain.EventSourcing.Exception;

namespace OpenStore.Domain.EventSourcing.Internal;

internal static class ReflectionHelper
{
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, string>> AggregateEventHandlerCache =
        new ConcurrentDictionary<Type, ConcurrentDictionary<Type, string>>();

    public static Dictionary<Type, string> FindEventHandlerMethodsInAggregate(Type aggregateType)
    {
        if (AggregateEventHandlerCache.ContainsKey(aggregateType) == false)
        {
            var eventHandlers = new ConcurrentDictionary<Type, string>();

            var eventType = typeof(IDomainEvent);

            var voidMethods = aggregateType.GetMethodsBySig(typeof(void), typeof(AggregateEventApplierAttribute), true, eventType).ToList();
            var asyncMethods = aggregateType.GetMethodsBySig(typeof(Task), typeof(AggregateEventApplierAttribute), true, eventType).ToList();

            var methods = voidMethods.Union(asyncMethods).ToList();

            if (methods.Any())
            {
                foreach (var m in methods)
                {
                    var parameter = m.GetParameters().First();
                    if (eventHandlers.TryAdd(parameter.ParameterType, m.Name) == false)
                    {
                        throw new System.Exception($"Multiple methods found handling same event in {aggregateType.Name}");
                    }
                }
            }

            AggregateEventHandlerCache.GetOrAdd(aggregateType, eventHandlers);
        }


        return AggregateEventHandlerCache[aggregateType].ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private static IEnumerable<MethodInfo> GetMethodsBySig(this Type type, Type returnType, Type customAttributeType, bool matchParameterInheritance, params Type[] parameterTypes)
    {
        return type.GetRuntimeMethods().Where((m) =>
        {
            if (m.ReturnType != returnType) return false;

            if ((customAttributeType != null) && (m.GetCustomAttributes(customAttributeType, true).Any() == false)) return false;

            var parameters = m.GetParameters();

            if ((parameterTypes == null || parameterTypes.Length == 0)) return parameters.Length == 0;

            if (parameters.Length != parameterTypes.Length) return false;

            return parameterTypes.Select((param, index) =>
            {
                var paramTypeMatched = parameters[index].ParameterType == param;
                var paramTypeIsAssignable = param.GetTypeInfo().IsAssignableFrom(parameters[index].ParameterType.GetTypeInfo());

                return paramTypeMatched || (matchParameterInheritance && paramTypeIsAssignable);
            }).All(r => r);
        });
    }

    public static string GetTypeName(Type t) => t.Name;

    public static string GetTypeFullName(Type t) => t.AssemblyQualifiedName;

    public static MethodInfo[] GetMethods(Type t) => t.GetTypeInfo().DeclaredMethods.ToArray();

    public static MethodInfo GetMethod(Type t, string methodName, Type[] paramTypes) => t.GetRuntimeMethod(methodName, paramTypes);

    public static MemberInfo[] GetMembers(Type t) => t.GetTypeInfo().DeclaredMembers.ToArray();

    public static void InvokeOnAggregate(EventSourcedAggregateRoot aggregate, string methodName, object @event)
    {
        var method = GetMethod(aggregate.GetType(), methodName, new Type[] {@event.GetType()}); //Find the right method

        if (method == null)
        {
            throw new AggregateEventOnApplyMethodMissingException($"No event Apply method found on {aggregate.GetType()} for {@event.GetType()}");
        }

        var task = method.Invoke(aggregate, new[] {@event}); //invoke with the event as argument
        if (task != null && task.GetType() == typeof(Task))
            throw new NotSupportedException($"Async event applier not supported on {aggregate.GetType()} for {@event.GetType()}");
    }
}