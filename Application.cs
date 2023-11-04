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
using Win32;
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

    class Canvas
    {
        public Color[,][,] Chunks;
        readonly Color InitialColor;
        readonly int ChunksWidth;
        readonly int ChunksHeight;

        public int Width => ChunksWidth * byte.MaxValue;
        public int Height => ChunksHeight * byte.MaxValue;

        public Canvas(int chunksWidth, int chunksHeight, Color initialColor)
        {
            Chunks = new Color[chunksWidth, chunksHeight][,];
            ChunksWidth = chunksWidth;
            ChunksHeight = chunksHeight;
            InitialColor = initialColor;
        }

        public Color this[int x, int y]
        {
            get
            {
                Color[,]? chunk = GetChunk(x, y);
                if (chunk == null) return InitialColor;
                return chunk[x % byte.MaxValue, y % byte.MaxValue];
            }
            set
            {
                Color[,]? chunk = GetChunk(x, y);
                if (chunk == null) return;
                chunk[x % byte.MaxValue, y % byte.MaxValue] = value;
            }
        }

        public Color this[float x, float y]
        {
            get => this[(int)Math.Round(x), (int)Math.Round(y)];
            set => this[(int)Math.Round(x), (int)Math.Round(y)] = value;
        }

        public Color this[Point point]
        {
            get => this[point.X, point.Y];
            set => this[point.X, point.Y] = value;
        }

        public Color this[Point<float> point]
        {
            get => this[(int)Math.Round(point.X), (int)Math.Round(point.Y)];
            set => this[(int)Math.Round(point.X), (int)Math.Round(point.Y)] = value;
        }

        public Color[,]? GetChunk(int x, int y)
        {
            int x_ = x / byte.MaxValue;
            int y_ = y / byte.MaxValue;

            if (x < 0 || y < 0)
            { return null; }

            if (x_ >= ChunksWidth || y_ >= ChunksHeight)
            { return null; }

            Color[,] chunk = Chunks[x_, y_];
            if (chunk == null)
            {
                Color[,] newChunk = new Color[byte.MaxValue, byte.MaxValue];
                ArrayUtils.Fill(newChunk, InitialColor);
                Chunks[x_, y_] = newChunk;
                return newChunk;
            }
            return chunk;
        }

        public bool IsVisible(Point p)
        {
            if (p.X < 0 || p.Y < 0) return false;
            if (p.X >= Width || p.Y >= Height) return false;
            return true;
        }
    }

    internal class Application : IDisposable
    {
        readonly Gradient Gradient = new(Color.FromHex("#8000FF"), Color.FromHex("#00FFA2"));
        readonly Canvas Canvas;
        Rect ViewPort;
        readonly float viewportAspectRatio;
        Coroutine? RendererCoroutine = null;
        readonly Mapper2D Mapper;
        SavedStatus SavedStatus = new();
        bool IsRunning = true;
        bool ShouldQuit = false;
        bool ShouldRestartRendering = false;

        /// <summary>
        /// In canvas space
        /// </summary>
        Point MousePosition;

        static readonly byte[] PingPayload = Encoding.ASCII.GetBytes("bruh");

        readonly SdlWindow SdlWindow;
        readonly Form Win32Form;

        readonly Button ButtonSave;
        readonly Button ButtonStop;
        readonly Button ButtonStart;

        internal Application(string[] args)
        {
            SdlWindow = new SdlWindow("NetMap", byte.MaxValue * 4, byte.MaxValue * 3);
            SdlWindow.OnEvent += HandleEvent;

            Win32Form = new Form("NetMap - Config", 300, 200);

            ushort ID_ButtonSave;
            ushort ID_ButtonStop;
            ushort ID_ButtonStart;

            ButtonSave = new(Win32Form.Handle, "Save", new Win32.Rect(10, 10, 60, 24), Win32Form.GenerateControlId(out ID_ButtonSave));
            ButtonStop = new(Win32Form.Handle, "Stop", new Win32.Rect(80, 10, 60, 24), Win32Form.GenerateControlId(out ID_ButtonStop));
            ButtonStart = new(Win32Form.Handle, "Start", new Win32.Rect(150, 10, 60, 24), Win32Form.GenerateControlId(out ID_ButtonStart));

            Win32Form.Controls.Add(ID_ButtonSave, ButtonSave);
            ButtonSave.OnClick += (sender) =>
            {
                Save();
            };

            Win32Form.Controls.Add(ID_ButtonStop, ButtonStop);
            ButtonStop.OnClick += (sender) =>
            {
                IsRunning = false;
                sender.Enabled = false;
                ButtonStart.Enabled = true;
            };
            ButtonStop.Enabled = false;

            Win32Form.Controls.Add(ID_ButtonStart, ButtonStart);
            ButtonStart.OnClick += (sender) =>
            {
                IsRunning = true;
                sender.Enabled = false;
                ButtonStop.Enabled = true;

                if (!IsRunning)
                { Start(); }
            };
            ButtonStart.Enabled = false;

            Canvas = new Canvas(4, 3, Color.Gray);
            ViewPort = new Rect(0, 0, SdlWindow.Width, SdlWindow.Height);
            viewportAspectRatio = SdlWindow.Width / SdlWindow.Height;
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

            ButtonStop.Enabled = true;
            ButtonStart.Enabled = false;

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

                Point point = IPv4ToGrid(ipv4.Int);

                if (point.X < 0 || point.Y < 0) continue;
                if (point.Y > SdlWindow.Height) break;
                if (point.X > SdlWindow.Width) continue;

                Canvas[point.X, point.Y] = Color.Black;

                if (!ipv4.IsPingable) continue;

                Canvas[point.X, point.Y] = Color.Blue;

                try
                {
                    while (t++ > 16)
                    {
                        t = 0;
                        Thread.Sleep(500);
                    }

                    Ping ping = new();
                    Task<PingReply> task = ping.SendPingAsync(ipv4.Address, byte.MaxValue, PingPayload);
                    task.ContinueWith(OnPingResult, ipv4);

                    Thread.Sleep(100);
                }
                catch (Exception)
                {
                    Canvas[point.X, point.Y] = Color.Magenta;
                    continue;
                }
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

                if (reply.Status == IPStatus.Success)
                { Console.WriteLine($" < {address} - {reply.Status} - {reply.RoundtripTime} ms"); }
                else
                { Console.WriteLine($" < {address} - {reply.Status}"); }

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
            Form.HandleEvents();

            Coroutines.Tick();

            unsafe
            {
                byte* keys = (byte*)SDL_GetKeyboardState(out int l).ToPointer();
                bool left = keys[(int)SDL_Scancode.SDL_SCANCODE_LEFT] != 0;
                bool down = keys[(int)SDL_Scancode.SDL_SCANCODE_DOWN] != 0;
                bool right = keys[(int)SDL_Scancode.SDL_SCANCODE_RIGHT] != 0;
                bool up = keys[(int)SDL_Scancode.SDL_SCANCODE_UP] != 0;
                bool plus = keys[(int)SDL_Scancode.SDL_SCANCODE_KP_PLUS] != 0;
                bool minus = keys[(int)SDL_Scancode.SDL_SCANCODE_KP_MINUS] != 0;

                bool changed = false;

                float t = .1f;

                if (!plus && minus)
                {
                    ViewPort.X -= 1 * t;
                    ViewPort.Y -= 1 * t;
                    ViewPort.Width += 2 * t;
                    ViewPort.Height -= 2 * t;
                    changed = true;
                }
                else if (plus && !minus)
                {
                    ViewPort.X += 1 * t;
                    ViewPort.Y += 1 * t;
                    ViewPort.Width -= 2 * t;
                    ViewPort.Height -= 2 * t;
                    changed = true;
                }

                if (left && !right)
                {
                    ViewPort.X -= t;
                    changed = true;
                }
                else if (!left && right)
                {
                    ViewPort.X += t;
                    changed = true;
                }

                if (up && !down)
                {
                    ViewPort.Y -= t;
                    changed = true;
                }
                else if (!up && down)
                {
                    ViewPort.Y += t;
                    changed = true;
                }

                if (changed)
                {
                    // ViewPort.X = Math.Clamp(ViewPort.X, 0f, Math.Max(0, -(Canvas.Width - ViewPort.Width)));
                    // ViewPort.Y = Math.Clamp(ViewPort.Y, 0f, Math.Max(0, -(Canvas.Height - ViewPort.Height)));

                    // ViewPort.Height = Math.Clamp(ViewPort.Height, 16, byte.MaxValue * 3);

                    ViewPort.Width = viewportAspectRatio * ViewPort.Height;

                    // SDL_Rect rect = ViewPort.ToSdl();
                    // SDL_RenderSetViewport(SdlWindow.Renderer, ref rect);

                    SdlWindow.RenderColor = Color.Gray.To255;
                    SDL_RenderClear(SdlWindow.Renderer);

                    ShouldRestartRendering = true;
                }
            }

            Render();
            SdlWindow.Render();

            return IsRunning || !ShouldQuit;
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

            ShouldRestartRendering = false;

            if (false)
            {
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
            else
            {
                RectInt rounded = ViewPort.Round();
                Point position = rounded.Position;

                /*
                float ratioX = SdlWindow.Width / ViewPort.Width;
                float ratioY = SdlWindow.Height / ViewPort.Height;
                Point<float> ratio = new(ratioX, ratioY);
                */

                void DrawSelection()
                {
                    /*
                    Point p = MousePosition / 16 * 32;
                    if (p.X < 0 || p.Y < 0) return;
                    p -= position;
                    SDL_SetRenderDrawColor(SdlWindow.Renderer, 255, 255, 255, 255);
                    SDL_RenderDrawLines(SdlWindow.Renderer, new SDL_Point[5]
                    {
                        (SDL_Point)p,
                        (SDL_Point)(p + new Point(0, 32)),
                        (SDL_Point)(p + new Point(32, 32)),
                        (SDL_Point)(p + new Point(32, 0)),
                        (SDL_Point)p,
                    }, 5);
                    */
                }

                for (int y = 0; y < Canvas.Height; y++)
                {
                    for (int x = 0; x < Canvas.Width; x++)
                    {
                        Point p = new(x, y);

                        p += position;

                        p /= 2;

                        if (!Canvas.IsVisible(p)) continue;

                        Color c = Canvas[p];
                        DrawPixel(x, y, c);

                        if (ShouldRestartRendering) yield break;

                        if (t++ > 500)
                        {
                            t = 0;
                            DrawSelection();
                            yield return null;
                        }
                    }
                }
            }
        }

        Point ViewportToCanvas(Point viewportPoint)
        {
            Point viewport = ViewPort.Position.Round();
            return (viewportPoint + viewport) / 2;
        }

        void DrawPixel(int x, int y, Color color)
        {
            SdlWindow.RenderColor = color.To255;
            SdlWindow.DrawPixel(x, y);
        }

        public void Dispose()
        {
            IsRunning = false;
            ShouldQuit = true;

            Save();
            SdlWindow.Dispose();
            Win32Form.Dispose();
        }

        void HandleEvent(SDL_Event @event)
        {
            switch (@event.type)
            {
                case SDL_EventType.SDL_QUIT:
                    {
                        IsRunning = false;
                        ShouldQuit = true;
                        break;
                    }
                case SDL_EventType.SDL_MOUSEMOTION:
                    {
                        _ = SDL_GetMouseState(out int x, out int y);
                        Point p = ViewportToCanvas(new Point(x, y));

                        MousePosition = p;

                        if (!Canvas.IsVisible(p))
                        {
                            SDL_SetWindowTitle(SdlWindow.Window, $"NetMap");
                        }
                        else
                        {
                            IPv4 ipv4 = new(IPv4FromGrid(p.X, p.Y));
                            SDL_SetWindowTitle(SdlWindow.Window, $"NetMap - {ipv4}");
                        }
                        break;
                    }
            }
        }

        void Save()
        {
            string? savePath = GetFilePath();
            if (savePath == null) return;

            string? backupSavePath = GetFilePath("pings-backup.bin");
            if (File.Exists(savePath) && backupSavePath != null)
            { File.Copy(savePath, backupSavePath, true); }

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

        static string? GetFilePath(string filename = "pings.bin")
        {
            string? path = System.Reflection.Assembly.GetEntryAssembly()?.Location;
            if (path == null) return null;
            FileInfo file = new(path);
            if (file.Directory == null) return null;
            return Path.Combine(file.Directory.FullName, filename);
        }
    }
}
