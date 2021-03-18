using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Guilded.NET;
using Guilded.NET.Objects;
using Guilded.NET.Objects.Events;
using Newtonsoft.Json.Linq;

namespace Guilded_Notifications
{
    internal struct Program
    {
        #region WinAPI
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)] private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        internal static void FlashOtherWindow(IntPtr windowHandle)
        {
            var fInfo = new FLASHWINFO();
            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.dwFlags = 2;
            fInfo.dwTimeout = 0;
            fInfo.hwnd = windowHandle;
            fInfo.uCount = 3;

            FlashWindowEx(ref fInfo);
        }
        #endregion
        #region Variables
        /// <summary>
        /// The config file name.
        /// </summary>
        private const string file = "config.json";

        /// <summary>
        /// Guilded's process name.
        /// </summary>
        private const string name = "Guilded";

        /// <summary>
        /// The prefix to use for console messages.
        /// </summary>
        private const string prefix = "[*] ";

        /// <summary>
        /// The current owner id.
        /// </summary>
        private static GId id;
        #endregion

        /// <summary>
        /// Print a message with <see cref="prefix"/>.
        /// </summary>
        private static void print_pf(string content) => Console.WriteLine(prefix + content);

        /// <summary>
        /// Startup function.
        /// </summary>
        private static async Task Main() => await setup_bot();

        /// <summary>
        /// Setup the bot
        /// </summary>
        private static async Task setup_bot()
        {
            var config = JObject.Parse(await File.ReadAllTextAsync(file));
            string? email = config["email"]?.ToString(), password = config["password"]?.ToString();
            print_pf("Starting bot");
            await start_bot(email, password);
        }

        /// <summary>
        /// Start the bot.
        /// </summary>
        private static async Task start_bot(string? email, string? password)
        {
            // Use no prefix.
            using GuildedUserClient client = new(email, password,
                new GuildedClientConfig(GuildedClientConfig.BasicPrefix(string.Empty)));
            print_pf("Connecting...");
            await client.ConnectAsync();
            print_pf($"Connected as {client.Me.Username}!");
            id = client.Me.Id;
            client.MessageCreated += ClientOnMessageCreated;
            await Task.Delay(-1);
        }

        /// <summary>
        /// Called when a message has been received.
        /// </summary>
        private static void ClientOnMessageCreated(object? sender, MessageCreatedEvent e)
        {
            // If the author id is the same as the owner, ignore it.
            if (e.AuthorId == id) return;
            foreach (Process process in Process.GetProcessesByName(name)) FlashOtherWindow(process.MainWindowHandle);
        }
    }
}