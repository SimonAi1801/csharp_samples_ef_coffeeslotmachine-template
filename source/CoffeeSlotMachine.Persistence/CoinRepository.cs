﻿using CoffeeSlotMachine.Core.Contracts;
using CoffeeSlotMachine.Core.Entities;
using System.Collections.Generic;
using System.Linq;

namespace CoffeeSlotMachine.Persistence
{
    public class CoinRepository : ICoinRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public CoinRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Coin[] GetAllCoins() =>
                                    _dbContext
                                    .Coins
                                    .ToArray();

        public void AddCoin(int coinValue)
        {
            var dbCoin = _dbContext
                          .Coins
                          .SingleOrDefault(c => c.CoinValue == coinValue);
            dbCoin.Amount++;
            _dbContext.SaveChanges();
        }
    }
}
