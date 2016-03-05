namespace Dapper.Basics.Playground.POCO
{
    public class CustomerCustomerDemo
    {
        public string CustomerID { get; set; }
        public string CustomeTypeID { get; set; }

        #region References

        public Customer Customer { get; set; }
        public CustomerDemographic CustomerDemographic { get; set; }

        #endregion References
    }
}
