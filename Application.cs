using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using DataUtilities;
using DataUtilities.Serializer;
using InternetScanner.Mapping;
using Win32.Utilities;
using static SDL2.SDL;

namespace InternetScanner
{
    enum PingStatus : byte
    {
        InQueue,
        Pinging,
        Online,
        Offline,
        Error,
    }

    struct Utils
    {
        public static ushort GetMask(PingStatus status, byte rtt)
        {
            return (ushort)(((byte)status << 8) | rtt);
        }
    }

    struct PingResult : ISerializable<PingResult>
    {
        public IPv4 Address;
        public ushort RTT;
        public IPStatus Status;

        public void Deserialize(Deserializer deserializer)
        {
            Address = deserializer.DeserializeObject<IPv4>();
            RTT = deserializer.DeserializeUInt16();
            Status = (IPStatus)deserializer.DeserializeInt32();
        }

        public readonly void Serialize(Serializer serializer)
        {
            serializer.Serialize(Address);
            serializer.Serialize(RTT);
            serializer.Serialize((int)Status);
        }
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    internal readonly struct Pixel
    {
        public readonly int X;
        public readonly int Y;
        public readonly Color Color;

        public Pixel(int x, int y, Color color)
        {
            X = x;
            Y = y;
            Color = color;
        }

        string GetDebuggerDisplay() => $"({X}, {Y}, {Color})";
    }

    struct SavedStatus : ISerializable<SavedStatus>
    {
        public List<PingResult> PingResults;
        public uint[] LastPinged;

        public SavedStatus()
        {
            PingResults = new();
            LastPinged = Array.Empty<uint>();
        }

        public void Deserialize(Deserializer deserializer)
        {
            PingResults = new List<PingResult>(deserializer.DeserializeObjectArray<PingResult>(INTEGER_TYPE.INT32));
            LastPinged = deserializer.DeserializeArray<uint>(INTEGER_TYPE.INT8);
        }

        public readonly void Serialize(Serializer serializer)
        {
            serializer.Serialize(PingResults.ToArray(), (s, item) => s.Serialize(item), INTEGER_TYPE.INT32);
            serializer.Serialize(LastPinged, INTEGER_TYPE.INT8);
        }

        public readonly bool TryGetPingResult(IPv4 address, out PingResult result)
        {
            foreach (PingResult item in PingResults)
            {
                if (item.Address == address)
                {
                    result = item;
                    return true;
                }
            }
            result = default;
            return false;
        }

        public void SetPingResult(PingResult pingResult)
        {
            for (int i = 0; i < PingResults.Count; i++)
            {
                if (PingResults[i].Address != pingResult.Address) continue;
                PingResults[i] = pingResult;
            }
            PingResults.Add(pingResult);
        }
    }

    readonly struct PingerThreadInfo
    {
        public readonly IPv4Range Range;
        public readonly int Id;

        public PingerThreadInfo(IPv4Range range, int id)
        {
            Range = range;
            Id = id;
        }
    }

    internal class Application : IDisposable
    {
        readonly Gradient Gradient = new(Color.FromHex("#8000FF"), Color.FromHex("#00FFA2"));
        readonly Color[,] Canvas;
        Coroutine? RendererCoroutine = null;
        readonly Mapper2D Mapper;
        SavedStatus SavedStatus = new();
        bool IsRunning = true;

        static readonly byte[] PingPayload = Encoding.ASCII.GetBytes("bruh");

        readonly SdlWindow SdlWindow;
        readonly Form Win32Window;

        internal Application(string[] args)
        {
            SdlWindow = new SdlWindow("Bruh", byte.MaxValue * 4, byte.MaxValue * 3);
            SdlWindow.OnEvent += HandleEvent;

            Win32Window = new Form("Bruh2", 300, 200);

            Canvas = new Color[SdlWindow.Width, SdlWindow.Height];
            ArrayUtils.Fill(Canvas, Color.Gray);
            Mapper = new Mapping.CustomMapper(SdlWindow.Width, SdlWindow.Height);
        }

        public void Initialize()
        {
            SdlWindow.Initialize();
        }

        public void Start()
        {
            const uint threadCount = 8;
            const uint step = (uint)(((long)uint.MaxValue + 1) / threadCount);

            SavedStatus.LastPinged = new uint[threadCount];

            Load();

            for (int i = 0; i < threadCount; i++)
            {
                IPv4Range range = new((uint)(i * step), (uint)((i * step) + step - 1));

                if (SavedStatus.LastPinged[i] != 0)
                {
                    range.From = SavedStatus.LastPinged[i];
                }
                else
                {
                    SavedStatus.LastPinged[i] = range.From;
                }

                Thread thread = new(StartPing)
                {
                    Name = $"Pinger Thread {i}",
                    Priority = ThreadPriority.Lowest,
                };
                thread.Start(new PingerThreadInfo(range, i));
            }
        }

        void StartPing(object? _args)
        {
            if (_args is not PingerThreadInfo args) return;

            IPv4Range ipv4Range = args.Range;

            Console.WriteLine($"Pinging range {ipv4Range} started");

            int t = 0;

            foreach (IPv4 ipv4 in ipv4Range)
            {
                if (!IsRunning)
                {
                    Console.WriteLine($"Pinging range {ipv4Range} aborted");
                    return;
                }

                SavedStatus.LastPinged[args.Id] = ipv4.Int;

                if (!ipv4.IsPingable) continue;

                try
                {
                    Point point = IPv4ToGrid(ipv4.Int);

                    if (point.X < 0 || point.Y < 0) continue;
                    if (point.Y > SdlWindow.Height) break;
                    if (point.X > SdlWindow.Width) continue;

                    Canvas[point.X, point.Y] = Color.Gray;

                    while (t++ > 16)
                    {
                        t = 0;
                        Thread.Sleep(500);
                    }

                    Canvas[point.X, point.Y] = Color.Blue;

                    Ping ping = new();
                    Task<PingReply> task = ping.SendPingAsync(ipv4.Address, byte.MaxValue, PingPayload);
                    task.ContinueWith(OnPingResult, ipv4);

                    Thread.Sleep(100);
                }
                catch (Exception)
                { continue; }
            }

            Console.WriteLine($"Pinging range {ipv4Range} finished");
        }

        void OnPingResult(Task<PingReply> task, object? arg)
        {
            if (arg is not IPv4 address) return;

            Pixel pixel;
            if (task.IsCompletedSuccessfully)
            {
                PingReply reply = task.Result;

                Console.WriteLine($" < {address} - {reply.Status} - {reply.RoundtripTime} ms");

                SavedStatus.SetPingResult(new PingResult()
                {
                    Address = address,
                    RTT = (ushort)Math.Clamp(reply.RoundtripTime, ushort.MinValue, ushort.MaxValue),
                    Status = reply.Status,
                });

                if (reply.Status == IPStatus.Success)
                { pixel = MakePixel(address, (byte)reply.RoundtripTime, PingStatus.Online, reply.Status); }
                else
                { pixel = MakePixel(address, byte.MaxValue, PingStatus.Offline, reply.Status); }
            }
            else
            {
                pixel = MakePixel(address, byte.MaxValue, PingStatus.Error, IPStatus.Unknown);
                Console.WriteLine($" ! {address} - {ExceptionUtils.GetRootException(task.Exception)?.Message ?? "null"}");
            }

            if (pixel.X < 0 || pixel.Y < 0 ||
                pixel.X > SdlWindow.Width || pixel.Y > SdlWindow.Height) return;

            Canvas[pixel.X, pixel.Y] = pixel.Color;
        }

        Pixel MakePixel(IPv4 address, ushort rtt, PingStatus pingStatus, IPStatus ipStatus)
        {
            Color color = pingStatus switch
            {
                PingStatus.InQueue => Color.White,
                PingStatus.Pinging => Color.White,
                PingStatus.Online => Gradient.Sample(rtt / 500f),
                PingStatus.Offline => Color.Black,
                PingStatus.Error => Color.Red,
                _ => Color.Red,
            };

            if (ipStatus != IPStatus.Unknown &&
                ipStatus != IPStatus.Success &&
                ipStatus != IPStatus.TimedOut)
            {
                color = Color.Magenta;
            }

            Point point = IPv4ToGrid(address.Int);

            return new Pixel(point.X, point.Y, color);
        }

        Point IPv4ToGrid(uint v) => Mapper.Map(v - IPv4.Min.Int);
        uint IPv4FromGrid(int x, int y) => (uint)Mapper.Unmap(x, y) + IPv4.Min.Int;

        public bool Tick()
        {
            SdlWindow.HandleEvents();
            // Win32Window.HandleEvents();

            Coroutines.Tick();

            Render();
            SdlWindow.Render();

            return IsRunning;
        }

        void Render()
        {
            if (RendererCoroutine == null || RendererCoroutine.IsFinished)
            { RendererCoroutine = new Coroutine(RenderCanvas()); }

            RendererCoroutine.Tick();
        }

        IEnumerator RenderCanvas()
        {
            int t = 0;
            for (int y = 0; y < SdlWindow.Height; y++)
            {
                for (int x = 0; x < SdlWindow.Width; x++)
                {
                    Color p = Canvas[x, y];
                    DrawPixel(x, y, p);

                    if (t++ > 500)
                    {
                        t = 0;
                        yield return null;
                    }
                }
            }
        }

        void DrawPixel(int x, int y, Color color)
        {
            SdlWindow.RenderColor = color.To255;
            SdlWindow.DrawPixel(x, y);
        }

        public void Dispose()
        {
            IsRunning = false;

            Save();
            SdlWindow.Dispose();
            Win32Window.Dispose();
        }

        void HandleEvent(SDL_Event @event)
        {
            switch (@event.type)
            {
                case SDL_EventType.SDL_QUIT:
                    {
                        IsRunning = false;
                        break;
                    }
                case SDL_EventType.SDL_MOUSEMOTION:
                    {
                        _ = SDL_GetMouseState(out int x, out int y);
                        IPv4 ipv4 = new(IPv4FromGrid(x, y));
                        SDL_SetWindowTitle(SdlWindow.Window, $"{ipv4}");
                        break;
                    }
            }
        }

        void Save()
        {
            string? savePath = GetFilePath();
            if (savePath == null) return;
            Serializer serializer = new();
            serializer.Serialize(SavedStatus);
            File.WriteAllBytes(savePath, serializer.Result);
            Console.WriteLine($"Saved {serializer.Result.Length} bytes to file");
        }

        void Load()
        {
            string? savePath = GetFilePath();
            if (savePath == null) return;
            if (!File.Exists(savePath)) return;
            byte[] bytes = File.ReadAllBytes(savePath);
            Deserializer deserializer = new(bytes);
            SavedStatus = deserializer.DeserializeObject<SavedStatus>();

            Console.WriteLine($"Loaded {bytes.Length} bytes from file");

            for (int i = 0; i < SavedStatus.PingResults.Count; i++)
            {
                PingResult item = SavedStatus.PingResults[i];

                Pixel pixel;

                if (item.Status == IPStatus.Success)
                { pixel = MakePixel(item.Address, item.RTT, PingStatus.Online, item.Status); }
                else
                { pixel = MakePixel(item.Address, byte.MaxValue, PingStatus.Offline, item.Status); }

                if (pixel.X < 0 || pixel.Y < 0 || pixel.X > SdlWindow.Width || pixel.Y > SdlWindow.Height) continue;

                Canvas[pixel.X, pixel.Y] = pixel.Color;
            }
        }

        static string? GetFilePath()
        {
            string? path = System.Reflection.Assembly.GetEntryAssembly()?.Location;
            if (path == null) return null;
            FileInfo file = new(path);
            if (file.Directory == null) return null;
            return Path.Combine(file.Directory.FullName, "pings.bin");
        }
    }
}
