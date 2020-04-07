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
    /// Module for storing event commands and they're related methods
    /// </summary>
    public class eventModule : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        /// Displays a specified event
        /// </summary>
        /// <param name="reqEvent">the name of the event to display</param>
        /// <returns>returns task complete</returns>
        [Command("showEvent")]
        [Summary("Displays an event that matches the specified name")]
        public async Task showEventAsync([Remainder] [Summary("The name of the event to display")] string reqEvent)
        {
            if (Context.Channel.Id == globals.guildSettings[$"g{Context.Guild.Id}"][1])
            {
                globals.logMessage(Context, Properties.strings.catCmdTrigger, Properties.strings.cmdShowEvent);
                await retrieveEvent(reqEvent, Properties.strings.msgShowEvent).ConfigureAwait(false);
            }
        }


        /// <summary>
        /// creates a new event
        /// </summary>
        /// <param name="name">the name of the new event. must be unique</param>
        /// <param name="start">the start date of the new event</param>
        /// <param name="end">the end date of the new event</param>
        /// <param name="type"></param>
        /// <param name="mentions"></param>
        /// <param name="description"></param>
        /// <returns>returns task complete</returns>
        [Command("newEvent")]
        [Summary("Stores and event with the specified parameters")]
        public async Task createEventAsync([Summary("event name")] string name,
            [Summary("Start Date")] string start, [Summary("End Date")] string end,
            [Summary("type")] string type = "other", [Summary("mentions")] string mentions = "none",
            [Remainder] [Summary("Description")] string description = "N/A")
        {
            if (Context.Channel.Id == globals.guildSettings[$"g{Context.Guild.Id}"][1])
            {
                if (globals.eventCalendar.ContainsKey(name))
                {
                    await ReplyAsync(Properties.strings.errorEventExists).ConfigureAwait(false);
                    globals.logMessage(Context, Properties.strings.catError, Properties.strings.logErrorEventExists);
                }
                else
                {
                    globals.logMessage(Context, Properties.strings.catCmdTrigger, Properties.strings.cmdNewEvent);
                    await createEvent(name, start, end, type, mentions, description).ConfigureAwait(false);
                }
            }
            else
            {
                globals.logMessage(Properties.strings.catError);
            }
        }

        /// <summary>
        /// alters the specified details fo an event
        /// </summary>
        /// <param name="name">name of the event to be changed</param>
        /// <param name="field">event field to be edited</param>
        /// <param name="changes">changes to be made to the event field</param>
        /// <returns></returns>
        [Command("editEvent")]
        [Summary("Alters specified details of an event")]
        public async Task editEventAsync([Summary("event name")] string name, [Summary("field to edit")] string field, [Remainder] [Summary("changes to be made")] string changes)
        {
            await editEvent(name, field, changes).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds or Removes the user from the RSVP list of a specified event
        /// </summary>
        /// <param name="name">name of specified event</param>
        /// <returns>returns task complete</returns>
        [Command("rsvp")]
        [Summary("Adds or Removes the user from the RSVP list of a specified event")]
        public async Task toggleRSVPAsync([Remainder] [Summary("name of specified event")] string name)
        {
            if (Context.Channel.Id == globals.guildSettings[$"g{Context.Guild.Id}"][1])
            {
                if (globals.eventCalendar.ContainsKey(name))
                {
                    globals.logMessage(Context, Properties.strings.catCmdTrigger, Properties.strings.cmdRSVP);
                    await toggleRSVP(name).ConfigureAwait(false);
                }
                else
                {
                    await ReplyAsync(Properties.strings.msgEventNotFound).ConfigureAwait(false);
                    globals.logMessage(Context, Properties.strings.catError, Properties.strings.errorEventNotFound);
                }
            }
        }

        /// <summary>
        /// shows all active events
        /// </summary>
        /// <returns></returns>
        [Command("events")]
        [Summary("Shows all active events")]
        public async Task showAllEventsAsync()
        {
            globals.logMessage(Context, Properties.strings.catCmdTrigger, Properties.strings.cmdEvents);
            await showAllEvents().ConfigureAwait(false);
        }

        /// <summary>
        /// asyncronous task to store the new event
        /// </summary>
        /// <param name="name">the name of the new event. must be unique</param>
        /// <param name="start">the start date of the new event</param>
        /// <param name="end">the end date of the new event</param>
        /// <param name="type"></param>
        /// <param name="mentions"></param>
        /// <param name="description"></param>
        /// <returns>returns task complete</returns>
        public async Task createEvent(string name, string start, string end, string type, string mentions, string description)
        {
            try
            {
                //create an event using the specified parameters
                botEvents storedEvent = new botEvents(name, DateTime.Parse(start), DateTime.Parse(end), globals.commandStorage)
                {
                    author = Context.User.Username
                };
                Uri.TryCreate(Context.User.GetAvatarUrl(ImageFormat.Auto, 128), UriKind.RelativeOrAbsolute, out Uri uriResult);
                storedEvent.authorURL = uriResult;
                storedEvent.type = type;
                storedEvent.mentions = mentions;
                storedEvent.description = description;
                storedEvent.guild = Context.Guild.Id.ToString();
                //add the event to the xml tree
                globals.storeEvent(storedEvent);
                //display the created event to the user
                await retrieveEvent(storedEvent.name, Properties.strings.msgEventCreated).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                globals.logMessage(ex.ToString());
                await Context.Channel.SendMessageAsync(Properties.strings.msgUnkownError).ConfigureAwait(false);
            }

        }

        /// <summary>
        /// finds a specified event and displays it to the user
        /// </summary>
        /// <param name="eventName">the name of the requested event</param>
        /// <param name="caption">The caption to be displayed before the embed</param>
        /// <returns>returns task complete</returns>
        public async Task retrieveEvent(string eventName, string caption)
        {
            if (globals.getEvent(eventName, out botEvents retrievedEvent))
            {
                //create an embed based off of the loaded event
                EmbedBuilder eventEmbed;

                eventEmbed = globals.generateEventEmbed(retrievedEvent);

                //display the loaded event to the user
                await Context.Channel.SendMessageAsync(caption, false, eventEmbed.Build()).ConfigureAwait(false);
            }
            else
            {
                await Context.Channel.SendMessageAsync(Properties.strings.msgEventNotFound).ConfigureAwait(false);
            }

        }

        /// <summary>
        /// alters the specified details fo an event
        /// </summary>
        /// <param name="name">name of the event to be changed</param>
        /// <param name="field">event field to be edited</param>
        /// <param name="changes">changes to be made to the event field</param>
        /// <returns></returns>
        public async Task editEvent(string name, string field, string changes)
        {
            System.Diagnostics.Contracts.Contract.Requires(field != null);
            if (globals.getEvent(name, out botEvents retrievedEvent))
            {
                IEnumerable<XElement> eventRetrieve =
                 from el in globals.commandStorage.Elements("events")
                 select el;

                eventRetrieve = from el in eventRetrieve.Elements("event")
                                where (string)el.Element("name") == retrievedEvent.name
                                select el;
                XElement changeTarget;
                int intChange;
#pragma warning disable CA1304 // Specify CultureInfo
                switch (field.ToLower())
#pragma warning restore CA1304 // Specify CultureInfo
                {
                    case "start":
                        retrievedEvent.startDate = DateTime.Parse(changes);
                        changeTarget = (from el in eventRetrieve.Descendants("start")
                                        select el).Last();
                        changeTarget.Value = retrievedEvent.startDate.ToString();
                        await Context.Channel.SendMessageAsync($"The start date of the event \"{name}\" has been changed to {changes}").ConfigureAwait(false);
                        break;
                    case "end":
                        retrievedEvent.endDate = DateTime.Parse(changes);
                        changeTarget = (from el in eventRetrieve.Descendants("end")
                                        select el).Last();
                        changeTarget.Value = retrievedEvent.endDate.ToString();
                        await Context.Channel.SendMessageAsync($"The end date of the event \"{name}\" has been changed to {changes}").ConfigureAwait(false);
                        break;
                    case "type":
                        retrievedEvent.type = changes;
                        changeTarget = (from el in eventRetrieve.Descendants("type")
                                        select el).Last();
                        changeTarget.Value = retrievedEvent.type;
                        await Context.Channel.SendMessageAsync($"The type of the event \"{name}\" has been changed to {changes}").ConfigureAwait(false);
                        break;
                    case "description":
                        retrievedEvent.description = changes;
                        changeTarget = (from el in eventRetrieve.Descendants("description")
                                        select el).Last();
                        changeTarget.Value = retrievedEvent.description;
                        await Context.Channel.SendMessageAsync($"The description of the event \"{name}\" has been changed to {changes}").ConfigureAwait(false);
                        break;
                    case "remindermin":
                    case "reminder":
                        if (int.TryParse(changes, out intChange))
                        {
                            retrievedEvent.reminderMinutes = intChange;
                            changeTarget = (from el in eventRetrieve.Descendants("reminder")
                                            select el).Last();
                            changeTarget.Value = retrievedEvent.reminderMinutes.ToString();
                        }
                        await Context.Channel.SendMessageAsync($"The reminder option of the event \"{name}\" has been changed to {changes} minutes").ConfigureAwait(false);
                        break;
                    case "reminderhrs":
                        if (int.TryParse(changes, out intChange))
                        {
                            retrievedEvent.reminderHours = intChange;
                            changeTarget = (from el in eventRetrieve.Descendants("reminder")
                                            select el).Last();
                            changeTarget.Value = retrievedEvent.reminderMinutes.ToString();
                        }
                        await Context.Channel.SendMessageAsync($"The reminder option of the event \"{name}\" has been changed to {changes} hours").ConfigureAwait(false);
                        break;
                    case "repeat":
                        retrievedEvent.repeat = changes;
                        changeTarget = (from el in eventRetrieve.Descendants("repeat")
                                        select el).Last();
                        changeTarget.Value = retrievedEvent.repeat;
                        await Context.Channel.SendMessageAsync($"The repeat option of the event \"{name}\" has been changed to {changes}").ConfigureAwait(false);
                        break;
                    case "mentions":
                        retrievedEvent.mentions = changes;
                        changeTarget = (from el in eventRetrieve.Descendants("mentions")
                                        select el).Last();
                        changeTarget.Value = retrievedEvent.mentions;
                        await Context.Channel.SendMessageAsync($"The mentions option of the event \"{name}\" has been changed to {changes}").ConfigureAwait(false);
                        break;
                    default:
                        await Context.Channel.SendMessageAsync(Properties.strings.msgFieldNotFound).ConfigureAwait(false);
                        break;
                }
                globals.commandStorage.Save(globals.storageFilePath);
            }
            else
            {
                await Context.Channel.SendMessageAsync(Properties.strings.msgEventNotFound).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Toggles the users RSVP for the specified event
        /// </summary>
        /// <param name="name">the name of the event to be retrieved</param>
        /// <returns>task complete</returns>
        public async Task toggleRSVP(string name)
        {
            string nickname;
            if (Context.Guild.GetUser(Context.User.Id).Nickname != null)
            {
                nickname = Context.Guild.GetUser(Context.User.Id).Nickname;
            }
            else
            {
                nickname = Context.User.Username;
            }
            string id = "<@" + Context.User.Id.ToString() + ">";

            if (globals.getEvent(name, out botEvents retrievedEvent))
            {
                bool toggle = false;

                if (retrievedEvent.rSVPs != null)
                {
                    toggle = retrievedEvent.rSVPs.Contains(nickname);
                }
                if (toggle)
                {
                    retrievedEvent.removeRSVP(nickname, id);

                    IEnumerable<XElement> eventRetrieve =
                 from el in globals.commandStorage.Elements("events")
                 select el;

                    eventRetrieve = from el in eventRetrieve.Elements("event")
                                    where (string)el.Element("name") == retrievedEvent.name
                                    select el;

                    XElement rsvp = (from el in eventRetrieve.Descendants("rsvps")
                                     select el).Last();

                    rsvp.Value = retrievedEvent.allRSVPs();

                    rsvp = (from el in eventRetrieve.Descendants("rsvpids")
                            select el).Last();

                    rsvp.Value = retrievedEvent.allRSVPIDs();

                    globals.commandStorage.Save(globals.storageFilePath);
                    await Context.Channel.SendMessageAsync($"You have sucessfully removed your RSVP for the event {name}").ConfigureAwait(false);
                }
                else
                {
                    retrievedEvent.addRSVP(nickname, id);

                    IEnumerable<XElement> eventRetrieve =
                        from el in globals.commandStorage.Elements("events")
                        select el;

                    eventRetrieve = from el in eventRetrieve.Elements("event")
                                    where (string)el.Element("name") == retrievedEvent.name
                                    select el;

                    XElement rsvp = (from el in eventRetrieve.Descendants("rsvps")
                                     select el).Last();

                    rsvp.Value = retrievedEvent.allRSVPs();

                    rsvp = (from el in eventRetrieve.Descendants("rsvpids")
                            select el).Last();

                    rsvp.Value = retrievedEvent.allRSVPIDs();


                    globals.commandStorage.Save(globals.storageFilePath);
                    await Context.Channel.SendMessageAsync($"You have successfully RSVP'd for the event {name}").ConfigureAwait(false);
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync(Properties.strings.msgEventNotFound).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// shows all events in the dictionary
        /// </summary>
        /// <returns></returns>
        public async Task showAllEvents()
        {
            EmbedBuilder listEmbed = new EmbedBuilder()
            {
                Title = "Active Events"
            };

            foreach (KeyValuePair<string, DateTime> item in globals.eventCalendar)
            {
                listEmbed.AddField(item.Key, $"Starts at: {item.Value}");
            }
            await Context.Channel.SendMessageAsync(Properties.strings.msgDisplayAllEvents, false, listEmbed.Build()).ConfigureAwait(false);
        }
    }

}
