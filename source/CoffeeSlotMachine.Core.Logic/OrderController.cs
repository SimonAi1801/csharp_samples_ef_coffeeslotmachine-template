using CoffeeSlotMachine.Core.Contracts;
using CoffeeSlotMachine.Core.Entities;
using CoffeeSlotMachine.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeSlotMachine.Core.Logic
{
    /// <summary>
    /// Verwaltet einen Bestellablauf. 
    /// </summary>
    public class OrderController : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICoinRepository _coinRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public OrderController()
        {
            _dbContext = new ApplicationDbContext();

            _coinRepository = new CoinRepository(_dbContext);
            _orderRepository = new OrderRepository(_dbContext);
            _productRepository = new ProductRepository(_dbContext);
        }

        /// <summary>
        /// Gibt alle Produkte sortiert nach Namen zurück
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Product> GetProducts() => _productRepository
                                                     .GetAllProducts()
                                                     .OrderBy(p => p.Name);


        /// <summary>
        /// Eine Bestellung wird für das Produkt angelegt.
        /// </summary>
        /// <param name="product"></param>
        public Order OrderCoffee(Product product)
        {
            return new Order()
            {
                Product = product,
                ProductId = product.Id
            };
        }

        /// <summary>
        /// Münze einwerfen. 
        /// Wurde zumindest der Produktpreis eingeworfen, Münzen in Depot übernehmen
        /// und für Order Retourgeld festlegen. Bestellug abschließen.
        /// </summary>
        /// <returns>true, wenn die Bestellung abgeschlossen ist</returns>
        public bool InsertCoin(Order order, int coinValue)
        {
            bool isFinished = false;
            if (order.InsertCoin(coinValue))
            {
                string[] parts = order.ThrownInCoinValues.Split(';');
                foreach (var item in parts)
                {
                    _coinRepository.AddCoin(Convert.ToInt32(item));
                }

                order.FinishPayment(_coinRepository.GetAllCoins());

                _orderRepository.UpdateOrder(order);

                isFinished = true;
            }
            return isFinished;
        }

        /// <summary>
        /// Gibt den aktuellen Inhalt der Kasse, sortiert nach Münzwert absteigend zurück
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Coin> GetCoinDepot() => _coinRepository
                                                   .GetAllCoins()
                                                   .OrderByDescending(c => c.CoinValue);

        /// <summary>
        /// Gibt den Inhalt des Münzdepots als String zurück
        /// </summary>
        /// <returns></returns>
        public string GetCoinDepotString()
        {
            StringBuilder sb = new StringBuilder();
            var coins = _coinRepository.GetAllCoins();
            foreach (var item in coins.OrderByDescending(c => c.CoinValue))
            {
                sb.Append($"{item.Amount}*{item.CoinValue}");
                if (item.CoinValue != _coinRepository.GetAllCoins()[0].CoinValue)
                {
                    sb.Append(" + ");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Liefert alle Orders inkl. der Produkte zurück
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Order> GetAllOrdersWithProduct() => _orderRepository
                                                               .GetAllOrders();

        /// <summary>
        /// IDisposable:
        ///
        /// - Zusammenräumen (zB. des ApplicationDbContext).
        /// </summary>
        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}
