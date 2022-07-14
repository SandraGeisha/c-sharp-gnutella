using System.Net;

namespace Gnutella;
public class PongPacket : Packet {
    public NodeTableEntry NodeEntry { get; set; }

    public PongPacket(byte[] packet) : base(packet) {
        if (Payload == null)
            throw new ArgumentException("No Payload");

        NodeEntry = new NodeTableEntry(Payload);
    }
}

//VC = Vendor Code
//GUE = https://github.com/gtk-gnutella/gtk-gnutella/blob/devel/doc/gnutella/GUESS
//UP = ?
//DU = Daily uptime, Extract daily uptime into `uptime', from the GGEP "DU" extensions.
//6= IPV6 https://github.com/gtk-gnutella/gtk-gnutella/blob/devel/doc/gnutella/IPv6
//TLS = https://github.com/gtk-gnutella/gtk-gnutella/blob/devel/doc/gnutella/gnet-TLS-upgrade
//DHT = https://github.com/gtk-gnutella/gtk-gnutella/blob/devel/doc/gnutella/DHT/messages



public class NodeTableEntry {
    public UInt16 PortNumber { get; set; }
    public IPAddress Address { get; set; }
    public UInt32 NumberOfFiles { get; set; }
    public UInt32 NumberOfKbShared { get; set; }
    public List<GGEPExtensionBlock> ExtensionBlocks { get; set; }


    public NodeTableEntry(byte[] payloadData)
    {
        PortNumber = BitConverter.ToUInt16(payloadData[..2]);
        Address = new IPAddress(payloadData[2..6]);
        NumberOfFiles = BitConverter.ToUInt32(payloadData[6..10]);
        NumberOfKbShared = BitConverter.ToUInt32(payloadData[10..14]);
        ExtensionBlocks = new List<GGEPExtensionBlock>();


        //GGEP mapping
        if (payloadData.Length > 14 && payloadData[14] == 195) {
            do {
                int GGEPBlockStart = ExtensionBlocks.Any()
                    ? ExtensionBlocks.Select(eb => eb.TotalLength).Sum() + 15
                    : 15;
                ExtensionBlocks.Add(new GGEPExtensionBlock(payloadData[GGEPBlockStart..]));
            } while (!ExtensionBlocks.Last().IsLastExtensionBlock);
        }
    }
}

public class NodeTable {
    public List<NodeTableEntry> Entries { get; set; }

    public NodeTable(IEnumerable<PongPacket> pongPackets) {
        Entries = pongPackets.Select(p => p.NodeEntry).ToList();
    }

    public override string ToString()
    {
        string result = "";
        Entries.ForEach(e => {
            result += $"Entry\naddress is {e.Address}:{e.PortNumber}\n Sharing {e.NumberOfFiles} files totaling {e.NumberOfKbShared}kb\n";
        });
        return result;
    }
}