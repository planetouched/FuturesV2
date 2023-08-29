using System.Collections.Generic;
using FuturesV2._Base;
using FuturesV2.Enums;

namespace FuturesV2.NotThreadSafe
{
    public class SequenceFuture : Future, IFutureCollection
    {
        private readonly Queue<IFuture> _futures = new Queue<IFuture>();
        private IFuture _current;
        public IFuture[] Futures => _futures.ToArray();
        public int FuturesCount => _futures.Count;
        
        public SequenceFuture(IEnumerable<IFuture> futures)
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
        
        public SequenceFuture(params IFuture[] futures)
        {
            Initialize(futures);
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
        
        protected override void OnRun()
        {
            if (_futures.Count == 0)
            {
                Complete();
                return;
            }

            RunNextFuture();
        }

        private void RunNextFuture()
        {
            _current = _futures.Dequeue();
            _current.Run();
            _current.AddListener(FutureCompletionState.Both, OnFutureComplete);
        }

        private void OnFutureComplete(IFuture _)
        {
            if (_futures.Count > 0)
            {
                RunNextFuture();
            }
            else
            {
                Complete();
            }
        }

        protected override void OnComplete()
        {
            _futures.Clear();
            
            if (_current != null)
            {
                _current.RemoveListener(OnFutureComplete);
                
                if (IsCancelled)
                {
                    _current.Cancel();
                }
                else if (IsCompletedForcibly)
                {
                    _current.Complete(true);
                }
            }

            _current = null;
        }
    }
}