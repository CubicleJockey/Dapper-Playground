using Dapper.Basics.Playground.Helpers;
using Dapper.Basics.Playground.POCO;
using Dapper.Basics.Playground.ResultObjects;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static Dapper.SqlMapper;
using static System.Console;

namespace Dapper.Basics.Playground
{
    [TestClass]
    public class Basics : BaseTest
    {
        #region Setup and Cleanup

        [TestInitialize]
        public void TestInitialize()
        {
            DatabaseInitialize(Location.Home);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            DatabaseCleanup();
        }

        #endregion Setup and Cleanup

        [TestMethod]
        [Description("Executing a basic query with the Dapper IDbConnection Query extension method.")]
        public void Query()
        {
            //Not using var to show the return type if not using the Strongly-typed templating
            IEnumerable<dynamic> allProducts;
            using (database)
            {
                allProducts = database.Query("SELECT * FROM [dbo].[Products]");
            }

            allProducts.Should().BeAssignableTo<IEnumerable<dynamic>>();
            allProducts.Should().NotBeNull();
            allProducts.Should().NotBeEmpty();
            allProducts.Count().Should().Be(77);
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
            allProducts.Count().Should().Be(77);

            foreach (var product in allProducts)
            {
                ObjectDumper.Write(product);
            }
        }

        [TestMethod]
        [Description("Executing a basic query with the Dapper IDbConnection Query extensions method - Property name missmatch")]
        public void Query_PropertyMismatch()
        {
            IList<ProductMissingProperty> products;
            using (database)
            {
                products = database.Query<ProductMissingProperty>("SELECT * FROM [dbo].[Products]").ToList();
            }

            products.Should().NotBeNull();
            products.Should().NotBeEmpty();
            products.Count.Should().Be(77);

            foreach (var product in products)
            {
                //Noticing that if a Property is misspelled results come back null/empty/whitespace
                product.ProductNames.Should().BeNullOrWhiteSpace();
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
            rows.Count.Should().Be(4);

            //Row 1
            ((int)rows[0].ProductID).Should().Be(4);
            ((string)rows[0].ProductName).Should().BeEquivalentTo("Chef Anton's Cajun Seasoning");

            //Row 2
            ((int)rows[1].ProductID).Should().Be(5);
            ((string)rows[1].ProductName).Should().BeEquivalentTo("Chef Anton's Gumbo Mix");

            //Row 3
            ((int)rows[2].ProductID).Should().Be(65);
            ((string)rows[2].ProductName).Should().BeEquivalentTo("Louisiana Fiery Hot Pepper Sauce");

            //Row 4
            ((int)rows[3].ProductID).Should().Be(66);
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
            rows.Count.Should().Be(4);

            //Row 1
            rows[0].ProductID.Should().Be(4);
            rows[0].ProductName.Should().BeEquivalentTo("Chef Anton's Cajun Seasoning");

            //Row 2
            rows[1].ProductID.Should().Be(5);
            rows[1].ProductName.Should().BeEquivalentTo("Chef Anton's Gumbo Mix");

            //Row 3
            rows[2].ProductID.Should().Be(65);
            rows[2].ProductName.Should().BeEquivalentTo("Louisiana Fiery Hot Pepper Sauce");

            //Row 4
            rows[3].ProductID.Should().Be(66);
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
            using (database)
            {
                using (GridReader results = database.QueryMultiple(query))
                {
                    suppliers = results.Read<Supplier>().ToArray();
                    products = results.Read<Product>().ToArray();
                }
            }

            products.Should().BeAssignableTo<IEnumerable<Product>>();
            products.Should().NotBeNull();
            products.Should().NotBeEmpty();
            products.Count().Should().Be(77);

            foreach (var product in products)
            {
                ObjectDumper.Write(product);
            }
            WriteLine();

            suppliers.Should().BeAssignableTo<IEnumerable<Supplier>>();
            suppliers.Should().NotBeNull();
            suppliers.Should().NotBeEmpty();
            suppliers.Count().Should().Be(29);

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
            using (database)
            {
                products = database.Query<Product>(query, new { SupplierID = 2 });
            }

            products.Should().NotBeNull();
            products.Should().NotBeEmpty();
            products.Count().Should().Be(4);
        }

        [TestMethod]
        [Description("Execute a Command that returns no results - Successful")]
        public void Execute_NoResult_Successful()
        {
            int successful;
            using (database)
            {
                successful = database.Execute("UPDATE [dbo].[Territories] SET RegionID = 1 WHERE TerritoryID = 01581");
            }
            successful.Should().Be(1);
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
            successful.Should().Be(0);
        }

        [TestMethod]
        [Description("Stored Procedures - Dynamic Result")]
        public void Execute_StoredProcedure_DynamicResult()
        {
            IList<dynamic> historyResult;
            using (database)
            {
                historyResult = database.Query("CustOrderHist", new { CustomerID = "ALFKI" }, commandType: CommandType.StoredProcedure).ToList();
            }
            historyResult.Should().NotBeNull();
            historyResult.Should().NotBeEmpty();
            historyResult.Count.Should().Be(11);

            //Row 1
            ((string)historyResult[0].ProductName).Should().Be("Aniseed Syrup");
            ((int)historyResult[0].Total).Should().Be(6);

            //Row 2
            ((string)historyResult[1].ProductName).Should().Be("Chartreuse verte");
            ((int)historyResult[1].Total).Should().Be(21);

            //Row 3
            ((string)historyResult[2].ProductName).Should().Be("Escargots de Bourgogne");
            ((int)historyResult[2].Total).Should().Be(40);

            //Row 4
            ((string)historyResult[3].ProductName).Should().Be("Flotemysost");
            ((int)historyResult[3].Total).Should().Be(20);

            //Row 5
            ((string)historyResult[4].ProductName).Should().Be("Grandma's Boysenberry Spread");
            ((int)historyResult[4].Total).Should().Be(16);

            //Row 6
            ((string)historyResult[5].ProductName).Should().Be("Lakkalikööri");
            ((int)historyResult[5].Total).Should().Be(15);

            //Row 7
            ((string)historyResult[6].ProductName).Should().Be("Original Frankfurter grüne Soße");
            ((int)historyResult[6].Total).Should().Be(2);

            //Row 8
            ((string)historyResult[7].ProductName).Should().Be("Raclette Courdavault");
            ((int)historyResult[7].Total).Should().Be(15);

            //Row 9
            ((string)historyResult[8].ProductName).Should().Be("Rössle Sauerkraut");
            ((int)historyResult[8].Total).Should().Be(17);

            //Row 10
            ((string)historyResult[9].ProductName).Should().Be("Spegesild");
            ((int)historyResult[9].Total).Should().Be(2);

            //Row 11
            ((string)historyResult[10].ProductName).Should().Be("Vegie-spread");
            ((int)historyResult[10].Total).Should().Be(20);
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
            historyResult.Count.Should().Be(11);

            //Row 1
            historyResult[0].ProductName.Should().Be("Aniseed Syrup");
            historyResult[0].Total.Should().Be(6);

            //Row 2
            historyResult[1].ProductName.Should().Be("Chartreuse verte");
            historyResult[1].Total.Should().Be(21);

            //Row 3
            historyResult[2].ProductName.Should().Be("Escargots de Bourgogne");
            historyResult[2].Total.Should().Be(40);

            //Row 4
            historyResult[3].ProductName.Should().Be("Flotemysost");
            historyResult[3].Total.Should().Be(20);

            //Row 5
            historyResult[4].ProductName.Should().Be("Grandma's Boysenberry Spread");
            historyResult[4].Total.Should().Be(16);

            //Row 6
            historyResult[5].ProductName.Should().Be("Lakkalikööri");
            historyResult[5].Total.Should().Be(15);

            //Row 7
            historyResult[6].ProductName.Should().Be("Original Frankfurter grüne Soße");
            historyResult[6].Total.Should().Be(2);

            //Row 8
            historyResult[7].ProductName.Should().Be("Raclette Courdavault");
            historyResult[7].Total.Should().Be(15);

            //Row 9
            historyResult[8].ProductName.Should().Be("Rössle Sauerkraut");
            historyResult[8].Total.Should().Be(17);

            //Row 10
            historyResult[9].ProductName.Should().Be("Spegesild");
            historyResult[9].Total.Should().Be(2);

            //Row 11
            historyResult[10].ProductName.Should().Be("Vegie-spread");
            historyResult[10].Total.Should().Be(20);
        }

        [TestMethod]
        [Description("Multi Mapping")]
        public void MultiMapping()
        {
            const string query = @"SELECT *
                                   FROM [dbo].[Orders] AS O
                                   JOIN [dbo].[Customers] AS C ON C.CustomerID = O.CustomerID
                                   WHERE C.[CustomerID] = 'VINET'
                                   ORDER BY O.[OrderDate]";

            IList<Order> orders;
            using (database)
            {
                /*Multi Mapp assumes you are splitting on Id or id if you do not specify*/
                orders = database.Query<Order, Customer, Order>(query,
                                                                (order, customer) => { order.Customer = customer; return order; },
                                                                splitOn: "CustomerID").ToList();
            }

            orders.Should().NotBeNull();
            orders.Should().NotBeEmpty();
            orders.Count.Should().Be(5);

            IEnumerable<int> expectedOrderIds = new[] { 10248, 10274, 10295, 10737, 10739 };
            foreach (var order in orders)
            {
                expectedOrderIds.Contains(order.OrderID).Should().Be(true);
                order.ShipCountry.Should().Be("France");
                order.ShipPostalCode.Should().Be("51100");

                //Check joined object
                order.Customer.Should().NotBeNull();
                order.Customer.ContactName.Should().Be("Paul Henriot");
            }
        }

        [TestMethod]
        [Description("Multi Mapping - Parameterized")]
        public void MultiMapping_Parameterized()
        {
            const string query = @"SELECT
                                     o.OrderID
                                    ,o.CustomerID
                                    ,o.EmployeeID
                                    ,o.OrderDate
                                    ,o.RequiredDate
                                    ,o.ShippedDate
                                    ,o.ShipVia
                                    ,o.Freight
                                    ,o.ShipName
                                    ,o.ShipAddress
                                    ,o.ShipCity
                                    ,o.ShipRegion
                                    ,o.ShipPostalCode
                                    ,o.ShipCountry
                                    ,c.CustomerID
                                    ,c.CompanyName
                                    ,c.ContactName
                                    ,c.ContactTitle
                                    ,c.Address
                                    ,c.City
                                    ,c.Region
                                    ,c.PostalCode
                                    ,c.Country
                                    ,c.Phone
                                    ,c.Fax
                                   FROM [dbo].[Orders] AS o
                                   JOIN [dbo].[Customers] AS c ON c.CustomerID = o.CustomerID
                                   WHERE C.[CustomerID] = @CustomerID
                                   ORDER BY O.[OrderDate]";

            IList<Order> orders;
            using (database)
            {
                /*Multi Mapp assumes you are splitting on Id or id if you do not specify*/
                orders = database.Query<Order, Customer, Order>(query,
                                                                (order, customer) => { order.Customer = customer; return order; },
                                                                new { CustomerID = "VINET" },
                                                                splitOn: "CustomerID"
                                                                ).ToList();
            }

            orders.Should().NotBeNull();
            orders.Should().NotBeEmpty();
            orders.Count.Should().Be(5);

            IEnumerable<int> expectedOrderIds = new[] { 10248, 10274, 10295, 10737, 10739 };
            foreach (var order in orders)
            {
                expectedOrderIds.Contains(order.OrderID).Should().Be(true);
                order.ShipCountry.Should().Be("France");
                order.ShipPostalCode.Should().Be("51100");

                //Check joined object
                order.Customer.Should().NotBeNull();
                order.Customer.ContactName.Should().Be("Paul Henriot");
            }
        }

        [TestMethod]
        [Description("Query - Just checking that ordering of the SELECT columns is unimportant.")]
        public void Query_ColumnOrderingNotImportant()
        {
            IList<EmployeeNameResult> employeeNameResults;
            using (database)
            {
                employeeNameResults =
                    database.Query<EmployeeNameResult>("SELECT EmployeeID, FirstName, LastName FROM [dbo].[Employees]").ToList();
            }

            employeeNameResults.Should().NotBeNull();
            employeeNameResults.Should().NotBeEmpty();
            employeeNameResults.Count.Should().Be(9);

            employeeNameResults[2].EmployeeID.Should().Be(3);
            employeeNameResults[2].FirstName.Should().BeEquivalentTo("Janet");
            employeeNameResults[2].LastName.Should().BeEquivalentTo("Leverling");
        }
    }
}
