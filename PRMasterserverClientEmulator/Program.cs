using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;

namespace PRMasterserverClientEmulator
{
    public static class GlobalPropertys
    {
        public const int GamespyGamePID = 1059;   //Define the PID on the Game
    }


    class Program
    {
 

        const int DefaultCDKeyServerUDPPort= 29910;

        const int DefaultCDKeyClientEmulatorRoundTrips = 100000000;
        const int DefaultCDKeyClientEmulatorInterPacketGap = 0;
        const string CDKeyServerIP = "192.168.1.16";

        static void Main(string[] args)
        {
            ClientEmulatorLogging.Log(ClientEmulatorType.Main, 0, MessageType.Info, "Booting Emulator Up");

            Emulators.CDKeyServerClientEmulator client = new Emulators.CDKeyServerClientEmulator(
                new IPEndPoint( IPAddress.Parse(CDKeyServerIP), 
                DefaultCDKeyServerUDPPort ),
                DefaultCDKeyClientEmulatorRoundTrips,
                DefaultCDKeyClientEmulatorInterPacketGap
                );

            Console.WriteLine("Finished");
            Console.ReadLine();
        }
    }
}
