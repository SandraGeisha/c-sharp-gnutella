namespace Gnutella;


public enum MessageType {
    Ping = 0x00,
    Pong = 0x01,
    Bye = 0x02,
    Query = 0x80,
    QueryHit = 0x81
}

public class Packet {
    private byte[] _messageId;

    /// <summary>
    /// You can see that the setter will set byte 8 to 255 and the last byte to 0.
    /// The last byte is reserved for latter use (not that the time will come probably :) )
    /// The 8th byte is to indicate it is a modern servlet (I assume this means someone following gnutella 0.6 as opposed to 0.4)
    /// </summary>
    public Guid MessageId {
        get => new Guid(_messageId);
        set {
            _messageId = value.ToByteArray();
            _messageId[7] = 0xff;
            _messageId[15] = 0x0;
        }
    }

    private Byte _messageType;

    public MessageType MessageType
    {
        get => (MessageType) _messageType;
        set => _messageType = (byte)value;
    }


    public sbyte TTL;
    public sbyte Hops;
    public Int32 PayloadLength => Payload?.Length ?? 0;
    public byte[]? Payload;
    public Int32 TotalLength => 23 + PayloadLength;


    /// <summary>
    /// Constructor for sending basic messages over the network.
    /// Can be used standalone for a ping message, bye or as a base
    /// </summary>
    /// <param name="type">The message type (ping, bye,...)</param>
    /// <param name="ttl">How many times should this message be forwarded</param>
    /// <param name="hops">How many times this message was forwarded </param>
    /// <param name="payload">An optional payload to send along with the package</param>
    /// <exception cref="ArgumentException">If the payload data is too big according to the spec</exception>
    public Packet(MessageType type, sbyte ttl, sbyte hops, byte[]? payload = null) {
        if (payload?.Length > 4096) {
            throw new ArgumentException("Payload too big");
        }
        MessageId = Guid.NewGuid();
        MessageType = type;
        TTL = ttl;
        Hops = hops;
        Payload = payload;
    }

    /// <summary>
    /// This constructor will deconstruct a byte array to a packet instance.
    /// Usually it is used as a base class to decode the message header and the rest is handled in the child class
    /// </summary>
    /// <param name="packageData">The byte array to deconstruct</param>
    /// <exception cref="ArgumentException">
    /// If the package data doesn't contain enough bytes to construct the package
    /// or too much as indicated by the spec
    /// </exception>
    public Packet(byte[] packageData)
    {
        //check max size and check if at least header data is present
        if (packageData.Length is < 23 or > 4119)
            throw new ArgumentException("package data invalid");

        _messageId = packageData[..16];
        _messageType = packageData[16];
        TTL = (sbyte)packageData[17];
        Hops = (sbyte)packageData[18];
        if (packageData.Length > 23) {
            Int32 supposedLength = BitConverter.ToInt32(packageData[19..24]);
            //this might seem quite weird why not take the complete payload with offset
            //well one network package might contain multiple pong packages so this is important to only take the length indicated
            //by byte 19-23
            Payload = packageData[23..(23+supposedLength)];
        }
    }

    /// <summary>
    /// converts the package to a byte array in the format the spec expects
    /// </summary>
    /// <returns>A byte array of this package to be sent over the network</returns>
    public byte[] ToBytes() {
        return _messageId
            .Concat(new[] { _messageType, (byte)TTL, (byte)Hops })
            .Concat(BitConverter.GetBytes(PayloadLength))
            .Concat(Payload ?? new byte[] { })
            .ToArray();
    }
}