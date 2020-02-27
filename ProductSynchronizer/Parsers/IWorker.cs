namespace ProductSynchronizer.Parsers
{
    public interface IWorker
    {
        void GetSyncedData(Product product);
    }
}
