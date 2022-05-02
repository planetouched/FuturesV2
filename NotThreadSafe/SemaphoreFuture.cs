using System;
using System.Collections.Generic;
using FuturesV2._Base;
using FuturesV2.Enums;
using FuturesV2.Utils.ThreadSafe;

namespace FuturesV2.NotThreadSafe
{
    public class SemaphoreFuture : Future
    {
        private int _simultaneously;
        private Queue<IFuture> _futures;
        private FutureWatcher _futureWatcher;
        
        public SemaphoreFuture(int simultaneously, params IFuture[] futures)
        {
            Init(simultaneously, futures);
        }

        public SemaphoreFuture(int simultaneously, IEnumerable<IFuture> futures)
        {
            Init(simultaneously, futures);
        }

        private void Init(int simultaneously, IEnumerable<IFuture> futures) 
        {
            _simultaneously = simultaneously;
            _futures = new Queue<IFuture>(futures);
        }

        private void RunNextFuture()
        {
            var f = _futures.Dequeue();
            _futureWatcher.AddFuture(f);
            f.AddListener(FutureCompletionState.Both, FutureComplete);
            f.Run();
        }
        
        protected override void OnRun()
        {
            _futureWatcher = new FutureWatcher();
            
            for (int i = 0; i < Math.Min(_simultaneously, _futures.Count) ; i++)
            {
                RunNextFuture();
            }
        }

        private void FutureComplete(IFuture obj)
        {
            if (IsCancelled)
            {
                return;
            }

            if (_futures.Count == 0)
            {
                if (_futureWatcher.FuturesCount == 0)
                {
                    Complete();
                }
                return;
            }
            
            RunNextFuture();
        }

        protected override void OnComplete()
        {
            if (IsCancelled)
            {
                _futureWatcher.CancelFutures();
            }
        }
    }
}