using System;
using FuturesV2._Base;
using FuturesV2.Enums;
using FuturesV2.NotThreadSafe;
using FuturesV2.ThreadSafe;

namespace FuturesV2.Utils
{
    public class FutureScenario : IFutureContainer
    {
        private SequenceFuture _sequenceFuture;
        private CompositeFuture _current;
        public bool IsEmpty => _sequenceFuture.FuturesCount == 0 && _current.FuturesCount == 0;
        public bool IsRun => _sequenceFuture.IsRun;
        public event Action<bool> Completed;

        public FutureScenario()
        {
            Initialize();
        }

        private void Initialize()
        {
            _sequenceFuture = new SequenceFuture();
            _current = new CompositeFuture();
        }

        public bool AddFuture(IFuture future)
        {
            if (IsRun || future.IsRun || future.IsCancelled || future.IsDone)
            {
                return false;
            }

            _current.AddFuture(future);
            return true;
        }
        
        public void AddTask(Action method)
        {
            AddFuture(new FutureTask(method));
        }
        
        public void Next()
        {
            if (_current.FuturesCount == 0)
            {
                return;
            }

            _sequenceFuture.AddFuture(_current);
            _current = new CompositeFuture();
        }
        
        public void Run()
        {
            if (IsRun || IsEmpty)
            {
                return;
            }

            _sequenceFuture.Run().AddListener(FutureCompletionState.Both, OnComplete);
        }

        private void OnComplete(IFuture _)
        {
            Completed?.Invoke(true);
            Initialize();
        }

        public void Cancel()
        {
            _sequenceFuture.RemoveListener(OnComplete);
            _sequenceFuture.Cancel();
            Completed?.Invoke(false);
            Initialize();
        }
    }
}