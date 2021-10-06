using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.DayTrading
{
    public class User
    {
        public string userName;
        [JsonIgnore]
        public Bank bank = new Bank();
        public int shareValue;
        public string stateBuffer;

        public static User CreateUser(IUser discordUser)
        {
            User user = new User()
            {
                userName = discordUser.Username
            };


            return user;
        }

    }
}
