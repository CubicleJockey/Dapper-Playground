using static System.Console;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper.Basics.Playground.POCO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Timer = System.Timers.Timer;

namespace Dapper.Basics.Playground
{
    [TestClass]
    public class AsyncMethods : BaseTest
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
        [Description("Peforming a basic Dapper Sync method call.")]
        public void BasicAsync()
        {
            Task<IEnumerable<Product>> allProductsTask;
            var timer = new Timer();
            timer.Elapsed += (sender, args) =>
            {
                WriteLine($"Getting all products please wait..... Currently Taking: {args.SignalTime}");
            };

            using(database)
            {
                timer.Start();
                using(allProductsTask = database.QueryAsync<Product>("SELECT * FROM [dbo].[Products]"))
                {
                    while(!allProductsTask.IsCompleted)
                    {
                        //Check progress every 100 milliseconds
                        Thread.Sleep(100);
                    }
                    timer.Stop();
                }
            }
            allProductsTask.Result.Should().NotBeNull();
            allProductsTask.Result.Should().NotBeEmpty();
            allProductsTask.Result.Count().ShouldBeEquivalentTo(77);
        }
    }
}
