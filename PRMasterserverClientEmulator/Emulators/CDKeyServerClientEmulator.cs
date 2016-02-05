using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using PRMasterserverClientEmulator.Util;
using System.Diagnostics;

namespace PRMasterserverClientEmulator.Emulators
{
    /// <summary>
    /// Descibe the State of the CDKey Server Emulator Client
    /// </summary>
    public enum CDKeyServerClientEmulatorState
    {
        Initializing,
        Connecting,
        Connected,
        Sending,
        WaitingForNextSending,
        Closing,
        Done,
        Error
    }

    public class CDKeyServerClientEmulator
    {
        private readonly Regex sendDataPattern = new Regex(@"^\\auth\\\\pid\\1059\\ch\\[a-zA-z0-9]{8,10}\\resp\\(?<Challenge>[a-zA-z0-9]{72})\\ip\\\d+\\skey\\(?<Key>\d+)(\\reqproof\\[01]\\)?$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        private readonly Regex recieveDataPattern = new Regex(@"^\\uok\\\cd\\\[a-zA-z0-9]\\skey\\d+?$");

        #region "Propertys"
        /// <summary>
        /// InstanceCount of all cdKeyServers
        /// </summary>
        private static int cdKeyServerInstanceCount;

        /// <summary>
        /// Instance Id of the CDKeyServerClientEmulator Instance
        /// </summary>
        private int instanceId;

        /// <summary>
        /// Instance Id for the CDKeyServerClientEmulator
        /// </summary>
        public int InstanceId { get { return this.instanceId; } }

        /// <summary>
        /// IPEndpoint to the CD Key Server
        /// </summary>
        private IPEndPoint cdKeyServer;

        /// <summary>
        /// UDPClient Object the Emulator use
        /// </summary>
        private UdpClient client;

        /// <summary>
        /// Send Buffer used by UDPClient
        /// </summary>
        private byte[] sendBuffer;

        /// <summary>
        /// Recieve Buffer used by UDPClient
        /// </summary>
        private byte[] recieveBuffer;

        private Stopwatch roundTripWatcher;

        /// <summary>
        /// Current Roundtrips to the cdKeyServer
        /// </summary>
        private int currentCDKeyServerRoundTripCounter;

        /// <summary>
        /// CDKey Server Roundtrip Counter
        /// </summary>
        public int CurrentCDKeyServerRoundTripCounter { get { return this.currentCDKeyServerRoundTripCounter; } }

        private int currentCDKeyServerRoundTripSuccessCounter;
        public int CurrentCDKeyServerRoundTripSuccessCounter { get { return this.currentCDKeyServerRoundTripSuccessCounter; } }

        /// <summary>
        /// Average Round Trip Time of the CDKey Request
        /// </summary>
        private int averageRoundtripTime;

        /// <summary>
        /// Average Round Trip Time of the CDKey Request
        /// </summary>
        public int AverageRoundTripTime { get { return averageRoundtripTime; } }

        #endregion

        /// <summary>
        /// Define Current State of the CDKey Server Client Emulator
        /// </summary>
        private CDKeyServerClientEmulatorState state;

        /// <summary>
        /// Current State the CDKey ServerClient Emulator is
        /// </summary>
        public CDKeyServerClientEmulatorState State { get { return this.state;} }

        /// <summary>
        /// Create a new CDKey Server Client Emulator Instance
        /// </summary>
        /// <param name="CDKeyServerIPEndPoint">CDKey Server IP Endpoint to Connect to</param>
        /// <param name="RoundTips">Ammount of Retrys to Send to the CD Key Server before finishing</param>
        /// <param name="InterPacketGap">Milliseconds to wait between the Retrys</param>
        public CDKeyServerClientEmulator(IPEndPoint CDKeyServerIPEndPoint,int RoundTrips,int InterPacketGap)
        {
            this.cdKeyServer = CDKeyServerIPEndPoint;

            this.state = CDKeyServerClientEmulatorState.Initializing;
            //Add InstanceToInstanceNumber
            cdKeyServerInstanceCount++;
            this.instanceId = cdKeyServerInstanceCount;

            this.currentCDKeyServerRoundTripCounter = 0;
            this.currentCDKeyServerRoundTripSuccessCounter = 0;

            this.roundTripWatcher = new Stopwatch();

            //Start Initalizing the Client
            if (this.initalizeUDPClient())
            {
                ///Lets Hammer him
                for (currentCDKeyServerRoundTripCounter = 0; currentCDKeyServerRoundTripCounter < RoundTrips; currentCDKeyServerRoundTripCounter++)
                {
                    

                    this.state = CDKeyServerClientEmulatorState.Connecting;
                    client.Connect(cdKeyServer);

                    roundTripWatcher.Start();
                    if (client.Client.Connected)
                    {
                        //Client is Connected
                        this.state = CDKeyServerClientEmulatorState.Connected;

                        if(sendBuffer !=null) Array.Clear(sendBuffer, 0, sendBuffer.Length);
                        if(recieveBuffer != null) Array.Clear(recieveBuffer, 0, recieveBuffer.Length);

                        sendBuffer = Encoding.UTF8.GetBytes(
                            /* Decode it with Xor */
                             Xor(
                                 /* Generate Fake Request */
                                 GenerateRandomClientCDKeyValidationRequest()
                            ) 
                        );

                        //Send Data to the Client
                        client.Send(sendBuffer,sendBuffer.Length);

                        //Now lets Wait for the Request Recieve the Client Server Token
                        this.recieveBuffer = client.Receive(ref CDKeyServerIPEndPoint);

                        //Check if we got some Data
                        if(recieveBuffer.Length != 0)
                        {
                            //We recived Something
                            
                            //Decode it with Xor
                            String Response = Xor(Encoding.UTF8.GetString(this.recieveBuffer));

                            //Lets check if we got an Valid Response
                            if(recieveDataPattern.Match(Response).Success)
                            {
                                this.currentCDKeyServerRoundTripSuccessCounter++;
                            }
                        }
                        
                    }
                    else
                    {
                        this.throwCritical("Unable to connect to CDKey Server IP:" + cdKeyServer.Address.ToString() + ":" + cdKeyServer.Port.ToString());
                    }

                    roundTripWatcher.Stop();

                    if(averageRoundtripTime==0)
                    {
                        averageRoundtripTime = roundTripWatcher.Elapsed.Milliseconds;
                    }
                    else
                    {
                        averageRoundtripTime = roundTripWatcher.Elapsed.Milliseconds + averageRoundtripTime / 2;
                    }
                    //Add up the Retry Counter
                    this.currentCDKeyServerRoundTripCounter++;
                    Console.WriteLine(averageRoundtripTime + ":" + currentCDKeyServerRoundTripSuccessCounter + ":" +  currentCDKeyServerRoundTripCounter);
                }

            }
        }
        
        /// <summary>
        /// Decode/Encode Data with Xor and KeyString "gamespy"
        /// </summary>
        /// <param name="s">String to Decode/Encode</param>
        /// <returns>Encoded/Decoded String</returns>
        private static string Xor(string s)
        {
            const string gamespy = "gamespy";
            int length = s.Length;
            char[] data = s.ToCharArray();
            int index = 0;

            for (int i = 0; length > 0; length--)
            {
                if (i >= gamespy.Length)
                    i = 0;

                data[index++] ^= gamespy[i++];
            }

            return new String(data);
        }

        /// <summary>
        /// Generate a Random Client CD Key Request as String
        /// </summary>
        /// <returns>String of Random Client CD Key Request</returns>
        private string GenerateRandomClientCDKeyValidationRequest()
        {
            return @"\auth\\pid\1059" +
                @"\ch\" + Util.StringUtils.CreateRandomString(10) +
                @"\resp\" + Util.StringUtils.CreateRandomString(72) +
                @"\ip\" + Util.StringUtils.CreateRandomNumberString(7) +
                @"\skey\" + Util.StringUtils.CreateRandomNumberString(10) +
                @"\reqproof\0\"; ;
        }

        /// <summary>
        /// Initialize UDPClient
        /// </summary>
        private bool initalizeUDPClient()
        {
            try
            {
                client = new UdpClient();
                ClientEmulatorLogging.Log(this, MessageType.Debug, "UdpClient Created");
            }
            catch(Exception e)
            {
                this.throwCritical("UdpClientError: " + e.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Helper Function to easily throw a Critical Message and set the State of the ClientEmulator
        /// </summary>
        /// <param name="Message">Message to send to Log</param>
        private void throwCritical(string Message)
        {
            ClientEmulatorLogging.Log(this, MessageType.Critical, Message);
            this.state = CDKeyServerClientEmulatorState.Error;
        }

        /// <summary>
        /// Dispose Ovverrides
        /// </summary>
        ~CDKeyServerClientEmulator()
        {
            client.Close();
        }


    }
}
