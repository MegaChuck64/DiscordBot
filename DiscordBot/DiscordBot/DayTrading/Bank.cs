using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.DayTrading
{

    public class Bank
    {
        public int gold;
        [JsonIgnore]
        public Dictionary<User, int> shares = new Dictionary<User, int>();

    }
}
