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
using N3rdyBacon.Commands;
using System.Net;

///github.com/n3rdydzn
///Started: 12:10 PM | 06/08/2022
///Status: Not Finished...


///Banco de Dados Flexível
string db_dados = "server=" + N3rdyBacon.Config.db_server + ";database=" + N3rdyBacon.Config.db_data + ";Uid=" + N3rdyBacon.Config.db_user + ";pwd=" + N3rdyBacon.Config.db_pass;
MySqlConnection con_db = new MySqlConnection(db_dados);


string bot_appid = N3rdyBacon.Config.bot_appid;
string bot_token = N3rdyBacon.Config.bot_token;
bool bot_on = false;

WebClient web = new WebClient();
string bot_title = web.DownloadString("https://pastebin.com/raw/5s8KFjCV");



main();


void main()
{
    Console.Title = "[ST] | N3rdyBot";
    verify();
    start(bot_on, bot_token, bot_title).GetAwaiter().GetResult();

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
        MinimumLogLevel = LogLevel.Debug,
        LogTimestampFormat = "hh:mm:ss tt",
        Intents = DiscordIntents.AllUnprivileged
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
        commands.RegisterCommands<Funcoes>();
        
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
    
    
    banco.iniciar_conexao();


    Console.Title = "[BS] | N3rdyBot";
    Console.Write("\nBanco de Dados | ");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("Conectado com Sucesso!");
    Console.ResetColor();
    
}


Console.ReadKey();