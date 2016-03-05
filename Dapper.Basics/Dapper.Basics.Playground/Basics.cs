using System;
using static System.Console;
using static Dapper.SqlMapper;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper.Basics.Playground.Helpers;
using Dapper.Basics.Playground.POCO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dapper.Basics.Playground
{
    [TestClass]
    public class Basics
    {
        private const string CONNECTIONSTRING = @"Server=(localdb)\V11.0;Database=northwnd;Trusted_Connection=True;";
        private IDbConnection database;

        #region Setup and Cleanup

        [TestInitialize]
        public void TestInitialize()
        {
            database = new SqlConnection(CONNECTIONSTRING);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            database = null;
        }

        #endregion Setup and Cleanup

        [TestMethod]
        [Description("Executing a basic query with the Dapper IDbConnection Query extension method.")]
        public void Query()
        {
            //Not using var to show the return type if not using the Strongly-typed templating
            IEnumerable<dynamic> allProducts;
            using(database)
            {
                allProducts = database.Query("SELECT * FROM [dbo].[Products]");
            }

            allProducts.Should().BeAssignableTo<IEnumerable<dynamic>>();
            allProducts.Should().NotBeNull();
            allProducts.Should().NotBeEmpty();
            allProducts.Count().ShouldBeEquivalentTo(77);
        }

        [TestMethod]
        [Description("Executing a basic query with the Dapper IDbConnection Query extension method and Strongly-Typing the result")]
        public void Query_StronglyTyped()
        {
            //Not using var to show the return type if not using the Strongly-typed templating
            IEnumerable<Product> allProducts;
            using (database)
            {
                allProducts = database.Query<Product>("SELECT * FROM [dbo].[Products]");
            }

            allProducts.Should().BeAssignableTo<IEnumerable<Product>>();
            allProducts.Should().NotBeNull();
            allProducts.Should().NotBeEmpty();
            allProducts.Count().ShouldBeEquivalentTo(77);

            foreach (var product in allProducts)
            {
                ObjectDumper.Write(product);
            }
        }

        //[TestMethod]
        //[Description("")]
        //public void 

        [TestMethod]
        [Description("Executing a basic version of multiple queries with the Dapper IDbConection extension method")]
        public void MultipleQuery()
        {
            const string query = @"
                            SELECT * FROM [dbo].[Suppliers]
            
                            SELECT * FROM [dbo].[Products]
                             ";

            IEnumerable<Product> products;
            IEnumerable<Supplier> suppliers;
            using(database)
            {
                using(GridReader results = database.QueryMultiple(query))
                {
                    suppliers = results.Read<Supplier>().ToArray();
                    products = results.Read<Product>().ToArray();
                }
            }

            products.Should().BeAssignableTo<IEnumerable<Product>>();
            products.Should().NotBeNull();
            products.Should().NotBeEmpty();
            products.Count().ShouldBeEquivalentTo(77);

            foreach(var product in products)
            {
                ObjectDumper.Write(product);
            }
            WriteLine();

            suppliers.Should().BeAssignableTo<IEnumerable<Supplier>>();
            suppliers.Should().NotBeNull();
            suppliers.Should().NotBeEmpty();
            suppliers.Count().ShouldBeEquivalentTo(29);

            foreach (var supplier in suppliers)
            {
                ObjectDumper.Write(supplier);
            }
            WriteLine();
        }

        [TestMethod]
        [Description("Executing a basic version a query with parameters with Dapper IDbConnection extension methods")]
        public void Query_Parametized()
        {
            const string query = "SELECT * FROM [dbo].[Products] WHERE SupplierID = @SupplierID";

            IEnumerable<Product> products;
            using(database)
            {
                products = database.Query<Product>(query, new { SupplierID = 2 });
            }

            products.Should().NotBeNull();
            products.Should().NotBeEmpty();
            products.Count().ShouldBeEquivalentTo(4);
        }
    }
}
