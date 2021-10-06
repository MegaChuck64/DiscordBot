using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeBot.Modules
{    
    public class BlackjackModule : ModuleBase<SocketCommandContext>
    {
        //[Command("cards")]
        //public async Task CardsAsync()
        //{
        //    var deck = new BJDeck();

        //    var msgBuilder = new StringBuilder();
        //    foreach (var card in deck.cards)
        //    {
        //        msgBuilder.AppendLine(card.ToString());                
        //    }

        //    await ReplyAsync(msgBuilder.ToString());
        
        //}
        //[Command("shuffle")]
        //public async Task ShuffleAsync()
        //{
        //    var deck = new BJDeck();
        //    deck.Shuffle();

        //    var msgBuilder = new StringBuilder();
        //    foreach (var card in deck.cards)
        //    {
        //        msgBuilder.AppendLine(card.ToString());
        //    }

        //    await ReplyAsync(msgBuilder.ToString());

        //}

        Blackjack bjGame;

        [Command("blackjack")]
        public async Task BlackjackAsync()
        {
            bjGame = new Blackjack();
            //bjGame.DealerHand.card1 = new Card(Card.Suit.Clubs, Card.Rank.Ace);
            //bjGame.DealerHand.card2 = new Card(Card.Suit.Spades, Card.Rank.Ace);
            
            //bjGame.PlayerHand.card1 = new Card(Card.Suit.Hearts, Card.Rank.King);
            //bjGame.PlayerHand.card2 = new Card(Card.Suit.Diamonds, Card.Rank.Ten);


            await ReplyAsync("Deck shuffled...");

            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle("Black Jack")
                .WithColor(Color.Orange);




            var dlrWorth = bjGame.DealerHand.card2.GetWorth();
            builder.AddField(
                $"Dealer Hand (Showing {dlrWorth})",
                $"? of ? || {bjGame.DealerHand.card2}", false);



            var plyrWorth = bjGame.PlayerHand.GetWorth();
            bool hasOptions = plyrWorth.Item1 != plyrWorth.Item2;
            var msg = hasOptions ?
                plyrWorth.Item1.ToString() + " or " + plyrWorth.Item2.ToString() :
                plyrWorth.Item1.ToString();

            builder.AddField(
                $"{Context.User.Username} Hand ({msg})",
                $"{bjGame.PlayerHand.card1} || {bjGame.PlayerHand.card2}", false);


            await Context.Channel.SendMessageAsync("", false, builder.Build());



            bool dealerHasBJ = false;
            bool playerHasBJ = false;
            EmbedBuilder dlrBJBldr = new EmbedBuilder();
            EmbedBuilder plrBJBldr = new EmbedBuilder();
            //dealer is showing ACE
            if (dlrWorth == 11)
            {
                if (bjGame.DealerHand.card1.GetWorth() == 10)
                {
                    dlrBJBldr = new EmbedBuilder()
                        .WithTitle("Dealer has BLACKJACK")
                        .WithColor(Color.Red);

                    dealerHasBJ = true;

                }
            }

            if (plyrWorth.Item1 == 21 || plyrWorth.Item2 == 21)
            {
                plrBJBldr = new EmbedBuilder()
                    .WithTitle($"{Context.User.Username} has BLACKJACK")
                    .WithColor(Color.Green);

                playerHasBJ = true;

            }

            EmbedBuilder endGameBldr;
            if (dealerHasBJ && playerHasBJ)
            {
                endGameBldr = new EmbedBuilder()
                    .WithTitle("Draw")
                    .WithColor(Color.Orange);
                await Context.Channel.SendMessageAsync("", false, dlrBJBldr.Build());
                await Context.Channel.SendMessageAsync("", false, plrBJBldr.Build());
                await Context.Channel.SendMessageAsync("", false, endGameBldr.Build());

            }
            else if (dealerHasBJ)
            {
                endGameBldr = new EmbedBuilder()
                    .WithTitle("YOU LOSE")
                    .WithColor(Color.Red);
                await Context.Channel.SendMessageAsync("", false, dlrBJBldr.Build());
                await Context.Channel.SendMessageAsync("", false, endGameBldr.Build());
            }
            else if (playerHasBJ)
            {
                endGameBldr = new EmbedBuilder()
                    .WithTitle("YOU WIN")
                    .WithColor(Color.Green);
                await Context.Channel.SendMessageAsync("", false, plrBJBldr.Build());
                await Context.Channel.SendMessageAsync("", false, endGameBldr.Build());
            }


        }

        public async Task HitAsync()
        {

        }
    }

    public class Blackjack
    {
        public BJDeck BJDeck;
        public BJHand DealerHand;
        public BJHand PlayerHand;

        public Blackjack()
        {
            BJDeck = new BJDeck();
            BJDeck.Shuffle();

            var dc1 = BJDeck.Deal();
            var pc1 = BJDeck.Deal();

            var dc2 = BJDeck.Deal();
            var pc2 = BJDeck.Deal();

            DealerHand = new BJHand(dc1, dc2, true);
            PlayerHand = new BJHand(pc1, pc2, false);
        }
    }

    public class BJDeck
    {
        public List<BJCard> cards;

        public BJDeck()
        {
            cards = new List<BJCard>();
            var options = Enum.GetValues(typeof(BJCard.Rank));
            foreach (BJCard.Rank item in options)
            {
                cards.Add(new BJCard(BJCard.Suit.Clubs, item));
                cards.Add(new BJCard(BJCard.Suit.Spades, item));
                cards.Add(new BJCard(BJCard.Suit.Hearts, item));
                cards.Add(new BJCard(BJCard.Suit.Diamonds, item));
            }
        }

        public BJCard Deal()
        {
            var crd = cards[0];
            cards.RemoveAt(0);
            return crd;
        }

        public void Shuffle()
        {
            var rng = new Random();
            cards = cards.OrderBy(a => rng.Next()).ToList();
        }
    }

    public class BJHand
    {
        public BJCard card1;
        public BJCard card2;
        public bool isDealer;
        public BJHand(BJCard _card1, BJCard _card2, bool _isDealer)
        {
            card1 = _card1;
            card2 = _card2;
            isDealer = _isDealer;
        }

        public Tuple<int, int> GetWorth()
        {
            int val1;
            int val2;

            //aces are 1 or 11
            if (card1.rank == BJCard.Rank.Ace)
            {
                if (card2.rank == BJCard.Rank.Ace)
                {
                    val1 = 2;
                    val2 = 12;
                    return new Tuple<int, int>(val1, val2);

                }
                else
                {
                    val1 = 1;
                    val2 = 11;
                }

            }
            else
            {
                //if face card, val is 10
                if ((int)card1.rank > 10) val1 = 10;
                else val1 = (int)card1.rank;

                val2 = val1;
            }

            
            val1 += card2.rank == BJCard.Rank.Ace ? 11 : card2.GetWorth();
            val2 += card2.GetWorth();

            return new Tuple<int, int>(val1, val2);

        }
    }


    public struct BJCard
    {
        public enum Suit
        {
            Clubs,Spades,Hearts,Diamonds
        }

        public enum Rank
        {
            Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King
        }

        public Suit suit { get;  }
        public Rank rank { get; }


        public int GetWorth()
        {
            if (rank == Rank.Ace)   return 11;

            if ((int)rank > 10)     return 10;
            else                    return (int)rank;
        }

        public BJCard(Suit _suit, Rank _rank)
        {
            suit = _suit;
            rank = _rank;
        }

        public override string ToString()
        {
            return $"{rank} of {suit}";
        }

    }
}
