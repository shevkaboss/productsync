namespace ProductSynchronizer
{
    public static class Constants
    {
        public const string STOCK_URL = "https://stockx.com";
        public const string FOOTASYLUM_URL = "https://www.footasylum.com";
        public const string JIMMY_JAZZ_URL = "https://www.jimmyjazz.com";
        public const string REGEX_FOR_DOMAIN = @"^(?:https?:\/\/)?(?:[^@\/\n]+@)?(?:www\.)?([^:\/?\n]+)";
    }

    public static class SqlQueries
    {
        public const string GET_PRODUCTS_QUERY = "Select product_id, location from oc_product where location is not null and location != \"\"";
    }
}
