using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace N3rdyBacon
{
    public class config
    {
        public static string logs = string.Empty;
        

        //BOT CONFIG
        public static string bot_name = "N3rdyBOT";
        public static string prefixo = "$";
        public static string bot_title = string.Empty;
        public static bool bot_on = false;
        public static bool bot_status_loop = false;
        public static string bot_title_link = "https://pastebin.com/raw/5s8KFjCV";
        public static string bot_pfp = "";
        public static string bot_webhook = "";


        public static ulong dev_id = 833438251553128448;
        
        //BOT INFO
        public static ulong bot_id = 0;
        public static string bot_token = "";

        //Dados de conexão
        public static string db_server = "";
        public static string db_data = "nrdydesi_n3rdybot";
        public static string db_user = "";
        public static string db_pass = "";
        public static int db_timeout = 100;
        
        //Tabelas
        public static string db_table_users = "users";
        public static string db_table_servers = "servers";
        

        //Banco de dados Flexível/Dinamico
        public static string db_dados = ($"server={config.db_server};database={config.db_data};Uid={config.db_user};pwd={config.db_pass};Connection Timeout={db_timeout}");

    }
}
