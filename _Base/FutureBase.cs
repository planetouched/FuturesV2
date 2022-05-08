using System;
using System.Collections.Generic;
using FuturesV2.Enums;
#if REFVIEW
using SharedClasses.Debugging;
#endif

namespace FuturesV2._Base
{
    public abstract class FutureBase : 
#if REFVIEW
        ReferenceCounter,
#endif 
        IFuture
    {
        public bool IsCancelled { get; protected set; }
        public bool IsDone { get; protected set; }
        public bool IsRun { get; protected set; }
        public bool IsForceCompleted { get; protected set; }
        public bool IsPromise { get; private set; }

        private event Action<IFuture> Finalized;
        private event Action<IFuture> Started;

        private readonly List<(FutureCompletionState state, Action<IFuture> action)> _handlers = new List<(FutureCompletionState state, Action<IFuture> action)>(2); 

        private bool CallCheck(FutureCompletionState state)
        {
            return state == FutureCompletionState.Both ||
                   state == FutureCompletionState.Done && IsDone ||
                   state == FutureCompletionState.Cancelled && IsCancelled;
        }

        protected void CallStartingHandlers()
        {
            Started?.Invoke(this);
            Started = null;
        }

        protected void CallHandlers()
        {
            var copy = new List<(FutureCompletionState state, Action<IFuture> action)>(_handlers);
            
            for (int i = 0; i < copy.Count; i++)
            {
                var state = copy[i].state;
                if (CallCheck(state))
                {
                    copy[i].action(this);
                }
            }
            
            _handlers.Clear();
        }
        
        protected void CallFinalizingHandlers()
        {
            Finalized?.Invoke(this);
            Finalized = null;
        }

        public IFuture AddListenerOnStart(Action<IFuture> method)
        {
            if (!IsRun)
            {
                Started += method;
            }
            else
            {
                method(this);
            }

            return this;
        }

        public void RemoveListenerOnStart(Action<IFuture> method)
        {
            Started -= method;
        }

        public IFuture AddListener(FutureCompletionState state, Action<IFuture> method)
        {
            if (!IsDone && !IsCancelled)
            {
                _handlers.Add((state, method));
            }
            else
            {
                if (CallCheck(state))
                {
                    method(this);
                }
            }

            return this;
        }

        public void RemoveListener(Action<IFuture> method)
        {
            for (int i = 0; i < _handlers.Count; i++)
            {
                if (_handlers[i].action == method)
                {
                    _handlers.RemoveAt(i);
                    break;
                }
            }
        }
        
        public IFuture AddListenerOnFinalize(Action<IFuture> method)
        {
            if (!IsDone && !IsCancelled)
            {
                Finalized += method;
            }
            else
            {
                method(this);
            }

            return this;
        }

        public void RemoveListenerOnFinalize(Action<IFuture> method)
        {
            Finalized -= method;
        }

        public abstract void Complete(bool force = false);
        public abstract void Cancel();
        public abstract IFuture Run();

        private static T StaticCast<T>(IFuture future)
        {
            return (T)future;
        }

        public T Cast<T>() where T : IFuture
        {
            return StaticCast<T>(this);
        }

        public abstract void ReUse();

        protected void SetAsPromise()
        {
            IsPromise = true;
        }
    }
}
