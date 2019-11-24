using System;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Lari
{
    public class Program
    {
        //Základní definice bota

        private CommandService commands;    //Vytvoření command handleru
        private DiscordSocketClient client; //=user a bot
        private IServiceProvider services;  //=bot

        static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();
        public async Task Start() //Zapnutí bota
        {
 
            client = new DiscordSocketClient();
            commands = new CommandService();

            string token = null; //bot token
            client.UserJoined += AnnounceUserJoined;
            services = new ServiceCollection()
                    .BuildServiceProvider();

            await InstallCommands();

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            Console.WriteLine("[System] Bot je online!");
            await client.SetGameAsync("Dej @Lari#7454 pomoc pro pomoc!");

            await Task.Delay(-1);

        }

         public async Task AnnounceUserJoined(SocketGuildUser user)
        {
            var guild = user.Guild;
            var channel = guild.DefaultChannel;
            await channel.SendMessageAsync("Vítej, " + user.Mention);
        }

        //Teď nadefinujeme správu příkazů, nediv se, že tu jsou chyby, ten code je níže
        //Jo a je to v angličtině, chtělo se mi :D

        public async Task InstallCommands()
        {
            //Co se stane, po doručení zprávy
            client.MessageReceived += HandleCommand;
            // Načte to příkazy ze Assembly
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        //Teď nastavíme "informace" o botovi
        public async Task HandleCommand(SocketMessage messageParam)
        {
            // Příkaz bude fungovat jen, jestli je poslán uživatelem
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;

            if (!(message.HasStringPrefix("L?", ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))) return; //Vytvoření kontextu
                                                                                                                                  //Zde si nastav tvůj prefix ->


            var context = new CommandContext(client, message);


            var result = await commands.ExecuteAsync(context, argPos, services);

            if (!result.IsSuccess) //Tak, zde si nastvíš, co se stane, když se vyskytne chyba.
            {

                //Do konzole (spravovací okno) to napíše čas, příkaz a kanál
                Console.WriteLine("[ERROR] --> " + System.DateTime.Now.ToString("h:mm:ss tt ") + " Kanál: » " + context.Channel + " Zpráva: » " + context.Message + " Server: » " + context.Guild + " Popis chyby: » " + result.ErrorReason);


                //Také to napíše důvod chyby na Discord
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }

            




        }
    }
}
