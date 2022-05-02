using System;
using System.Collections.Generic;
using FuturesV2._Base;

namespace FuturesV2.Utils
{
    public static class FuturePool
    {
        private static readonly Dictionary<Type, Queue<IFuture>> _pool = new Dictionary<Type, Queue<IFuture>>();

        public static bool Has<T>()
        {
            return _pool.ContainsKey(typeof(T));
        }
        
        public static T Take<T>() where T : class, IFuture, new()
        {
            var future = TakeUnsafe<T>();

            future.AddListenerOnFinalize(Return<T>);
            
            return future;
        }

        public static T TakeUnsafe<T>() where T : class, IFuture, new()
        {
            var type = typeof(T);
            if (!_pool.TryGetValue(type, out var queue))
            {
                _pool.Add(type, queue = new Queue<IFuture>());
            }

            T future;
            if (queue.Count > 0)
            {
                future = (T)queue.Dequeue();
                future.Reuse();
            }
            else
            {
                future = new T();
            }
            
            return future;
        }

        public static void Return<T>(IFuture future) where T : class, IFuture
        {
            _pool[typeof(T)].Enqueue(future);
        }

        public static void Free()
        {
            _pool.Clear();
        }
    }
}