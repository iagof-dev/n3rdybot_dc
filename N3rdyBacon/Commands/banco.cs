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
namespace N3rdyBacon.Commands
{
    public class banco
    {
        public static string db_dados = "server=" + N3rdyBacon.Config.db_server + ";database=" + N3rdyBacon.Config.db_data
        + ";Uid=" + N3rdyBacon.Config.db_user + ";pwd=" + N3rdyBacon.Config.db_pass + ";Connection Timeout=100";
        static MySqlConnection con_db = new MySqlConnection(db_dados);
        public static string db_table = "users";
        public static bool dev = false;
        public static void iniciar_conexao()
        {
            try
            {

                con_db.Open();
            }
            catch (Exception dberror)
            {
                Console.Title = "[BR] | N3rdyBot";
                Console.Write("\nBanco de Dados | ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Erro!");
                Console.WriteLine("\n" + dberror);
                Console.ResetColor();
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        public static double get_saldo(ulong id)
        {
            double user_sld = 0;
            string verifica = banco.verify(id);

            if (verifica.Contains("Banido!"))
            {

            }
            else
            {
                string comando_saldo = "select * from " + db_table + " where id='" + id + "'";
                MySqlCommand comm = new MySqlCommand();
                var com = con_db.CreateCommand();
                com.CommandText = comando_saldo;
                var reader = com.ExecuteReader();
                while (reader.Read())
                {
                    user_sld = reader.GetDouble("saldo");
                }

                reader.Close();
            }
            return user_sld;
        }

        public static string verify(ulong id)
        {
            bool banido = false;
            string ban_mot = string.Empty;
            string ban_author = string.Empty;
            string retornar = "Ok";


            Thread.Sleep(1500);
            Console.WriteLine("MySql | Verificando status...");
            if (banco.con_db.State == ConnectionState.Open)
            {
                //Aberto
                Console.WriteLine("MySql | Conexão Está Aberta!");
            }
            else
            {
                //Fechado
                Console.WriteLine("MySql | Conexão está fechada, Iniciando nova conexão...");
                banco.iniciar_conexao();
            }
            Console.WriteLine("MySql | Status= " + con_db.State);
            Console.WriteLine("Verificação | Verificando ID('" + id + "')");
            string verify_existuser = "select * from " + db_table + " where id='" + id + "';";
            MySqlCommand user_exists = new MySqlCommand();
            var user_existss = con_db.CreateCommand();
            user_existss.CommandText = verify_existuser;
            var user_ver = user_existss.ExecuteReader();
            if (user_ver.HasRows)
            {
                Console.WriteLine("Verificação | ID Existe no banco de dados");

                while (user_ver.Read())
                {
                    if (user_ver.GetBoolean("ban") == true)
                    {
                        Console.WriteLine("MySql | USUARIO ESTÁ BANIDO!!!!!!!");
                        banido = true;
                        ban_mot = user_ver.GetString("ban_reason");
                        ban_author = user_ver.GetString("ban_author");
                        retornar = "Banido!\nMotivo: " + ban_mot + "\nAutor: " + ban_author;
                    }
                    else
                    {
                        Console.WriteLine("MySql | Usuario não está banido :)");
                    }
                }
                user_ver.Close();
            }
            else
            {
                user_ver.Close();
                Console.WriteLine("Verificação | ID Não Existe");
                string criar_user = "insert into " + db_table + " values ('" + id + "', '0', default, default);";
                MySqlCommand create_user = new MySqlCommand();
                var criarusr = con_db.CreateCommand();
                criarusr.CommandText = criar_user;
                var usrcr = criarusr.ExecuteNonQuery();
                Console.WriteLine("Verificação | ID (" + id + ") Registrado no banco!");
            }
            System.Threading.Thread.Sleep(3000);
            return retornar;
        }

        public static bool e_dev(ulong id)
        {
            Console.WriteLine("É Dev? | Verificando...");
            ulong id_dev = 833438251553128448;
            ulong id_bot = 956660539113738250;

            if (id == id_dev || id == id_bot)
            {
                Console.WriteLine("É Dev? | Sim é o brabo");
                banco.dev = true;
                return true;
            }
            else
            {
                Console.WriteLine("É Dev? | Não é o brabo");
                banco.dev = false;
                return false;
            }
        }
        public static void debug2(int valor)
        {
            if (valor == 1)
            {
                Console.WriteLine("Conexão Fechada!");
                con_db.Close();
            }
            else
            {
                Console.WriteLine("Conexão Aberta!");
                con_db.Open();
            }
        }
        public static bool debug()
        {
            Console.WriteLine("MySql | Verificando!");
            bool debug = false;
            var temp = con_db.State.ToString();
            if (banco.con_db.State == ConnectionState.Open && temp == "Open")
            {
                debug = true;
            }
            else
            {
                debug = false;
            }
            Console.WriteLine("MySql | Status=" + temp);
            return debug;
        }
        public static bool user_transfer(ulong user_id, ulong target_id, double transfer_saldo)
        {
            banco.verify(user_id);
            banco.verify(target_id);
            double user_saldo = 0;
            double target_saldo = 0;
            string get_tgsaldo = "select * from " + db_table + " where id='" + target_id + "';";
            MySqlCommand com_tgsaldo = new MySqlCommand();
            var com = con_db.CreateCommand();
            com.CommandText = get_tgsaldo;
            var reader = com.ExecuteReader();
            while (reader.Read())
            {
                target_saldo = reader.GetDouble("saldo");
            }
            reader.Close();
            string get_usersaldo = "select * from " + db_table + " where id='" + user_id + "';";
            MySqlCommand com_usersaldo = new MySqlCommand();
            var com2 = con_db.CreateCommand();
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
                string set_usersaldo = "update " + db_table + " set saldo='" + final_saldouser + "' where id='" + user_id + "';";
                MySqlCommand setar_usersaldo = new MySqlCommand();
                var com3 = con_db.CreateCommand();
                com3.CommandText = set_usersaldo;
                var teste1 = com3.ExecuteNonQuery();
                string set_tgtsaldo = "update " + db_table + " set saldo='" + final_saldotgt + "' where id='" + target_id + "';";
                MySqlCommand setar_tgtsaldo = new MySqlCommand();
                var com4 = con_db.CreateCommand();
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

        //probabilidade de cair 75 pra cima
        //25% de chance de ganhar
        public static double roleta(ulong id, double valor, double saldo)
        {
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
                Console.WriteLine(sorte_azar + " - Sorte");
                valor_setar = valor * 1.5;
                perdeu = valor_setar + saldo;
                perde = valor_setar;

            }
            else
            {
                Console.WriteLine(sorte_azar + " - Azar");
                valor_setar = saldo - valor;
                perdeu = valor_setar;
                perde = -valor;

            }
            string set_usersaldo = "update " + db_table + " set saldo='" + perdeu + "' where id='" + id + "';";
            MySqlCommand setar_usersaldo = new MySqlCommand();
            var com3 = con_db.CreateCommand();
            com3.CommandText = set_usersaldo;
            var teste1 = com3.ExecuteNonQuery();
            return perde;
            //double teste = banco.get_saldo(id);
        }


        public static void set_saldo(ulong id, double valor)
        {
            string set_usersaldo = "update " + db_table + " set saldo='" + valor + "' where id='" + id + "';";
            MySqlCommand setar_usersaldo = new MySqlCommand();
            var com3 = con_db.CreateCommand();
            com3.CommandText = set_usersaldo;
            var teste1 = com3.ExecuteNonQuery();
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
                string user_set = "update " + db_table + " set saldo='" + user_setar + "' where id='" + user_id + "';";
                string target_set = "update " + db_table + " set saldo='" + target_setar + "' where id='" + target_id + "';";
                MySqlCommand setsaldo_user = new MySqlCommand();
                var settar_user = con_db.CreateCommand();
                settar_user.CommandText = user_set;
                var bom1 = settar_user.ExecuteNonQuery();
                Console.WriteLine("Ok");
                MySqlCommand setsaldo_target = new MySqlCommand();
                var settar_target = con_db.CreateCommand();
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

        public static double preco = 200000;
        public static bool comprararma(ulong id)
        {
            int ownguns = 0;
            double saldo = banco.get_saldo(id);

            if (saldo >= preco)
            {
                //Comprar
                MySqlCommand vergun = new MySqlCommand();
                string verguncom = "select * from " + db_table + " where id='" + id + "';";
                var vergun_r = con_db.CreateCommand();
                vergun_r.CommandText = verguncom;
                var resuytkladio = vergun_r.ExecuteReader();
                while (resuytkladio.Read())
                {
                    ownguns = resuytkladio.GetInt32("arma");
                }
                resuytkladio.Close();
                double valor_setar = saldo - preco;
                MySqlCommand retvalor = new MySqlCommand();
                string setvalor_st = "update " + db_table + " set saldo='" + valor_setar + "' where id='" + id + "';";
                var setvalor = con_db.CreateCommand();
                setvalor.CommandText = setvalor_st;
                var setvalorzado = setvalor.ExecuteNonQuery();
                ownguns += 1;
                MySqlCommand buygun = new MySqlCommand();
                string setgun = "update " + db_table + " set arma='" + ownguns + "' where id='" + id + "';";
                var setgun_m = con_db.CreateCommand();
                setgun_m.CommandText = setgun;
                var setado = setgun_m.ExecuteNonQuery();
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string verifycazao(ulong id)
        {
            double espiao = banco.get_saldo(id);
            System.Threading.Thread.Sleep(1500);
            string inventario = banco.inv(id);
            string retornar = "\nSaldo de <@" + id + "> : R$" + espiao + "\n" + inventario;
            return retornar;
        }

        public static string inv(ulong id)
        {
            string dados = string.Empty;
            double saldo = 0;
            int armas = 0;
            MySqlCommand inv_dados = new MySqlCommand();
            string invcom = "select * from " + db_table + " where id='" + id + "';";
            var invinfo = con_db.CreateCommand();
            invinfo.CommandText = invcom;
            var reader = invinfo.ExecuteReader();
            while (reader.Read())
            {
                saldo = reader.GetDouble("saldo");
                armas = reader.GetInt32("arma");
            }
            reader.Close();
            if (armas <= 0 && saldo < 0)
            {
                dados = "Seu Inventário:\n```nada...```";
            }
            else
            {
                dados = "Seu Inventário:\n```" + saldo + "x | Dinheirinhos\n" + armas + "x | Armita```";
            }
            return dados;
        }

        public static void banir(ulong id, string motivo, string autor)
        {
            banco.verify(id);
            

            //update users set ban=true, ban_reason='testando', ban_author='N3rdyDzn' where id='956660539113738250';
            string com_banir = "update users set ban=true, ban_reason='"+motivo+"', ban_author='"+autor+"' where id='"+id+"';";
            MySqlCommand banidor = new MySqlCommand();
            var comban = con_db.CreateCommand();
            comban.CommandText = com_banir;
            var teste1 = comban.ExecuteNonQuery();
        }




    }
}
