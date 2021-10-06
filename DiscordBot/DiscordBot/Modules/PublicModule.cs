using Discord;
using Discord.Commands;
using DiscordBot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        // Dependency Injection will fill this value in for us
        public PictureService PictureService { get; set; }

        [Command("ping")]
        [Alias("pong", "hello")]
        public Task PingAsync()
            => ReplyAsync("pong!");

        [Command("cat")]
        public async Task CatAsync()
        {
            // Get a stream containing an image of a cat
            var stream = await PictureService.GetCatPictureAsync();
            // Streams must be seeked to their beginning before being uploaded!
            stream.Seek(0, SeekOrigin.Begin);
            await Context.Channel.SendFileAsync(stream, "cat.png");
        }

        [Command("light")]
        public async Task Light(byte red, byte green, byte blue)
        {

            var _serialPort = new SerialPort
            {
                PortName = "COM3",//Set your board COM
                BaudRate = 9600
            };
            _serialPort.Open();

            var data = new byte[]
            {
                9, red, green, blue
            };
            _serialPort.Write(data, 0, data.Length);

            _serialPort.Close();

            await Context.Channel.SendMessageAsync("Color changed.");

        }


    }
}
