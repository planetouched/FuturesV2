using System;
using System.Collections.Generic;
using FuturesV2._Base;
using FuturesV2.Enums;
using FuturesV2.Utils.ThreadSafe;

namespace FuturesV2.NotThreadSafe
{
    public class SemaphoreFuture : Future, IFutureCollection
    {
        private int _simultaneously;
        private readonly Queue<IFuture> _futures = new Queue<IFuture>();
        private readonly FutureWatcher _futureWatcher = new FutureWatcher();
        public IFuture[] Futures => _futures.ToArray();
        public int FuturesCount => _futures.Count;
        
        public SemaphoreFuture(int simultaneously, params IFuture[] futures)
        {
            Initialize(simultaneously, futures);
        }

        public SemaphoreFuture(int simultaneously, IEnumerable<IFuture> futures)
        {
            Initialize(simultaneously, futures);
        }

        private IFuture Initialize(int simultaneously, IEnumerable<IFuture> futures) 
        {
            _simultaneously = simultaneously;
            
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
            
            _futures.Enqueue(future);
            
            return true;
        }

        private void RunNextFuture()
        {
            var f = _futures.Dequeue();
            _futureWatcher.AddFuture(f);
            f.AddListener(FutureCompletionState.Both, OnFutureComplete);
            f.Run();
        }
        
        protected override void OnRun()
        {
            for (int i = 0; i < Math.Min(_simultaneously, _futures.Count) ; i++)
            {
                RunNextFuture();
            }
        }

        private void OnFutureComplete(IFuture obj)
        {
            if (IsCancelled || IsCompletedForcibly)
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
            _futures.Clear();
            
            if (IsCancelled)
            {
                _futureWatcher.CancelFutures();
            }
            else if (IsCompletedForcibly)
            {
                _futureWatcher.ForceCompleteFutures();
            }
        }
    }
}