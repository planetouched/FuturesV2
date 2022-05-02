using System;
using System.Collections.Generic;
using FuturesV2._Base;
using FuturesV2.Enums;

namespace FuturesV2.ThreadSafe
{
    public abstract class ThreadSafeFuture : IFuture
    {
        private readonly object _syncRoot;
        public bool IsCancelled { get; private set; }
        public bool IsDone { get; private set; }
        public bool IsRun { get; private set; }
        public bool IsForceCompletion { get; private set;}
        public bool IsPromise { get; private set; }

        private event Action<IFuture> Finalized;
        private event Action<IFuture> Started;
        
        private readonly List<(FutureCompletionState state, Action<IFuture> action)> _onComplete = new List<(FutureCompletionState state, Action<IFuture> action)>(2); 

        protected ThreadSafeFuture()
        {
            _syncRoot = new object();
        }

        private void CallStartingHandlers()
        {
            Started?.Invoke(this);
            Started = null;
        }

        private void CallHandlers()
        {
            var copy = new List<(FutureCompletionState state, Action<IFuture> action)>(_onComplete);
            
            for (int i = 0; i < copy.Count; i++)
            {
                var state = copy[i].state;
                if (CallCheck(state))
                {
                    copy[i].action(this);
                }
            }

            // it is ok. because IsDone or IsCancelled have been already set
            _onComplete.Clear();
        }

        private void CallFinalizingHandlers()
        {
            Finalized?.Invoke(this);
            Finalized = null;
        }

        private bool CallCheck(FutureCompletionState state)
        {
            return state == FutureCompletionState.Both ||
                   state == FutureCompletionState.Done && IsDone ||
                   state == FutureCompletionState.Cancelled && IsCancelled;
        }

        public IFuture AddListenerOnStart(Action<IFuture> method)
        {
            bool call = false;
            
            lock (_syncRoot)
            {
                if (!IsRun)
                {
                    Started += method;
                }
                else
                {
                    call = true;
                }
            }

            if (call)
            {
                method(this);
            }

            return this;
        }

        public void RemoveListenerOnStart(Action<IFuture> method)
        {
            lock (_syncRoot)
            {
                Started -= method;
            }
        }

        public IFuture AddListener(FutureCompletionState state, Action<IFuture> method)
        {
            bool call = false;
            
            lock (_syncRoot)
            {
                if (!IsDone && !IsCancelled)
                {
                    _onComplete.Add((state, method));
                }
                else
                {
                    call = true;
                }
            }

            if (call)
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
            lock (_syncRoot)
            {
                for (int i = 0; i < _onComplete.Count; i++)
                {
                    if (_onComplete[i].action == method)
                    {
                        _onComplete.RemoveAt(i);
                        break;
                    }
                }
            }
        }
        
        public IFuture AddListenerOnFinalize(Action<IFuture> method)
        {
            bool call = false;
            
            lock (_syncRoot)
            {
                if (!IsDone && !IsCancelled)
                {
                    Finalized += method;
                }
                else
                {
                    call = true;
                }
            }

            if (call)
            {
                method(this);
            }

            return this;
        }

        public void RemoveListenerOnFinalize(Action<IFuture> method)
        {
            lock (_syncRoot)
            {
                Finalized -= method;
            }
        }

        public void Cancel()
        {
            lock (_syncRoot)
            {
                if (IsPromise || IsCancelled || IsDone)
                {
                    return;
                }

                IsCancelled = true;
            }

            OnComplete();
            CallHandlers();
            CallFinalizingHandlers();
        }

        public void Complete(bool force = false)
        {
            lock (_syncRoot)
            {
                if (IsCancelled || IsDone)
                {
                    return;
                }

                IsDone = true;
                IsForceCompletion = force;
            }

            OnComplete();
            CallHandlers();
            CallFinalizingHandlers();
        }

        public IFuture Run()
        {
            lock (_syncRoot)
            {
                if (IsRun || IsCancelled || IsDone) return this;
                IsRun = true;
            }

            OnRun();
            CallStartingHandlers();
            return this;
        }

        protected abstract void OnRun();
        protected abstract void OnComplete();

        private static T StaticCast<T>(IFuture future)
        {
            return (T)future;
        }

        public T Cast<T>() where T : IFuture
        {
            return StaticCast<T>(this);
        }

        public virtual bool Reuse()
        {
            lock (_syncRoot)
            {
                if (!IsDone && !IsCancelled) return false;
            
                IsCancelled = false;
                IsDone = false;
                IsRun = false;
                return true;
            }
        }

        protected void SetAsPromise()
        {
            lock (_syncRoot)
            {
                IsPromise = true;
            }
        }
    }
}
