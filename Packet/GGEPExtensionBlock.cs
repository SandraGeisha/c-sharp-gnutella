using System.Collections;
using System.Text;

namespace Gnutella;

public class GGEPExtensionBlock
{
    public bool IsLastExtensionBlock { get; set; }
    public bool IsCobsEncoded { get; set; }
    public bool CompressionEnabled { get; set; }
    public int AmountOfIdBytes { get; set; }
    public int AmountOfLengthBytes { get; set; }
    public byte[] IDBytes { get; set; }
    public string IDString => IDBytes.Length > 0 ? Encoding.ASCII.GetString(IDBytes) : "";

    public byte[] GGEPData { get; set; }
    public string GGEPDataString => GGEPData.Length > 0 ? Encoding.ASCII.GetString(GGEPData) : "";
    public int TotalLength => 1 + AmountOfIdBytes + AmountOfLengthBytes + GGEPData.Length; 

    public GGEPExtensionBlock(byte[] payload)
    {
        byte flagByte = payload[0];

        //get fifth bit
        if ((flagByte & 0x10) != 0)
            throw new ArgumentException("The fifth bit is set this is invalid");

        //equivalent of getting the first fourt bits and converting those to decimal 

        IsLastExtensionBlock = (flagByte & 0x80) != 0;
        IsCobsEncoded = (flagByte & 0x40) != 0;
        CompressionEnabled = (flagByte & 0x20) != 0;
        AmountOfIdBytes = flagByte & 0x0F;
        IDBytes = payload[1..(AmountOfIdBytes + 1)];

        
        List<bool> bitList = new List<bool>();
        bool isLastLengthByte = false;
        do {
            var lengthByte = payload[AmountOfIdBytes + (bitList.Count % 6) + 1];
            ReverseBitArray rba = new ReverseBitArray(new[] { lengthByte });
            bitList.AddRange(rba[2..8]);
            isLastLengthByte = rba[1];
        } while (!isLastLengthByte);
        int GGEPDataLength = new ReverseBitArray(bitList.ToArray()).Toint();
        AmountOfLengthBytes = bitList.Count() / 6;

        int dataoffset = AmountOfLengthBytes + 1 + AmountOfIdBytes;
        GGEPData = payload[(dataoffset)..(dataoffset + GGEPDataLength)];
    }
}