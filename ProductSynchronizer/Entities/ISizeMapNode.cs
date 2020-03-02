namespace ProductSynchronizer.Entities
{
    public interface ISizeMapNode
    {
        string InternalSize { get; set; }
        string ExternalSize { get; set; }
        int Quantity { get; set; }
        string ExternalPrice { get; set; }
        string InternalPrice { get; set; }
    }
}
