/* Title: Program.cs
 * Author: Neal Jamieson
 * Version: 0.1.0.0
 * 
 * Description:
 *     This class acts as the main entry point for the application
 * 
 * Dependencies:
 *     botEvents.cs
 *     Form1.cs
 *     globals.cs
 *     HavocBot.cs
 *     InfoModule.cs
 *     CommandHandler.cs
 *     
 * Dependent on:
 *      
 * References:
 */
using System;
using System.Timers;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HavocBot
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        static System.Timers.Timer tmrCalendar = new System.Timers.Timer(1000);
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main() => new Program().MainAsync().GetAwaiter().GetResult();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <returns></returns>        
        public async Task MainAsync()
        {
            // declare a new instance of HavocBot
            HavocBot botThread = new HavocBot();

            // Load the XML file from our project directory containing the events
            string filename = "commandData.xml";

            globals.storageFilePath = filename;
            // Load the xml file containing command data
            // if file is not found create a new one
            try
            {
                globals.commandStorage = XElement.Load(globals.storageFilePath);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Notice: Command Data was not found. A new command data file will be created");
                XElement NewCommandData = new XElement("root");
                NewCommandData.Add(
                new XElement("events", null),
                new XElement("settings",
                    new XElement("TargetChannel", "622084719030304810"),
                    new XElement("TargetEventChannel", "623528068358733824"),
                    new XElement("StatusMessage", "Hello There")),
                new XElement("images",
                        new XElement("diadem", "https://i.imgur.com/n08fbk9.png"),
                        new XElement("dungeon", "https://i.imgur.com/yrwlXyE.png"),
                        new XElement("eureka", "https://i.imgur.com/5zO3f3w.png"),
                        new XElement("fates", "https://i.imgur.com/aAonLHG.png"),
                        new XElement("gates", "https://i.imgur.com/SXao8Ir.png"),
                        new XElement("HighEnd", "https://i.imgur.com/TGMTLLd.png"),
                        new XElement("hunts", "https://i.imgur.com/9ZN2arg.png"),
                        new XElement("maps", "https://i.imgur.com/Cc5va3T.png"),
                        new XElement("other", "https://i.imgur.com/TG0PHQL.png"),
                        new XElement("potd", "https://i.imgur.com/ut7zxvM.png"),
                        new XElement("pvp", "https://i.imgur.com/75fTXKw.png"),
                        new XElement("raid", "https://i.imgur.com/hzCYA1S.png"),
                        new XElement("roulette", "https://i.imgur.com/7lVkWfk.png"),
                        new XElement("trial", "https://i.imgur.com/FormlqY.png"),
                        new XElement("movie", "https://i.imgur.com/s0nkwot.png"),
                        new XElement("jackbox", "https://i.imgur.com/kVJfuMV.png")),
                    new XElement("characters", null),
                    new XElement("maintenance",
                        new XElement("maint",
                            new XAttribute("patch", "0.0"),
                            new XElement("start", null),
                            new XElement("end", null))),
                    new XElement("codes", null)
                    );
                NewCommandData.Save(globals.storageFilePath);
                globals.commandStorage = XElement.Load(globals.storageFilePath);
            }


            // load in all stored events
            IEnumerable<XElement> eventRetrieve =
                 from el in globals.commandStorage.Elements("events")
                 select el;
            eventRetrieve = from el in eventRetrieve.Elements("event")
                            select el;

            // store events in the eventCalendar Dictionary, filtering out old events
            foreach (XElement el in eventRetrieve)
            {
                if (DateTime.Now <= DateTime.Parse((string)el.Element("start")) && !el.Element("name").Value.Contains("Reminder:"))
                {
                    try
                    {
                        globals.eventCalendar.Add((string)el.Element("name"), DateTime.Parse((string)el.Element("start")));
                    }
                    catch (ArgumentException)
                    {
                        throw;
                    }

                }
            }

            botEvents retrievedEvent;
            int count = globals.eventCalendar.Count;
            for (int i = 0; i < count; i++)
            {
                if (globals.getEvent(globals.eventCalendar.ElementAt(i).Key, out retrievedEvent))
                {
                    if (retrievedEvent.ReminderMinutes > 0)
                    {
                        globals.eventCalendar.Add("Reminder: " + globals.eventCalendar.ElementAt(i).Key, retrievedEvent.StartDate.AddMinutes(-retrievedEvent.ReminderMinutes));
                        globals.commandStorage.Element("events").Add(new XElement("event",
                            new XAttribute("sid", retrievedEvent.storageID.ToString()),
                            new XElement("name", "Reminder: " + retrievedEvent.name),
                            new XElement("type", retrievedEvent.type),
                            new XElement("description", retrievedEvent.Description),
                            new XElement("start", retrievedEvent.StartDate),
                            new XElement("end", retrievedEvent.EndDate),
                            new XElement("reminder", retrievedEvent.ReminderMinutes),
                            new XElement("repeat", 0),
                            new XElement("mentions", retrievedEvent.Mentions),
                            new XElement("rsvps", retrievedEvent.allRSVPs()),
                            new XElement("rsvpids", retrievedEvent.allRSVPIDs()),
                            new XElement("author", retrievedEvent.Author),
                            new XElement("authorURL", retrievedEvent.AuthorURL)
                        ));
                    }
                }
            }

            // load in all stored settings
            IEnumerable<XElement> settingRetrieve =
                 from el in globals.commandStorage.Elements("settings")
                 select el;

            globals.targetChannel = (ulong)
                 (from el in settingRetrieve.Descendants("TargetChannel")
                  select el).First();

            globals.targetEventChannel = (ulong)
                 (from el in settingRetrieve.Descendants("TargetEventChannel")
                  select el).First();

            globals.StatusMessage = (string)
                 (from el in settingRetrieve.Descendants("StatusMessage")
                  select el).First();

            // start the bot
            await botThread.MainAsync();
            // start timer
            tmrCalendar.Enabled = true;
            tmrCalendar.Start();
        }

        /// <summary>
        /// Secondary timer that checks if events have occcured
        /// checks for events once every minute
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrCalendar_Elapsed(object sender, EventArgs e)
        {
            // iterate through the dictionary of active events
            foreach (KeyValuePair<string, DateTime> entry in globals.eventCalendar)
            {
                // identify events that have occured
                if (DateTime.Now >= entry.Value)
                {
                    // have the bot announce the event
                    HavocBot.eventTriggered(entry.Key);
                    // remove the event from the list of active events
                    globals.eventCalendar.Remove(entry.Key);
                    // exit the loop, any remaining events will be announced 1 minute later
                    break;
                }
            }
        }
    }
}
