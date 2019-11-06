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
        /// lists all active events
        /// </summary>
        public static List<botEvents> EventsList = new List<botEvents>();
        
        /// <summary>
        /// holds the file path for the current directory
        /// </summary>
        public static string storageFilePath;

        /// <summary>
        /// lists all active events
        /// </summary>
        public static Dictionary<string, DateTime> eventCalendar = new Dictionary<string, DateTime>();
        
        /// <summary>
        /// Holds the ID of the channel commands are permitted in
        /// </summary>
        public static ulong targetChannel = 622084719030304810;

        /// <summary>
        /// Holds the ID of the channel events should be announced and permitted in
        /// </summary>
        public static ulong targetEventChannel = 622084719030304810;

        /// <summary>
        /// holds the api commands for accessing the lodestone
        /// </summary>
        public static lodestone LodestoneAPI = new lodestone();

        /// <summary>
        /// 
        /// </summary>
        public static string token = "";

        /// <summary>
        /// Stores an event in the command Data file and adds it to the event calendar
        /// </summary>
        /// <param name="storedEvent">The botEvents to be stored in the xml file</param>
        public static void storeEvent(botEvents storedEvent)
        {
            //store the data in the xml file
            commandStorage.Element("events").Add(new XElement("event",
                    new XAttribute("sid", storedEvent.StorageID.ToString()),
                        new XElement("name", storedEvent.Name),
                        new XElement("type", storedEvent.Type),
                        new XElement("description", storedEvent.Description),
                        new XElement("start", storedEvent.StartDate),
                        new XElement("end", storedEvent.EndDate),
                        new XElement("reminder", storedEvent.ReminderMinutes),
                        new XElement("repeat", storedEvent.Repeat),
                        new XElement("mentions", storedEvent.Mentions),
                        new XElement("rsvps", storedEvent.allRSVPs()),
                        new XElement("rsvpids", storedEvent.allRSVPIDs()),
                        new XElement("author", storedEvent.Author),
                        new XElement("authorURL", storedEvent.AuthorURL)
                        ));
            //add the event to the event calendar
            eventCalendar.Add(storedEvent.Name, storedEvent.StartDate);
            //Save the tree to the file
            commandStorage.Save(storageFilePath);
        }

        /// <summary>
        /// Changes the target channel to the requested id
        /// </summary>
        /// <param name="target">id of the new target channel</param>
        public static void changeTarget(ulong target)
        {
            targetChannel = target;

            commandStorage.Element("settings").Element("TargetChannel").SetValue(target);

            commandStorage.Save(storageFilePath);
        }

        /// <summary>
        /// Changes the target channel to the requested id
        /// </summary>
        /// <param name="target">id of the new target channel</param>
        public static void changeEventTarget(ulong target)
        {
            targetEventChannel = target;

            commandStorage.Element("settings").Element("TargetEventChannel").SetValue(target);

            commandStorage.Save(storageFilePath);
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
                retrievedEvent = new botEvents(testName, DateTime.Parse(testStart), DateTime.Parse(testEnd), globals.commandStorage);
                retrievedEvent.Type = testtype;
                retrievedEvent.Description = testDescription;
                retrievedEvent.ReminderMinutes = int.Parse(testReminder);
                retrievedEvent.Repeat = testRepeat;
                retrievedEvent.Mentions = testMentions;
                retrievedEvent.importRSVPs(testRsvps, testRsvpIDs);
                retrievedEvent.Author = testAuthor;
                retrievedEvent.AuthorURL = testAuthorURL;
                return true;
            }
            catch (InvalidOperationException)
            {
                //if no command with the requested name was found, display an error
                Console.WriteLine("Exception thrown--invalidOperationException--No command found");
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
            //create an embed based off of the loaded event
            EmbedBuilder eventEmbed = new EmbedBuilder()
            {
                Title = "Event"
            };
            eventEmbed.AddField("Event Name", retrievedEvent.Name, true);
            eventEmbed.AddField("Event Type", retrievedEvent.Type, true);
            eventEmbed.AddField("Description", retrievedEvent.Description, false);
            eventEmbed.AddField("Start Date", retrievedEvent.StartDate.ToString(), true);
            eventEmbed.AddField("End Date", retrievedEvent.EndDate.ToString(), true);
            eventEmbed.AddField("Reminder", retrievedEvent.ReminderMinutes.ToString(), true);
            eventEmbed.AddField("Repeat", retrievedEvent.Repeat, true);
            eventEmbed.AddField("Mentions", retrievedEvent.Mentions, true);
            eventEmbed.AddField("RSVPs", retrievedEvent.allRSVPs(), true);
            eventEmbed.WithAuthor(retrievedEvent.Author, retrievedEvent.AuthorURL, null);
            eventEmbed.WithCurrentTimestamp();
            eventEmbed.WithColor(Color.Red);
            eventEmbed.WithThumbnailUrl(retrievedEvent.TypeImagePath);

            return eventEmbed;
        }
    }
}
