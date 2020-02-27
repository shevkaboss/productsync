namespace ProductSynchronizer
{
    public interface ISizeMapNode
    {
        string InternalSize { get; set; }
        string ExternalSize { get; set; }
        int Quantity { get; set; }
    }
}
