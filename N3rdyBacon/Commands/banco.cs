using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using log4net.Util;
using System.Timers;
using DSharpPlus.EventArgs;
using System.Data;
using System.Configuration;
using DSharpPlus.CommandsNext;
using static System.Net.Mime.MediaTypeNames;
using Org.BouncyCastle.Bcpg;
using DSharpPlus.Entities;
using System.Diagnostics;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Signers;
using DSharpPlus.VoiceNext;
using System.Diagnostics.Metrics;

namespace N3rdyBacon.Commands
{
    public class banco
    {

        public static string db_table_users = "users";
        public static string db_table_servers = "servers";
        public static bool dev = false;
        public static long server_channel_chatban = 0;
        public static long server_channel_logs = 0;

        public static int debug_status = 0;


        public static void server_setup(ulong server_id, ulong chat_bans, ulong chat_logs)
        {
            Console.WriteLine("Server Setup | Definindo configurações: " + server_id + " | Ban: " + chat_bans + " | Logs: " + chat_logs);
            bool configurado = server_config(server_id);
            string comando = string.Empty;

            if (configurado != false)
            {
                comando = $"UPDATE servers SET clogs={chat_logs}, cban={chat_bans} WHERE id={server_id}; ";
            }
            else
            {
                comando = $"INSERT INTO servers values({server_id},true,{chat_logs},{chat_bans}); ";
            }
            mysql.send_value(comando);
        }
        public static bool server_config(ulong server_id)
        {
            //verificação se o servidor esta configurado
            //No mysql possui uma tabela chamada servers, onde contem o id do servidor,id do canal de logs, id canal de bans
            //e um bool para verificar se o servidor esta configurado ou não (true/false)/(1/0)

            bool configurado = false;

            string comando_getconfig = "select * from " + db_table_servers + " where id='" + server_id + "';";
            MySqlCommand comm = new MySqlCommand();
            var com = mysql.con_db.CreateCommand();
            com.CommandText = comando_getconfig;
            var reader = com.ExecuteReader();
            while (reader.Read())
            {
                configurado = reader.GetBoolean("config");

            }

            reader.Close();

            return configurado;
        }
        public static double get_saldo(ulong id)
        {
            double user_sld = 0;

            string comando_saldo = "select * from " + db_table_users + " where id='" + id + "';";
            MySqlCommand comm = new MySqlCommand();
            var com = mysql.con_db.CreateCommand();
            com.CommandText = comando_saldo;
            var reader = com.ExecuteReader();
            while (reader.Read())
            {
                user_sld = reader.GetDouble("saldo");
            }

            reader.Close();
            return user_sld;
        }
        public static ulong get_chatban(ulong id)
        {
            ulong chat_id = 0;

            string comando_chatban = "select * from " + db_table_servers + " where id='" + id + "';";
            MySqlCommand comm = new MySqlCommand();
            var com = mysql.con_db.CreateCommand();
            com.CommandText = comando_chatban;
            var reader = com.ExecuteReader();

            debug_status += 1;
            Console.WriteLine("DEBUG | Status: " + debug_status);

            while (reader.Read())
            {
                chat_id = reader.GetUInt64("cban");
            }
            debug_status += 1;
            Console.WriteLine("DEBUG | Status: " + debug_status);
            reader.Close();
            return chat_id;
        }
        public static ulong get_chatlogs(ulong id)
        {
            bool config = false;
            ulong chat_id = 0;

            string comando_saldo = "select * from " + db_table_servers + " where id='" + id + "';";
            MySqlCommand comm = new MySqlCommand();
            var com = mysql.con_db.CreateCommand();
            com.CommandText = comando_saldo;
            var reader = com.ExecuteReader();
            while (reader.Read())
            {
                config = reader.GetBoolean("config");
                if (config == true)
                {
                    chat_id = reader.GetUInt64("clogs");
                }
                else
                {
                    chat_id = 0;
                }
            }

            reader.Close();
            return chat_id;
        }
        //
        ///User_Verify();
        ///Função: Verifica se usuario existe no banco de dados;
        ///Se sim, não faz nada | Se não, Registra usuário (id, saldo, arma...);
        ///Retorno: não;
        public static void user_verify(ulong id)
        {
            string retornar = "";
            Thread.Sleep(1500);
            bool valido = mysql.status_con();
            if (valido != false)
            {
                Console.WriteLine($"Verificação | Verificando ID('{id}')");
                string verify_existuser = ($"select * from {db_table_users} where id={id};");
                MySqlCommand user_exists = new MySqlCommand();
                var user_existss = mysql.con_db.CreateCommand();
                user_existss.CommandText = verify_existuser;
                var user_ver = user_existss.ExecuteReader();
                if (user_ver.HasRows)
                {
                    user_ver.Close();
                    Console.WriteLine("Verificação | ID Existe no banco de dados");
                }
                else
                {
                    user_ver.Close();
                    Console.WriteLine("Verificação | ID Não Existe");
                    string criar_user = ($"INSERT INTO {db_table_users} VALUES ({id}, 0, null, null, null, null);");
                    Console.WriteLine(criar_user);
                    mysql.send_value(criar_user);
                    Console.WriteLine($"Verificação | ID ({id}) Registrado no banco!");
                }
                System.Threading.Thread.Sleep(3000);
            }
        }
        public static bool e_dev(ulong id)
        {
            string retorno = "É Dev? | Não é o brabo";
            Console.WriteLine("É Dev? | Verificando...");

            if (id == config.dev_id || id == config.bot_id)
            {
                retorno = ("É Dev? | Sim é o brabo");
                Console.WriteLine(retorno);
                banco.dev = true;
                return true;
            }
            else
            {
                Console.WriteLine(retorno);
                banco.dev = false;
                return false;
            }
        }
        public static bool user_transfer(ulong user_id, ulong target_id, double transfer_saldo)
        {
            banco.user_verify(user_id);
            banco.user_verify(target_id);
            double user_saldo = 0;
            double target_saldo = 0;
            string get_tgsaldo = ($"select * from {db_table_users} where id='{target_id}';");
            MySqlCommand com_tgsaldo = new MySqlCommand();
            var com = mysql.con_db.CreateCommand();
            com.CommandText = get_tgsaldo;
            var reader = com.ExecuteReader();
            while (reader.Read())
            {
                target_saldo = reader.GetDouble("saldo");
            }
            reader.Close();
            string get_usersaldo = ($"select * from {db_table_users} where id='{user_id}';");
            MySqlCommand com_usersaldo = new MySqlCommand();
            var com2 = mysql.con_db.CreateCommand();
            com2.CommandText = get_usersaldo;
            var reader2 = com2.ExecuteReader();
            while (reader2.Read())
            {
                user_saldo = reader2.GetDouble("saldo");
            }
            reader2.Close();
            if (user_saldo > transfer_saldo)
            {
                double final_saldouser = user_saldo - transfer_saldo;
                double final_saldotgt = target_saldo + transfer_saldo;
                Console.WriteLine("Verificação | Usuario Possui Saldo suficiente!");
                string set_usersaldo = "update " + db_table_users + " set saldo='" + final_saldouser + "' where id='" + user_id + "';";
                MySqlCommand setar_usersaldo = new MySqlCommand();
                var com3 = mysql.con_db.CreateCommand();
                com3.CommandText = set_usersaldo;
                var teste1 = com3.ExecuteNonQuery();
                string set_tgtsaldo = "update " + db_table_users + " set saldo='" + final_saldotgt + "' where id='" + target_id + "';";
                MySqlCommand setar_tgtsaldo = new MySqlCommand();
                var com4 = mysql.con_db.CreateCommand();
                com4.CommandText = set_tgtsaldo;
                var teste2 = com4.ExecuteNonQuery();
                Console.WriteLine("Comando | Concluido!");
                return true;
            }
            else
            {
                Console.WriteLine("Verificação | Usuario não possui Saldo suficiente!");
                return false;
            }
        }
        public static double roleta(ulong id, double valor, double saldo)
        {
            //probabilidade de cair 75 pra cima
            //25% de chance de ganhar
            Random rnd = new Random();
            int sorte_azar = rnd.Next(0, 100);
            int chance = 50;
            double valor_setar = 0;

            double perdeu = 0;
            double perde = 0;

            if (dev == true)
            {
                chance = 0;
            }
            else
            {
                chance = 75;
            }
            if (sorte_azar >= chance)
            {
                Console.WriteLine($"{sorte_azar} - Sorte");
                valor_setar = valor * 1.5;
                perdeu = valor_setar + saldo;
                perde = valor_setar;

            }
            else
            {
                Console.WriteLine($"{sorte_azar} - Azar");
                valor_setar = saldo - valor;
                perdeu = valor_setar;
                perde = -valor;

            }
            string set_usersaldo = ($"update {db_table_users} set saldo='{perdeu}' where id='{id}';");
            MySqlCommand setar_usersaldo = new MySqlCommand();
            var com3 = mysql.con_db.CreateCommand();
            com3.CommandText = set_usersaldo;
            var teste1 = com3.ExecuteNonQuery();
            return perde;
            //double teste = banco.get_saldo(id);
        }
        
        public static double last_robbed = 0;
        public static double roubar(ulong user_id, ulong target_id, double user_saldo, double target_saldo)
        {
            //10% de roubar
            Random rnd = new Random();
            int chance = 90;
            int sorte_azar = rnd.Next(1, 100);
            bool edevi = banco.e_dev(user_id);
            Console.WriteLine("Roubar | " + sorte_azar);
            if (edevi == true)
            {
                chance = 0;
            }
            else
            {
                chance = 75;
            }
            if (sorte_azar >= chance)
            {
                double valor_roubar = rnd.Next(20, 2896);
                last_robbed = valor_roubar;
                double target_setar = target_saldo - valor_roubar;
                Console.WriteLine("Roubar | Valor Roubado: R$" + valor_roubar);
                double user_setar = user_saldo + valor_roubar;
                string user_set = "update " + db_table_users + " set saldo='" + user_setar + "' where id='" + user_id + "';";
                string target_set = "update " + db_table_users + " set saldo='" + target_setar + "' where id='" + target_id + "';";
                MySqlCommand setsaldo_user = new MySqlCommand();
                var settar_user = mysql.con_db.CreateCommand();
                settar_user.CommandText = user_set;
                var bom1 = settar_user.ExecuteNonQuery();
                Console.WriteLine("Ok");
                MySqlCommand setsaldo_target = new MySqlCommand();
                var settar_target = mysql.con_db.CreateCommand();
                settar_target.CommandText = target_set;
                var bom2 = settar_target.ExecuteNonQuery();
                return valor_roubar;
            }
            else
            {
                last_robbed = 0;
                return 0;
            }

        }
        public static bool comprararma(ulong id)
        {
            int ownguns = 0;
            double saldo = banco.get_saldo(id);

            if (saldo >= loja.price_arma)
            {
                //Comprar
                MySqlCommand vergun = new MySqlCommand();
                string verguncom = "select * from " + db_table_users + " where id='" + id + "';";
                var vergun_r = mysql.con_db.CreateCommand();
                vergun_r.CommandText = verguncom;
                var resuytkladio = vergun_r.ExecuteReader();
                while (resuytkladio.Read())
                {
                    ownguns = resuytkladio.GetInt32("arma");
                }
                resuytkladio.Close();
                double valor_setar = saldo - loja.price_arma;
                MySqlCommand retvalor = new MySqlCommand();
                string setvalor_st = "update " + db_table_users + " set saldo='" + valor_setar + "' where id='" + id + "';";
                var setvalor = mysql.con_db.CreateCommand();
                setvalor.CommandText = setvalor_st;
                var setvalorzado = setvalor.ExecuteNonQuery();
                ownguns += 1;
                MySqlCommand buygun = new MySqlCommand();
                string setgun = "update " + db_table_users + " set arma='" + ownguns + "' where id='" + id + "';";
                var setgun_m = mysql.con_db.CreateCommand();
                setgun_m.CommandText = setgun;
                var setado = setgun_m.ExecuteNonQuery();
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string get_inventory(ulong id)
        {
            double espiao = banco.get_saldo(id);
            System.Threading.Thread.Sleep(1500);
            string inventario = banco.inv(id);
            string retornar = $"\nSaldo de <@{id}> : R${espiao}";
            return retornar;
        }
        public static string inv(ulong id)
        {
            string dados = string.Empty;
            double saldo = 0;
            int armas = 0;
            MySqlCommand inv_dados = new MySqlCommand();
            string invcom = "select * from " + db_table_users + " where id='" + id + "';";
            var invinfo = mysql.con_db.CreateCommand();
            invinfo.CommandText = invcom;
            var reader = invinfo.ExecuteReader();
            while (reader.Read())
            {
                saldo = reader.GetDouble("saldo");
            }
            reader.Close();
            if (armas <= 0 && saldo < 0)
            {
                dados = "Inventário:\n```nada...```";
            }
            else
            {
                dados = "Inventário:\n```" + saldo + "x | Dinheirinhos\n" + armas + "x | Armita```";
            }
            return dados;
        }

        /// 
        /// Developer Commands
        /// Função: Teste do bot
        /// 
        public static string developer(CommandContext ctx, string option = "", ulong valor = 0, string valor2 = "")
        {
            string retornar = string.Empty;
            switch (option)
            {
                case "off":
                    Console.WriteLine("Developer Commands | Fechando Conexão com MySql");
                    mysql.con_db.Close();
                    Thread.Sleep(250);
                    Console.WriteLine("Developer Commands | Desligando bot...");
                    config.bot_on = false;
                    Environment.Exit(0);
                    break;

                case "restart":
                    retornar = ("Developer Commands | Path: " + Environment.ProcessPath);
                    Console.WriteLine(retornar);
                    Console.WriteLine("Developer Commands | Reiniciando bot...");
                    //System.Diagnostics.Process.Start(Environment.ProcessPath);
                    Environment.Exit(0);
                    break;

                case "cban":
                    ulong banchat = get_chatban(valor);
                    DiscordChannel channel = ctx.Guild.GetChannel(banchat);

                    string banq = "Alguem foi banido hehe";
                    Console.WriteLine("CANAU: " + channel);
                    ctx.Client.SendMessageAsync(channel, banq);
                    return "Sucesso";
                    break;

                case "log":
                    ulong logchat = get_chatlogs(valor);
                    DiscordChannel canau = ctx.Guild.GetChannel(logchat);

                    Console.WriteLine("CANAU: " + canau);
                    ctx.Client.SendMessageAsync(canau, "Teste");
                    return "Sucesso";
                    break;
                case "mysql":
                    string debug = "0";
                    
                    return ("Developer Commands | Conexão MySQL:" + debug);
                    break;
                case "smysql":
                    string bruh = string.Empty;
                    if (valor == 0)
                    {
                        Console.WriteLine("Conexão Fechada!");
                        mysql.con_db.Close();
                        bruh = "False";
                    }
                    if (valor == 1)
                    {
                        Console.WriteLine("Conexão Aberta!");
                        mysql.con_db.Open();
                        bruh = "True";
                    }
                    return bruh;
                    break;

                case "cstatus":
                    DiscordActivity activity = new DiscordActivity(valor2, ActivityType.Playing);
                    string status = valor2;
                    string tipo = string.Empty;
                    if (valor == 0)
                    {
                        activity = new DiscordActivity
                        {
                            Name = status,
                            ActivityType = ActivityType.Playing
                        };
                        tipo = "Playing";
                    }
                    if (valor == 1)
                    {
                        activity = new DiscordActivity
                        {
                            Name = status,
                            ActivityType = ActivityType.Competing
                        };
                        tipo = "Competing";
                    }
                    if (valor == 2)
                    {
                        activity = new DiscordActivity
                        {
                            Name = status,
                            ActivityType = ActivityType.Watching
                        };
                        tipo = "Watching";

                    }
                    if (valor == 3)
                    {
                        activity = new DiscordActivity
                        {
                            Name = status,
                            ActivityType = ActivityType.Streaming
                        };
                        tipo = "Streaming";

                    }
                    if (valor == 4)
                    {
                        activity = new DiscordActivity
                        {
                            Name = status,
                            ActivityType = ActivityType.ListeningTo
                        };
                        tipo = "ListeningTo";
                    }
                    retornar = "Status definido para: " + valor2 + "\nTipo: " + tipo;

                    ctx.Client.UpdateStatusAsync(activity);
                    break;

                case "configst":
                    bool configurado = server_config(valor);
                    Console.WriteLine("DEBUG | DEVELOPER SWITCH, VALOR RECEBIDO: " + configurado);
                    if (configurado == true)
                    {
                        Console.WriteLine("DEBUG | Entrou true");

                        ulong chat_ban = get_chatban(valor);
                        Console.WriteLine("DEBUG | passou #1");

                        ulong chat_logs = get_chatban(valor);
                        Console.WriteLine("DEBUG | passou #2");

                        string valor_ren = emojis.sucesso + "Servidor está configurado!\nInformações:```" +
                            "\nServer Id: " + valor +
                            "\nChat de Ban: " + chat_ban +
                            "\nChat de Logs: " + chat_logs +
                            "```";
                        Console.WriteLine("DEBUG | passou #3");

                        retornar = valor_ren;
                    }
                    else
                    {
                        Console.WriteLine("DEBUG | Entrou falso");

                        string valor_ren = emojis.error + "Servidor não configurado!";
                        retornar = valor_ren;
                    }
                    break;


                case "mscon":
                    break;
                case "msplay":
                    break;
                case "msstop":
                    break;

                case "webhookteste":
                    retornar = "enviando...";
                    break;

                case "getlogs":
                    retornar = ($"Logs: \n```{config.logs}```");
                    break;
                case "setsaldo":
                    string comando = ($"update {config.db_table_users} set saldo={valor2} where id={valor};");
                    mysql.send_value(comando);
                    retornar = ($"Developer Commands | {emojis.sucesso} Saldo de ({valor}) definido para {valor2}!");
                    break;
                   

            }
            return retornar;
        }
    }
}
