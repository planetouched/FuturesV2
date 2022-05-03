using FuturesV2._Base;

namespace FuturesV2.NotThreadSafe
{
    public abstract class Future : FutureBase
    {
        public override void Cancel()
        {
            if (IsPromise || IsCancelled || IsDone)
            {
                return;
            }

            IsCancelled = true;
            OnComplete();
            CallHandlers();
            CallFinalizingHandlers();
        }

        public override void Complete(bool force = false)
        {
            if (IsCancelled || IsDone)
            {
                return;
            }

            IsForceCompletion = force;
            IsDone = true;
            OnComplete();
            CallHandlers();
            CallFinalizingHandlers();
        }

        public override IFuture Run()
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

        protected virtual void OnReuse()
        {
        }
        
        public override bool Reuse()
        {
            if (!IsDone && !IsCancelled)
            {
                return false;
            }

            IsCancelled = false;
            IsDone = false;
            IsRun = false;

            OnReuse();
            return true;
        }
    }
}
