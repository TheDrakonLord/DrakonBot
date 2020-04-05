/* Title: Program.cs
 * Author: Neal Jamieson
 * Version: 0.3.1.0
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
using System.Threading;
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
using System.Net;

namespace HavocBot
{
    /// <summary>
    /// 
    /// </summary>
    public class program
    {
        private static System.Timers.Timer _tmrCalendar;
        private static System.Timers.Timer _tmrNews;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main() => new program().mainAsync().GetAwaiter().GetResult();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <returns></returns>        
        public async Task mainAsync()
        {
            globals.logMessage("Application Start");

            //Declare a timer

            _tmrCalendar = new System.Timers.Timer(1000);
            _tmrCalendar.Elapsed += tmrCalendar_Elapsed;
            _tmrCalendar.AutoReset = true;

            _tmrNews = new System.Timers.Timer(900000);
            _tmrNews.Elapsed += tmrNews_Elapsed;
            _tmrNews.AutoReset = true;

            // declare a new instance of HavocBot
            havocBotClass botThread = new havocBotClass();

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
                globals.logMessage("Notice: Command Data was not found. A new command data file will be created");
                XElement NewCommandData = new XElement("root");
                NewCommandData.Add(
                new XElement("events", null),
                new XElement("StatusMessage", "Hello There"),
                new XElement("settings",
                    new XElement("g622084719030304806",
                        new XElement("TargetChannel", "622084719030304810"),
                        new XElement("TargetEventChannel", "623528068358733824"))),
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
                            new XElement("start", "01/01/2001"),
                            new XElement("end", "01/01/2001")),
                    new XElement("lodeMaint",
                            new XElement("start", "01/01/2001"),
                            new XElement("end", "01/01/2001"))),
                new XElement("codes", null),                new XElement("guilds",
                    new XElement("guild", "622084719030304806")),
                new XElement("news",
                    new XElement("topics", "3f999b278389c9fe9dfe3362f477059577df769e"),
                    new XElement("notices", "bc41a298e01c832f9b552c35d5314392f7edd479"),
                    new XElement("maint", "f66590bde2734203fa56e32c82780681f155cd59"),
                    new XElement("updates", "58cc5a4cb0c56567da42d0fa08e696097b755cb3"),
                    new XElement("status", "8a2616e2d864f35449d97551312ca11d1d23896d")
                    ));
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

            int count = globals.eventCalendar.Count;
            for (int i = 0; i < count; i++)
            {
                if (globals.getEvent(globals.eventCalendar.ElementAt(i).Key, out botEvents retrievedEvent))
                {
                    if (retrievedEvent.reminderMinutes > 0)
                    {
                        globals.eventCalendar.Add("Reminder: " + globals.eventCalendar.ElementAt(i).Key, retrievedEvent.startDate.AddMinutes(-retrievedEvent.reminderMinutes));
                        globals.commandStorage.Element("events").Add(new XElement("event",
                            new XAttribute("sid", retrievedEvent.storageID.ToString()),
                            new XElement("name", "Reminder: " + retrievedEvent.name),
                            new XElement("type", retrievedEvent.type),
                            new XElement("description", retrievedEvent.description),
                            new XElement("start", retrievedEvent.startDate),
                            new XElement("end", retrievedEvent.endDate),
                            new XElement("reminder", retrievedEvent.reminderMinutes),
                            new XElement("repeat", 0),
                            new XElement("mentions", retrievedEvent.mentions),
                            new XElement("rsvps", retrievedEvent.allRSVPs()),
                            new XElement("rsvpids", retrievedEvent.allRSVPIDs()),
                            new XElement("author", retrievedEvent.author),
                            new XElement("authorURL", retrievedEvent.authorURL),
                            new XElement("guild", retrievedEvent.guild)
                        ));
                    }
                }
            }

            // load in all stored settings
            IEnumerable<XElement> settingRetrieve =
                 from el in globals.commandStorage.Elements("settings")
                 select el;
            settingRetrieve = settingRetrieve.Elements();

            foreach (XElement x in settingRetrieve)
            {
                    string name = x.Name.LocalName;
                    ulong[] data = { ulong.Parse(x.Element("TargetChannel").Value), ulong.Parse(x.Element("TargetEventChannel").Value) };
                    globals.guildSettings.Add(name, data);
               
            }


            globals.statusMessage = (string)globals.commandStorage.Element("StatusMessage").Value;

            settingRetrieve = from el in globals.commandStorage.Elements("maintenance")
                 select el;
            settingRetrieve = from el in settingRetrieve.Elements("lodeMaint")
                            select el;

            globals.lodeMaintStart = DateTime.Parse((string)
                 (from el in settingRetrieve.Descendants("start")
                  select el).First());

            globals.lodeMaintEnd = DateTime.Parse((string)
                 (from el in settingRetrieve.Descendants("start")
                  select el).First());

            // instantiate the news class with loaded events

            // start timer
            _tmrCalendar.Enabled = true;
            _tmrNews.Enabled = true;

            
            // start the bot
            await botThread.mainAsync().ConfigureAwait(true);

            
        }

        /// <summary>
        /// Secondary timer that checks if events have occcured
        /// checks for events once every minute
        /// </summary>
        /// <param name="e"></param>
        /// <param name="source"></param>
        private static void tmrCalendar_Elapsed(Object source, ElapsedEventArgs e)
        {
            // iterate through the dictionary of active events
            foreach (KeyValuePair<string, DateTime> entry in globals.eventCalendar)
            {
                // identify events that have occured
                if (DateTime.Now >= entry.Value)
                {
                    // have the bot announce the event
                    havocBotClass.eventTriggered(entry.Key);
                                       
                    // exit the loop, any remaining events will be announced 1 minute later
                    break;
                }
            }
        }

        /// <summary>
        /// Timer that checks if new news items have been posted
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void tmrNews_Elapsed(Object source, ElapsedEventArgs e)
        {
            if (DateTime.Now >= globals.lodeMaintStart && DateTime.Now <= globals.lodeMaintEnd)
            {
                globals.logMessage("News refresh skipped due to lodestone maintenance");
            }
            else
            {
                //globals.logMessage("Begin News Refresh");
                //globals.xivNews.refresh();
            }
        }
    }
}
