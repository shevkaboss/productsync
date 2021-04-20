namespace ProductSynchronizer.Entities
{
    public interface ISizeMapNode
    {
        string InternalSize { get; set; }
        string ExternalSize { get; set; }
        int Quantity { get; set; }
        double ExternalPrice { get; set; }
        double InternalPrice { get; set; }
        int OptionValueId { get; set; }
    }
}
