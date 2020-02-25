/* Title: HavocBot.cs
 * Author: Neal Jamieson
 * Version: 0.0.0.0
 * 
 * Description:
 *     This class handles the basic functions of the bot and also provides the method necessary for the bot to announce 
 *     a triggered event.
 * 
 * Dependencies:
 *     botEvents.cs
 *     CommandHandler.cs
 *     globals.cs
 *     Form1.cs
 *     InfoModule.cs
 *     Program.cs
 * 
 * Dependent on:
 *     Discord.net
 *     xml.linq
 *      
 * References:
 *     Code adapted from examples in the following. This document also provided a reference for the use of the discord.net api
 *         https://docs.stillu.cc/guides/introduction/intro.html
 *     xml.linq: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/
 *     xml: https://www.w3schools.com/xml/xml_elements.asp 
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using Discord.Commands;
using System.Xml.Linq;
using System.IO;

namespace HavocBot
{
    /// <summary>
    /// Main class for the havoc bot. initializes the command handler and service as well as the client.
    /// also contains methods for the announcing of events.
    /// </summary>
    public sealed class havocBotClass
    {
        //declare necessary variables
        private static DiscordSocketClient _client;
        private commandHandler _cHandler;
        private CommandService _cService;
        private static IMessageChannel _mainChannel;
        private static IMessageChannel _utilityChannel;

        /// <summary>
        /// The main asyncronous method. this must be run asyncronously or it will not function
        /// </summary>
        /// <returns></returns>
        public async Task mainAsync()
        {
            //initialize the client, command handler, and command service
            _client = new DiscordSocketClient();
            _cService = new CommandService();
            _cHandler = new commandHandler(_client, _cService);




            _client.Log += log;


            try
            {
                globals.token = System.IO.File.ReadAllText("token.txt");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("error no token file found");
                throw;
            }

            
            // have the client login and start
            await _client.LoginAsync(TokenType.Bot, globals.token).ConfigureAwait(false);
            await _client.StartAsync().ConfigureAwait(false);
            await _client.SetGameAsync(globals.statusMessage, null, ActivityType.Playing).ConfigureAwait(false);

            _client.JoinedGuild += joinedGuild;
            _client.LeftGuild += leftGuild;
            

            // load the commands
            await _cHandler.installCommandsAsync().ConfigureAwait(false);
            // Block this task until the program is closed.
            await Task.Delay(-1).ConfigureAwait(false);
        }

        /// <summary>
        /// When a message is recieved this sends the appropriate message to the log
        /// </summary>
        /// <param name="msg">Message to be sent to the log</param>
        /// <returns>completed task state</returns>
        private Task log(LogMessage msg)
        {
            // queue the message to be sent to the log
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        /// <summary>
        /// When an event is triggered, this loads the event from the data file and sends the announcement to the target channel.
        /// </summary>
        /// <param name="name">the name of the event to be triggered. case sensitive.</param>
        public static void eventTriggered(string name)
        {
            //identify the target channel (this is set in globals)
            _mainChannel = _client.GetChannel(globals.targetEventChannel) as IMessageChannel;

            if (globals.getEvent(name, out botEvents retrievedEvent))
            {
                //build the embed to be sent to the target channel
                EmbedBuilder eventEmbed;
                string mentions = retrievedEvent.mentions;
                string caption = $"The event \"{name}\" has begun";

                if (mentions.Contains("<@&"))
                {
                    caption = $"{mentions} {caption}";
                }
                if (mentions.Equals("rsvp"))
                {
                    mentions = retrievedEvent.allRSVPIDs();
                    caption = $"{caption} {mentions}";
                }

                eventEmbed = globals.generateEventEmbed(retrievedEvent);

                //send the announcement to the target channel
                _mainChannel.SendMessageAsync(caption, false, eventEmbed.Build());

                //log that an event has been triggered in the log
                Console.WriteLine("Event Triggered: " + retrievedEvent.name);

                retrievedEvent.repeatDate();

                IEnumerable<XElement> eventRetrieve =
                       from el in globals.commandStorage.Elements("events")
                       select el;

                eventRetrieve = from el in eventRetrieve.Elements("event")
                                where (string)el.Element("name") == retrievedEvent.name
                                select el;

                XElement xdate = (from el in eventRetrieve.Descendants("start")
                                  select el).Last();

                xdate.Value = retrievedEvent.startDate.ToString();

                xdate = (from el in eventRetrieve.Descendants("end")
                         select el).Last();

                xdate.Value = retrievedEvent.endDate.ToString();

                globals.commandStorage.Save(globals.storageFilePath);
                if (!retrievedEvent.repeat.Equals("none"))
                {
                    Console.WriteLine("Advanced date based on repeat");
                }
            }
            else
            {
                //if the event is missing send an error to the target channel and log the error
                Console.WriteLine("Exception thrown--event trigger");
                _mainChannel.SendMessageAsync("Error: event trigger failed");
            }
        }

        /// <summary>
        /// Displays message that the bot is undergoing maintenance
        /// </summary>
        public static void showDownTime()
        {
            _utilityChannel = _client.GetChannel(globals.targetChannel) as IMessageChannel;
            EmbedBuilder downEmbed = new EmbedBuilder();

            downEmbed.WithTitle("Bot Undergoing Maintenance");
            downEmbed.AddField("Details", "The bot is now undergoing maintenance, during this time the bot may go offline several times and may not respond to commands. Downtime is not expected to take longer than an hour.");

            _utilityChannel.SendMessageAsync("", false, downEmbed.Build());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="retTop"></param>
        public static void showNewsEmbed(topic retTop)
        {
            _utilityChannel = _client.GetChannel(globals.targetChannel) as IMessageChannel;
            EmbedBuilder newsEmbed;
            newsEmbed = globals.xivNews.generateEmbed(retTop);
            _utilityChannel.SendMessageAsync("", false, newsEmbed.Build());
            
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="retNews"></param>
        /// <param name="type"></param>
        public static void showNewsEmbed(news retNews, newsType type)
        {
            _utilityChannel = _client.GetChannel(globals.targetChannel) as IMessageChannel;
            EmbedBuilder newsEmbed;
            newsEmbed = globals.xivNews.generateEmbed(retNews, type);
            _utilityChannel.SendMessageAsync("", false, newsEmbed.Build());

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="retMaint"></param>
        public static void showNewsEmbed(maintNews retMaint)
        {
            _utilityChannel = _client.GetChannel(globals.targetChannel) as IMessageChannel;
            EmbedBuilder newsEmbed;
            newsEmbed = globals.xivNews.generateEmbed(retMaint);
            _utilityChannel.SendMessageAsync("", false, newsEmbed.Build());

        }


        private async Task joinedGuild(SocketGuild newGuild)
        {

        }

        private async Task leftGuild(SocketGuild leavingGuild)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static void statusChange(string message)
        {
            _client.SetGameAsync(message, null, ActivityType.Playing).ConfigureAwait(false);
        }
    }
}
