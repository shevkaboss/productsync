namespace ProductSynchronizer.Parsers
{
    public interface IWorker
    {
        Product GetSyncedData(Product product);
    }
}
