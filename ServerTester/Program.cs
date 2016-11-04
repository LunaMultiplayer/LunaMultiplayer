using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LunaCommon.Message;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Server;
using ICSharpCode.SharpZipLib.GZip;

namespace ServerTester
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            //TestCompression();
            Test2();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        }

        private static void Test2()
        {
            var facS = new ServerMessageFactory(true);

            var bytes = new byte[10000];
            new Random().NextBytes(bytes);

            var msg = facS.CreateNew<VesselSrvMsg>(new VesselProtoMsgData
            {
                VesselId = Guid.NewGuid(),
                VesselData = bytes,
                SentTime = DateTime.UtcNow.Ticks,
            });

            //Serialize and compress
            var serialized = msg.Serialize(true);
            serialized = msg.Serialize(true);
            serialized = msg.Serialize(true);
            serialized = msg.Serialize(true);

            //Serialize no compress
            var serializedNC = msg.Serialize(false);
            serializedNC = msg.Serialize(false);
            serializedNC = msg.Serialize(false);
            serializedNC = msg.Serialize(false);

            var msg2 = facS.Deserialize(serialized, Environment.TickCount);
            msg2 = facS.Deserialize(serialized, Environment.TickCount);
            msg2 = facS.Deserialize(serialized, Environment.TickCount);
            msg2 = facS.Deserialize(serialized, Environment.TickCount);

            var msg2NC = facS.Deserialize(serializedNC, Environment.TickCount);
            msg2NC = facS.Deserialize(serializedNC, Environment.TickCount);
            msg2NC = facS.Deserialize(serializedNC, Environment.TickCount);
            msg2NC = facS.Deserialize(serializedNC, Environment.TickCount);

            var ok = ((VesselProtoMsgData)msg.Data).VesselData.SequenceEqual(((VesselProtoMsgData)msg2.Data).VesselData) &&
                 ((VesselProtoMsgData)msg2NC.Data).VesselData.SequenceEqual(((VesselProtoMsgData)msg2.Data).VesselData);
        }

        private static void TestCompression()
        {
            var pk =
                "&lt;RSAKeyValue&gt;&lt;Modulus&gt;vibV+i8ePmw2TeCWc/drZapWy0FQzJUBI9viVs4FG6XIWq54lw7rsX6x8DWVfu48f+Gl4lOHWAcuH+AaKJtwS3GL8oPtFFsxokihTThOpEcopNh5IJ9guBHfencoHvkAHLXw5UM0VR/Xy2hUUjmHy6yg043tGKU/ybYoi1uNF80=&lt;/Modulus&gt;&lt;Exponent&gt;EQ==&lt;/Exponent&gt;&lt;P&gt;/vQQdDSCkZd2O48dWdRd8TlybXUMuxWy5kpze4GO62asq7yqveeVyaRiU2wor+UOr+/2PxykQoXAdKeWNT/OXQ==&lt;/P&gt;&lt;Q&gt;vu6rkDQMHeCWIESisKqMA+iZnRybZoaALSHP2nsSn9X15eQ3LMmmHhXhbuco3D0gMfh8v4a8XREDB6AcoFd4MQ==&lt;/Q&gt;&lt;DP&gt;O/0xDEiXMVDQhnwG5/W7wEnAkjmopH2TgXrt/vFOzfoKgsL6/4HI5CatuUagKWMSg7/9tH81tUylwRhflA8DYQ==&lt;/DP&gt;&lt;DQ&gt;hsaXOJ01urymNOUni8OuIOBsbucESF7xEMySuFbf+FrLsVXMmBXeq9M1t7I69dDLbpFI/6pm2Eg+X7xQcS6vMQ==&lt;/DQ&gt;&lt;InverseQ&gt;1HfJP73cAsL8Rmj5bkp+gf+l4jIxCjqBwFbhFPYFohEkA70jlHh6oEPhrchwmjyZA2kK4FOmG6E9udiKO6AVeQ==&lt;/InverseQ&gt;&lt;D&gt;IY5h/vlBkouRHM1Huhyae1pLjUfC9u0eUZ9GD1GIbkpuppdCdQKiEEOI0Al0vAvsjwmz3KVUHpfa9pD1jrIEwbZLCZ4IU9MCofycvo+dIOFMdVGDM2F9CGkjZBTpjprmQrsdLcRTXs2q27PiDy2ohRS3IBlJPpeWTGSRFyay6LE=&lt;/D&gt;&lt;/RSAKeyValue&gt;";

            byte[] array = Encoding.ASCII.GetBytes(pk);

            byte[] compressed1 = Compress(array);
            var decompressed1 = Decompress(compressed1);
        }

        private static byte[] Compress(byte[] inputBytes)
        {
            using (var ms = new MemoryStream())
            using (var zipOut = new GZipOutputStream(ms, inputBytes.Length))
            {
                zipOut.Write(inputBytes, 0, inputBytes.Length);
                zipOut.Close();
                return ms.ToArray();
            }
        }

        private static byte[] Decompress(byte[] bytes)
        {
            using (var memInput = new MemoryStream(bytes))
            using (var zipInput = new GZipInputStream(memInput))
            using (var output = new MemoryStream())
            {
                zipInput.CopyTo(output);
                return output.ToArray();
            }
        }
    }
}