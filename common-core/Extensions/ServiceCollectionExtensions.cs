// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using log4net;

namespace Core.Extensions;

public static class ServiceCollectionExtensions
{
    private static ILog logger = LogManager.GetLogger(nameof(ServiceCollectionExtensions));

    public static IServiceCollection AddTransientFromNamespace(
    this IServiceCollection services,
    string namespaceName,
    params Assembly[] assemblies)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (string.IsNullOrWhiteSpace(namespaceName))
            throw new ArgumentException("Namespace cannot be null or empty", nameof(namespaceName));

        // 如果没有提供程序集，默认使用调用程序集
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        foreach (var assembly in assemblies.Distinct())
        {
            if (assembly == null) continue;

            try
            {
                // 使用GetExportedTypes只获取公共类型，提高性能
                var types = assembly.GetExportedTypes()
                    .Where(IsConcreteClass)  // 只选择具体类
                    .Where(t => IsInNamespace(t, namespaceName))
                    .ToList();

                foreach (var type in types)
                {
                    if (!IsAlreadyRegistered(services, type))
                    {
                        services.AddTransient(type);
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                // 处理类型加载异常
                logger.Error($"Error loading types from assembly {assembly.FullName}: {ex.Message}");
                throw;
            }
        }

        return services;
    }

    // 帮助方法：检查是否是具体类
    private static bool IsConcreteClass(Type type)
    {
        return type.IsClass &&
               !type.IsAbstract &&
               !type.IsGenericTypeDefinition &&  // 可选：排除开放泛型
               !type.ContainsGenericParameters;  // 可选：排除包含未指定类型参数的泛型
    }

    // 帮助方法：检查是否在指定命名空间
    private static bool IsInNamespace(Type type, string namespaceName)
    {
        return type.Namespace != null &&
               type.Namespace.StartsWith(namespaceName, StringComparison.OrdinalIgnoreCase);
    }

    // 帮助方法：检查是否已注册
    private static bool IsAlreadyRegistered(IServiceCollection services, Type type)
    {
        return services.Any(s => s.ServiceType == type && s.ImplementationType == type);
    }
}
