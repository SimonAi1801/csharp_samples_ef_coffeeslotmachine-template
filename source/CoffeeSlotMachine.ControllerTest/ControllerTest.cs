using CoffeeSlotMachine.Core.Logic;
using CoffeeSlotMachine.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace CoffeeSlotMachine.ControllerTest
{
    [TestClass]
    public class ControllerTest
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            using (ApplicationDbContext applicationDbContext = new ApplicationDbContext())
            {
                applicationDbContext.Database.EnsureDeleted();
                applicationDbContext.Database.Migrate();
            }
        }


        [TestMethod]
        public void T01_GetCoinDepot_CoinTypesCount_ShouldReturn6Types_3perType_SumIs1155Cents()
        {
            using (OrderController controller = new OrderController())
            {
                var depot = controller.GetCoinDepot().ToArray();
                Assert.AreEqual(6, depot.Count(), "Sechs Münzarten im Depot");
                foreach (var coin in depot)
                {
                    Assert.AreEqual(3, coin.Amount, "Je Münzart sind drei Stück im Depot");
                }

                int sumOfCents = depot.Sum(coin => coin.CoinValue * coin.Amount);
                Assert.AreEqual(1155, sumOfCents, "Beim Start sind 1155 Cents im Depot");
            }
        }

        [TestMethod]
        public void T02_GetProducts_9Products_FromCappuccinoToRistretto()
        {
            using (OrderController statisticsController = new OrderController())
            {
                var products = statisticsController.GetProducts().ToArray();
                Assert.AreEqual(9, products.Length, "Neun Produkte wurden erzeugt");
                Assert.AreEqual("Cappuccino", products[0].Name);
                Assert.AreEqual("Ristretto", products[8].Name);
            }
        }

        [TestMethod]
        public void T03_BuyOneCoffee_OneCoinIsEnough_CheckCoinsAndOrders()
        {
            using (OrderController controller = new OrderController())
            {
                var products = controller.GetProducts();
                var product = products.Single(p => p.Name == "Cappuccino");
                var order = controller.OrderCoffee(product);
                bool isFinished = controller.InsertCoin(order, 100);
                Assert.AreEqual(true, isFinished, "100 Cent genügen");
                Assert.AreEqual(100, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(100 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("20;10;5", order.ReturnCoinValues);

                // Depot überprüfen
                var coins = controller.GetCoinDepot().ToArray();
                int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
                Assert.AreEqual(1220, sumOfCents, "Beim Start sind 1155 Cents + 65 Cents für Cappuccino");
                Assert.AreEqual("3*200 + 4*100 + 3*50 + 2*20 + 2*10 + 2*5", controller.GetCoinDepotString());

                var orders = controller.GetAllOrdersWithProduct().ToArray();
                Assert.AreEqual(1, orders.Length, "Es ist genau eine Bestellung");
                Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
                Assert.AreEqual(100, orders[0].ThrownInCents, "100 Cents wurden eingeworfen");
                Assert.AreEqual("Cappuccino", orders[0].Product.Name, "Produktname Cappuccino");
            }
        }

        [TestMethod]
        public void T04_BuyOneCoffee_ExactThrowInOneCoin_CheckCoinsAndOrders()
        {
            using (OrderController controller = new OrderController())
            {
                var products = controller.GetProducts();
                var product = products.Single(p => p.Name == "Latte");
                var order = controller.OrderCoffee(product);
                Sbool isFinished = controller.InsertCoin(order, 50);
                Assert.AreEqual(true, isFinished, "50 Cent sind genau");
                Assert.AreEqual(50, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(50 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("0", order.ReturnCoinValues);

                var coins = controller.GetCoinDepot().ToArray();
                int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
                Assert.AreEqual(1205, sumOfCents, "Beim Start sind 1255 Cents + 50 Cents für Latte");
                Assert.AreEqual("3*200 + 3*100 + 4*50 + 3*20 + 3*10 + 3*5", controller.GetCoinDepotString());

                var orders = controller.GetAllOrdersWithProduct().ToArray();
                Assert.AreEqual(1, orders.Length, "Es ist genau eine Bestellung");
                Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
                Assert.AreEqual(50, orders[0].ThrownInCents, "50 Cents wurden eingeworfen");
                Assert.AreEqual("Latte", orders[0].Product.Name, "Produktname Latte");
            }
        }

        [TestMethod]
        public void T05_BuyOneCoffee_MoreCoins_CheckCoinsAndOrders()
        {
            using (OrderController controller = new OrderController())
            {
                var products = controller.GetProducts();
                var product = products.Single(p => p.Name == "Doppio");
                var order = controller.OrderCoffee(product);
                bool isFinished = controller.InsertCoin(order, 10);
                Assert.AreEqual(false, isFinished, "10 Cent sind zu wenig");
                isFinished = controller.InsertCoin(order, 20);
                Assert.AreEqual(false, isFinished, "30 Cent sind zu wenig");
                isFinished = controller.InsertCoin(order, 200);
                Assert.AreEqual(true, isFinished, "230 Cent sind viel mehr");
                Assert.AreEqual(230, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(230 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("100;50", order.ReturnCoinValues);

                var coins = controller.GetCoinDepot().ToArray();
                int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
                Assert.AreEqual(1235, sumOfCents, "Beim Start sind 1155 Cents + 80 Cents für Doppio");
                Assert.AreEqual("4*200 + 2*100 + 2*50 + 4*20 + 4*10 + 3*5", controller.GetCoinDepotString());

                var orders = controller.GetAllOrdersWithProduct().ToArray();
                Assert.AreEqual(1, orders.Length, "Es ist genau eine Bestellung");
                Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
                Assert.AreEqual(230, orders[0].ThrownInCents, "230 Cents wurden eingeworfen");
                Assert.AreEqual("Doppio", orders[0].Product.Name, "Produktname Doppio");
            }
        }


        [TestMethod()]
        public void T06_BuyMoreCoffees_OneCoins_CheckCoinsAndOrders()
        {
            using (OrderController controller = new OrderController())
            {
                var products = controller.GetProducts();
                var product = products.Single(p => p.Name == "Ristretto");
                var order = controller.OrderCoffee(product);
                bool isFinished = controller.InsertCoin(order, 10);
                Assert.AreEqual(false, isFinished, "10 Cent sind zu wenig");
                isFinished = controller.InsertCoin(order, 100);
                Assert.AreEqual(true, isFinished, "110 Cent sind zu viel");

                Assert.AreEqual(110, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(110 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("50", order.ReturnCoinValues);

                product = products.Single(p => p.Name == "Lungo");
                order = controller.OrderCoffee(product);
                isFinished = controller.InsertCoin(order, 10);
                Assert.AreEqual(false, isFinished, "10 Cent sind zu wenig");
                isFinished = controller.InsertCoin(order, 50);
                Assert.AreEqual(false, isFinished, "60 Cent sind zu wenig");
                isFinished = controller.InsertCoin(order, 5);
                Assert.AreEqual(true, isFinished, "65 Cent sind genau");

                Assert.AreEqual(65, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(65 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("0", order.ReturnCoinValues);

                var coins = controller.GetCoinDepot().ToArray();
                int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
                Assert.AreEqual(1280, sumOfCents, "Beim Start sind 1155 Cents + 60 Cents für Ristretto + 65 Cent für Lungo");
                Assert.AreEqual("3*200 + 4*100 + 3*50 + 3*20 + 5*10 + 4*5", controller.GetCoinDepotString());

                var orders = controller.GetAllOrdersWithProduct().ToArray();
                Assert.AreEqual(2, orders.Length, "Es sind zwei Bestellungen");
                Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
                Assert.AreEqual(110, orders[0].ThrownInCents, "230 Cents wurden eingeworfen");
                Assert.AreEqual("Ristretto", orders[0].Product.Name, "Produktname Ristretto");

                Assert.AreEqual(0, orders[1].DonationCents, "Keine Spende");
                Assert.AreEqual(65, orders[1].ThrownInCents, "230 Cents wurden eingeworfen");
                Assert.AreEqual("Lungo", orders[1].Product.Name, "Produktname Lungo");
            }
        }


        [TestMethod()]
        public void T07_BuyMoreCoffees_UntilDonation_CheckCoinsAndOrders()
        {
            using (OrderController controller = new OrderController())
            {
                var products = controller.GetProducts();
                var product = products.Single(p => p.Name == "Ristretto");
                var order = controller.OrderCoffee(product);
                bool isFinished = controller.InsertCoin(order, 200);
                Assert.AreEqual(true, isFinished, "200 Cent sind zu viel");

                Assert.AreEqual(200, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(200 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("100;20;20", order.ReturnCoinValues);

                product = products.Single(p => p.Name == "Lungo");
                order = controller.OrderCoffee(product);
                isFinished = controller.InsertCoin(order, 10);
                Assert.AreEqual(false, isFinished, "10 Cent sind zu wenig");
                isFinished = controller.InsertCoin(order, 50);
                Assert.AreEqual(false, isFinished, "60 Cent sind zu wenig");
                isFinished = controller.InsertCoin(order, 200);
                Assert.AreEqual(true, isFinished, "260 Cent sind zu viel");

                Assert.AreEqual(260, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(260 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("100;50;20;10;10;5", order.ReturnCoinValues);

                product = products.Single(p => p.Name == "Lungo");
                order = controller.OrderCoffee(product);
                isFinished = controller.InsertCoin(order, 10);
                Assert.AreEqual(false, isFinished, "10 Cent sind zu wenig");
                isFinished = controller.InsertCoin(order, 50);
                Assert.AreEqual(false, isFinished, "60 Cent sind zu wenig");
                isFinished = controller.InsertCoin(order, 200);
                Assert.AreEqual(true, isFinished, "260 Cent sind zu viel");

                Assert.AreEqual(260, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(260 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(5, order.DonationCents);
                Assert.AreEqual("100;50;10;10;10;5;5", order.ReturnCoinValues);


                product = products.Single(p => p.Name == "Ristretto");
                order = controller.OrderCoffee(product);
                isFinished = controller.InsertCoin(order, 200);
                Assert.AreEqual(true, isFinished, "200 Cent sind zu viel");

                Assert.AreEqual(200, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(200 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(40, order.DonationCents);
                Assert.AreEqual("50;50", order.ReturnCoinValues);

                var coins = controller.GetCoinDepot().ToArray();
                int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
                Assert.AreEqual(1450, sumOfCents, "Beim Start sind 1270 Cents + 60 Cents für Ristretto + 2 * 65 Cent für Lungo");
                Assert.AreEqual("7*200 + 0*100 + 1*50 + 0*20 + 0*10 + 0*5", controller.GetCoinDepotString());

                var orders = controller.GetAllOrdersWithProduct().ToArray();
                Assert.AreEqual(4, orders.Length, "Es sind vier Bestellungen");
                Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
                Assert.AreEqual(200, orders[0].ThrownInCents, "200 Cents wurden eingeworfen");
                Assert.AreEqual("Ristretto", orders[0].Product.Name, "Produktname Ristretto");

                Assert.AreEqual(0, orders[1].DonationCents, "Keine Spende");
                Assert.AreEqual(260, orders[1].ThrownInCents, "260 Cents wurden eingeworfen");
                Assert.AreEqual("Lungo", orders[1].Product.Name, "Produktname Lungo");

                Assert.AreEqual(5, orders[2].DonationCents, "Eine Spende von 5 Cent");
                Assert.AreEqual(260, orders[2].ThrownInCents, "260 Cents wurden eingeworfen");
                Assert.AreEqual("Lungo", orders[2].Product.Name, "Produktname Lungo");


                Assert.AreEqual(40, orders[3].DonationCents, "Eine Spende von 5 Cent");
                Assert.AreEqual(200, orders[3].ThrownInCents, "200 Cents wurden eingeworfen");
                Assert.AreEqual("Ristretto", orders[3].Product.Name, "Produktname Lungo");
            }
        }

    }
}
