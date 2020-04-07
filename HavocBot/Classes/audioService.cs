using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Discord;
using Discord.Audio;
using VideoLibrary;

namespace HavocBot
{
    /// <summary>
    /// Class that enables the bot to connect to voice channels and play songs
    /// </summary>
    public class audioService
    {
#pragma warning disable IDE0044 // Add readonly modifier
        private static Dictionary<ulong, IAudioClient> _connectedChannels = new Dictionary<ulong, IAudioClient>();
#pragma warning restore IDE0044 // Add readonly modifier
        /// <summary>
        /// The default constructor of the audioService class
        /// </summary>
        public audioService()
        {
        }

        /// <summary>
        /// Makes the bot join the specified voice channel
        /// </summary>
        /// <param name="guild">The guild of the voice channel to join</param>
        /// <param name="client">The audio client the bot is using</param>
        /// <returns></returns>
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task joinAudio(IGuild guild, IAudioClient client)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
#pragma warning restore CA1822 // Mark members as static
        {
            System.Diagnostics.Contracts.Contract.Requires(guild != null);
            

            _connectedChannels.Add(guild.Id, client);
        }

        /// <summary>
        /// Makes the bot leave the voice channel it is in
        /// </summary>
        /// <param name="guild">the guild containing the voice channel to leave</param>
        /// <returns></returns>
#pragma warning disable CA1822 // Mark members as static
        public async Task leaveAudio(IGuild guild)
#pragma warning restore CA1822 // Mark members as static
        {
            System.Diagnostics.Contracts.Contract.Requires(guild != null);
            if (_connectedChannels.Remove(guild.Id, out IAudioClient _client))
            {
                await _client.StopAsync().ConfigureAwait(false);
                //_client.Dispose();
            }
        }

        /// <summary>
        /// Has the bot download and play a specified youtube link
        /// </summary>
        /// <param name="guild">The guild that called the command</param>
        /// <param name="channel">The channel the bot is in</param>
        /// <param name="link">the youtube link to be played</param>
        /// <param name="service">the audioservice in use</param>
        /// <returns></returns>
        public async Task loadAndPlay(IGuild guild, IMessageChannel channel, string link, audioService service)
        {
            System.Diagnostics.Contracts.Contract.Requires(service != null);
            System.Diagnostics.Contracts.Contract.Requires(guild != null);
            

            string _link = link;
            string _guildID = guild.Id.ToString();
            string _path = $"{_guildID}-savedVid.mp4";

            globals.logMessage(Properties.strings.logAudioDownload);
            YouTube _youTube = YouTube.Default; // starting point for YouTube actions
            YouTubeVideo _video = _youTube.GetVideo(_link); // gets a Video object with info about the video
            File.WriteAllBytes(_path, _video.GetBytes());

            globals.logMessage(Properties.strings.logAudioPlayback);

            await service.sendAudioAsync(guild, channel, _path).ConfigureAwait(false);

            if (globals.playbackQueues[guild.Id].Peek() != null)
            {
                await loadAndPlay(guild, channel, globals.playbackQueues[guild.Id].Dequeue(), service).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sends audio to the currently connected channel
        /// </summary>
        /// <param name="guild">The guild requesting audio playback</param>
        /// <param name="channel">The voice channel the bot is in</param>
        /// <param name="path">The path of the file to be played</param>
        /// <returns></returns>
        public async Task sendAudioAsync(IGuild guild, IMessageChannel channel, string path)
        {
            System.Diagnostics.Contracts.Contract.Requires(channel != null);
            System.Diagnostics.Contracts.Contract.Requires(guild != null);
            
            if (!File.Exists(path))
            {
                await channel.SendMessageAsync(Properties.strings.audioFileNotFound).ConfigureAwait(false);
                return;
            }
                

            IAudioClient _client = _connectedChannels[guild.Id];
            // Create FFmpeg using the previous example
#pragma warning disable IDE0063 // Use simple 'using' statement
            using (var ffmpeg = createProcess(path))
#pragma warning restore IDE0063 // Use simple 'using' statement
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = _client.CreatePCMStream(AudioApplication.Mixed))
            {
                try {
                    globals.playbackPIDs[guild.Id] = ffmpeg.Id;
                    await output.CopyToAsync(discord).ConfigureAwait(false); }
                finally { await discord.FlushAsync().ConfigureAwait(false); }

                ffmpeg.Dispose();
            }
            
            
        }

        private Process createProcess(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                Verb = "runas"
            });
        }

        /// <summary>
        /// Ends the specified process
        /// </summary>
        /// <param name="id">the ID of the process to be stopped</param>
#pragma warning disable CA1822 // Mark members as static
        public void endProcess(int id)
#pragma warning restore CA1822 // Mark members as static
        {
            Process killProc = Process.GetProcessById(id);
            killProc.Kill();
        }
    }
    
}
