using System;
using FuturesV2.Enums;

namespace FuturesV2._Base
{
    public interface IFuture
    {
        bool IsCancelled { get; }
        bool IsDone { get; }
        bool IsRun { get; }
        bool IsCompletedForcibly { get; }
        bool IsPromise { get; }
        IFuture AddListenerOnStart(Action<IFuture> method);
        IFuture AddListener(FutureCompletionState state, Action<IFuture> method);
        IFuture AddListenerOnFinalize(Action<IFuture> method);
        void RemoveListenerOnStart(Action<IFuture> method);
        void RemoveListener(Action<IFuture> method);
        void RemoveListenerOnFinalize(Action<IFuture> method);
        void Complete(bool force = false);
        void Cancel();
        IFuture Run();
        T Cast<T>() where T : IFuture;
        void ReUse();
    }
}
