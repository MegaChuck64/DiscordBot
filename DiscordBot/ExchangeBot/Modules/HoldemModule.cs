using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeBot.Modules
{
    public class HoldemModule : ModuleBase<SocketCommandContext>
    {
        [Command("cards")]
        public async Task CardsAsync()
        {
            var deck = new BJDeck();

            var msgBuilder = new StringBuilder();
            foreach (var card in deck.cards)
            {
                msgBuilder.AppendLine(card.ToString());
            }

            await ReplyAsync(msgBuilder.ToString());



            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle("Test Jack")
                .WithColor(Color.Orange);


            await Context.Channel.SendMessageAsync("", false, builder.Build());

        }
        [Command("shuffle")]
        public async Task ShuffleAsync()
        {
            var deck = new BJDeck();
            deck.Shuffle();

            var msgBuilder = new StringBuilder();
            foreach (var card in deck.cards)
            {
                msgBuilder.AppendLine(card.ToString());
            }

            await ReplyAsync(msgBuilder.ToString());

        }


        public static TexasHoldem game;
        public static ulong lobbyID;

        [Command("holdem")]
        public async Task HoldemAsync()
        {
            if (game != null)
            {
                await PrintLobby();
                return;
            }


            game = new TexasHoldem()
            {
                currentState = TexasHoldem.HoldemState.Lobby,
                buttonLocation = 0
            };

            var creator = new Player()
            {
                User = Context.User,
                Bank = 100
            };

            game.Players.Add(creator);

            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle("New Game")
                .WithColor(Color.LightOrange)
                .WithDescription("Click 👌 below to join");

            var newGameMessage = await Context.Channel.SendMessageAsync("", false, builder.Build());

            var emoj = new Emoji("👌");
            await newGameMessage.AddReactionAsync(emoj);
            lobbyID = newGameMessage.Id;
            Context.Client.ReactionAdded += Client_ReactionAdded;


        }


        [Command("start game")]
        public async Task StartGameAsync()
        {
            if (game.currentState != TexasHoldem.HoldemState.Lobby)
            {
                await ReplyAsync("Game already started");
                await PrintLobby();
                return;
            }

            game.MoveButton();
            //deal
            for (int i = 0; i < 2; i++)
                foreach (var ply in game.Players)
                {
                    ply.PrivateCards.Add(game.deck.Deal());
                }

            foreach (var ply in game.Players)
            {
                var card1 = ply.PrivateCards[0];
                var card2 = ply.PrivateCards[1];
                await ply.User.SendMessageAsync(card1.ToString());
                await ply.User.SendMessageAsync(card2.ToString());
            }

            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle("Cards Dealt")
                .WithColor(Color.Magenta)
                .WithDescription("Chem them DMs");
            await Context.Channel.SendMessageAsync("", false, builder.Build());


        }


        private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> message, Discord.WebSocket.ISocketMessageChannel channel, Discord.WebSocket.SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot) return;
           // if (game.Players.Count(p => p.user.Username == reaction.User.Value.Username) > 0) return;

            if (message.Id == lobbyID)
            {
                if (reaction.Emote.Name == "👌")
                {
                    game.Players.Add(new Player()
                    {
                        Bank = 100,
                        User = reaction.User.Value
                    });


                    await channel.DeleteMessageAsync(lobbyID);
                    await PrintLobby();
                }
            }

        }

        [Command("print lobby")]
        public async Task PrintLobby()
        {
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle("Lobby")
                .WithColor(Color.Orange)
                .WithDescription("Click 👌 below to join");


            foreach (var ply in game.Players)
            {
                builder.AddField(ply.User.Username, ply.Bank);
            }

            var msg = await Context.Channel.SendMessageAsync("", false, builder.Build());
            lobbyID = msg.Id;

            var emoj = new Emoji("👌");
            await msg.AddReactionAsync(emoj);
        }

    }

    public class TexasHoldem
    {
        public List<Player> Players = new List<Player>();
        public int buttonLocation;
        public List<BettingRound.BetAction> ActionOptions = new List<BettingRound.BetAction>();
        public HoldemState currentState;
        public Deck deck;

        public TexasHoldem()
        {
            deck = new Deck();
            deck.Shuffle();
        }
        public enum HoldemState
        {
            Lobby,
            //Anti,
            Deal,
            Flop,
            Turn,
            River
        }



        public void MoveButton()
        {
            if (++buttonLocation >= Players.Count)
            {
                buttonLocation = 0;

                if (currentState != HoldemState.River)
                {
                    currentState++;
                }
            }

        }
    }

    public class BettingRound
    {
        public int currentPlayer;
        public BetAction lastAction;
        public bool openTable = true;
        public int raisesLeft = 3;
        //playerNdx, lastBetAction, amountOwed
        //public List<Tuple<int, int>> playerBets;
        public List<int> playerAmountOwed;
        public bool bettingStarted;
        public enum BetAction
        {
            Check,
            Call,
            Bet,
            Raise,
            Fold,
        }

        public BettingRound(TexasHoldem game)
        {
            game.Players.ForEach(x => playerAmountOwed.Add(0));
            foreach (var _ in game.Players)
                playerAmountOwed.Add(0);
            
            currentPlayer = 0;
            openTable = true;
            lastAction = BetAction.Check;
            bettingStarted = false;
        }

        public void PlayerTurn(int playerIndex)
        {
            currentPlayer = playerIndex;
        }

        public List<BetAction> PossibleActions()
        {
            var betActions = new List<BetAction>();

            if (openTable)
            {
                if (currentPlayer == 0 )
                {
                    if (!bettingStarted)
                    { 
                        bettingStarted = true;
                        betActions.Add(BetAction.Bet);
                        betActions.Add(BetAction.Check);
                    }
                    else
                    {
                        if (playerAmountOwed[currentPlayer] > 0)
                        {
                            if (raisesLeft > 0)
                            {
                                betActions.Add(BetAction.Raise);
                            }
                            betActions.Add(BetAction.Call);
                            betActions.Add(BetAction.Fold);
                        }
                    }
                }
            }
            else
            {

            }

            return betActions;
        }
    }

    public class Player
    {
        public IUser User{ get; set; }
        public int Bank { get; set; }
        public List<Card> PrivateCards { get; set; }

        public Player()
        {
            PrivateCards = new List<Card>();
        }
        public override string ToString()
        {
            return $"{User.Username} : {Bank}";
        }
    }



    public class Deck
    {
        public List<Card> cards;

        public Deck()
        {
            cards = new List<Card>();
            var options = Enum.GetValues(typeof(Card.Rank));
            foreach (Card.Rank item in options)
            {
                cards.Add(new Card(Card.Suit.Clubs, item));
                cards.Add(new Card(Card.Suit.Spades, item));
                cards.Add(new Card(Card.Suit.Hearts, item));
                cards.Add(new Card(Card.Suit.Diamonds, item));
            }
        }

        public Card Deal()
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

    public struct Card
    {
        public enum Suit
        {
            Clubs, Spades, Hearts, Diamonds
        }

        public enum Rank
        {
            Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King
        }

        public Suit suit { get; }
        public Rank rank { get; }


        public Card(Suit _suit, Rank _rank)
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
