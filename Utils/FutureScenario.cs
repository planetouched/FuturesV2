using System;
using System.Collections.Generic;
using FuturesV2._Base;
using FuturesV2.Enums;
using FuturesV2.NotThreadSafe;
using FuturesV2.ThreadSafe;

namespace FuturesV2.Utils
{
    public class FutureScenario : IFutureContainer
    {
        private readonly List<CompositeFuture> _compositeFutures = new List<CompositeFuture>();
        private CompositeFuture _current;
        public event Action<bool> Completed;
        public bool IsRun { get; private set; }
        public bool IsCancelled { get; private set; }
        public bool IsEmpty => _compositeFutures.Count == 1 && _compositeFutures[0].FuturesCount == 0;

        public FutureScenario()
        {
            Init();
        }

        private void CompleteFuture(IFuture future)
        {
            IFuture nextFuture = null;
            _compositeFutures.RemoveAt(0);
            if (_compositeFutures.Count > 0)
            {
                nextFuture = _compositeFutures[0];
            }

            if (nextFuture == null)
            {
                Complete();
            }
            else
            {
                nextFuture.Run();
            }
        }

        private void Complete()
        {
            _current = null;
            IsRun = false;
            Init();

            Completed?.Invoke(IsCancelled);
        }
        
        private void Init()
        {
            _compositeFutures.Add(new CompositeFuture());
            _current = _compositeFutures[0];
            _current.AddListener(FutureCompletionState.Both, CompleteFuture);
        }
        
        public void Next()
        {
            if (_current.FuturesCount == 0)
            {
                return;
            }
            
            var newFuture = new CompositeFuture();

            _compositeFutures.Add(newFuture);
            newFuture.AddListener(FutureCompletionState.Both, CompleteFuture);
            _current = newFuture;
        }

        public void Run()
        {
            if (IsRun || _compositeFutures[0].FuturesCount == 0)
            {
                return;
            }
            
            IsRun = true;
            IsCancelled = false;
            _compositeFutures[0].Run();
        }

        public void AddFuture(IFuture future)
        {
            if (future.IsRun || future.IsCancelled || future.IsDone)
            {
                throw new Exception("future already run or completed");
            }

            _current.AddFuture(future);
        }

        public void ExecuteTask(Action method)
        {
            AddFuture(new FutureTask(method));
        }

        public void Cancel()
        {
            if (IsCancelled)
            {
                return;
            }
            
            IsCancelled = true;
            var cpy = new List<CompositeFuture>(_compositeFutures);
            _compositeFutures.Clear();

            foreach (var f in cpy)
            {
                f.RemoveListener(CompleteFuture);
                f.Cancel();
            }

            Complete();
        }
    }
}
