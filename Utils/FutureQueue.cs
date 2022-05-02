using System;
using System.Collections.Generic;
using FuturesV2._Base;
using FuturesV2.Enums;

namespace FuturesV2.Utils
{
    public class FutureQueue : IFutureContainer
    {
        private readonly Queue<IFuture> _queueFutures = new Queue<IFuture>();
        private IFuture _current;
        public int FuturesCount => _queueFutures.Count;
        public event Action<IFuture> FutureCompleted;
        public event Action AllFuturesCompleted;
        
        public void AddFuture(IFuture future)
        {
            if (future.IsDone || future.IsCancelled || future.IsRun)
            {
                throw new Exception("future already run or completed");
            }

            _queueFutures.Enqueue(future);
            future.AddListener(FutureCompletionState.Both, FutureComplete);
            
            if (_queueFutures.Count == 1)
            {
                _current = future;
                future.Run();
            }
        }

        private void FutureComplete(IFuture f)
        {
            _queueFutures.Dequeue();
            _current = null;
            
            if (_queueFutures.Count > 0)
            {
                _current = _queueFutures.Peek();
            }
            else
            {
                AllFuturesCompleted?.Invoke();
            }
            
            FutureCompleted?.Invoke(f);
            _current?.Run();
        }

        public void CancelCurrent()
        {
            _current?.Cancel();
        }

        public void Cancel()
        {
            foreach (var future in new List<IFuture>(_queueFutures))
            {
                future.RemoveListener(FutureComplete);
                future.Cancel();
            }
            
            _queueFutures.Clear();
        }
    }
}
