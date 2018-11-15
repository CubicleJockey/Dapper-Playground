using System.Data;
using System.Data.SqlClient;

namespace Dapper.Basics.Playground
{
    public abstract class BaseTest
    {
        private const string DEFAULT_CONNECTIONSTRING = @"Server=(localdb)\mssqllocaldb;Database=Northwind;Trusted_Connection=True;";

        protected IDbConnection database;

        protected virtual void DatabaseInitialize(Location location)
        {
            string connectionString;
            switch (location)
            {
                case Location.Home:
                    connectionString = DEFAULT_CONNECTIONSTRING;
                    break;
                case Location.Work:
                    connectionString = @"Server=localhost;Database=Northwind;Trusted_Connection=True;";
                    break;
                default:
                    connectionString = DEFAULT_CONNECTIONSTRING;
                    break;
            }

            database = new SqlConnection(connectionString);
        }

        protected virtual void DatabaseCleanup()
        {
            database = null;
        }
    }

    //Wanted quicker way to switch since I have different setups at home and work
    public enum Location
    {
        Home,
        Work
    }
}
