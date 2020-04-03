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
    /// .
    /// </summary>
    public class audioService
    {
        private static Dictionary<ulong, IAudioClient> _connectedChannels = new Dictionary<ulong, IAudioClient>();
        /// <summary>
        /// 
        /// </summary>
        public audioService()
        {
        }
        
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
                //_client.Dispose();
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

            await service.sendAudioAsync(guild, channel, _path);

            if (globals.playbackQueues[guild.Id].Peek() != null)
            {
                await loadAndPlay(guild, channel, globals.playbackQueues[guild.Id].Dequeue(), service);
            }
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
            
            if (!File.Exists(path))
            {
                await channel.SendMessageAsync(Properties.strings.audioFileNotFound).ConfigureAwait(false);
                return;
            }
                

            IAudioClient _client = _connectedChannels[guild.Id];
            // Create FFmpeg using the previous example
            using (var ffmpeg = createProcess(path))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = _client.CreatePCMStream(AudioApplication.Mixed))
            {
                try {
                    globals.playbackPIDs[guild.Id] = ffmpeg.Id;
                    await output.CopyToAsync(discord); }
                finally { await discord.FlushAsync(); }

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
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void endProcess(int id)
        {
            Process killProc = Process.GetProcessById(id);
            killProc.Kill();
        }
    }
    
}
