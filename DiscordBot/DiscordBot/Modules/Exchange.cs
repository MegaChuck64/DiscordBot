using Discord;
using Discord.Commands;
using DiscordBot.DayTrading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    class Exchange : ModuleBase<SocketCommandContext>
    {

        [Command("help")]
        [Alias("details", "hello", "info", "h", "ping")]
        public async Task HelpAsync()
        {

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("Help")
                .WithDescription("Commands")
                .AddField("-", "-")
                .AddField("me","Get info about yourself.")              
                .WithColor(Color.Teal);

            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        public async Task MeAsync()
        {
            //var user = User.
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("Me")
                .WithDescription("Info about yourself")
                .AddField("-", "-")
                //.AddField("Username", $"{}")
                .WithColor(Color.Teal);

            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }
    }
}