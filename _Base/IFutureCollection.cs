namespace FuturesV2._Base
{
    public interface IFutureCollection : IFutureContainer
    {
        IFuture [] Futures { get; }
        int FuturesCount { get; }
    }
}