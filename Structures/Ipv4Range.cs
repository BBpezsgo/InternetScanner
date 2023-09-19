using System.Collections;
using System.Diagnostics;
using DataUtilities.Serializer;

namespace InternetScanner
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    struct IPv4Range : IEnumerable<IPv4>, IEnumerable, ISerializable<IPv4Range>
    {
        public uint From;
        public uint To;

        public IPv4Range(uint from, uint to)
        {
            From = from;
            To = to;
        }

        public void Deserialize(Deserializer deserializer)
        {
            From = deserializer.DeserializeUInt32();
            To = deserializer.DeserializeUInt32();
        }

        public readonly void Serialize(Serializer serializer)
        {
            serializer.Serialize(From);
            serializer.Serialize(To);
        }

        public override readonly string ToString() => $"{new IPv4(From)} - {new IPv4(To)}";
        readonly string GetDebuggerDisplay() => ToString();

        readonly IEnumerator<IPv4> IEnumerable<IPv4>.GetEnumerator()
        {
            for (uint i = From; i <= To; i++)
            { yield return new IPv4(i); }
        }

        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            uint min = Math.Min(From, To);
            uint max = Math.Max(From, To);
            for (uint i = min; i <= max; i++)
            { yield return new IPv4(i); }
        }

        public readonly bool Contains(IPv4 address)
            => Contains(address.Int);

        public readonly bool Contains(uint address)
        {
            uint min = Math.Min(From, To);
            uint max = Math.Max(From, To);
            return address >= min && address <= max;
        }
    }
}
