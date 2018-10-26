using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MyBot
{
    public class Program
    {
        private string privToken = "";
        private bool loginFound = false;
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            var client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                WebSocketProvider = Discord.Net.Providers.WS4Net.WS4NetProvider.Instance
            });

            client.Log += Log;
            client.MessageReceived += MessageReceived;

            string token = privToken; // Remember to keep this private!
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            File.AppendAllText(@"C:\temp\BotLogging."+ DateTime.Now.ToString("DDMMYYYY") +".log",msg.ToString());
            return Task.CompletedTask;
        }

        private async Task MessageReceived(SocketMessage message)
        {
            if (message.Content == "!ping")
            {
                await CheckLogin();
                if (loginFound)
                {
                    await message.Channel.SendMessageAsync("Someone is logged in!");
                } else
                {
                    await message.Channel.SendMessageAsync("No one seems to be logged in!");
                }
                    
                loginFound = false;
            }
        }

        private Task CheckLogin()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C qwinsta | FIND /i \"active\"";
            process.StartInfo = startInfo;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            process.Start();
            StreamReader sr = process.StandardOutput;

            string result = sr.ReadToEnd();
            Console.WriteLine(result);
            if (result.ToLower().Contains("active"))
            {
                loginFound = true;
                
            }
            Console.WriteLine(loginFound);

            return Task.CompletedTask;
        }
    }
}