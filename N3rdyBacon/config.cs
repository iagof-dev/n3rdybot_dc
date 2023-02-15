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
        public static string bot_webhook = "https://discord.com/api/webhooks/1058161536864100412/VVNSwhDIneegytRw59fcUBIex7ayqTXTaEN06NAt1mlzaJUqHjnBoAmOl3eORSZ20oBD";


        public static ulong dev_id = 833438251553128448;
        
        //BOT INFO
        public static ulong bot_id = 956660539113738250;
        public static string bot_token = "OTU2NjYwNTM5MTEzNzM4MjUw.G-U9rJ.FH0qh2nWjcSh5XvVQDEBxMEW2Rn8wY2HKDu6aE";

        //Dados de conexão
        public static string db_server = "db2.n3rdydzn.software";
        public static string db_data = "nrdydesi_n3rdybot";
        public static string db_user = "n3rdy";
        public static string db_pass = "132490kj";
        public static int db_timeout = 100;
        
        //Tabelas
        public static string db_table_users = "users";
        public static string db_table_servers = "servers";
        

        //Banco de dados Flexível/Dinamico
        public static string db_dados = ($"server={config.db_server};database={config.db_data};Uid={config.db_user};pwd={config.db_pass};Connection Timeout={db_timeout}");

    }
}
