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
    public class HavocBot
    {
        //declare necessary variables
        private static DiscordSocketClient _client;
        private CommandHandler _cHandler;
        private CommandService _cService;
        private static IMessageChannel mainChannel;

        /// <summary>
        /// The main asyncronous method. this must be run asyncronously or it will not function
        /// </summary>
        /// <returns></returns>
        public async Task MainAsync()
        {
            //initialize the client, command handler, and command service
            _client = new DiscordSocketClient();
            _cService = new CommandService();
            _cHandler = new CommandHandler(_client, _cService);




            _client.Log += Log;


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
            await _client.LoginAsync(TokenType.Bot, globals.token);
            await _client.StartAsync();
            await _client.SetGameAsync(globals.StatusMessage, null, ActivityType.Playing);


            // load the commands
            await _cHandler.InstallCommandsAsync();
            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        /// <summary>
        /// When a message is recieved this sends the appropriate message to the log
        /// </summary>
        /// <param name="msg">Message to be sent to the log</param>
        /// <returns>completed task state</returns>
        private Task Log(LogMessage msg)
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
            mainChannel = _client.GetChannel(globals.targetEventChannel) as IMessageChannel;

            botEvents retrievedEvent;
            if (globals.getEvent(name, out retrievedEvent))
            {
                //build the embed to be sent to the target channel
                EmbedBuilder eventEmbed;
                string mentions = retrievedEvent.Mentions;
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
                mainChannel.SendMessageAsync(caption, false, eventEmbed.Build());

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

                xdate.Value = retrievedEvent.StartDate.ToString();

                xdate = (from el in eventRetrieve.Descendants("end")
                                  select el).Last();

                xdate.Value = retrievedEvent.EndDate.ToString();

                globals.commandStorage.Save(globals.storageFilePath);
                if (!retrievedEvent.Repeat.Equals("none"))
                {
                    Console.WriteLine("Advanced date based on repeat");
                }
            }
            else
            {
                //if the event is missing send an error to the target channel and log the error
                Console.WriteLine("Exception thrown--event trigger");
                mainChannel.SendMessageAsync("Error: event trigger failed");
            }                
        }
    }
}
