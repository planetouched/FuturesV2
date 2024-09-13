using System;
using FuturesV2._Base;

namespace FuturesV2.NotThreadSafe
{
    public abstract class Future : FutureBase
    {
        public sealed override void Cancel()
        {
            if (IsCancelled || IsDone)
            {
                return;
            }

            if (IsPromise)
            {
                throw new Exception("Try to cancel promise");
            }

            IsCancelled = true;
            OnComplete();
            CallHandlers();
            CallFinalizingHandlers();
        }

        public sealed override void Complete(bool force = false)
        {
            if (IsCancelled || IsDone)
            {
                return;
            }

            IsCompletedForcibly = force;
            IsDone = true;
            OnComplete();
            CallHandlers();
            CallFinalizingHandlers();
        }

        public sealed override IFuture Run()
        {
            if (IsRun || IsCancelled || IsDone)
            {
                return this;
            }

            IsRun = true;
            CallStartingHandlers();
            OnRun();
            return this;
        }

        protected abstract void OnRun();
        protected abstract void OnComplete();

        public override void ReUse()
        {
            if (IsDone || IsCancelled)
            {
                IsCancelled = false;
                IsDone = false;
                IsRun = false;
            }
        }
    }
}
