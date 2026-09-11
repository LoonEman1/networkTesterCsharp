using System.Buffers.Binary;
namespace TestZadanie;

public class Packet
{
    public const uint Signature = 0x54455354;

    public ulong Sequence { get; set; }
    public long Timestamp { get; set; }
    public byte[] Payload { get; set; } = [];

    public byte[] ToBytes()
    {
        var data = new byte[24 + Payload.Length];
        BinaryPrimitives.WriteUInt32BigEndian(data, Signature);
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(4), data.Length);
        BinaryPrimitives.WriteUInt64BigEndian(data.AsSpan(8), Sequence);
        BinaryPrimitives.WriteInt64BigEndian(data.AsSpan(16), Timestamp);
        Payload.CopyTo(data, 24);
        return data;
    }

    public static Packet? FromBytes(byte[] data)
    {
        if (data.Length < 24)
            return null;

        if (BinaryPrimitives.ReadUInt32BigEndian(data) != Signature)
            return null;

        int packetSize = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(4));

        if (packetSize != data.Length)
            return null;

        return new Packet
        {
            Sequence = BinaryPrimitives.ReadUInt64BigEndian(data.AsSpan(8)),
            Timestamp = BinaryPrimitives.ReadInt64BigEndian(data.AsSpan(16)),
            Payload = data[24..]
        };
    }
}
