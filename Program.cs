using System.IO.Compression;
using System.Net.Sockets;
using System.Text;

namespace Gnutella;
public class Program {
    public static void Main(string[] args)
    {

        //buffer
        byte[] buffer = new byte[10000];
        //open a tcp connection with a using state to terminate the connection gracefully afterwards
        using (TcpClient client = new TcpClient("185.187.74.173", 53489)) {
            //gets the stream so we can send data
            NetworkStream stream = client.GetStream();
            //sends the header
            stream.Write(File.ReadAllBytes("Handshake/header-1.txt"));
            //reads the header
            int length = stream.Read(buffer);
            //writes out the result
            Console.WriteLine(Encoding.ASCII.GetString(buffer[..length]));
            //writes the confirmation
            stream.Write(File.ReadAllBytes("Handshake/header-2.txt"));


            //makes the packet, converts it with zlib to adhere to the deflate algorithm and send it over the network
            Packet packet = new Packet(MessageType.Ping, 0x01, 0x00);
            stream.Write(packet.ToBytes().Compress());
            Console.WriteLine(packet.MessageId);

            //reads the pong response and decompresses it we use a helper class ebcause a response may contain multiple messages
            length = stream.Read(buffer);
            var decompressedResponse = buffer[..length].Decompress();
            MessageDeserializer md = new MessageDeserializer();
            IEnumerable<PongPacket> pongs = md.DeserializePongPackets(decompressedResponse);
            NodeTable nodeTable = new NodeTable(pongs);
            Console.WriteLine(nodeTable);
            //todo pong response parsing
        }
    }
}