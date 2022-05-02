using System;
using System.Collections.Generic;
using FuturesV2._Base;

namespace FuturesV2.Coroutine
{
    public class CoroutineFuture<T1> : CoroutineFutureBase
    {
        private Func<T1, IEnumerator<IFuture>> _func;

        private readonly T1 _param1;

        public CoroutineFuture(Func<T1, IEnumerator<IFuture>> func, T1 param1)
        {
            _param1 = param1;
            _func = func;
        }

        protected override void OnRun()
        {
            enumerator = _func(_param1);
            Next(null);
        }

        protected override void OnComplete()
        {
            base.OnComplete();
            _func = null;
        }
    }
}
