using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using Discord;
using Discord.Audio;
using VideoLibrary;

namespace HavocBot
{
    /// <summary>
    /// .
    /// </summary>
    public class audioService
    {
        
        private static Dictionary<ulong, IAudioClient> _connectedChannels = new Dictionary<ulong, IAudioClient>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public async Task joinAudio(IGuild guild, IAudioClient client)
        {
            System.Diagnostics.Contracts.Contract.Requires(guild != null);
            

            _connectedChannels.Add(guild.Id, client);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        public async Task leaveAudio(IGuild guild)
        {
            System.Diagnostics.Contracts.Contract.Requires(guild != null);
            if (_connectedChannels.Remove(guild.Id, out IAudioClient _client))
            {
                await _client.StopAsync().ConfigureAwait(false);
                //await Log(LogSeverity.Info, $"Disconnected from voice on {guild.Name}.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="channel"></param>
        /// <param name="link"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        public async Task loadAndPlay(IGuild guild, IMessageChannel channel, string link, audioService service)
        {
            System.Diagnostics.Contracts.Contract.Requires(service != null);
            System.Diagnostics.Contracts.Contract.Requires(guild != null);
            

            string _link = link;
            string _guildID = guild.Id.ToString();
            string _path = $"{_guildID}-savedVid.mp4";

            Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {"begin video download"}");
            YouTube _youTube = YouTube.Default; // starting point for YouTube actions
            YouTubeVideo _video = _youTube.GetVideo(_link); // gets a Video object with info about the video
            File.WriteAllBytes(_path, _video.GetBytes());

            Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {"begin video playback"}");

            await service.sendAudioAsync(guild, channel, _path).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="channel"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task sendAudioAsync(IGuild guild, IMessageChannel channel, string path)
        {
            System.Diagnostics.Contracts.Contract.Requires(channel != null);
            System.Diagnostics.Contracts.Contract.Requires(guild != null);
            // Your task: Get a full path to the file if the value of 'path' is only a filename.
            try
            {
                if (!File.Exists(path))
                {
                    await channel.SendMessageAsync("File does not exist.").ConfigureAwait(false);
                    return;
                }
                //if (_connectedChannels.TryGetValue(guild.Id, out _client))
                // {

                IAudioClient _client = _connectedChannels[guild.Id];
                    // Create FFmpeg using the previous example
                    using (var ffmpeg = createProcess(path))
                    using (var output = ffmpeg.StandardOutput.BaseStream)
                    using (var discord = _client.CreatePCMStream(AudioApplication.Mixed))
                    {
                        try { await output.CopyToAsync(discord); }
                        finally { await discord.FlushAsync(); }
                    }

                   
               // }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
        }

        private Process createProcess(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }

        public void endProcess()
        {
            Process killFfmpeg = new Process();
            ProcessStartInfo taskkillStartInfo = new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = "/F /IM ffmpeg.exe",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            killFfmpeg.StartInfo = taskkillStartInfo;
            killFfmpeg.Start();
        }
    }
}
