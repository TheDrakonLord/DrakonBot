using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


namespace HavocBot
{
    /// <summary>
    /// Module that contains commands related to the audio features of the bot
    /// </summary>
    public class audioModule : ModuleBase<ICommandContext>
    {
        // Scroll down further for the AudioService.
        // Like, way down

#pragma warning disable IDE0044 // Add readonly modifier
        private audioService _service = new audioService();
#pragma warning restore IDE0044 // Add readonly modifier

        // You *MUST* mark these commands with 'RunMode.Async'
        // otherwise the bot will not respond until the Task times out.
        /// <summary>
        /// Makes the bot join a specified channel or the channel the user is in
        /// </summary>
        /// <returns></returns>
        [Command("join", RunMode = RunMode.Async)]
        public async Task joinCmd(IVoiceChannel channel = null)
        {
            globals.logMessage("Audio Command triggered", $"join by {Context.User.Username} in {Context.Guild.Name} ({Context.Guild.Id})");
            channel ??= (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null) { await Context.Channel.SendMessageAsync(Properties.strings.noVoiceChannelError).ConfigureAwait(false); return; }

            // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
            var audioClient = await channel.ConnectAsync().ConfigureAwait(false);

            await _service.joinAudio(Context.Guild, audioClient).ConfigureAwait(false);

        }

        // Remember to add preconditions to your commands,
        // this is merely the minimal amount necessary.
        // Adding more commands of your own is also encouraged.
        /// <summary>
        /// Makes the bot leave the voice channel it is in
        /// </summary>
        /// <returns></returns>
        [Command("leave", RunMode = RunMode.Async)]
        public async Task leaveCmd()
        {
            globals.logMessage("Audio Command triggered", $"leave by {Context.User.Username} in {Context.Guild.Name} ({Context.Guild.Id})");
            await _service.leaveAudio(Context.Guild).ConfigureAwait(false);
        }

        /// <summary>
        /// Makes the bot download and play the specified youtube link
        /// </summary>
        /// <param name="song">the youtube link of the song to be played</param>
        /// <returns></returns>
        [Command("play", RunMode = RunMode.Async)]
        public async Task playCmd([Remainder] string song)
        {
            globals.logMessage("Audio Command triggered", $"play {song} by {Context.User.Username} in {Context.Guild.Name} ({Context.Guild.Id})");
            await _service.loadAndPlay(Context.Guild, Context.Channel, song, _service).ConfigureAwait(false);
        }

        /// <summary>
        /// Makes the bot stop playing the current song
        /// </summary>
        /// <returns></returns>
        [Command("stop", RunMode = RunMode.Async)]
        public async Task stopCmd()
        {
            globals.logMessage("Audio Command triggered", $"stop by {Context.User.Username} in {Context.Guild.Name} ({Context.Guild.Id})");
            int playbackPID = globals.playbackPIDs[Context.Guild.Id];
            if (playbackPID != 0)
            {
                await Task.Run(() => _service.endProcess(playbackPID)).ConfigureAwait(false);
            }
            else
            {
                await Context.Channel.SendMessageAsync(Properties.strings.audioNoPlaybackError).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Adds a song to the queue to be played after the current one finishes
        /// </summary>
        /// <param name="song">The youtube link of the song to be added to the queue</param>
        /// <returns></returns>
        [Command("queue", RunMode = RunMode.Async)]
        public async Task queueCmd([Remainder] string song)
        {
            globals.logMessage("Audio Command triggered", $"queue by {Context.User.Username} in {Context.Guild.Name} ({Context.Guild.Id})");
            await Task.Run(() => globals.playbackQueues[Context.Guild.Id].Enqueue(song)).ConfigureAwait(false);
        }

    }
}
