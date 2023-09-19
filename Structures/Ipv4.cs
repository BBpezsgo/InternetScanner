using System.Diagnostics;
using System.Net;
using DataUtilities.Serializer;

namespace InternetScanner
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    struct IPv4 : IEquatable<IPv4>, ISerializable<IPv4>
    {
        public byte Segment1;
        public byte Segment2;
        public byte Segment3;
        public byte Segment4;

        public IPv4(byte segment1, byte segment2, byte segment3, byte segment4)
        {
            Segment1 = segment1;
            Segment2 = segment2;
            Segment3 = segment3;
            Segment4 = segment4;
        }

        public IPv4(IPAddress address) : this(address.GetAddressBytes())
        {

        }

        public IPv4(uint v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            Segment1 = (bytes.Length > 0) ? bytes[0] : byte.MinValue;
            Segment2 = (bytes.Length > 1) ? bytes[1] : byte.MinValue;
            Segment3 = (bytes.Length > 2) ? bytes[2] : byte.MinValue;
            Segment4 = (bytes.Length > 3) ? bytes[3] : byte.MinValue;
        }

        public IPv4(byte[] v)
        {
            byte[] bytes = v;
            Segment1 = (bytes.Length > 0) ? bytes[0] : byte.MinValue;
            Segment2 = (bytes.Length > 1) ? bytes[1] : byte.MinValue;
            Segment3 = (bytes.Length > 2) ? bytes[2] : byte.MinValue;
            Segment4 = (bytes.Length > 3) ? bytes[3] : byte.MinValue;
        }

        public readonly byte[] Bytes => new byte[] { Segment1, Segment2, Segment3, Segment4 };
        public readonly uint Int
        {
            get
            {
                byte[] bytes = Bytes;
                if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
                return BitConverter.ToUInt32(bytes);
            }
        }
        public readonly IPAddress Address => IPv4.UIntToIpv4(Int);

        public readonly bool IsInvalid =>
            Segment1 == 0 ||
            Segment2 == 0 ||
            Segment3 == 0 ||
            Segment4 == 0;

        public readonly bool IsPingable =>
            !IsInvalid &&
            Segment1 < 240;

        public static IPAddress UIntToIpv4(uint address)
            => new(IPv4.UIntToBytes(address));

        public static uint Ipv4ToUInt(IPAddress address)
            => IPv4.BytesToUInt(address.GetAddressBytes());

        public static uint BytesToUInt(byte[] bytes) =>
            (((uint)(new byte[bytes.Length])[0]) << 24) |
            (((uint)(new byte[bytes.Length])[1]) << 16) |
            (((uint)(new byte[bytes.Length])[2]) << 8) |
            (((uint)(new byte[bytes.Length])[3]));

        public static byte[] UIntToBytes(uint value) => new byte[] {
            (byte)((value >> 24) & 0xFF),
            (byte)((value >> 16) & 0xFF),
            (byte)((value >> 8)  & 0xFF),
            (byte)((value)       & 0xFF),
        };

        public override readonly string ToString() => $"{Segment1}.{Segment2}.{Segment3}.{Segment4}";
        readonly string GetDebuggerDisplay() => ToString();

        public override readonly bool Equals(object? obj)
            => obj is IPv4 pv && Equals(pv);

        public readonly bool Equals(IPv4 other) =>
            Segment1 == other.Segment1 &&
            Segment2 == other.Segment2 &&
            Segment3 == other.Segment3 &&
            Segment4 == other.Segment4;

        public override readonly int GetHashCode()
            => HashCode.Combine(Segment1, Segment2, Segment3, Segment4);

        public readonly void Serialize(Serializer serializer)
        {
            serializer.Serialize(Segment1);
            serializer.Serialize(Segment2);
            serializer.Serialize(Segment3);
            serializer.Serialize(Segment4);
        }

        public void Deserialize(Deserializer deserializer)
        {
            Segment1 = deserializer.DeserializeByte();
            Segment2 = deserializer.DeserializeByte();
            Segment3 = deserializer.DeserializeByte();
            Segment4 = deserializer.DeserializeByte();
        }

        public static bool operator ==(IPv4 left, IPv4 right) => left.Equals(right);

        public static bool operator !=(IPv4 left, IPv4 right) => !(left == right);

        public static IPv4 Min => new(1, 1, 1, 1);
        public static IPv4 Max => new(255, 255, 255, 255);
    }
}
