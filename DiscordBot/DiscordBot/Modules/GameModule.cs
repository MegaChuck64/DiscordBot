using Discord;
using Discord.Commands;
using DiscordBot.Models;
using DiscordBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    public class GameModule : ModuleBase<SocketCommandContext>
    {

        [Command("play")]
        public async Task PlayAsync()
        {

            if (!User.UserExists(Context.User))
            {
                var user = new User()
                {
                    Username = Context.User.Username,                    
                };

                user.SaveNew();

            }

                GameController.CreateMap(10, 30);

            var upEmo = new Emoji("👆");
            var rightEmo = new Emoji("👉");
            var downEmo = new Emoji("👇");
            var leftEmo = new Emoji("👈");

            var msg = await ReplyAsync(GameController.PrintMap());
            await msg.AddReactionAsync(upEmo);
            await msg.AddReactionAsync(rightEmo);
            await msg.AddReactionAsync(downEmo);
            await msg.AddReactionAsync(leftEmo);

            GameController.MapMessageID = msg.Id;
            GameController.CurrentUser = Context.User.Username;

            GameController.PollReactions(Context);
        }



    }
}
