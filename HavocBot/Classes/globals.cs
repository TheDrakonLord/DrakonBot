/* Title: globals.cs
 * Author: Neal Jamieson
 * Version: 0.0.0.0
 * 
 * Description:
 *     This class holds the variables and methods that will be used in more than one class within the namespace
 * 
 * Dependencies:
 *     botEvents.cs
 *     CommandHandler.cs
 *     Form1.cs
 *     HavocBot.cs
 *     InfoModule.cs
 *     Program.cs
 *     
 * Dependent on:
 *     xml.linq
 *      
 * References:
 *     xml.linq: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/
 *     xml: https://www.w3schools.com/xml/xml_elements.asp  
 */
using Discord;
using Discord.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace HavocBot
{
    /// <summary>
    /// Holds variables and methods that are used in multiple classes
    /// </summary>
    public static class globals
    {
        /// <summary>
        /// Holds the command data xml tree
        /// </summary>
        public static XElement commandStorage;
        
        /// <summary>
        /// holds the file path for the current directory
        /// </summary>
        public static string storageFilePath;

        /// <summary>
        /// lists all active events
        /// </summary>
        public static Dictionary<string, DateTime> eventCalendar = new Dictionary<string, DateTime>();

        /// <summary>
        /// 
        /// </summary>
        public static Dictionary<string, ulong[]> guildSettings = new Dictionary<string, ulong[]>();

        /// <summary>
        /// Status message the bot displays on the server
        /// </summary>
        public static string statusMessage = "Hello There";

        /// <summary>
        /// holds the api commands for accessing the lodestone
        /// </summary>
        public static lodestone lodestoneAPI = new lodestone();

        /// <summary>
        /// 
        /// </summary>
        public static List<ulong> allGuilds = new List<ulong>();

        /// <summary>
        /// 
        /// </summary>
        public static Dictionary<ulong, int> playbackPIDs = new Dictionary<ulong, int>();

        public static Dictionary<ulong, Queue<string>> playbackQueues = new Dictionary<ulong, Queue<string>>();

        /// <summary>
        /// 
        /// </summary>
        public static string token = "";

        /// <summary>
        /// 
        /// </summary>
        public static string patchnotes = "--Added a queue feature to the music function (!queue [video link] to add a video to the queue)\n" +
                "--Addressed an issue where the !stop command was not working \n" + 
                "--Added the ability for admins to cancel and event with !cancelEvent \n" +
                "--Addressed an issue where the music features were not working \n" +
                "--We have transferred servers! We are no longer running on linux Ubuntu. We are now on Windows Server 2016 \n" +
                "\n\n **Known Issue:**\n" +
                "--!newevent does not support its optional parameters. A fix is in the works for this issue. Please use !editevent in the meantime";

        /// <summary>
        /// 
        /// </summary>
        public static string versionID = "Version: 0.5.0.0";

        /// <summary>
        /// 
        /// </summary>
        public static string patchID = "Patch 0.5.0.0";

        /// <summary>
        /// 
        /// </summary>
        public static DateTimeOffset patchDate = new DateTimeOffset(2020, 04, 03, 17, 00, 00, new TimeSpan(-6, 0, 0));

        /// <summary>
        /// 
        /// </summary>
        public static DateTime lodeMaintStart;

        /// <summary>
        /// 
        /// </summary>
        public static DateTime lodeMaintEnd;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        /// <param name="Context"></param>
        /// <param name="message"></param>
        public static void logMessage(SocketCommandContext Context, string category, string message)
        {
            System.Diagnostics.Contracts.Contract.Requires(Context != null);
            Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {category}: {message} by {Context.User.Username} in {Context.Guild.Name} ({Context.Guild.Id})");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        /// <param name="message"></param>
        public static void logMessage(string category, string message)
        {
            Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {category}: {message}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void logMessage(string message)
        {
            Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {message}");
        }

        /// <summary>
        /// Stores an event in the command Data file and adds it to the event calendar
        /// </summary>
        /// <param name="storedEvent">The botEvents to be stored in the xml file</param>
        public static void storeEvent(botEvents storedEvent)
        {
            System.Diagnostics.Contracts.Contract.Requires(storedEvent != null);
            //store the data in the xml file
            commandStorage.Element("events").Add(new XElement("event",
                    new XAttribute("sid", storedEvent.storageID.ToString()),
                        new XElement("name", storedEvent.name),
                        new XElement("type", storedEvent.type),
                        new XElement("description", storedEvent.description),
                        new XElement("start", storedEvent.startDate),
                        new XElement("end", storedEvent.endDate),
                        new XElement("reminder", storedEvent.reminderMinutes),
                        new XElement("repeat", storedEvent.repeat),
                        new XElement("mentions", storedEvent.mentions),
                        new XElement("rsvps", storedEvent.allRSVPs()),
                        new XElement("rsvpids", storedEvent.allRSVPIDs()),
                        new XElement("author", storedEvent.author),
                        new XElement("authorURL", storedEvent.authorURL),
                        new XElement("guild", storedEvent.guild)
                        ));
            //add the event to the event calendar
            eventCalendar.Add(storedEvent.name, storedEvent.startDate);
            //Save the tree to the file
            commandStorage.Save(storageFilePath);
        }

        /// <summary>
        /// Changes the target channel to the requested id
        /// </summary>
        /// <param name="target">id of the new target channel</param>
        /// <param name="guild">id of the target guild</param>
        public static void changeTarget(ulong target, ulong guild)
        {
            guildSettings[$"g{guild.ToString()}"][0] = target;

            commandStorage.Element("settings").Element($"g{guild}").Element("TargetChannel").SetValue(target);

            commandStorage.Save(storageFilePath);
        }

        /// <summary>
        /// Changes the target channel to the requested id
        /// </summary>
        /// <param name="target">id of the new target channel</param>
        /// <param name="guild">id of the target guild</param>
        public static void changeEventTarget(ulong target, ulong guild)
        {
            guildSettings[$"g{guild.ToString()}"][0] = target;

            commandStorage.Element("settings").Element($"g{guild}").Element("TargetEventChannel").SetValue(target);

            commandStorage.Save(storageFilePath);
        }

        /// <summary>
        /// Changes the current status message of the bot
        /// </summary>
        /// <param name="status">status to be set to the bot</param>
        public static void changeStatus(string status)
        {
            statusMessage = status;

            commandStorage.Element("settings").Element("StatusMessage").SetValue(status);

            commandStorage.Save(storageFilePath);

            havocBotClass.statusChange(status);
        }

        /// <summary>
        /// retrieves and passes an event from the xml tree
        /// </summary>
        /// <param name="eventName">Name of the event to be retrieved. Case sensitive</param>
        /// <param name="retrievedEvent">data structure to store the retrieved event in</param>
        /// <returns>returns true if successful, false if unsuccessful</returns>
        public static bool getEvent(string eventName, out botEvents retrievedEvent)
        {
            try
            {
                //find and load the requested event
                IEnumerable<XElement> eventRetrieve =
                 from el in commandStorage.Elements("events")
                 select el;

                eventRetrieve = from el in eventRetrieve.Elements("event")
                                where (string)el.Element("name") == eventName
                                select el;

                string testName = (string)
                    (from el in eventRetrieve.Descendants("name")
                     select el).Last();
                string testtype = (string)
                    (from el in eventRetrieve.Descendants("type")
                     select el).Last();
                string testDescription = (string)
                    (from el in eventRetrieve.Descendants("description")
                     select el).Last();
                string testStart = (string)
                    (from el in eventRetrieve.Descendants("start")
                     select el).Last();
                string testEnd = (string)
                    (from el in eventRetrieve.Descendants("end")
                     select el).Last();
                string testReminder = (string)
                    (from el in eventRetrieve.Descendants("reminder")
                     select el).Last();
                string testRepeat = (string)
                    (from el in eventRetrieve.Descendants("repeat")
                     select el).Last();
                string testMentions = (string)
                    (from el in eventRetrieve.Descendants("mentions")
                     select el).Last();
                string testRsvps = (string)
                    (from el in eventRetrieve.Descendants("rsvps")
                     select el).Last();
                string testRsvpIDs = (string)
                    (from el in eventRetrieve.Descendants("rsvpids")
                     select el).Last();
                string testAuthor = (string)
                    (from el in eventRetrieve.Descendants("author")
                     select el).Last();
                string testAuthorURL = (string)
                    (from el in eventRetrieve.Descendants("authorURL")
                     select el).Last();
                string testGuild = (string)
                    (from el in eventRetrieve.Descendants("guild")
                     select el).Last();
                retrievedEvent = new botEvents(testName, DateTime.Parse(testStart), DateTime.Parse(testEnd), globals.commandStorage)
                {
                    type = testtype,
                    description = testDescription,
                    reminderMinutes = int.Parse(testReminder),
                    repeat = testRepeat,
                    mentions = testMentions
                };
                retrievedEvent.importRSVPs(testRsvps, testRsvpIDs);
                retrievedEvent.author = testAuthor;
                Uri.TryCreate(testAuthorURL, UriKind.RelativeOrAbsolute, out Uri uriResult);
                retrievedEvent.authorURL = uriResult;
                retrievedEvent.guild = testGuild;
                return true;
            }
            catch (InvalidOperationException)
            {
                //if no command with the requested name was found, display an error
                logMessage("Exception thrown--invalidOperationException--No command found");
                retrievedEvent = null;
                return false;
                throw;
            }
        }
        
        /// <summary>
        /// generates a discord embed based on a event
        /// </summary>
        /// <param name="retrievedEvent">event to build the embed from</param>
        /// <returns>Completed embed</returns>
        public static EmbedBuilder generateEventEmbed(botEvents retrievedEvent)
        {
            System.Diagnostics.Contracts.Contract.Requires(retrievedEvent != null);
            //create an embed based off of the loaded event
            EmbedBuilder eventEmbed = new EmbedBuilder()
            {
                Title = "Event"
            };
            eventEmbed.AddField("Event Name", retrievedEvent.name, true);
            eventEmbed.AddField("Event Type", retrievedEvent.type, true);
            eventEmbed.AddField("Description", retrievedEvent.description, false);
            eventEmbed.AddField("Start Date", retrievedEvent.startDate.ToString(), true);
            eventEmbed.AddField("End Date", retrievedEvent.endDate.ToString(), true);
            eventEmbed.AddField("Reminder", retrievedEvent.reminderMinutes.ToString(), true);
            eventEmbed.AddField("Repeat", retrievedEvent.repeat, true);
            eventEmbed.AddField("Mentions", retrievedEvent.mentions, true);
            eventEmbed.AddField("RSVPs", retrievedEvent.allRSVPs(), true);
            eventEmbed.WithAuthor(retrievedEvent.author, retrievedEvent.authorURL.ToString(), null);
            eventEmbed.WithCurrentTimestamp();
            eventEmbed.WithColor(Color.Red);
            eventEmbed.WithThumbnailUrl(retrievedEvent.typeImagePath);

            return eventEmbed;
        }
    }
}
