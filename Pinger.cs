using System.Net;
using System.Net.NetworkInformation;

namespace InternetScanner
{
    internal class Pinger
    {
        readonly Ping[] Pings;
        readonly bool[] Busy;

        public Pinger(int capacity)
        {
            Pings = new Ping[capacity];
            Busy = new bool[capacity];
        }

        bool TryGetPinger(out int pinger)
        {
            for (int i = 0; i < Pings.Length; i++)
            {
                if (Pings[i] == null)
                {
                    Pings[i] = new Ping();
                    Pings[i].PingCompleted += (sender, e) =>
                    {
                        Busy[i] = false;
                    };

                    pinger = i;
                    return true;
                }

                if (!Busy[i])
                {
                    pinger = i;
                    return true;
                }
            }

            pinger = -1;
            return false;
        }

        public bool TryPing(IPAddress address, int timeout, byte[] buffer, out Task<PingReply> task)
        {
            Ping pinger = new();
            task = pinger.SendPingAsync(address, timeout, buffer);
            return true;
            /*
            if (!TryGetPinger(out int i))
            {
                task = null;
                return false;
            }

            Busy[i] = true;
            task = Pings[i].SendPingAsync(address, timeout, buffer);
            return true;
            */
        }
    }
}
