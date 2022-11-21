using System;
using System.Collections.Generic;
using FuturesV2._Base;

namespace FuturesV2.Utils
{
    public static class NTSFuturePool
    {
        private static readonly Dictionary<Type, Queue<IFuture>> _pool = new Dictionary<Type, Queue<IFuture>>();

        public static T Take<T>() where T : class, IFuture, new()
        {
            var future = TakeUnsafe<T>();

            future.AddListenerOnFinalize(Return<T>);
            
            return future;
        }

        private static T TakeUnsafe<T>() where T : class, IFuture, new()
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
                future.ReUse();
            }
            else
            {
                future = new T();
            }
            
            return future;
        }

        private static void Return<T>(IFuture future) where T : class, IFuture
        {
            _pool[typeof(T)].Enqueue(future);
        }

        public static void Clean()
        {
            _pool.Clear();
        }
    }
}