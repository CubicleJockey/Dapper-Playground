using System.Collections.Generic;

namespace Dapper.Basics.Playground.POCO
{
    public class Supplier
    {
        public int SupplierId { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        #region References

        public IEnumerable<Product> Products { get; set; }

        #endregion References
    }
}
