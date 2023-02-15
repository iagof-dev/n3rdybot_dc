using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Net;
using DSharpPlus.VoiceNext;
using MySql.Data.MySqlClient;
using N3rdyBacon.Commands;
using MySql;
using MySql.Data;
using N3rdyBacon;
using DSharpPlus.Interactivity.Extensions;
using System.Reflection.Metadata;
using System.Data;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.Cms;
using System.Threading.Channels;
using Google.Protobuf;
using MySqlX.XDevAPI;
using System.Net.NetworkInformation;

namespace N3rdyBacon.Commands
{
    public class funcoes : BaseCommandModule
    {

        [Command("limpar")]
        public async Task limpar(CommandContext ctx, int quantidade = 0)
        {
            string retorno = (emojis.error + " | Sem Permissão!");

            var cargo = ctx.Member.Roles.First();

            if (cargo.Permissions.HasPermission(Permissions.ManageMessages))
            {
                if (quantidade != 0)
                {
                    var chat = ctx.Channel;
                    var mensagens = await chat.GetMessagesAsync(quantidade);


                    //12 msg = rate limit
                    int cmsg = 0;
                    foreach (var mensagem in mensagens)
                    {
                        cmsg += 1;
                        if (cmsg >= 10)
                        {
                            Thread.Sleep(1500);
                            cmsg = 0;
                            Thread.Sleep(1500);
                        }
                        else
                        {
                            await mensagem.DeleteAsync();
                            Thread.Sleep(300);
                        }

                    }
                    retorno = ($"{emojis.trash_bin} | Chat limpo com sucesso!");
                }
                else
                {
                    retorno = ($"{emojis.error} | Você precisa especificar a quantidade de mensagens que deseja apagar!");
                }

            }

            await ctx.RespondAsync(retorno);
        }

        [Command("configurar")]
        public async Task configurar(CommandContext ctx)
        {

            var loading = await ctx.RespondAsync(emojis.dbload);
            
            bool dev = banco.e_dev(ctx.User.Id);

            Thread.Sleep(500);
            
            if (ctx.Guild.OwnerId == ctx.User.Id || dev == true)
            {
                await loading.DeleteAsync();
                
                ulong chat_logs_id = 0;
                ulong chat_ban_id = 0;

                var chat_logs_msg = await ctx.RespondAsync("Digite o ID do canal para enviar logs de comandos (10s)");
                var chat_logs_resp = await ctx.Channel.GetNextMessageAsync(TimeSpan.FromSeconds(10));
                chat_logs_id = ulong.Parse(chat_logs_resp.Result.Content);
                await chat_logs_resp.Result.DeleteAsync();
                await chat_logs_msg.DeleteAsync();

                var chat_bans_msg = await ctx.RespondAsync("Digite o ID do canal para enviar aviso de banimento de usuarios (10s)");
                var chat_bans_resp = await ctx.Channel.GetNextMessageAsync(TimeSpan.FromSeconds(10));
                chat_ban_id = ulong.Parse(chat_bans_resp.Result.Content);
                await chat_bans_msg.DeleteAsync();
                await chat_bans_resp.Result.DeleteAsync();
                

                var confirmacao = await ctx.Channel.SendMessageAsync($"Chat LOGS ID: {chat_logs_resp.Result.Content}\n" +
                    $"Chat BANS ID: {chat_bans_resp.Result.Content}" +
                    "\n\nConfirma? (10s)");
                var emoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
                await confirmacao.CreateReactionAsync(emoji);
                var member = ctx.Member;
                var confirmado = await confirmacao.WaitForReactionAsync(member, emoji, TimeSpan.FromSeconds(10));
                if (!confirmado.TimedOut)
                {
                    await confirmacao.DeleteAsync();
                    await ctx.RespondAsync($"{emojis.sucesso} Sucesso! | Enviando configurações...");
                    banco.server_setup(ctx.Guild.Id, chat_ban_id, chat_logs_id);

                }
                else
                {
                    await confirmacao.DeleteAsync();
                    await ctx.RespondAsync($"{emojis.error} Erro! | Você não fez a confirmação das configurações...");
                }

            }
            else
            {
                await ctx.RespondAsync($"{emojis.error} Erro! | Você não tem permissão para executar este comando!");
            }
        }

        [Command("dev")]
        public async Task dev(CommandContext ctx, string option = "", ulong valor = 0, string valor2 = "")
        {
            var loading = await ctx.RespondAsync(emojis.dbload);
            bool dev = banco.e_dev(ctx.User.Id);
            if (dev != false)
            {
                if (option == "" || option == null)
                {
                    string cmds = emojis.active_dev + " Developer Commands:" + "\n```" +
                        "$dev restart <0/1>\n" +
                        "$dev off <0/1>\n" +
                        "$dev cban <id_server>\n" +
                        "$dev log <id_server>\n" +
                        "$dev mysql <-1/1>\n" +
                        "$dev smysql <0/1/>\n" +
                        "$dev cstatus <0/1/2/3/4> <texto>\n" +
                        "$dev configst <server_id>\n" +
                        "$dev setsaldo <user_id> <valor>" +
                        "```";
                    await ctx.RespondAsync(cmds);
                    await loading.DeleteAsync();

                }
                else
                {
                    string retorno = String.Empty;
                    await Task.Run(() => { retorno = banco.developer(ctx, option, valor, valor2); });
                    await ctx.RespondAsync($"{emojis.active_dev} Developer Commands:\n" + retorno);
                    await loading.DeleteAsync();
                    
                }
            }
            else
            {
                await ctx.RespondAsync($"{emojis.error} | Você não é um desenvolvedor!");
            }
        }
        
        [Command("roleta")]
        [Cooldown(1, 10000, CooldownBucketType.User)]
        public async Task roleta(CommandContext ctx, double valor = 0)
        {
            bool edev = banco.e_dev(ctx.User.Id);

            var loading = await ctx.RespondAsync(emojis.dbload);
            if (valor != 0)
            {

                double saldo = banco.get_saldo(ctx.User.Id);
                if (saldo < valor)
                {
                    await loading.DeleteAsync();
                    await ctx.RespondAsync($"{emojis.error} Erro! Sem saldo para fazer aposta...");
                }
                else
                {
                    await loading.DeleteAsync();
                    var teste = await ctx.RespondAsync($"<@{ctx.User.Id}>\n{emojis.loading} Rodando... (25%)");
                    Thread.Sleep(5000);
                    double valor_ganho = banco.roleta(ctx.User.Id, valor, saldo);
                    string verify = Convert.ToString(valor_ganho);
                    await teste.DeleteAsync();
                    string mensagem = string.Empty;
                    if (verify.Contains("-"))
                    {
                        mensagem = ($"{emojis.notstonks} Perdeu! {valor_ganho} pila");
                    }
                    else
                    {
                        mensagem = ($"{emojis.stonks} Ganhou! {valor_ganho} pila");
                    }

                    var deletar_after = await ctx.RespondAsync(mensagem);
                    Thread.Sleep(15000);
                    await deletar_after.DeleteAsync();
                }
                banco.dev = false;
            }
            else
            {
                await loading.DeleteAsync();
                await ctx.RespondAsync("Uso:\n```$roleta <valor>```\n25% de dobrar o valor");
            }
        }

        [Command("ping")]
        public async Task píng(CommandContext ctx)
        {
            var latencia = (DateTime.Now.Millisecond - ctx.Message.Timestamp.DateTime.Millisecond);
            int db_resp = mysql.latencia(DateTime.Now);
            ctx.RespondAsync($"{emojis.working} Status\n" +
                $"```" +
                $"API: {latencia}ms\n" +
                $"DB: {db_resp}ms" +
                $"```");
        }

        [Command("banir")]
        public async Task banir(CommandContext ctx, DiscordMember membro, string motivo)
        {
            var load = await ctx.RespondAsync(emojis.loading);
            bool dev = false;
            await Task.Run(() => {dev = banco.e_dev(ctx.User.Id); });

            if (dev == true)
            {
                await load.DeleteAsync();
                DiscordChannel membro_dm = membro.CreateDmChannelAsync().Result;
                ulong banchat = banco.get_chatban(ctx.Guild.Id);
                DiscordChannel channel = ctx.Guild.GetChannel(banchat);
                string server_name = ctx.Guild.Name;
                var server_pfp = ctx.Guild.IconUrl;

                
                await ctx.RespondAsync(emojis.sucesso + " Membro banido com sucesso!\n```\nMembro: " + membro.Username + "#" + membro.Discriminator + "\nMotivo: " + motivo + "```");

                var embed = new DiscordEmbedBuilder();
                embed.WithTitle(server_name);
                embed.WithDescription("Um usuário foi banido!");
                embed.WithThumbnail(server_pfp);
                embed.AddField("Nome:", $"{membro.Username}#{membro.Discriminator}", true);
                embed.AddField("Autor:", $"{ctx.User.Username}#{ctx.User.Discriminator}", true);
                embed.AddField("Motivo:", $"{motivo}", false);
                embed.AddField("Banimento:", $"00:01 - 01/01/0000", true);
                embed.AddField("Expirar:", $"11:59 - 31/12/9999", true);
                embed.WithFooter($"| © Copyright reserved to {config.bot_name} 2022-2023 |");
                embed.WithColor(DiscordColor.Aquamarine);

                var message = embed.Build();
                
                await ctx.Client.SendMessageAsync(channel, embed);
                await ctx.Guild.BanMemberAsync(membro, 0, motivo);
                try
                {
                    await ctx.Client.SendMessageAsync(membro_dm, $"{emojis.error} Você foi banido do servidor {server_name}!\nMotivo: {motivo}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            else
            {
                await load.DeleteAsync();
                var resposta = await ctx.RespondAsync(emojis.error + " | Erro! Você não possui permissão");
                Thread.Sleep(5000);
                await resposta.DeleteAsync();
            }
        }
        
        [Command("transferir")]
        public async Task transferir(CommandContext ctx, DiscordMember membro, double valor)
        {
            string valor_conv = Convert.ToString(valor);
            if (valor_conv.Contains("-"))
            {
                Console.WriteLine("Erro! Há simbolo de menos");
                await ctx.RespondAsync($"<@{ctx.User.Id}>\n{emojis.error} Erro! Valor está incorreto...");
            }
            else
            {

                Console.WriteLine("Comando | Transferir foi utilizado por " + ctx.User.Username + " Valor: R$" + valor);
                var emoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
                var teste2 = await ctx.RespondAsync($"<@{ctx.Member.Id}>\nVocê está Transferindo R${valor} para <@{membro.Id}>\nConfirma?");
                var react = teste2.CreateReactionAsync(emoji);

                var member = ctx.Member;

                var result = await teste2.WaitForReactionAsync(member, emoji);



                if (!result.TimedOut)
                {
                    Console.WriteLine("Comando | Transferir foi aceito pelo Usuário");
                    await teste2.DeleteAsync();
                    var loading = await ctx.RespondAsync(emojis.dbload);

                    bool concluido = await Task.Run(() => banco.user_transfer(ctx.User.Id, membro.Id, valor));

                    if (concluido == true)
                    {

                        await loading.DeleteAsync();
                        var success = await ctx.RespondAsync(emojis.sucesso + " Valor foi transferido com sucesso!");
                        await membro.SendMessageAsync(emojis.pix + " Você recebeu uma transferência de R$" + valor + " de " + ctx.User.Username + "#" + ctx.User.Discriminator);
                        Thread.Sleep(15000);
                        await success.DeleteAsync();
                    }
                    else
                    {
                        await loading.DeleteAsync();
                        Console.WriteLine("Sem Saldo");
                        var err = await ctx.RespondAsync(emojis.error + " Erro! Sem Saldo!");
                        Thread.Sleep(5000);
                        await err.DeleteAsync();
                    }
                }
            }
        }
        
        [Command("roubar")]
        public async Task roubar(CommandContext ctx, DiscordMember membro)
        {
            var loading = await ctx.RespondAsync(emojis.dbload);

            if (ctx.User.Id == membro.Id)
            {
                await ctx.RespondAsync($"{emojis.error} Erro! Você não pode roubar a si mesmo...");
                await loading.DeleteAsync();
            }
            else
            {


                bool edev = banco.e_dev(ctx.User.Id);

                if (edev == true)
                {
                    banco.dev = true;

                }
                else
                {
                    banco.dev = false;
                }


                bool assaltoadev = banco.e_dev(membro.Id);
                if (assaltoadev == true)
                {
                    await loading.DeleteAsync();
                    var block = await ctx.RespondAsync(emojis.error + " Erro! Usuário não permitido para Roubar");
                    Thread.Sleep(8000);
                    await block.DeleteAsync();
                }
                else
                {

                    //pegar saldo
                    double user_saldo = banco.get_saldo(ctx.User.Id);
                    double target_saldo = banco.get_saldo(membro.Id);

                    double roubou = 0;


                    await Task.Run(() => { roubou = banco.roubar(ctx.User.Id, membro.Id, user_saldo, target_saldo); });


                    System.Threading.Thread.Sleep(800);

                    if (banco.last_robbed == 0)
                    {
                        //Deu errado
                        await loading.DeleteAsync();
                        await ctx.RespondAsync(emojis.error + " Não conseguiu roubar!");
                    }
                    else
                    {
                        //Deu certo
                        await loading.DeleteAsync();
                        await ctx.RespondAsync(emojis.sucesso + " Roubado com sucesso! Roubou: R$" + roubou);


                    }
                }
            }

        }
        
        [Command("inv")]
        public async Task inv(CommandContext ctx)
        {
            var loading = await ctx.RespondAsync(emojis.dbload);
            string inventario = banco.inv(ctx.User.Id);

            await loading.DeleteAsync();
            await ctx.RespondAsync(inventario);
        }
        
        [Command("matar")]
        public async Task matar(CommandContext ctx, DiscordMember membro)
        {
            var loading = await ctx.RespondAsync(emojis.dbload);
            System.Threading.Thread.Sleep(1500);
            await ctx.RespondAsync($"{emojis.catwork} Em desenvolvimento (não finalizado)...");
            await loading.DeleteAsync();
        }

        [Command("comprar")]
        public async Task comprar(CommandContext ctx)
        {

            var emoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
            var teste2 = await ctx.RespondAsync($"<@{ctx.User.Id}>\nDesejas comprar a armita por R${loja.price_arma}?");
            var react = teste2.CreateReactionAsync(emoji);

            var result = await teste2.WaitForReactionAsync(ctx.User, emoji);

            if (!result.TimedOut)
            {
                await teste2.DeleteAsync();

                var loading = await ctx.RespondAsync(emojis.dbload);

                bool comprou = banco.comprararma(ctx.User.Id);

                if (comprou == true)
                {
                    await loading.DeleteAsync();
                    await ctx.RespondAsync(emojis.sucesso + " | Você comprou uma arminha PEW PEW");
                }
                else
                {
                    await loading.DeleteAsync();
                    await ctx.RespondAsync(emojis.error + " | Você não possui saldo para uma arminha");
                }

            }
            else
            {
                await ctx.RespondAsync(emojis.loading + " | Você não respondeu a tempo...");
            }
        }
        
        [Command("saldo")]
        public async Task saldo(CommandContext ctx, DiscordMember? membro = null)
        {
            Console.WriteLine("Comando | saldo foi utilizado");
            var loading = await ctx.RespondAsync(emojis.dbload);

            if (membro != null)
            {
                Thread.Sleep(500);
                string inventario = String.Empty;
                await Task.Run(() => { inventario = banco.get_inventory(membro.Id); });


                await ctx.RespondAsync($"<@{ctx.User.Id}> {inventario}");
                await loading.DeleteAsync();
            }
            else
            {


                ulong user_id = ctx.Member.Id;
                try
                {
                    double saldinho = banco.get_saldo(user_id);
                    await loading.DeleteAsync();
                    await ctx.RespondAsync($"<@{user_id}>\n{emojis.money} Seu Saldo: R${saldinho}");

                }
                catch (Exception error)
                {
                    await loading.DeleteAsync();
                    await ctx.RespondAsync($"{emojis.error} Erro! Tente novamente...");
                    Console.WriteLine(error);
                }
            }

        }
    }

}