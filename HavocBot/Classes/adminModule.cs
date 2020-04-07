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
    /// Set of commands only executable by Admins or the Bot Owner
    /// </summary>
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    public class adminModule : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        /// Sets the target channel where commands are permitted
        /// this must be executed before commands may be created and announced properly.
        /// </summary>
        /// <returns></returns>
        [Command("setTarget")]
        [Summary("Sets the target channel for event announcments")]
        public async Task setTargetChannel()
        {
            globals.logMessage(Context, Properties.strings.catAdminTrigger, Properties.strings.cmdSetTarget);
            await storeTarget((Context.Channel.Id), Context.Guild.Id).ConfigureAwait(false);
        }

        /// <summary>
        /// Stores and assigns the specified channel id as the new target channel
        /// </summary>
        /// <param name="target">the channel id to be set as the new target</param>
        /// <param name="gID">The guild id of the target</param>
        /// <returns>returns task complete</returns>
        public async Task storeTarget(ulong target, ulong gID)
        {
            await Task.Run(() => globals.changeTarget(target, gID)).ConfigureAwait(false);
            await ReplyAsync(Properties.strings.utilityAnnounceSet).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the target channel where event announcments will be sent.
        /// this must be executed before events may be created and announced properly.
        /// </summary>
        /// <returns></returns>
        [Command("setEventTarget")]
        [Summary("Sets the target channel for event announcments")]
        public async Task setTargetEventChannel()
        {
            globals.logMessage(Context, Properties.strings.catAdminTrigger, Properties.strings.cmdSetEventTarget);
            await storeEventTarget((Context.Channel.Id), Context.Guild.Id).ConfigureAwait(false);
        }

        /// <summary>
        /// Stores and assigns the specified channel id as the new target event channel
        /// </summary>
        /// <param name="target">the channel id to be set as the new target</param>
        /// <param name="gID">The guild id of the target</param>
        /// <returns>returns task complete</returns>
        public async Task storeEventTarget(ulong target, ulong gID)
        {
            await Task.Run(() => globals.changeEventTarget(target, gID)).ConfigureAwait(false);
            await ReplyAsync(Properties.strings.eventAnnounceSet).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the date and time of upcoming maintenance
        /// </summary>
        /// <param name="start">Start date of maintenance</param>
        /// <param name="end">End date of maintenance</param>
        /// <param name="patch">Patch for maintenance</param>
        /// <returns></returns>
        [Command("addMaint")]
        [Summary("Sets upcoming maintenance")]
        public async Task setMaint([Summary("Start date of maintenance")] string start, [Summary("End date of maintenance")] string end, [Remainder][Summary("Patch for maintenance")] string patch)
        {
            await storeMaint(start, end, patch).ConfigureAwait(false);
        }

        /// <summary>
        /// stores the date and time of upcoming maintenance
        /// </summary>
        /// <param name="start">Start date of maintenance</param>
        /// <param name="end">End date of maintenance</param>
        /// <param name="patch">Patch for maintenance</param>
        /// <returns></returns>
        public async Task storeMaint(string start, string end, string patch)
        {
            IEnumerable<XElement> maintRetrieve =
                 from el in globals.commandStorage.Elements("maintenance")
                 select el;

            maintRetrieve = from el in maintRetrieve.Elements("maint")
                            select el;

            XElement changeTarget = (from el in maintRetrieve.Descendants("start")
                                     select el).First();
            changeTarget.Value = DateTime.Parse(start).ToString();

            changeTarget = (from el in maintRetrieve.Descendants("end")
                            select el).First();

            changeTarget.Value = DateTime.Parse(end).ToString();

            XAttribute patchChange = (from el in maintRetrieve.Attributes("patch")
                                      select el).First();

            patchChange.Value = patch;

            globals.commandStorage.Save(globals.storageFilePath);

            await ReplyAsync(Properties.strings.maintAddSuccess).ConfigureAwait(false);
            globals.logMessage(Context, Properties.strings.catAdminTrigger, Properties.strings.cmdSetMaint);
        }

        /// <summary>
        /// Changes the bots currently displayed status message
        /// </summary>
        /// <param name="status">status to be set to the bot</param>
        /// <returns></returns>
        [Command("setStatus")]
        [Summary("Sets bots displayed status")]
        public async Task setStatus([Remainder][Summary("Status to be set to the bot")] string status)
        {
            globals.logMessage(Context, Properties.strings.catAdminTrigger, Properties.strings.cmdSetStatus);
            await storeStatusMessage(status).ConfigureAwait(false);
        }

        /// <summary>
        /// Changes the bots currently displayed status message
        /// </summary>
        /// <param name="status">status to be set to the bot</param>
        /// <returns></returns>
        public async Task storeStatusMessage(string status)
        {
            await Task.Run(() => globals.changeStatus(status)).ConfigureAwait(false);
            await ReplyAsync(Properties.strings.botStatusSet).ConfigureAwait(false);
        }

        /// <summary>
        /// Triggers an embed announcing bot downtime
        /// </summary>
        /// <returns></returns>
        [Command("startBotDowntime")]
        [Summary("triggers an embed announcing bot downtime")]
        public async Task showDownTime()
        {
            globals.logMessage(Context, Properties.strings.catAdminTrigger, Properties.strings.cmdDowntimeStart);
            await Task.Run(() => havocBotClass.showDownTime()).ConfigureAwait(false);
        }

        /// <summary>
        /// triggers an embed displaying patch notes
        /// </summary>
        /// <returns></returns>
        [Command("showAllPatchNotes")]
        [Summary("triggers an embed displaying patch notes")]
        public async Task showAllPatchNotes()
        {
            globals.logMessage(Context, Properties.strings.catAdminTrigger, Properties.strings.cmdShowAllPatchNotes);
            await Task.Run(() => havocBotClass.showAllPatchNotes()).ConfigureAwait(false);
        }

        /// <summary>
        /// Ends an event early or ends an event that was repeating
        /// </summary>
        /// <param name="name">the name of the event to be cancelled</param>
        /// <returns></returns>
        [Command("CancelEvent")]
        [Summary("ends the specified event")]
        public async Task cancelEventAsync([Remainder] string name)
        {
            globals.logMessage(Context, Properties.strings.catAdminTrigger, $"Cancel Event: {name}");
            await cancelEvent(name).ConfigureAwait(false);
        }

        /// <summary>
        /// Ends an event early or ends an event that was repeating
        /// </summary>
        /// <param name="name">the name of the event to be cancelled</param>
        /// <returns></returns>
        public async Task cancelEvent(string name)
        {
            if (globals.getEvent(name, out botEvents retrievedEvent))
            {
                IEnumerable<XElement> eventRetrieve =
                 from el in globals.commandStorage.Elements("events")
                 select el;

                eventRetrieve = from el in eventRetrieve.Elements("event")
                                where (string)el.Element("name") == retrievedEvent.name
                                select el;
                XElement changeTarget = (from el in eventRetrieve.Descendants("start")
                                         select el).Last();

                retrievedEvent.startDate = DateTime.Now.AddDays(-1);

                changeTarget.Value = retrievedEvent.startDate.ToString();
                globals.commandStorage.Save(globals.storageFilePath);
                await Context.Channel.SendMessageAsync($"The event \"{name}\" has been cancelled").ConfigureAwait(false);
            }

        }
    }

}
