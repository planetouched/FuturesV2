using System;
using System.Collections.Generic;
using FuturesV2._Base;
using FuturesV2.Enums;

namespace FuturesV2.NotThreadSafe
{
    public class SequenceFuture : FutureBase, IFutureCollection
    {
        public int FuturesCount => _futures.Count;
        
        private readonly List<IFuture> _futures = new List<IFuture>();
        
        public SequenceFuture(IEnumerable<IFuture> futures)
        {
            foreach (var future in futures)
            {
                if (future == null)
                {
                    continue;
                }
                AddFuture(future);
            }
        }

        public SequenceFuture(params IFuture[] futures)
        {
            foreach (var future in futures)
            {
                if (future == null)
                {
                    continue;
                }
                AddFuture(future);
            }
        }

        public List<IFuture> GetFuturesCopyList()
        {
            return new List<IFuture>(_futures);
        }
        
        public void AddFuture(IFuture future)
        {
            if (IsRun || IsDone || IsCancelled || future.IsDone || future.IsCancelled)
            {
                return;
            }
            
            _futures.Add(future);
        }

        private void OnFutureComplete(IFuture future)
        {
            _futures.Remove(future);
            future.RemoveListener(OnFutureComplete);

            if (_futures.Count > 0)
            {
                _futures[0].Run();
                _futures[0].AddListener(FutureCompletionState.Both, OnFutureComplete);
                return;
            }
            
            IsDone = true;
            IsRun = false;

            CallHandlers();
            CallFinalizingHandlers();
        }

        public override IFuture Run()
        {
            if (IsRun)
            {
                return this;
            }
            
            IsRun = true;
            CallStartingHandlers();
            IsDone = _futures.Count == 0;
            
            if (IsDone)
            {
                IsRun = false;
                CallHandlers();
                CallFinalizingHandlers();
            }
            else
            {
                _futures[0].Run();
                _futures[0].AddListener(FutureCompletionState.Both, OnFutureComplete);

            }
            
            return this;
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
            return true;
        }

        public override void Complete(bool force = false)
        {
            if (IsPromise || IsCancelled || IsDone)
            {
                return;
            }

            IsForceCompletion = force;
            IsDone = true;
            IsRun = false;

            var copy = GetFuturesCopyList();
            _futures.Clear();

            foreach (var future in copy)
            {
                if (future.IsPromise)
                {
                    throw new Exception("future.isPromise == true");
                }

                if (future.IsCancelled || future.IsDone)
                {
                    continue;
                }
                
                future.RemoveListener(OnFutureComplete);
                future.Complete(force);
            }

            CallHandlers();
            CallFinalizingHandlers();
        }
        
        public override void Cancel()
        {
            if (IsPromise || IsCancelled || IsDone)
            {
                return;
            }
            
            IsCancelled = true;
            IsRun = false;

            var copy = GetFuturesCopyList();
            _futures.Clear();

            foreach (var future in copy)
            {
                if (future.IsPromise)
                {
                    throw new Exception("future.isPromise == true");
                }
                
                if (future.IsCancelled || future.IsDone) continue;
                
                future.RemoveListener(OnFutureComplete);
                future.Cancel();
            }

            CallHandlers();
            CallFinalizingHandlers();
        }
    }
}