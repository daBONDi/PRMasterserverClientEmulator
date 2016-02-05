using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRMasterserverClientEmulator
{
    /// <summary>
    /// Define the ClientEmulator Type from witch the LogMessage came from
    /// </summary>
    public enum ClientEmulatorType
    {
        Main,
        CDKeyServerClient,
        LoginServerClient
    }

    /// <summary>
    /// Define a Message Type for Logging
    /// </summary>
    public enum MessageType
    {
        Info,
        Warning,
        Error,
        Critical,
        Debug
    }

 
    /// <summary>
    /// Log Message Structure to Handle Logging Events
    /// </summary>
    public class ClientEmulatorLogMessage
    {
        public ClientEmulatorType ClientType { get; set; }
        public int InstanceId { get; set; }
        public MessageType MsgType { get; set; }
        public string Message { get; set; }

        /// <summary>
        /// Return a new Message
        /// </summary>
        /// <param name="ClientType">Client Type for this Log Message</param>
        /// <param name="InstanceId">InstanceId of the Client</param>
        /// <param name="Type">Type of the Message</param>
        /// <param name="Message">Messge Text</param>
        /// <returns>a new Instance of ClientEmulatorLogMessage</returns>
        public static ClientEmulatorLogMessage NewMessage(ClientEmulatorType ClientType, int InstanceId, MessageType MsgType, string Message)
        {
            ClientEmulatorLogMessage msg = new ClientEmulatorLogMessage();
            msg.ClientType = ClientType;
            msg.InstanceId = InstanceId;
            msg.MsgType = MsgType;
            msg.Message = Message;
            return msg;
        }
    }

    public static class ClientEmulatorLogging
    {

        public static void Log(Emulators.CDKeyServerClientEmulator Emulator,MessageType MsgType, string Message)
        {
            Log(ClientEmulatorLogMessage.NewMessage(
                ClientEmulatorType.CDKeyServerClient, Emulator.InstanceId, MsgType, Message
                ));
        }

        public static void Log(ClientEmulatorType ClientType, int InstanceId, MessageType MsgType, string Message)
        {
            Log(ClientEmulatorLogMessage.NewMessage(ClientType, InstanceId, MsgType, Message));
        }
        public static void Log(ClientEmulatorLogMessage Message)
        {
            //TODO: Add Multiple Logging Mechanismen
            Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}",
                    DateTime.Now.ToString("HH:mm:ss:ffff"),
                    Message.ClientType.ToString(),
                    Message.InstanceId,
                    Message.MsgType.ToString(),
                    Message.Message.ToString()
                );

        }
    }
}
