using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using N3rdyBacon.Commands;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N3rdyBacon
{
    public class mysql
    {
        public static MySqlConnection con_db = new MySqlConnection(config.db_dados);
        
        //Criar uma conexão com MySql
        //ao iniciar o bot
        public static void iniciar_conexao()
        {
            try
            {
                mysql.con_db.Open();
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

        
        //fazer alterações ou adicionar valores ao banco de dados MySql
        //exemplo: mysql.send_value("select * from usuarios;");
        //sem retorno
        public static void send_value(string comando)
        {
            Console.WriteLine("MySql | Enviando valores...");
            bool mysqlcon = status_con();
            if (mysqlcon != false)
            {

                MySqlCommand db_comando = new MySqlCommand();
                var comandinho = mysql.con_db.CreateCommand();
                comandinho.CommandText = comando;
                var definir = comandinho.ExecuteNonQuery();
                Console.WriteLine("MySql | Sucesso!");
            }
            else
            {
                Console.WriteLine("MySql | Erro!");
            }


        }

        //Verificar conexão do MySql
        //Retorno:
        //True = Conexão Aberta ou False = Conexão Fechada
        public static bool status_con()
        {
            Console.WriteLine("MySql | Verificando conexão...");

            if (mysql.con_db.State == ConnectionState.Open)
            {
                //Aberto
                Console.WriteLine("MySql | Conexão Está Aberta!");
                return true;
            }
            else
            {
                //Fechado
                Console.WriteLine("MySql | Conexão está fechada, Iniciando nova conexão...");
                mysql.iniciar_conexao();
                bool ok = status_con();
                if (ok != false)
                {
                    return true;
                }
            }
            return false;
        }
        

        public static int latencia(DateTime tempo)
        {
            


            string comando_saldo = ($"select * from {config.db_table_servers};");
            MySqlCommand comm = new MySqlCommand();
            var com = mysql.con_db.CreateCommand();
            com.CommandText = comando_saldo;
            var reader = com.ExecuteReader();
            reader.Close();

            int late = (DateTime.Now.Millisecond - tempo.Millisecond);
            return late;


        }



        

    }
}
