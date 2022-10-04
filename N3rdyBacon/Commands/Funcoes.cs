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

namespace N3rdyBacon.Commands
{
    public class Funcoes : BaseCommandModule
    {
        string emoji_error = "<:error:1005870378473771089>";
        string emoji_sucesso = "<a:sucesso:1005870400196055040>";
        string emoji_loading = "<a:loading:1005865161715875901>";
        string emoji_notstonks = "<:stonks_not:1005868742418366646>";
        string emoji_stonks = "<a:stonks:1005867620446253096>";
        string emoji_money = "<a:money:1008546937244483706>";
        string emoji_dbload = "<a:load:1009212463981531166>";
        string emoji_pix = "<:pix:1009255147014213643>";
        string emoji_catwork = "<:catwork:1009581584380342353>";
        [Command("roleta")]
        //[Cooldown(1, 10000, CooldownBucketType.User)]
        public async Task roleta(CommandContext ctx, double valor)
        {
            bool edev = banco.e_dev(ctx.User.Id);
            var loading = await ctx.RespondAsync(emoji_dbload);
            double saldo = banco.get_saldo(ctx.User.Id);
            if (saldo < valor)
            {
                await loading.DeleteAsync();
                await ctx.RespondAsync(emoji_error + " Erro! Sem saldo para fazer aposta...");
            }
            else {
                await loading.DeleteAsync();
                var teste = await ctx.RespondAsync("<@" + ctx.User.Id + ">\n" + emoji_loading + " Rodando... (25%)");
                Thread.Sleep(5000);
                double valor_ganho = banco.roleta(ctx.User.Id, valor, saldo);
                string verify = Convert.ToString(valor_ganho);
                await teste.DeleteAsync();
                string mensagem = string.Empty;
                if (verify.Contains("-"))
                {
                    mensagem = (emoji_notstonks + " Perdeu! " + valor_ganho + " pila");
                }
                else
                {
                    mensagem = (emoji_stonks + " Ganhou! " + valor_ganho + " pila");
                }

                var deletar_after = await ctx.RespondAsync(mensagem);
                Thread.Sleep(15000);
                await deletar_after.DeleteAsync();
            }
            banco.dev = false;
        }
        [Command("comandos")]
        public async Task comandos(CommandContext ctx)
        {

            ///$saldo
            ///$comprararma
            ///$matar[@user]
            ///$inv
            ///$loja
            ///$roubar[@user]
            ///$verificar[@user]
            ///$transferir[@user][valor]
            ///$setsaldo[@user][valor]
            ///$comandos
            ///$roleta[valor]



            await ctx.RespondAsync("Comandos:```\n$roubar <@usuario> (50% de chance)\n$roleta <valor> (25%)\n$transferir <@usuario> <valor>\n$saldo\n$verificar <@usuario>```");
        }

        [Command("banir")]
        public async Task banir(CommandContext ctx, ulong id, string motivo)
        {
            var load = await ctx.RespondAsync(emoji_loading);
            bool dev = banco.e_dev(ctx.User.Id);
            Thread.Sleep(500);
            if (dev == true)
            {
                await load.DeleteAsync();
            }
            else
            {
                await load.DeleteAsync();
                var resposta = await ctx.RespondAsync(emoji_error + " | Erro! Você não possui permissão");
                Thread.Sleep(5000);
                await resposta.DeleteAsync();
            }
        }


        [Command("setsaldo")]
        public async Task setsaldo(CommandContext ctx, DiscordMember user, double valor)
        {
            Console.WriteLine("Comando | SetSaldo Utilizado");

            bool dev = banco.e_dev(ctx.User.Id);


            if (dev == true)
            {
                banco.set_saldo(user.Id, valor);
                Console.WriteLine("Comando | SetSaldo Usuario possui permissao");
                await ctx.RespondAsync(emoji_sucesso + " Sucesso!\nNovo saldo de R$" + valor + " para " + user.DisplayName);
            }
            else
            {
                Console.WriteLine("Comando | SetSaldo Usuario Sem Permissão");
                await ctx.RespondAsync(emoji_error + "Erro!\nVocê não possui Permissão para utilizar este comando.");
            }

            //banco.set_saldo(user.Id, valor);
        }

        [Command("transferir")]
        public async Task user_transfer(CommandContext ctx, DiscordMember membro, double valor)
        {
            string valor_conv = Convert.ToString(valor);
            if (valor_conv.Contains("-"))
            {
                Console.WriteLine("Erro! Há simbolo de menos");
                await ctx.RespondAsync("<@" + ctx.User.Id + ">\n" + emoji_error + "Erro! Valor está incorreto...");
            }
            else {

                Console.WriteLine("Comando | Transferir foi utilizado por " + ctx.User.Username + " Valor: R$" + valor);
                var emoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
                var teste2 = await ctx.RespondAsync("<@" + ctx.Member.Id + ">\nVocê está Transferindo R$" + valor + " para <@" + membro.Id + ">\nConfirma?");
                var react = teste2.CreateReactionAsync(emoji);

                var member = ctx.Member;

                var result = await teste2.WaitForReactionAsync(member, emoji);



                if (!result.TimedOut)
                {
                    Console.WriteLine("Comando | Transferir foi aceito pelo Usuário");
                    await teste2.DeleteAsync();
                    var loading = await ctx.RespondAsync(emoji_dbload);

                    bool concluido = await Task.Run(() => banco.user_transfer(ctx.User.Id, membro.Id, valor));

                    if (concluido == true)
                    {
                        
                        await loading.DeleteAsync();
                        var success = await ctx.RespondAsync(emoji_sucesso + " Valor foi transferido com sucesso!");
                        await membro.SendMessageAsync(emoji_pix + " Você recebeu uma transferência de R$" + valor + " de " + ctx.User.Username + "#" + ctx.User.Discriminator);
                        Thread.Sleep(15000);
                        await success.DeleteAsync();
                    }
                    else
                    {
                        await loading.DeleteAsync();
                        Console.WriteLine("Sem Saldo");
                        var err = await ctx.RespondAsync(emoji_error + " Erro! Sem Saldo!");
                        Thread.Sleep(5000);
                        await err.DeleteAsync();
                    }
                }
            }
        }

        [Command("verificar")]
        public async Task verificar(CommandContext ctx, DiscordMember membro)
        {
            var loading = await ctx.RespondAsync(emoji_dbload);
            Console.WriteLine("Ok");
            Thread.Sleep(500);
            string inventario = String.Empty;
            await Task.Run(() => { inventario = banco.verifycazao(membro.Id); });
            

            await ctx.RespondAsync("<@" + ctx.User.Id+">" + inventario);
            await loading.DeleteAsync();
        }


        [Command("rodolfo")]
        public async Task rodolfo(CommandContext ctx, string bruh)
        {
            await ctx.RespondAsync(bruh);
        }


        [Command("roubar")]
        public async Task roubar(CommandContext ctx, DiscordMember membro)
        {
            var loading = await ctx.RespondAsync(emoji_dbload);

            if (ctx.User.Id == membro.Id)
            {
                await ctx.RespondAsync(emoji_error + " Erro! Você não pode roubar a si mesmo...");
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
                    var block = await ctx.RespondAsync(emoji_error + " Erro! Usuário não permitido para Roubar");
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
                        await ctx.RespondAsync(emoji_error + " Não conseguiu roubar!");
                    }
                    else
                    {
                        //Deu certo
                        await loading.DeleteAsync();
                        await ctx.RespondAsync(emoji_sucesso + " Roubado com sucesso! Roubou: R$" + roubou);


                    }
                }
            }

        }

        [Command("loja")]
        public async Task loja(CommandContext ctx)
        {
            await ctx.RespondAsync("Loja de Itens: ```Armas Brancas/Cinzas:\n-🔫 Arma = R$200.000 ($comprararma)```");
        }

        [Command("inv")]
        public async Task inv(CommandContext ctx)
        {
            var loading = await ctx.RespondAsync(emoji_dbload);
            string inventario = banco.inv(ctx.User.Id);

            await loading.DeleteAsync();
            await ctx.RespondAsync(inventario);
        }

        [Command("debug")]
        public async Task debug(CommandContext ctx, int valor)
        {
            bool ver = banco.e_dev(ctx.User.Id);
            if (ver == true)
            {
                switch (valor)
                {
                    case 0:
                        bool teste = banco.debug();
                        await ctx.RespondAsync("Debug: " + teste);
                        break;
                    case 1:
                        banco.debug2(valor);
                        break;
                    case 2:
                        banco.debug2(valor);
                        break;
                }
                
            }
        }


        [Command ("matar")]
        public async Task matar(CommandContext ctx, DiscordMember membro)
        {
            var loading = await ctx.RespondAsync(emoji_dbload);

            System.Threading.Thread.Sleep(1500);
            await ctx.RespondAsync(emoji_catwork + " Em desenvolvimento (não finalizado)...");


            await loading.DeleteAsync();
        }


        [Command ("comprararma")]
        public async Task comprararma(CommandContext ctx)
        {
           
            var emoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
            var teste2 = await ctx.RespondAsync("<@"+ ctx.User.Id + ">\nDesejas comprar a armita por R$" + banco.preco + "?");
            var react = teste2.CreateReactionAsync(emoji);

            var result = await teste2.WaitForReactionAsync(ctx.User, emoji);

            if (!result.TimedOut)
            {
                await teste2.DeleteAsync();

                var loading = await ctx.RespondAsync(emoji_dbload);

                bool comprou = banco.comprararma(ctx.User.Id);

                if (comprou == true)
                {
                    await loading.DeleteAsync();
                    await ctx.RespondAsync(emoji_sucesso + " | Você comprou uma arminha PEW PEW");
                }
                else
                {
                    await loading.DeleteAsync();
                    await ctx.RespondAsync(emoji_error + " | Você não possui saldo para uma arminha");
                }

            }
            else
            {
                await ctx.RespondAsync(emoji_loading + " | Você não respondeu a tempo...");
            }
        }
        
        [Command("saldo")]
        public async Task saldo(CommandContext ctx)
        {
            Console.WriteLine("Comando | saldo foi utilizado");

            var loading = await ctx.RespondAsync(emoji_dbload);

            ulong user_id = ctx.Member.Id;
            try
            {
                double saldinho = banco.get_saldo(user_id);
                await loading.DeleteAsync();
                await ctx.RespondAsync("<@" + user_id + ">\n" + emoji_money + "Seu Saldo: R$" + saldinho);

            }
            catch (Exception error)
            {
                await loading.DeleteAsync();
                await ctx.RespondAsync(emoji_error + " Erro! Tente novamente...");
                Console.WriteLine(error);
            }
        }
    }

}