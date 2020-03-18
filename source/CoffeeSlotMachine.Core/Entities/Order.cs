using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CoffeeSlotMachine.Core.Entities
{
    /// <summary>
    /// Bestellung verwaltet das bestellte Produkt, die eingeworfenen Münzen und
    /// die Münzen die zurückgegeben werden.
    /// </summary>
    public class Order : EntityObject
    {
        private int _throwenInCents;
        private int _returnCents;
        private int _donationCents;



        /// <summary>
        /// Datum und Uhrzeit der Bestellung
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Werte der eingeworfenen Münzen als Text. Die einzelnen 
        /// Münzwerte sind durch ; getrennt (z.B. "10;20;10;50")
        /// </summary>
        public String ThrownInCoinValues { get; set; }

        /// <summary>
        /// Zurückgegebene Münzwerte mit ; getrennt
        /// </summary>
        public String ReturnCoinValues { get; set; }

        /// <summary>
        /// Summe der eingeworfenen Cents.
        /// </summary>
        public int ThrownInCents => _throwenInCents;

        /// <summary>
        /// Summe der Cents die zurückgegeben werden
        /// </summary>
        public int ReturnCents => _returnCents;


        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        /// <summary>
        /// Kann der Automat mangels Kleingeld nicht
        /// mehr herausgeben, wird der Rest als Spende verbucht
        /// </summary>
        public int DonationCents => _donationCents;

        public Order()
        {
            ThrownInCoinValues = "";
            ReturnCoinValues = "";
            Time = DateTime.Now;
        }


        /// <summary>
        /// Münze wird eingenommen.
        /// </summary>
        /// <param name="coinValue"></param>
        /// <returns>isFinished ist true, wenn der Produktpreis zumindest erreicht wurde</returns>
        public bool InsertCoin(int coinValue)
        {
            _throwenInCents += coinValue;
            ThrownInCoinValues = $"{ThrownInCoinValues}{coinValue}";

            if (_throwenInCents >= Product.PriceInCents)
            {
                _returnCents = _throwenInCents - Product.PriceInCents;
                return true;
            }
            ThrownInCoinValues = $"{ThrownInCoinValues};";
            return false;
        }

        /// <summary>
        /// Übernahme des Einwurfs in das Münzdepot.
        /// Rückgabe des Retourgeldes aus der Kasse. Staffelung des Retourgeldes
        /// hängt vom Inhalt der Kasse ab.
        /// </summary>
        /// <param name="coins">Aktueller Zustand des Münzdepots</param>
        public void FinishPayment(IEnumerable<Coin> coins)
        {
            int ret = _returnCents;
            foreach (var item in coins.OrderByDescending(c => c.CoinValue))
            {
                if (ret >= item.CoinValue && item.Amount > 0)
                {
                    if (ret <= item.CoinValue)
                    {
                        ReturnCoinValues = $"{ReturnCoinValues}{item.CoinValue}";
                    }
                    else
                    {
                        ReturnCoinValues = $"{ReturnCoinValues}{item.CoinValue};";
                    }

                    ret -= item.CoinValue;
                    item.Amount--;

                    while (ret > item.CoinValue && item.Amount > 0)
                    {
                        ret -= item.CoinValue;
                        item.Amount--;
                        ReturnCoinValues = $"{ReturnCoinValues}{item.CoinValue};";
                    }
                }
            }

            if (ret > 0)
            {
                _donationCents = _returnCents;
            }
        }
    }
}
