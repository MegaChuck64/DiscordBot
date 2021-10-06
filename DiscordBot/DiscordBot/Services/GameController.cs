using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public static class GameController
    {
        public static Tile[,] tiles;

        public static Player player;

        public static bool waiting = false;

        public static ulong MapMessageID;

        public static string CurrentUser;

        public static void PollReactions(SocketCommandContext context)
        {            
            int tryCount = 10;
            var upEmo = new Emoji("👆");
            var rightEmo = new Emoji("👉");
            var downEmo = new Emoji("👇");
            var leftEmo = new Emoji("👈");
            do
            {
                var msg = context.Channel.GetMessageAsync(MapMessageID).Result;
     

                if (msg.Reactions.ContainsKey(upEmo) && msg.Reactions[upEmo].ReactionCount > 1)
                {
                    player.x--;
                    tryCount = 0;
                }
                if (msg.Reactions.ContainsKey(rightEmo) && msg.Reactions[rightEmo].ReactionCount > 1)
                {
                    player.y++;
                    tryCount = 0;

                }
                if (msg.Reactions.ContainsKey(downEmo) && msg.Reactions[downEmo].ReactionCount > 1)
                {
                    player.x++;
                    tryCount = 0;

                }
                if (msg.Reactions.ContainsKey(leftEmo) && msg.Reactions[leftEmo].ReactionCount > 1)
                {
                    player.y--;
                    tryCount = 0;
                }

                tryCount--;
                Thread.Sleep(1000);
            } while (tryCount > 0);

            if (tryCount == -1)
            {
                context.Channel.DeleteMessageAsync(MapMessageID);
                var newMsg = context.Channel.SendMessageAsync(PrintMap()).Result;
                MapMessageID = newMsg.Id;
                newMsg.AddReactionAsync(upEmo);
                newMsg.AddReactionAsync(rightEmo);
                newMsg.AddReactionAsync(downEmo);
                newMsg.AddReactionAsync(leftEmo);
                PollReactions(context);
            }
        }

        public static void CreateMap(int width, int height)
        {
            player = new Player()
            {
                x = width / 2,
                y = height / 2,
                symbol = '@',
            };

            tiles = new Tile[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tiles[x, y].symbol = '^';
                    tiles[x, y].tiletype = TileType.ground;
                    tiles[x, y].options = new List<TileOption>();
                }
            }
        }

        public static string PrintMap()
        {
            StringBuilder result = new StringBuilder();

            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    if (player.x == x && player.y == y)
                    {
                        result.Append(player.symbol);
                    }
                    else
                    result.Append(tiles[x, y].symbol);
                }

                result.Append("\n");
            }

            var playerTile = tiles[player.x, player.y];
            if (playerTile.options.Count > 0)
            {
                for (int i = 0; i < playerTile.options.Count; i++)
                {
                    result.Append($"{i + 1}.{playerTile.options[i]}    ");
                }
            }
            else
            {
                //result.Append("north   east   south   west");
            }
            return result.ToString();
        }
    }

    public class Player
    {
        public int x;
        public int y;
        public char symbol;
    }
    public struct Tile
    {
        public char symbol;
        public TileType tiletype;
        public List<TileOption> options;
    }

    public enum TileType
    {
        ground, wall, water
    }

    public abstract class TileOption
    {
        public string display;
        public abstract void DoOption();
    }
}
