using System.IO.Compression;

namespace Gnutella;

public static class ByteExtensions {
    /// <summary>
    /// Uses a ZLibStream to encode a packet using the deflate standard
    /// </summary>
    /// <param name="packet">byte array to be compressed</param>
    /// <returns>compressed byte array</returns>
    public static byte[] Compress(this byte[] packet) {
        using (var mso = new MemoryStream()) {
            using (var gs = new ZLibStream(mso, CompressionMode.Compress)) {
                gs.Write(packet);
                gs.Close();
                return mso.ToArray();
            }
        }
    }

    /// <summary>
    /// Decompresses the byte array
    /// </summary>
    /// <param name="packet">Recieved byte array to decompressed</param>
    /// <returns>Decompressed byte array</returns>
    public static byte[] Decompress(this byte[] packet) {
        using (var msi = new MemoryStream())
        using (var mso = new MemoryStream(packet)) {
            using (var gs = new ZLibStream(mso, CompressionMode.Decompress)) {
                gs.CopyTo(msi);
                gs.Close();
                msi.Position = 0;
                return msi.ToArray();
            }
        }
    }

}