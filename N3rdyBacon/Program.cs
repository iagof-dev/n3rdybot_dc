using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Net;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using N3rdyBacon;
using N3rdyBacon.Commands;
using Org.BouncyCastle.Utilities;
using System.Net;

///github.com/n3rdydzn
///Started: 12:10 PM | 06/08/2022
///Status: Not Finished...

///Banco de Dados Flexível
WebClient web = new WebClient();
config.bot_title = web.DownloadString(config.bot_title_link);

Console.Clear();
main();
void main()
{
    Console.Title = "[ST] | N3rdyBot";
    verify();
    start(config.bot_on, config.bot_token, config.bot_title).GetAwaiter().GetResult();

}
static async Task start(bool bot_on, string bot_token, string bot_title)
{

    Console.Write("\nDiscord | ");
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.Write("Conectando...");
    Console.ResetColor();
    var discord = new DiscordClient(new DiscordConfiguration()
    {
        Token = bot_token,
        TokenType = TokenType.Bot,
        MinimumLogLevel = LogLevel.None,
        LogTimestampFormat = "hh:mm:ss tt",
        Intents = DiscordIntents.All
    });
    discord.UseInteractivity(new InteractivityConfiguration()
    {
        PollBehaviour = PollBehaviour.KeepEmojis,
        Timeout = TimeSpan.FromSeconds(30)
    });

    var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
    {
        StringPrefixes = new[] { "$" }
    });
    bot_on = true;
    
    Console.Clear();

    while (bot_on == true)
    {
        Console.WriteLine("\n" + bot_title);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("                  >                   Bot está online!                   <      ");
        Console.ResetColor();
        commands.RegisterCommands<funcoes>();
        
        await discord.ConnectAsync();
        await Task.Delay(-1);

        
    }

    
}
void verify()
{

    Console.Title = "[BT] | N3rdyBot";
    Console.Write("Banco de Dados | ");
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.Write("Conectando...");
    Console.ResetColor();
    
    
    mysql.iniciar_conexao();


    Console.Title = "[BS] | N3rdyBot";
    Console.Write("\nBanco de Dados | ");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("Conectado com Sucesso!");
    Console.ResetColor();
    

}
Console.ReadKey();