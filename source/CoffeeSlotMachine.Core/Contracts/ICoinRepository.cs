using CoffeeSlotMachine.Core.Entities;
using System.Collections.Generic;

namespace CoffeeSlotMachine.Core.Contracts
{
    public interface ICoinRepository
    {
        public Coin[] GetAllCoins();

        public void AddCoin(Coin[] coins);

        public void RemoveCoin(Coin[] coins);
    }
}