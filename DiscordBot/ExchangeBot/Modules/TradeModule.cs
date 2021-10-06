using Discord;
using Discord.Commands;
using ExchangeBot.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeBot.Modules
{
    public class TradeModule : ModuleBase<SocketCommandContext>
    {
        // Dependency Injection will fill this value in for us
        public DBService DBService { get; set; }


        // Get info on a user, or the user who invoked the command if one is not specified
        [Command("me")]
        public async Task MeAsync(IUser user = null)
        {
            user ??= Context.User;

            var res = DBService.GetSQLResults($"SELECT UserID, HasGonePublic FROM Users WHERE Username = '{user.Username}';");

            if (res.Count == 0)
            {
                await ReplyAsync("User not found. Please register ('?>register').");
            }
            else if (res.Count == 1)
            {
                EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle("ME")
                    .WithDescription($"Welcome Back, {user.Username}.")
                    .AddField("Gone Public?", res[0]["HasGonePublic"], true)                    
                    .WithColor(Color.Orange);

                if (!(bool)res[0]["HasGonePublic"])
                {
                    builder.AddField("Go Public", "?>public");
                }
                else
                {
                    var coinInfo = DBService.GetSQLResults($"SELECT NumShares, PricePerShare FROM UserCoinInfo WHERE UserID = '{(int)res[0]["UserID"]}';");
                    var numShares = (int)coinInfo[0]["NumShares"];
                    var pricePerShare = (double)coinInfo[0]["PricePerShare"];
                    builder.AddField("NumShares", numShares);
                    builder.AddField("PricePerShare", pricePerShare);
                    builder.AddField("Total Value", numShares * pricePerShare);

                }

                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
            else
            {
                await ReplyAsync("There are multiple accounts with this username found. Please contact support.");
            }
        }

        [Command("register")]
        public async Task RegisterAsync()
        {
            var user = Context.User;

            var updated = DBService.InsertSQL($"INSERT INTO Users (Username) VALUES ('{user.Username}');") == 1;

            if (updated)
            {
                await ReplyAsync("Registered successfully. ");
            }
            else
            {
                await ReplyAsync("Problem occured while registering. Please contact support.");
            }

        }

        [Command("public")]
        public async Task GoPublic(int? numShares = null)
        {
            var user = Context.User;

            if (!CurrentUserExists)
            {
                await ReplyAsync("User is not registerd. Use '?>register'.");
                return;
            }

            if (DBService.GetSQLCount("Users", "UserID", $"Username='{user.Username}' AND HasGonePublic = '1'") == 1)
            {
                //todo: give examples of what they can do next
                await ReplyAsync("User has already gone public.");
                return;
            }

            if (!numShares.HasValue)
            {
                EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle("Error")
                    .WithDescription($"Missing argument, 'number of shares'.")
                    .WithDescription("Everyones starts with a total value of $100. 100 shares would make each share worth $1")
                    .AddField("Usage", "?>public {numShares}")
                    .AddField("Example", "?>public 100")
                    .WithColor(Color.Red);

                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
            else
            {
                var updated = DBService.InsertSQL($"UPDATE Users SET HasGonePublic = '1' WHERE Username = '{user.Username}';") == 1;
                var userID = (int)DBService.GetSQLResults($"SELECT UserID FROM Users WHERE Username = '{user.Username}'")[0]["UserID"];

                if (updated)
                {
                    var created = DBService.InsertSQL(
                        $"INSERT INTO UserCoinInfo (UserID, NumShares, PricePerShare) values('{userID}', '{numShares.Value}', '{(100f/numShares.Value)}');") == 1;

                    if (created)
                    {
                        await ReplyAsync("Coin Created.");
                        await MeAsync();
                    }
                    else
                    {
                        await ReplyAsync("Error creating user coin. Please contact support.");
                    }
                }
                else
                {

                }
            }
        }


        private bool CurrentUserExists => DBService.GetSQLCount("Users", "UserID", $"Username='{Context.User.Username}'") == 1;

        private bool CurrentUserCoinExists => DBService.GetSQLCount("UserCoinInfo", "CoinID", $"UserID='{Context.User.Username}'") == 1;

    }
}
