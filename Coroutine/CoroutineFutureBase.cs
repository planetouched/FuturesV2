using System;
using System.Collections;
using FuturesV2._Base;
using FuturesV2.Enums;
using FuturesV2.NotThreadSafe;

namespace FuturesV2.Coroutine
{
    public abstract class CoroutineFutureBase : Future
    {
        protected IEnumerator enumerator;
        private IFuture _current;

        protected void Next(IFuture obj)
        {
            if (IsCancelled) return;
            
            if (enumerator.MoveNext())
            {
                if (IsCancelled) return;
                
                if (enumerator.Current != null)
                {
                    _current = (IFuture)enumerator.Current;
                    _current.AddListener(FutureCompletionState.Both, Next);
                }
                else
                {
                    throw new NullReferenceException();
                }
            }
            else
            {
                Complete();
            }
        }

        protected override void OnComplete()
        {
            if (IsCancelled)
            {
                if (_current != null)
                {
                    _current.RemoveListener(Next);
                    _current.Cancel();
                }
            }

            _current = null;
            enumerator = null;
        }

        public override void ReUse()
        {
            throw new NotSupportedException();
        }
    }
}
