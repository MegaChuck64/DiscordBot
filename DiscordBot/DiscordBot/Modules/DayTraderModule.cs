using Discord;
using Discord.Commands;
using DiscordBot.DayTrading;
using System;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    public class DayTraderModule : ModuleBase<SocketCommandContext>
    {

        [Command("info")]
        [Alias("details", "hello")]
        public Task PingAsync()
        {
            var user = DataController.Users.Find(u => u.userName == Context.Message.Author.Username);

            if (user == null)
            {
                return ReplyAsync("You aren't a registered user. Use command 'start' or 'register'. ");
            }
            else
            {
                return PrintPortfolio(user);
            }

        }


        [Command("register")]
        [Alias("start")]
        public async Task RegisterAsync()
        {
            var user = DataController.Users.Find(u => u.userName == Context.Message.Author.Username);

            if (user == null)
            {
                user = User.CreateUser(Context.Message.Author);

                user.stateBuffer = "created";
                user.bank.gold = -1;
                user.shareValue = -1;


                DataController.Users.Add(user);
                await ReplyAsync($"You have registered successfully. You own 100% of {user.userName} stock valued at 100,000 gold. You can now start trading it. Use command 'go public {{share count}}'.");
                await ReplyAsync("Example: go public 1000");
                await ReplyAsync("This example would split your stock into 1000 shares worth 100 gold each. This is because everyone starts with their discord value at 100,000 gold.");



                EmbedBuilder builder = new EmbedBuilder()
                .WithTitle("WARNING")
                .WithDescription("Division operations will round down at every step.")
                .AddField("Example: ", "go public 2333", true)
                .AddField("Networth: ", "97,986 gp", true)
                .AddField("Operation: ", "(100,000/2333) = 42 gp. (42 * 2333) = 97,986", true)

                .AddField("Example: ", "go public 1000", true)
                .AddField("Networth: ", "100,000 gp", true)
                .AddField("Operation: ", "(100,000/1000) = 100 gp. (100 * 1000) = 100,000", true)

                   .WithColor(Color.Orange);
                await Context.Channel.SendMessageAsync("", false, builder.Build());

            }
            else
            {
                await ReplyAsync("You are already registered");
            }
        }


        [Command("portfolio")]
        [Alias("port", "info")]
        public async Task PortfolioAsync() => await PrintPortfolio(DataController.Users.Find(u => u.userName == Context.Message.Author.Username));


        [Command("go public")]
        public async Task GoPublicAsync(int numShares)
        {
            var user = DataController.Users.Find(u => u.userName == Context.Message.Author.Username);

            if (user != null)
            {
                if (user.bank.gold == -1)
                {
                    var worth = 100_000;
                    user.bank.gold = 0;
                    if (!user.bank.shares.ContainsKey(user))
                    {
                        user.bank.shares.Add(user, numShares);
                        user.shareValue = worth / numShares;
                        user.stateBuffer = "public";

                        await ReplyAsync($"{user.userName} is now a publicly traded discord user.");

                        await PrintPortfolio(user);


                    }
                    else
                    {
                        await ReplyAsync("For some reason you already own your own shares. This is an error of order of operations. Please contact customer support.");
                    }
                }
                else
                {
                    await ReplyAsync($"Sorry sir, looks like you are already a publicly traded company");
                }
            }
            else
            {
                await ReplyAsync("You aren't a registered user. Use command 'start' or 'register'. ");
            }
        }

        public async Task PrintPortfolio(User user)
        {
            if (user == null)
            {
                await ReplyAsync("You aren't a registered user. Use command 'start' or 'register'. ");
            }
            else
            {
                EmbedBuilder builder = new EmbedBuilder();
                builder.WithTitle("Portfolio");

                int networth = 0;

                foreach (var share in user.bank.shares)
                {
                    builder.AddField("name", share.Key.userName, true);
                    builder.AddField("#shares", share.Value.ToString("N0"), true);
                    var val = share.Key.shareValue * share.Value;
                    builder.AddField("price(value)", $"{share.Key.shareValue.ToString("N0")}({val.ToString("N0")})", true);
                    networth += val;
                }

                builder.AddField("-", "-");
                builder.AddField("-", "Total");
                builder.AddField("Networth", networth.ToString("N0"));

                builder.WithColor(Color.Teal);

                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
        }

        [Command("buy")]
        public async Task BuyAsync(IUser user, [Remainder]string command)
        {
            var comSplit = command.Split('@');
            if (comSplit.Length == 2)
            {
                if (int.TryParse(comSplit[0], out int numShares)
                    && int.TryParse(comSplit[1], out int value))
                {
                    Offer offer = new Offer()
                    {
                        numberOfShares = numShares,
                        price = value,
                        proposedBy = DataController.Users.Find(u => u.userName == Context.Message.Author.Username),
                        stock = DataController.Users.Find(u => u.userName == user.Username),
                        offerType = Offer.OfferType.buy
                    };

                    DataController.Offers.Add(offer);


                    EmbedBuilder builder = new EmbedBuilder()
                        .WithTitle("Buy Offer")
                        .AddField("Stock", user.Username)
                        .AddField("Amount", offer.numberOfShares)
                        .AddField("Price", offer.price);


                    await Context.Channel.SendMessageAsync("", false, builder.Build());

                    var results = DataController.MatchOffers();

                    foreach (var result in results)
                    {
                        await ReplyAsync(result);
                    }

                }
                else
                {
                    await ReplyAsync("Command formatted incorrectly. " +
                        "Example: buy @HatefulB8 2@100. " +
                        "This would propse a buy offer of 2 HatefulB8 shares " +
                        "for 100 gold each.");
                }
            }
            else
            {
                await ReplyAsync("Command formatted incorrectly. " +
                    "Example: buy @HatefulB8 2@100. " +
                    "This would propse a buy offer of 2 HatefulB8 shares " +
                    "for 100 gold each.");
            }
        }

        [Command("sell")]
        public async Task SellAsync(IUser user, [Remainder]string command)
        {
            var comSplit = command.Split('@');
            if (comSplit.Length == 2)
            {
                if (int.TryParse(comSplit[0], out int numShares)
                    && int.TryParse(comSplit[1], out int value))
                {
                    Offer offer = new Offer()
                    {
                        numberOfShares = numShares,
                        price = value,
                        proposedBy = DataController.Users.Find(u => u.userName == Context.Message.Author.Username),
                        stock = DataController.Users.Find(u => u.userName == user.Username),
                        offerType = Offer.OfferType.sell
                    };

                    DataController.Offers.Add(offer);


                    EmbedBuilder builder = new EmbedBuilder()
                        .WithTitle("Sell Offer")
                        .AddField("Stock", user.Username)
                        .AddField("Amount", offer.numberOfShares)
                        .AddField("Price", offer.price);


                    await Context.Channel.SendMessageAsync("", false, builder.Build());

                    var results = DataController.MatchOffers();

                    foreach (var result in results)
                    {
                        await ReplyAsync(result);
                    }

                }
                else
                {
                    await ReplyAsync("Command formatted incorrectly. " +
                        "Example: sell @HatefulB8 2@100. " +
                        "This would propse a sale of 2 HatefulB8 shares " +
                        "for 100 gold each.");
                }
            }
            else
            {
                await ReplyAsync("Command formatted incorrectly. " +
                    "Example: sell @HatefulB8 2@100. " +
                    "This would propse a sale of 2 HatefulB8 shares " +
                    "for 100 gold each.");
            }
        }

        [Command("close")]
        [RequireOwner(ErrorMessage = "Attemp to close bot by someone besides the bot owner. You will be docked 5% of your shares.")]
        public async Task CloseAsync()
        {
            var notification = await ReplyAsync("Owner has initiated close sequence.");
            await Task.Delay(2000);
            await Context.Message.DeleteAsync();

            DataController.Final();

            await notification.DeleteAsync();            
        }

        [Command("open")]
        [RequireOwner(ErrorMessage = "Attemp to open bot by someone besides the bot owner. You will be docked 5% of your shares.")]
        public async Task OpenAsync()
        {
            var notification = await ReplyAsync("Owner has initiated open sequence.");
            await Task.Delay(2000);
            await Context.Message.DeleteAsync();

            DataController.Init();

            await notification.DeleteAsync();


        }


    }
}
