using System;
using System.Collections.Generic;
using FuturesV2._Base;

namespace FuturesV2.Coroutine
{
    public class CoroutineFuture<T1, T2, T3, T4, T5> : CoroutineFutureBase
    {
        private Func<T1, T2, T3, T4, T5, IEnumerator<IFuture>> _func;

        private readonly T1 _param1;
        private readonly T2 _param2;
        private readonly T3 _param3;
        private readonly T4 _param4;
        private readonly T5 _param5;

        public CoroutineFuture(Func<T1, T2, T3, T4, T5, IEnumerator<IFuture>> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            _param1 = param1;
            _param2 = param2;
            _param3 = param3;
            _param4 = param4;
            _param5 = param5;
            _func = func;
        }

        protected override void OnRun()
        {
            enumerator = _func(_param1, _param2, _param3, _param4, _param5);
            Next(null);
        }

        protected override void OnComplete()
        {
            base.OnComplete();
            _func = null;
        }
    }
}
