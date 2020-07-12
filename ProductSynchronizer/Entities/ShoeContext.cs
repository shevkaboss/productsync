namespace ProductSynchronizer.Entities
{
    public class ShoeContext : ISizeMapNode
    {
        public int Quantity { get; set; }
        public double InternalSize { get; set; }
        public double ExternalSize { get; set; }
        public double ExternalPrice { get; set; }
        public double InternalPrice { get; set; }
        public int OptionValueId { get; set; }
    }
}
