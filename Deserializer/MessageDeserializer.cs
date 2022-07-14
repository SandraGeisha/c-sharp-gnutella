namespace Gnutella;

internal class MessageDeserializer {
    public IEnumerable<PongPacket> DeserializePongPackets(byte[] raw) {
        List<PongPacket> packets = new List<PongPacket>();
        do {
            int offset = packets.Any()? packets.Select(p => p.TotalLength).Sum():0;
            packets.Add(new PongPacket(raw[offset..]));
        }while(packets.Select(p=>p.TotalLength).Sum() < raw.Length);

        return packets;
    }
}