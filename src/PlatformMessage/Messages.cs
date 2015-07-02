using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PlatformMessage
{
    public static class Messages
    {
        public static void ForConsole()
        {
            var defaultColor = Console.ForegroundColor;

            var arch = GetArchitecture();

            if (arch == "x86")
            {
                Console.WriteLine(DnxOn86);
                return;
            }

            foreach (var c in PiLogo)
            {
                Console.ForegroundColor = GetColor(c);
                Console.Write(c);
            }

            foreach (var c in DnxOnPi)
            {
                Console.Write(c);
            }

            Console.ResetColor();
        }

        public static async Task ForWeb(Stream responseStream)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<body style='background-color: #000;'>");
            sb.Append("<pre style='color: #fff'>");

            var arch = GetArchitecture();

            if (arch == "x86")
            {
                sb.Append(DnxOn86);
            }
            else
            {
                foreach (var c in PiLogo)
                {
                    var color = GetWebColor(c);
                    if (!string.IsNullOrEmpty(color))
                    {
                        sb.Append($"<span style='color:{GetWebColor(c)}'>{c}</span>");
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }

                foreach (var c in DnxOnPi)
                {
                    sb.Append(c);
                }
            }
            sb.AppendLine("</pre>");
            sb.AppendLine("</body>");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            await responseStream.WriteAsync(bytes, 0, bytes.Length);
        }

        private static string GetArchitecture() => Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");

        private static readonly IDictionary<char, ConsoleColor> ColorMap =
            new Dictionary<char, ConsoleColor>
            {
                ['1'] = ConsoleColor.Green,
                ['\''] = ConsoleColor.Red,
                [' '] = ConsoleColor.Black
            };

        private static readonly ConsoleColor DefaultForegroundColor = Console.ForegroundColor;

        private static ConsoleColor GetColor(char c) => ColorMap.ContainsKey(c)
            ? ColorMap[c] : DefaultForegroundColor;

        private static string GetWebColor(char c)
        {
            var consoleColor = GetColor(c);
            return consoleColor == ConsoleColor.Red
                ? "#f00"
                : (consoleColor == ConsoleColor.Green)
                    ? "#0f0"
                    : null;
        }

        private static readonly string PiLogo = @"
                                                           
          :||220$00$$211'        '112$$00$0221|:           
       '00802$22112122$$802'   1080$22211122$2$800:        
       |&0111111111111111108::881111111111111111$&1        
       :&8111111122111111110888111111112221111110&;        
        1&$111111112$$211112&&$11112$$2111111112&2         
         2&$1111111111200228&&&$2$0$111111111128$          
          18821111111111$&&&&&&&&811111111112081           
           '108$21111120&&&&&&&&&&021111122801:            
             '$&&8$$$8&&8$11||11$8&&80008&&$:              
            28$1;:'';0&1:' '''' ':181:::;12882'            
          :88;'  ';2&&8;' ''''''  :8&$|'   :2&8:           
          $&;  :1$&&&&&&021||||112088&&01:' '$&0           
          &8;|28&821;::;12&&&&&81;::::;108$1:1&&:          
        ;0&8&&&&1: '''''' ;0&&0: '''''' '|8&&88&0|         
      '081;:$&&1 ''''''''' 1&&1 ''''''''' ;8&|';$&0:       
      88; ' 1&$''''''''''' 1&&2 '''''''''' 28:'''1&8:      
     1&1 '' 2&0'''''''''' ;8&&&1'''''''''''0&:''''8&2      
     1&1 '':8&&2: '''' ':1&&&&&&$|''    ':2&&1 '':8&2      
     '88;  2&&&&821||11$0$1|;;;1208$1112$&&&&&; '2&&:      
      :0822&0008&&&&&&&2:' ''''' '|8&&&&&821|1020&0:       
        8&&1''':|$&&&&2 '''''''''' ;&&&0|: '' |&&8'        
        ;&8:'''''';$&&1 ''''''''''':8&1''''''':8&|         
         $&1 '''''''2&8; '''''''' '2&| '''''' |&0          
         '0&1''''''':8&&2;:'''':;10&$ '''''''|88:          
           2&$1:'''';8&&&&&00008&&&&$'  '':|$&2'           
            '1$00$$$8&&0211||||112$8&02220001:             
                |28&&&2'   '''''   2&&&8$|                 
                   :128$1;''''':;10&21:                    
                      '1$00$$$$08$1'                       
                          ':;;:'                           
                                                           ";

        private static readonly string DnxOnPi = @"
           ___  _  ___  __              ___  _ 
          / _ \/ |/ / |/_/ ___  ___    / _ \(_)
         / // /    />  <  / _ \/ _ \  / ___/ / 
        /____/_/|_/_/|_|  \___/_//_/ /_/  /_/  
                                                                                                                                                              
";

        private static readonly string DnxOn86 = @"
   ___  _  ___  __                   ___  ____
  / _ \/ |/ / |/_/ ___  ___    __ __( _ )/ __/
 / // /    />  <  / _ \/ _ \   \ \ / _  / _ \ 
/____/_/|_/_/|_|  \___/_//_/  /_\_\\___/\___/ 
                                              
";
    }
}
