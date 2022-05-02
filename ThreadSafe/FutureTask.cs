using System;

namespace FuturesV2.ThreadSafe
{
    public class FutureTask<T> : ThreadSafeFuture
    {
        public T Result { get; private set; }
        private readonly Func<T> _func;
        public event Action<Exception> Exception;
        
        public FutureTask(Func<T> func)
        {
            _func = func;
        }

        protected override void OnRun()
        {
            try
            {
                Result = _func();
            }
            catch (Exception e)
            {
                Exception?.Invoke(e);
                Exception = null;
                throw;
            }
            
            Complete();
        }

        protected override void OnComplete()
        {
        }
    }
    
    public class FutureTask : ThreadSafeFuture
    {
        private readonly Action _action;
        public event Action<Exception> Exception;
        
        public FutureTask(Action action)
        {
            _action = action;
        }

        protected override void OnRun()
        {
            try
            {
                _action();
            }
            catch (Exception e)
            {
                Exception?.Invoke(e);
                Exception = null;
                throw;
            }
            
            Complete();
        }

        protected override void OnComplete()
        {
        }
    }
}