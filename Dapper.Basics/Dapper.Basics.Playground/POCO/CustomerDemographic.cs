using System.Collections.Generic;

namespace Dapper.Basics.Playground.POCO
{
    public class CustomerDemographic
    {
        public string CustomerTypeID { get; set; }
        public string CustomerDesc { get; set; }

        #region References

        public IEnumerable<CustomerCustomerDemo> CustomerCustomerDemos { get; set; }

        #endregion References
    }
}