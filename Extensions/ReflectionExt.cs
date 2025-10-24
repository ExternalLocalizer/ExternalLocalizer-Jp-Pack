using System;
using System.Linq;
using System.Reflection;

internal static class ReflectionExt
{
    private const BindingFlags DEFAULT_BINDING_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    private static T GetInfoRecursiveOrThrow<T>(Func<Type, string, BindingFlags, T?> getter, Type type, string fieldName, BindingFlags bindingFlags = DEFAULT_BINDING_FLAGS)
    {
        Type? currentType = type;
        while (currentType != null)
        {
            T? info = getter(currentType, fieldName, bindingFlags);
            if (info != null)
                return info;

            currentType = currentType.BaseType;
        }

        throw new InvalidOperationException($"Field '{fieldName}' not found in type '{type.FullName}' or its base types with binding flags '{bindingFlags}'.");
    }

    public static MethodInfo GetMethodOrThrow(this Type type, string methodName, BindingFlags bindingFlags = DEFAULT_BINDING_FLAGS)
        => type.GetMethod(methodName, bindingFlags)
            ?? throw new InvalidOperationException($"Method '{methodName}' not found in type '{type.FullName}' with binding flags '{bindingFlags}'.");

    public static PropertyInfo GetPropertyOrThrow(this Type type, string propertyName, BindingFlags bindingFlags = DEFAULT_BINDING_FLAGS)
        => type.GetProperty(propertyName, bindingFlags)
            ?? throw new InvalidOperationException($"Property '{propertyName}' not found in type '{type.FullName}' with binding flags '{bindingFlags}'.");

    public static object GetValueOrThrow(this PropertyInfo property, object obj)
        => property.GetValue(obj)
            ?? throw new InvalidOperationException($"Property '{property.Name}' returned null for object of type '{obj.GetType().FullName}'.");

    public static FieldInfo GetFieldOrThrow(this Type type, string fieldName, BindingFlags bindingFlags = DEFAULT_BINDING_FLAGS)
        => type.GetField(fieldName, bindingFlags)
            ?? throw new InvalidOperationException($"Field '{fieldName}' not found in type '{type.FullName}' with binding flags '{bindingFlags}'.");

    public static ConstructorInfo GetConstructorOrThrow(this Type type, Type[] parameterTypes, BindingFlags bindingFlags = DEFAULT_BINDING_FLAGS)
        => type.GetConstructor(bindingFlags, null, parameterTypes, null)
            ?? throw new InvalidOperationException($"Constructor with parameter types '{string.Join(", ", parameterTypes.Select(t => t.FullName))}' not found in type '{type.FullName}' with binding flags '{bindingFlags}'.");

    public static MethodInfo GetMethodOrThrow(this object type, string methodName, BindingFlags bindingFlags = DEFAULT_BINDING_FLAGS)
        => type.GetType().GetMethodOrThrow(methodName, bindingFlags);
    public static PropertyInfo GetPropertyOrThrow(this object type, string propertyName, BindingFlags bindingFlags = DEFAULT_BINDING_FLAGS)
        => type.GetType().GetPropertyOrThrow(propertyName, bindingFlags);
    public static FieldInfo GetFieldOrThrow(this object type, string fieldName, BindingFlags bindingFlags = DEFAULT_BINDING_FLAGS)
        => type.GetType().GetFieldOrThrow(fieldName, bindingFlags);
    public static ConstructorInfo GetConstructorOrThrow(this object type, Type[] parameterTypes, BindingFlags bindingFlags = DEFAULT_BINDING_FLAGS)
        => type.GetType().GetConstructorOrThrow(parameterTypes, bindingFlags);

    public static Type GetTypeOrThrow(this Assembly assembly, string typeName)
        => assembly.GetType($"{typeName}", throwOnError: true)!;

    public static MethodInfo GetMethodRecursiveOrThrow(this Type type, string methodName, BindingFlags bindingFlags = DEFAULT_BINDING_FLAGS)
            => GetInfoRecursiveOrThrow((t, name, flags) => t.GetMethod(name, flags), type, methodName, bindingFlags);
    public static FieldInfo GetFieldRecursiveOrThrow(this Type type, string fieldName, BindingFlags bindingFlags = DEFAULT_BINDING_FLAGS)
        => GetInfoRecursiveOrThrow((t, name, flags) => t.GetField(name, flags), type, fieldName, bindingFlags);
    public static PropertyInfo GetPropertyRecursiveOrThrow(this Type type, string propertyName, BindingFlags bindingFlags = DEFAULT_BINDING_FLAGS)
        => GetInfoRecursiveOrThrow((t, name, flags) => t.GetProperty(name, flags), type, propertyName, bindingFlags);
    public static MethodInfo GetMethodRecursiveOrThrow(this object type, string methodName, BindingFlags bindingFlags = DEFAULT_BINDING_FLAGS)
        => type.GetType().GetMethodRecursiveOrThrow(methodName, bindingFlags);
    public static FieldInfo GetFieldRecursiveOrThrow(this object type, string fieldName, BindingFlags bindingFlags = DEFAULT_BINDING_FLAGS)
        => type.GetType().GetFieldRecursiveOrThrow(fieldName, bindingFlags);
    public static PropertyInfo GetPropertyRecursiveOrThrow(this object type, string propertyName, BindingFlags bindingFlags = DEFAULT_BINDING_FLAGS)
        => type.GetType().GetPropertyRecursiveOrThrow(propertyName, bindingFlags);
}
