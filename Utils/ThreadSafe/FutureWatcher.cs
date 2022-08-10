using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using FuturesV2._Base;
using FuturesV2.Enums;

namespace FuturesV2.Utils.ThreadSafe
{
    public class FutureWatcher
    {
        private readonly ConcurrentDictionary<IFuture, bool> _futures = new ConcurrentDictionary<IFuture, bool>(); 
        public IFuture[] Futures => _futures.Keys.ToArray();
        public int FuturesCount => _futures.Count;

        public void AddFuture(IFuture future)
        {
            if (future == null || future.IsDone || future.IsCancelled)
            {
                return;
            }

            var sw = new SpinWait();
            
            for(;;)
            {
                if (_futures.TryAdd(future, true))
                {
                    future.AddListener(FutureCompletionState.Both, InnerRemoveFuture);
                    break;
                }
                
                sw.SpinOnce();  
            }
        }

        private void InnerRemoveFuture(IFuture future)
        {
            var sw = new SpinWait();

            for (;;)
            {
                if (_futures.TryRemove(future, out _))
                {
                    future.RemoveListener(InnerRemoveFuture);
                    break;
                }

                sw.SpinOnce();
            }
        }

        public void CancelFutures()
        {
            foreach (var pair in _futures)
            {
                pair.Key.RemoveListener(InnerRemoveFuture);
                pair.Key.Cancel();
            }
        }
        
        public void ForceCompleteFutures()
        {
            foreach (var pair in _futures)
            {
                pair.Key.RemoveListener(InnerRemoveFuture);
                pair.Key.Complete(true);
            }
        }    
    }
}
