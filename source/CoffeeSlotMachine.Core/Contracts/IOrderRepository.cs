using CoffeeSlotMachine.Core.Entities;
using System.Collections.Generic;

namespace CoffeeSlotMachine.Core.Contracts
{
    public interface IOrderRepository
    {
        IEnumerable<Order> GetAllOrders();

        public void AddOrder(Order order);

        public void UpdateOrder(Order order);
    }
}