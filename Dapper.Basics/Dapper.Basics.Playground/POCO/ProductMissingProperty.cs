namespace Dapper.Basics.Playground.POCO
{
    public class ProductMissingProperty
    {
        public int ProductId { get; set; }
        public string ProductNames { get; set; }
        public int SupplierID { get; set; }
        public int CategoryID { get; set; }
        public string QuantityPerUnit { get; set; }
        public decimal UnitPrice { get; set; }
        public short? UnitsInStock { get; set; }
        public short? UnitsOnOrder { get; set; }
        public short? ReorderLevel { get; set; }
        public bool Discontinued { get; set; }

        #region References

        public Supplier Supplier { get; set; }

        #endregion References
    }
}
