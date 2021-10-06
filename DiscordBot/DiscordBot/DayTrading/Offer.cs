using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.DayTrading
{
    public class Offer
    {
        public enum OfferType
        {
            sell, buy
        }

        public OfferType offerType;
        public int numberOfShares;
        public User stock;
        public User proposedBy;
        public int price;
        public DateTime createdDate;

    }
}
