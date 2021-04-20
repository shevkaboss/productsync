namespace ProductSynchronizer.Entities
{
    public class ShoeContext : ISizeMapNode
    {
        public int Quantity { get; set; }
        public string InternalSize { get; set; }
        public string ExternalSize { get; set; }
        public double ExternalPrice { get; set; }
        public double InternalPrice { get; set; }
        public int OptionValueId { get; set; }
    }
}
