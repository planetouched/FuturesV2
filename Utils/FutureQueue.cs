using System;
using System.Collections.Generic;
using FuturesV2._Base;
using FuturesV2.Enums;

namespace FuturesV2.Utils
{
    public class FutureQueue
    {
        private readonly Queue<IFuture> _queue = new Queue<IFuture>();
        private IFuture _current;
        public IFuture[] Futures => _queue.ToArray();
        public int FuturesCount => _queue.Count;
        public event Action<IFuture> FutureCompleted;
        public event Action AllFuturesCompleted;
        
        public bool AddFuture(IFuture future)
        {
            if (future.IsDone || future.IsCancelled)
            {
                return false;
            }

            _queue.Enqueue(future);
            
            RunNext();
            return true;
        }

        private void RunNext()
        {
            if (_current == null)
            {
                _current = _queue.Dequeue();
                _current.AddListener(FutureCompletionState.Both, FutureComplete);
                _current.Run();
            }
        }

        private void FutureComplete(IFuture future)
        {
            FutureCompleted?.Invoke(future);
            _current = null;
            
            if (_queue.Count > 0)
            {
                RunNext();
            }
            else
            {
                AllFuturesCompleted?.Invoke();
            }
        }

        public void CancelCurrent()
        {
            _current?.Cancel();
        }

        public void Cancel()
        {
            foreach (var future in new List<IFuture>(_queue))
            {
                future.RemoveListener(FutureComplete);
                future.Cancel();
            }
            
            _queue.Clear();
        }
    }
}
