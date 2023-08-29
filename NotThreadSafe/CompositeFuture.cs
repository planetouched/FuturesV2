using System.Collections.Generic;
using FuturesV2._Base;
using FuturesV2.Enums;

namespace FuturesV2.NotThreadSafe
{
    public class CompositeFuture : Future, IFutureCollection
    {
        private readonly List<IFuture> _futures = new List<IFuture>();
        public IFuture[] Futures => _futures.ToArray();
        public int FuturesCount => _futures.Count;
        
        public CompositeFuture(IEnumerable<IFuture> futures)
        {
            Initialize(futures);
        }

        public CompositeFuture(params IFuture[] futures)
        {
            Initialize(futures);
        }
        
        public IFuture Initialize(IEnumerable<IFuture> futures)
        {
            foreach (var future in futures)
            {
                AddFuture(future);
            }

            return this;
        }
        
        public bool AddFuture(IFuture future)
        {
            if (IsRun || IsDone || IsCancelled || future == null || future.IsDone || future.IsCancelled)
            {
                return false;
            }
            
            _futures.Add(future);
            return true;
        }
        
        protected override void OnRun()
        {
            if (_futures.Count == 0)
            {
                Complete();
                return;
            }

            foreach (var future in new List<IFuture>(_futures))
            {
                future.Run();
                future.AddListener(FutureCompletionState.Both, OnFutureComplete);
            }
        }

        private void OnFutureComplete(IFuture future)
        {
            _futures.Remove(future);
            
            if (_futures.Count == 0)
            {
                Complete();
            }
        }

        protected override void OnComplete()
        {
            if (IsCancelled || IsCompletedForcibly)
            {
                foreach (var future in _futures)
                {
                    future.RemoveListener(OnFutureComplete);

                    if (IsCancelled)
                    {
                        future.Cancel();
                    }
                    else
                    {
                        future.Complete(true);
                    }
                }
            }
            
            _futures.Clear();
        }
    }
}