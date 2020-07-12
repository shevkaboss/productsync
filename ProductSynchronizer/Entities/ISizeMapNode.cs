namespace ProductSynchronizer.Entities
{
    public interface ISizeMapNode
    {
        double InternalSize { get; set; }
        double ExternalSize { get; set; }
        int Quantity { get; set; }
        double ExternalPrice { get; set; }
        double InternalPrice { get; set; }
        int OptionValueId { get; set; }
    }
}
