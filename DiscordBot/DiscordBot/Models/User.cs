using Discord;
using DiscordBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models
{
    public class User
    {
        public int UserID { get; }
        public string Username { get; set; }

        public EventResult SaveNew()
        {
            var result = new EventResult();

            try
            {

                result.IsSuccessful = DatabaseService.InsertSQL($"Insert into Users (Username) values ('{Username}');") > 0;


                if (result.IsSuccessful)
                {
                    result.Message = $"New user saved.";
                    result.Data = $"{Username} has new account.";
                }
                else
                {
                    result.Message = $"Save failed.";
                    result.Data = "No user has been added.";
                }
            }
            catch (Exception e)
            {
                result.IsSuccessful = false;
                result.Message = "Error while saving new user.";
                result.Error = e;
            }

            return result;

        }

        public static bool UserExists (IUser user)
        {
            try
            {
                return DatabaseService.GetSQLCount("Users", "UserID", $"Username='{user.Username}'") > 0;
            }
            catch
            {
                return false;
            }

        }
    }
}
