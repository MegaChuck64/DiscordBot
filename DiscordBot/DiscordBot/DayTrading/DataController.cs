using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.DayTrading
{
    public static class DataController
    {
        public static bool isOpen = false;

        public static List<User> Users = new List<User>();

        public static List<Offer> Offers = new List<Offer>();
        public static void Init()
        {
            try
            {
                isOpen = true;
                Users = LoadUsers();
                Offers = LoadOffers();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        public static void Final()
        {
            try
            {
                isOpen = false;

                SaveUsers(Users);
                SaveOffers(Offers);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }




        public static List<string> MatchOffers()
        {

            var results = new List<string>();


            List<Offer> fulfilledBuyOffers = new List<Offer>();

            var buyOffers = Offers.Where(o => o.offerType == Offer.OfferType.buy).ToList();
            for (int j = 0; j < buyOffers.Count; j++)
            {

                var buyOffer = buyOffers[j];

                List<Offer> fulfilledSellOffers = new List<Offer>();

                var sellOffers = Offers
                    .Where(o => o.offerType == Offer.OfferType.sell)
                    .Where(o => o.stock == buyOffers[j].stock)
                    .Where(o => o.price <= buyOffers[j].price)
                    .OrderBy(o => o.createdDate).ToList();

                for (int i = 0; i < sellOffers.Count; i++)
                {

                    var sellOffer = sellOffers[i];

                    bool sellOlder = sellOffer.createdDate < buyOffer.createdDate;

                    var price = sellOlder ? buyOffer.price : sellOffer.price;

                    bool sellingMore = sellOffer.numberOfShares > buyOffer.numberOfShares;

                    var totalPrice = sellingMore ? buyOffer.numberOfShares * price : sellOffer.numberOfShares * price;

                    //sell gets money, buyer loses money
                    Users[Users.IndexOf(sellOffer.proposedBy)].bank.gold += (totalPrice);
                    Users[Users.IndexOf(buyOffer.proposedBy)].bank.gold -= (totalPrice);
                    
                    if (sellingMore)
                    {
                        //if seller is trying to sell more than the buyer is buying
                        //sell offer loses some shares but doesnt get removed, buy offer is now fulfilled so it gets removed at the end
                        sellOffer.numberOfShares -= buyOffer.numberOfShares;
                        fulfilledBuyOffers.Add(buyOffer);
                        
                        results.Add($"{sellOffer.proposedBy.userName} has sold {buyOffer.numberOfShares} of {sellOffer.stock} for {price} gold each ({totalPrice} gold).");
                        results.Add($"{buyOffer.proposedBy.userName} has fulfilled a buy offer for {buyOffer.stock} at {price} gold each ({totalPrice} gold).");

                    }
                    else
                    {
                        //sell offer has been fulfilled 
                        buyOffer.numberOfShares -= sellOffer.numberOfShares;
                        fulfilledSellOffers.Add(sellOffer);

                        results.Add($"{sellOffer.proposedBy.userName} has fulfilled a sell offer for {sellOffer.stock.userName} at {price} gold each ({totalPrice} gold).");
                        results.Add($"{buyOffer.proposedBy.userName} has bought {buyOffer.numberOfShares } of {buyOffer.stock.userName} for {price} gold each ({totalPrice} gold).");
                    }

                }

                //remove fulfilled sell offers because we don't want any other buy offers to look at them
                for (int i = 0; i < fulfilledSellOffers.Count; i++)
                {
                    Offers.RemoveAt(Offers.IndexOf(fulfilledSellOffers[i]));
                }
            }

            //remove fullilled buy offers
            for (int i = 0; i < fulfilledBuyOffers.Count; i++)
            {
                Offers.RemoveAt(Offers.IndexOf(fulfilledBuyOffers[i]));
            }

            return results;
        }


        public static void SaveOffers(List<Offer> offers)
        {

            Directory.CreateDirectory("Offers");

            var json = JsonConvert.SerializeObject(offers);

            using (var fs = new FileStream(@"Offers\offers.json", FileMode.Create))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(json);
                }
            }

        }

        public static List<Offer> LoadOffers()
        {
            if (Directory.Exists("Offers"))
            {
                var offers = JsonConvert.DeserializeObject<List<Offer>>(File.ReadAllText(@"Offers\offers.json"));

                return offers;
            }
            else return new List<Offer>();
        }



        public static List<User> LoadUsers()
        {
            List<User> users = new List<User>();

            if (Directory.Exists("Users"))
            {
                var userNames = Directory.EnumerateDirectories("Users").ToList();

                foreach (var userFolder in userNames)
                {
                    var user = JsonConvert.DeserializeObject<User>(File.ReadAllText(Path.Combine(userFolder, "user.json")));
                    var bank = JsonConvert.DeserializeObject<Bank>(File.ReadAllText(Path.Combine(userFolder, "bank.json")));

                    foreach (var shareFile in Directory.EnumerateFiles(userFolder, "*.share"))
                    {
                        var share = JsonConvert.DeserializeObject<KeyValuePair<User, int>>(File.ReadAllText(shareFile));
                        bank.shares.Add(share.Key, share.Value);
                    }

                    user.bank = bank;
                    users.Add(user);
                }
            }


            return users;
        }


        public static void SaveUsers(List<User> users)
        {
            foreach (var user in users)
            {
                var userFolder = @"Users\" + user.userName;
                if (Directory.Exists(userFolder))
                {
                    Directory.Delete(userFolder, true);
                }

                Directory.CreateDirectory(userFolder);



                using (var fs = new FileStream(Path.Combine(userFolder, "user.json"), FileMode.Create))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.Write(JsonConvert.SerializeObject(user));
                    }
                }

                using (var fs = new FileStream(Path.Combine(userFolder, "bank.json"), FileMode.Create))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.Write(JsonConvert.SerializeObject(user.bank));

                        foreach (var share in user.bank.shares)
                        {
                            using (var sfs = new FileStream(Path.Combine(userFolder, share.Key.userName + ".share"), FileMode.Create))
                            {
                                using (var ssw = new StreamWriter(sfs))
                                {
                                    ssw.Write(JsonConvert.SerializeObject(share));
                                }
                            }
                        }
                    }
                }

            }
        }
    }
}
