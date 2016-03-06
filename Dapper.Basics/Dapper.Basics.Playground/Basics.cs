using static System.Console;
using static Dapper.SqlMapper;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper.Basics.Playground.Helpers;
using Dapper.Basics.Playground.POCO;
using Dapper.Basics.Playground.ResultObjects;
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

        [TestMethod]
        [Description("Execute a query and map it to a list of dynamic objects")]
        public void Query_MapToListObjects()
        {
            /*
                A row should take the form of:
                [ProductID, ProductName]
            */
            IList<dynamic> rows;
            using (database)
            {
                rows = database.Query("SELECT ProductID, ProductName FROM [dbo].[Products] WHERE SupplierID = 2").ToList();
            }

            rows.Should().NotBeNull();
            rows.Should().NotBeEmpty();
            rows.Count.ShouldBeEquivalentTo(4);

            //Row 1
            ((int)rows[0].ProductID).ShouldBeEquivalentTo(4);
            ((string)rows[0].ProductName).Should().BeEquivalentTo("Chef Anton's Cajun Seasoning");

            //Row 2
            ((int)rows[1].ProductID).ShouldBeEquivalentTo(5);
            ((string)rows[1].ProductName).Should().BeEquivalentTo("Chef Anton's Gumbo Mix");

            //Row 3
            ((int)rows[2].ProductID).ShouldBeEquivalentTo(65);
            ((string)rows[2].ProductName).Should().BeEquivalentTo("Louisiana Fiery Hot Pepper Sauce");

            //Row 4
            ((int)rows[3].ProductID).ShouldBeEquivalentTo(66);
            ((string)rows[3].ProductName).Should().BeEquivalentTo("Louisiana Hot Spiced Okra");
        }

        [TestMethod]
        [Description("Execute a query and map it to a list of dynamic objects - Strongly-Typed")]
        public void Query_MapToListObjects_StronglyTyped()
        {
            /*
                A row should take the form of:
                [ProductID, ProductName]
            */
            IList<ProductNameResult> rows;
            using (database)
            {
                rows = database.Query<ProductNameResult>("SELECT ProductID, ProductName FROM [dbo].[Products] WHERE SupplierID = 2").ToList();
            }

            rows.Should().NotBeNull();
            rows.Should().NotBeEmpty();
            rows.Count.ShouldBeEquivalentTo(4);

            //Row 1
            rows[0].ProductID.ShouldBeEquivalentTo(4);
            rows[0].ProductName.Should().BeEquivalentTo("Chef Anton's Cajun Seasoning");

            //Row 2
            rows[1].ProductID.ShouldBeEquivalentTo(5);
            rows[1].ProductName.Should().BeEquivalentTo("Chef Anton's Gumbo Mix");

            //Row 3
            rows[2].ProductID.ShouldBeEquivalentTo(65);
            rows[2].ProductName.Should().BeEquivalentTo("Louisiana Fiery Hot Pepper Sauce");

            //Row 4
            rows[3].ProductID.ShouldBeEquivalentTo(66);
            rows[3].ProductName.Should().BeEquivalentTo("Louisiana Hot Spiced Okra");
        }

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

        [TestMethod]
        [Description("Execute a Command that returns no results - Successful")]
        public void Execute_NoResult_Successful()
        {
            int successful;
            using(database)
            {
                successful = database.Execute("UPDATE [dbo].[Territories] SET RegionID = 1 WHERE TerritoryID = 01581");
            }
            successful.ShouldBeEquivalentTo(1);
        }

        [TestMethod]
        [Description("Execute a Command that returns no results - Failure")]
        public void Execute_NoResult_Failure()
        {
            int successful;
            using (database)
            {
                successful = database.Execute("UPDATE [dbo].[Territories] SET RegionID = 1 WHERE TerritoryID = -1");
            }
            successful.ShouldBeEquivalentTo(0);
        }

        [TestMethod]
        [Description("Stored Procedures - Dynamic Result")]
        public void Execute_StoredProcedure_DynamicResult()
        {
            IList<dynamic> historyResult;
            using(database)
            {
                historyResult = database.Query("CustOrderHist", new { CustomerID = "ALFKI" }, commandType: CommandType.StoredProcedure).ToList();
            }
            historyResult.Should().NotBeNull();
            historyResult.Should().NotBeEmpty();
            historyResult.Count.ShouldBeEquivalentTo(11);

            //Row 1
            ((string)historyResult[0].ProductName).ShouldBeEquivalentTo("Aniseed Syrup");
            ((int)historyResult[0].Total).ShouldBeEquivalentTo(6);

            //Row 2
            ((string)historyResult[1].ProductName).ShouldBeEquivalentTo("Chartreuse verte");
            ((int)historyResult[1].Total).ShouldBeEquivalentTo(21);
            
            //Row 3
            ((string)historyResult[2].ProductName).ShouldBeEquivalentTo("Escargots de Bourgogne");
            ((int)historyResult[2].Total).ShouldBeEquivalentTo(40);
            
            //Row 4
            ((string)historyResult[3].ProductName).ShouldBeEquivalentTo("Flotemysost");
            ((int)historyResult[3].Total).ShouldBeEquivalentTo(20);
            
            //Row 5
            ((string)historyResult[4].ProductName).ShouldBeEquivalentTo("Grandma's Boysenberry Spread");
            ((int)historyResult[4].Total).ShouldBeEquivalentTo(16);
            
            //Row 6
            ((string)historyResult[5].ProductName).ShouldBeEquivalentTo("Lakkalikööri");
            ((int)historyResult[5].Total).ShouldBeEquivalentTo(15);

            //Row 7
            ((string)historyResult[6].ProductName).ShouldBeEquivalentTo("Original Frankfurter grüne Soße");
            ((int)historyResult[6].Total).ShouldBeEquivalentTo(2);
            
            //Row 8
            ((string)historyResult[7].ProductName).ShouldBeEquivalentTo("Raclette Courdavault");
            ((int)historyResult[7].Total).ShouldBeEquivalentTo(15);
            
            //Row 9
            ((string)historyResult[8].ProductName).ShouldBeEquivalentTo("Rössle Sauerkraut");
            ((int)historyResult[8].Total).ShouldBeEquivalentTo(17);
            
            //Row 10
            ((string)historyResult[9].ProductName).ShouldBeEquivalentTo("Spegesild");
            ((int)historyResult[9].Total).ShouldBeEquivalentTo(2);
            
            //Row 11
            ((string)historyResult[10].ProductName).ShouldBeEquivalentTo("Vegie-spread");
            ((int)historyResult[10].Total).ShouldBeEquivalentTo(20);
        }

        [TestMethod]
        [Description("Stored Procedures - STrongly-Typed Result")]
        public void Execute_StoredProcedure_StronglyTypedResult()
        {
            IList<CustOrderHistResult> historyResult;
            using (database)
            {
                historyResult = database.Query<CustOrderHistResult>("CustOrderHist", new { CustomerID = "ALFKI" }, commandType: CommandType.StoredProcedure).ToList();
            }
            historyResult.Should().NotBeNull();
            historyResult.Should().NotBeEmpty();
            historyResult.Count.ShouldBeEquivalentTo(11);

            //Row 1
            historyResult[0].ProductName.ShouldBeEquivalentTo("Aniseed Syrup");
            historyResult[0].Total.ShouldBeEquivalentTo(6);

            //Row 2
            historyResult[1].ProductName.ShouldBeEquivalentTo("Chartreuse verte");
            historyResult[1].Total.ShouldBeEquivalentTo(21);

            //Row 3
            historyResult[2].ProductName.ShouldBeEquivalentTo("Escargots de Bourgogne");
            historyResult[2].Total.ShouldBeEquivalentTo(40);

            //Row 4
            historyResult[3].ProductName.ShouldBeEquivalentTo("Flotemysost");
            historyResult[3].Total.ShouldBeEquivalentTo(20);

            //Row 5
            historyResult[4].ProductName.ShouldBeEquivalentTo("Grandma's Boysenberry Spread");
            historyResult[4].Total.ShouldBeEquivalentTo(16);

            //Row 6
            historyResult[5].ProductName.ShouldBeEquivalentTo("Lakkalikööri");
            historyResult[5].Total.ShouldBeEquivalentTo(15);

            //Row 7
            historyResult[6].ProductName.ShouldBeEquivalentTo("Original Frankfurter grüne Soße");
            historyResult[6].Total.ShouldBeEquivalentTo(2);

            //Row 8
            historyResult[7].ProductName.ShouldBeEquivalentTo("Raclette Courdavault");
            historyResult[7].Total.ShouldBeEquivalentTo(15);

            //Row 9
            historyResult[8].ProductName.ShouldBeEquivalentTo("Rössle Sauerkraut");
            historyResult[8].Total.ShouldBeEquivalentTo(17);

            //Row 10
            historyResult[9].ProductName.ShouldBeEquivalentTo("Spegesild");
            historyResult[9].Total.ShouldBeEquivalentTo(2);

            //Row 11
            historyResult[10].ProductName.ShouldBeEquivalentTo("Vegie-spread");
            historyResult[10].Total.ShouldBeEquivalentTo(20);
        }
    }
}
