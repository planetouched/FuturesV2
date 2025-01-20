namespace FuturesV2._Base
{
    public interface IFutureCollection
    {
        bool AddFuture(IFuture future);
        IFuture [] Futures { get; }
        int FuturesCount { get; }
    }
}