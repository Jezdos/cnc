using AutoMapper;
using System.Collections.Concurrent;

namespace Core.Utils
{
    public static class MapperUtil
    {
        // 缓存 MapperConfiguration 和 Mapper 实例
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, IMapper>> _mappersCache =
            new ConcurrentDictionary<Type, ConcurrentDictionary<Type, IMapper>>();

        // 映射方法
        public static TTarget Map<TSource, TTarget>(TSource value)
            where TSource : class
            where TTarget : class
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            // 获取或创建 Mapper 实例
            var mapper = GetOrCreateMapper<TSource, TTarget>();

            // 执行映射
            return mapper.Map<TTarget>(value);
        }

        // 获取或创建 Mapper 实例
        private static IMapper GetOrCreateMapper<TSource, TTarget>()
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            // 检查是否已经存在对应的 Mapper 实例
            if (_mappersCache.TryGetValue(sourceType, out var targetMappers) &&
                targetMappers.TryGetValue(targetType, out var mapper))
            {
                return mapper;
            }

            // 创建新的 MapperConfiguration 和 Mapper 实例
            var config = new MapperConfiguration(cfg => cfg.CreateMap<TSource, TTarget>());
            var newMapper = config.CreateMapper();

            // 缓存 Mapper 实例
            targetMappers = _mappersCache.GetOrAdd(sourceType, _ => new ConcurrentDictionary<Type, IMapper>());
            targetMappers[targetType] = newMapper;

            return newMapper;
        }
    }
}
