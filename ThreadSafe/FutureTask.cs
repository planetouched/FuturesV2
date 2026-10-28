using System;

namespace FuturesV2.ThreadSafe
{
    public class FutureTask<TParam, TRet> : ThreadSafeFuture
    {
        public TRet Result { get; private set; }
        private readonly Func<TParam, TRet> _func;
        private readonly TParam _state;
        public Exception InnerException { get; private set; }
        
        public FutureTask(Func<TParam, TRet> func, TParam state)
        {
            _func = func;
            _state = state;
        }

        protected override void OnRun()
        {
            try
            {
                Result = _func(_state);
            }
            catch (Exception e)
            {
                InnerException = e;
            }
            
            Complete();
        }

        protected override void OnComplete()
        {
        }
    }
    
    public class FutureTask<TParam> : ThreadSafeFuture
    {
        private readonly Action<TParam> _action;
        public Exception InnerException { get; private set; }
        private readonly TParam _state;
        
        public FutureTask(Action<TParam> action, TParam state)
        {
            _action = action;
            _state = state;
        }

        protected override void OnRun()
        {
            try
            {
                _action(_state);
            }
            catch (Exception e)
            {
                InnerException = e;
            }

            Complete();
        }

        protected override void OnComplete()
        {
        }
    }
}