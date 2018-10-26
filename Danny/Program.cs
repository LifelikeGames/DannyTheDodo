using Discord;
using Discord.WebSocket;
using Microsoft.Win32;
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
            await MonitorLogins();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            File.AppendAllText(@"C:\temp\BotLogging."+ DateTime.Now.ToString("ddMMyyyy") +".log",msg.ToString() + Environment.NewLine);
            return Task.CompletedTask;
        }

        private async Task MessageReceived(SocketMessage message)
        {
            if (message.Content == "!ping")
            {
                if (loginFound)
                {
                    await message.Channel.SendMessageAsync("The server is unlocked!");
                } else
                {
                    await message.Channel.SendMessageAsync("The server is locked!");
                }
                    
                loginFound = false;
            }
        }

        private Task MonitorLogins()
        {
            Console.WriteLine("Started monitoring logins");
            Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);

            void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
            {
                if (e.Reason == SessionSwitchReason.SessionLock)
                {
                    Console.WriteLine("Logged out");
                    File.AppendAllText(@"C:\temp\login." + DateTime.Now.ToString("ddMMyyyy") + ".log", "User is now locked: " + DateTime.Now + Environment.NewLine);
                    loginFound = false;
                }
                else if (e.Reason == SessionSwitchReason.SessionUnlock)
                {
                    Console.WriteLine("Logged in");
                    File.AppendAllText(@"C:\temp\login." + DateTime.Now.ToString("ddMMyyyy") + ".log", "User is now unlocked: " + DateTime.Now + Environment.NewLine);
                    loginFound = true;
                }
            }

            return Task.CompletedTask;
        }
    }
}