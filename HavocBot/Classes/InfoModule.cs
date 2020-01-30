/* Title: InfoModule.cs
 * Author: Neal Jamieson
 * Version: 0.0.0.0
 * 
 * Description:
 *     This module holds all of the bots commands as well as certain related methods
 * 
 * Dependencies:
 *     botEvents.cs
 *     Form1.cs
 *     globals.cs
 *     HavocBot.cs
 *     CommandHandler.cs
 *     Program.cs
 *     
 * Dependent on:
 *     Discord.net
 *      
 * References:
 *     This code is adapted from the instructions and samples provided by the Discord.net Documentation found at:
 *     https://docs.stillu.cc/guides/introduction/intro.html
 */
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
    // Keep in mind your module **must** be public and inherit ModuleBase.
    // If it isn't, it will not be discovered by AddModulesAsync!
    /// <summary>
    /// Module for storing the commands used by the bot as well as any related methods
    /// </summary>
    public class infoModule : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        /// saves the specified FFXIV character data to the users profile
        /// </summary>
        /// <param name="server">Server character is on</param>
        /// <param name="name">Name of character to be saved</param>
        /// <returns></returns>
        [Command("iam")]
        [Summary("retrieves specified character data")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task getCharAsync([Summary("Server Character is on")] string server, [Remainder][Summary("Name of character to be save")] string name)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (Context.Channel.Id == globals.targetChannel)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                using (Context.Channel.EnterTypingState()) globals.lodestoneAPI.getCharacter(name, server, Context);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {"Command triggered: iam"} by {Context.User.Username}");
            }
        }

        /// <summary>
        /// Displays the character information stored in the lodestone the is assigned to the user
        /// </summary>
        /// <returns></returns>
        [Command("whoami")]
        [Summary("Displays saved character")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task showCharAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (Context.Channel.Id == globals.targetChannel)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                using (Context.Channel.EnterTypingState()) globals.lodestoneAPI.showCharacter(Context);
                Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {"Command triggered: whoami"} by {Context.User.Username}");

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        /// <summary>
        /// Displays the character information stored in the lodestone the is assigned to the specified user
        /// </summary>
        /// <returns></returns>
        [Command("whois")]
        [Summary("Displays specified saved character")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task showSpecificCharAsync(string userid)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            System.Diagnostics.Contracts.Contract.Requires(userid != null);
            if (Context.Channel.Id == globals.targetChannel)
            {
                string trimmedId = userid.Replace("<", "");
                trimmedId = trimmedId.Replace(">", "");
                trimmedId = trimmedId.Replace("@", "");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                using (Context.Channel.EnterTypingState()) globals.lodestoneAPI.showCharacter(Context, trimmedId);
                Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {"Command triggered: whois"} by {Context.User.Username}");

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        /// <summary>
        /// displays time until maintenance starts or ends
        /// </summary>
        /// <returns></returns>
        [Command("maint")]
        [Summary("displays time until maintenance")]
        public async Task showMaintCoundown()
        {
            if (Context.Channel.Id == globals.targetChannel)
            {
                await showMaint().ConfigureAwait(false);
                Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {"Command triggered: maint"} by {Context.User.Username}");
            }
        }

        /// <summary>
        /// displays time until maintenance starts or ends
        /// </summary>
        /// <returns></returns>
        public async Task showMaint()
        {
            DateTime start;
            DateTime end;
            string patch;
            IEnumerable<XElement> maintRetrieve =
             from el in globals.commandStorage.Elements("maintenance")
             select el;

            maintRetrieve = from el in maintRetrieve.Elements("maint")
                            select el;

            XElement changeTarget = (from el in maintRetrieve.Descendants("start")
                                     select el).First();
            start = DateTime.Parse(changeTarget.Value);

            changeTarget = (from el in maintRetrieve.Descendants("end")
                            select el).First();

            end = DateTime.Parse(changeTarget.Value);

            XAttribute patchChange = (from el in maintRetrieve.Attributes("patch")
                                      select el).First();

            patch = patchChange.Value;

            string dateDiff;
            EmbedBuilder eventEmbed;

            if (DateTime.Now < start)
            {
                dateDiff = ((start - DateTime.Now).ToString(@"dd' Days, 'hh' Hours, 'mm' Minutes, 'ss' Seconds'"));

                eventEmbed = new EmbedBuilder()
                {
                    Title = "Maintenance Hype!"
                };

                eventEmbed.AddField($"Time Until Patch {patch} Maintenance:", dateDiff);
                eventEmbed.WithColor(Color.Green);
                await Context.Channel.SendMessageAsync(null, false, eventEmbed.Build()).ConfigureAwait(false);
            }
            else if (DateTime.Now < end)
            {
                dateDiff = ((DateTime.Now - end).ToString(@"dd' Days, 'hh' Hours, 'mm' Minutes, 'ss' Seconds'"));

                eventEmbed = new EmbedBuilder()
                {
                    Title = "Maintenance Hype!"
                };

                eventEmbed.AddField($"Time Until Patch {patch} Maintenance Ends:", dateDiff);
                eventEmbed.WithColor(Color.Green);
                await Context.Channel.SendMessageAsync(null, false, eventEmbed.Build()).ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync("There is no upcoming maintenence currently").ConfigureAwait(false);
            }


        }

        /// <summary>
        /// saves a specific friend code
        /// </summary>
        /// <param name="code">Platform the code belongs to</param>
        /// <param name="platform">"Code for the specified platform"</param>
        /// <returns></returns>
        [Command("myCode")]
        [Summary("saves a specific friend code")]
        public async Task saveCodeAsync([Summary("Platform the code belongs to")] string platform, [Remainder][Summary("Code for the specified platform")] string code)
        {
            if (Context.Channel.Id == globals.targetChannel)
            {
                Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {"Command triggered: myCode"} by {Context.User.Username}");
                await saveCode(platform, code).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// saves a specific friend code
        /// </summary>
        /// <param name="platform">Platform the code belongs to</param>
        /// <param name="code">"Code for the specified platform"</param>
        /// <returns></returns>
        public async Task saveCode(string platform, string code)
        {
            System.Diagnostics.Contracts.Contract.Requires(platform != null);
            if (globals.commandStorage.Element("codes").Elements(Context.User.Username).Any())
            {
                switch (platform.ToLower())
                {

                    case "psn":
                    case "playstation":
                        globals.commandStorage.Element("codes").Element(Context.User.Username).Element("psn").Value = code;
                        await Context.Channel.SendMessageAsync("Code successfully added").ConfigureAwait(false);
                        break;
                    case "xbox":
                    case "xbl":
                        globals.commandStorage.Element("codes").Element(Context.User.Username).Element("xbox").Value = code;
                        await Context.Channel.SendMessageAsync("Code successfully added").ConfigureAwait(false);
                        break;
                    case "switch":
                    case "sw":
                        globals.commandStorage.Element("codes").Element(Context.User.Username).Element("switch").Value = code;
                        await Context.Channel.SendMessageAsync("Code successfully added").ConfigureAwait(false);
                        break;
                    case "nin3ds":
                        globals.commandStorage.Element("codes").Element(Context.User.Username).Element("nin3ds").Value = code;
                        await Context.Channel.SendMessageAsync("Code successfully added").ConfigureAwait(false);
                        break;
                    case "steam":
                        globals.commandStorage.Element("codes").Element(Context.User.Username).Element("steam").Value = code;
                        await Context.Channel.SendMessageAsync("Code successfully added").ConfigureAwait(false);
                        break;
                    default:
                        await Context.Channel.SendMessageAsync("Error: no platform found with that name. It is either misspelt or unsupported").ConfigureAwait(false);
                        break;
                }
            }
            else
            {
                switch (platform.ToLower())
                {

                    case "psn":
                    case "playstation":
                        try
                        {
                            Console.WriteLine(Context.User.Username);
                            globals.commandStorage.Element("codes").Add(new XElement(Context.User.Username,
                            new XElement("psn", code),
                            new XElement("xbox", "N/A"),
                            new XElement("switch", "N/A"),
                            new XElement("nin3ds", "N/A"),
                            new XElement("steam", "N/A")
                            ));
                            await Context.Channel.SendMessageAsync("Code successfully added").ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            throw;
                        }
                        
                        break;
                    case "xbox":
                    case "xbl":
                        globals.commandStorage.Element("codes").Add(new XElement(Context.User.Username,
                            new XElement("psn", "N/A"),
                            new XElement("xbox", code),
                            new XElement("switch", "N/A"),
                            new XElement("nin3ds", "N/A"),
                            new XElement("steam", "N/A")
                            ));
                        await Context.Channel.SendMessageAsync("Code successfully added").ConfigureAwait(false);
                        break;
                    case "switch":
                    case "sw":
                        globals.commandStorage.Element("codes").Add(new XElement(Context.User.Username,
                            new XElement("psn", "N/A"),
                            new XElement("xbox", "N/A"),
                            new XElement("switch", code),
                            new XElement("nin3ds", "N/A"),
                            new XElement("steam", "N/A")
                            ));
                        await Context.Channel.SendMessageAsync("Code successfully added").ConfigureAwait(false);
                        break;
                    case "nin3ds":
                        globals.commandStorage.Element("codes").Add(new XElement(Context.User.Username,
                            new XElement("psn", "N/A"),
                            new XElement("xbox", "N/A"),
                            new XElement("switch", "N/A"),
                            new XElement("nin3ds", code),
                            new XElement("steam", "N/A")
                            ));
                        await Context.Channel.SendMessageAsync("Code successfully added").ConfigureAwait(false);
                        break;
                    case "steam":
                        globals.commandStorage.Element("codes").Add(new XElement(Context.User.Username,
                            new XElement("psn", "N/A"),
                            new XElement("xbox", "N/A"),
                            new XElement("switch", "N/A"),
                            new XElement("nin3ds", "N/A"),
                            new XElement("steam", code)
                            ));
                        await Context.Channel.SendMessageAsync("Code successfully added").ConfigureAwait(false);
                        break;
                    default:
                        await Context.Channel.SendMessageAsync("Error: no platform found with that name. It is either misspelt or unsupported").ConfigureAwait(false);
                        break;
                }
            }
            globals.commandStorage.Save(globals.storageFilePath);

        }

        /// <summary>
        /// gets all the codes in the server
        /// </summary>
        /// <returns></returns>
        [Command("codes")]
        [Summary("gets all the codes in the server")]
        public async Task getCodesAsync()
        {
            if (Context.Channel.Id == globals.targetChannel)
            {
                Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {"Command triggered: codes()"} by {Context.User.Username}");
                await getCodes().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// gets all codes for a specific user
        /// </summary>
        /// <param name="username">user to retrieve codes for</param>
        /// <returns></returns>
        [Command("codes")]
        [Summary("gets all codes for a specific user")]
        public async Task getCodesAsync([Remainder][Summary("user to retrive codes for")] string username)
        {
            if (Context.Channel.Id == globals.targetChannel)
            {
                Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {"Command triggered: codes(Username)"} by {Context.User.Username}");
                await getCodes(username).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// gets all codes for the current user
        /// </summary>
        /// <returns></returns>
        [Command("mycodes")]
        [Summary("gets all codes for the current user")]
        public async Task getMyCodesAsync()
        {
            if (Context.Channel.Id == globals.targetChannel)
            {
                Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {"Command triggered: mycodes"} by {Context.User.Username}");
                await getCodes(Context.User.Username).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// gets all the codes in the server
        /// </summary>
        /// <returns></returns>
        public async Task getCodes()
        {
            IEnumerable<XElement> codeDatabase =
                 from el in globals.commandStorage.Elements("codes")
                 select el;

            codeDatabase = codeDatabase.Elements();

            IEnumerable<XElement> userCodes;

            EmbedBuilder codelist = new EmbedBuilder();
            string userN = "";
            string platform;
            string codes;
            string output = "";

                foreach (XElement user in codeDatabase)
                {
                    userCodes = user.Elements();
                    foreach (XElement code in userCodes)
                    {
                        userN = code.Parent.Name.ToString();
                        platform = code.Name.ToString();
                        codes = code.Value;
                        output += platform + ": " + codes + " | ";
                    }
                    codelist.AddField(userN, output);
                    userN = "";
                    platform = "";
                    codes = "";
                    output = "";
                }
            

            codelist.WithTitle("FC Code Dictionary");
            codelist.WithColor(Color.Green);
            codelist.WithTimestamp(DateTime.Now);
            await Context.Channel.SendMessageAsync(null, false, codelist.Build()).ConfigureAwait(false);
        }

        /// <summary>
        /// gets all codes for a specific user
        /// </summary>
        /// <param name="username">user to retrieve codes for</param>
        /// <returns></returns>
        public async Task getCodes(string username)
        {
            IEnumerable<XElement> codeDatabase =
                 from el in globals.commandStorage.Elements("codes")
                 select el;

            IEnumerable<XElement> userCodes = from el in codeDatabase.Elements(Context.User.Username)
                            select el;

            userCodes = userCodes.Elements();

            EmbedBuilder codelist = new EmbedBuilder();

            foreach (XElement code in userCodes)
            {
                codelist.AddField(code.Name.ToString(), code.Value);
            }
            
            codelist.WithTitle($"{username} Code Dictionary");
            codelist.WithColor(Color.Green);
            codelist.WithTimestamp(DateTime.Now);
            await Context.Channel.SendMessageAsync(null , false, codelist.Build()).ConfigureAwait(false);
        }

        /// <summary>
        /// Shows a list of commands available in the current context with brief descriptions of each
        /// </summary>
        /// <returns></returns>
        [Command("help")]
        [Summary("Displays a list of available commands")]
        public async Task showHelpAsync()
        {
            Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {"Command triggered: help"} by {Context.User.Username}");
            var helpEmbed = new EmbedBuilder()
            {
                Title = "Available Commands"
            };

            if (Context.Channel.Id == globals.targetEventChannel)
            {
                helpEmbed.AddField("!help <command>", "Displays a detailed explanation of the specified command");
                helpEmbed.AddField("!newEvent", "Creates a new event based on the specified details. See detailed description for more info");
                helpEmbed.AddField("!showEvent <event name>", "Displays the specified event. Warning: event names are case sensitive");
                helpEmbed.AddField("!editEvent <event name> <field to change> <changes>", "Modifies a specified field of the specified event. Warning: event names are case sensitive");
                helpEmbed.AddField("!rsvp <event name>", "Adds or removes your RSVP from the specified event. Warning: event names are case sensitive");
            }
            else
            {
                helpEmbed.AddField("help <command>", "Displays a detailed explanation of the specified command");
                helpEmbed.AddField("!setTarget", "Enables the use of normal commands in the current channel");
                helpEmbed.AddField("!setEventTarget", "Enables the use of event commands in the current channel");
                helpEmbed.AddField("!addMaint <start> <end> <patch>", "Adds maitenence to the maint command. Admin use only");
                if (Context.Channel.Id == globals.targetChannel)
                {
                    helpEmbed.AddField("!iam <server> <name>", "Saves your FFXIV character data");
                    helpEmbed.AddField("!whoami", "Displays your FFXIV Character Data, shows an error if you have never used !iam");
                    helpEmbed.AddField("!whois <user name>", "Displays the specified person's FFXIV Character Data, shows an error if they have never used !iam");
                    helpEmbed.AddField("!myCode <platform> <code>", "Adds your \"Friend Code\" for the specified platform to the code database. Throws an error if the platform is unsupported");
                    helpEmbed.AddField("!codes [user name]", "Displays all friend codes for the FC or the specified user");
                    helpEmbed.AddField("!myCodes", "Displays all your saved friend codes");
                    helpEmbed.AddField("!maint", "Displays the remaining time until maintenance starts or ends if maintenance has been scheduled");
                }
            }

            helpEmbed.WithFooter("Legend: <required> [optional]");
            helpEmbed.WithCurrentTimestamp();
            helpEmbed.WithColor(Color.Blue);

            //display the help embed to the user
            await Context.Channel.SendMessageAsync("Displaying Help", false, helpEmbed.Build()).ConfigureAwait(false);
        }

        /// <summary>
        /// Displays changes since last patch
        /// </summary>
        /// <returns></returns>
        [Command("patchNotes")]
        [Summary("Displays changes since last patch")]
        public async Task showPatchNotes()
        {
            Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {"Command triggered: patchNotes"} by {Context.User.Username}");
            var patchEmbed = new EmbedBuilder();
            patchEmbed.WithTitle("Version: 0.3.2.0");

            patchEmbed.AddField("**Changes**",
                "--Added FFXIV Lodestone News Functionality (Beta)" +
                "--!maint is now automated\n" +
                "--Performance improvements\n" +
                "--Addressed an issue where events were not properly mentioning roles\n" +
                "--Added bot maintenance handling and messaging");

            patchEmbed.WithFooter("Patch Date: 1/22/2020");
            patchEmbed.WithCurrentTimestamp();
            patchEmbed.WithColor(Color.Gold);

            await Context.Channel.SendMessageAsync("Displaying Help", false, patchEmbed.Build()).ConfigureAwait(false);
        }


        /// <summary>
        /// shows detailed information on a specific command
        /// </summary>
        /// <param name="cmdName">the command information is requested for</param>
        /// <returns></returns>
        [Command("help")]
        [Summary("displays help for a specific command")]
        public async Task showHelpAsync(string cmdName)
        {
            System.Diagnostics.Contracts.Contract.Requires(cmdName != null);
            Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {"Command triggered: help"} ({cmdName}) by {Context.User.Username}");
            var helpEmbed = new EmbedBuilder()
            {
                Title = $"Help: {cmdName}"
            };

            switch (cmdName.ToLower())
            {
                case "help":
                    helpEmbed.WithDescription("This command displays either a list of available commands or the details of a specific command.");
                    helpEmbed.AddField("__Usage:__", "**!help**\n see a list of available commands\n\n **!help <command>**\n see a detailed description of a specific command");
                    helpEmbed.AddField("__Parameters:__", "**[command]**\n the command you would like help with");
                    helpEmbed.AddField("__Examples__", "!help\n!help help\n!help iam");
                    break;
                case "iam":
                    helpEmbed.WithDescription("this command stores the Final Fantasy XIV character data for the specified character.\nThis command must be used before !whoami can be used.\n\n This command is only available in the channel where !setTarget was used.");
                    helpEmbed.AddField("__Usage:__", "**!iam <server> <name>**\nthis command has only one set of options");
                    helpEmbed.AddField("__Parameters:__", "**<server>**\n the name of the server your character is on\n\n **<name>**\nthe name of your character");
                    helpEmbed.AddField("__Examples:__", "!iam exodus John doe\n!iam gilgamesh jane doe");
                    break;
                case "whoami":
                    helpEmbed.WithDescription("This command displays your Final Fantasy XIV character data.\n!iam must be used at least once before you use this command or you will get an error.\n\n This command is only available in the channel where !setTarget was used.");
                    helpEmbed.AddField("__Usage:__", "**!whoami**\nthis command has only one set of options");
                    helpEmbed.AddField("__Parameters:__", "This command has no parameters");
                    helpEmbed.AddField("__Examples:__", "!whoami");
                    break;
                case "newevent":
                    helpEmbed.WithDescription("creates a new event with the specified details.\nthe bot will automatically announce this event to whichever channel specified with !setEventTarget.\n\n This command is only available in the channel where !setEventTarget was used.");
                    helpEmbed.AddField("__Usage:__", "**!newEvent<name> <start> <end>**\nthis command only has one set of options. More options are planned in the future");
                    helpEmbed.AddField("__Parameters:__", "**<name>**\nthe name of the event. this is case sensitive and cannot be the same as any other active event. Names with more than one word must be wrapped in quotes\n\n**<start>**\nthe start date and time for the event. Reccomended format is mm/dd/yy hh:mm. If using a time, you must wrap the whole thing in quotes.\n\n**<end>**\nthe end date and time for the event. Reccomended format is mm/dd/yy hh:mm. If using a time, you must wrap the whole thing in quotes.");
                    helpEmbed.AddField("__Examples:__", "!newEvent 1111 \"Test Event\" \"12/30/19 00:00\" \"12/30/19 01:00\"\n!newEvent 2222 \"Test Event 2\" 12/30/19 1/1/20\n!newEvent 3333 Test 12/21/19 12/22/19");
                    break;
                case "showevent":
                    helpEmbed.WithDescription("Displays the event with the specified name.\n\n This command is only available in the channel where !setEventTarget was used.");
                    helpEmbed.AddField("__Usage:__", "**!showEvent <name>**\nthis command has only one set of options");
                    helpEmbed.AddField("__Parameters:__", "**<name>**\nthe case sensitive name of the event you would like to display");
                    helpEmbed.AddField("__Examples:__", "!showEvent Test Event\n!showEvent Test Event 2\n!showEvent Test Event 3");
                    break;
                case "settarget":
                    helpEmbed.WithDescription("Sets the channel this command is used in as being the channel normal commands are authorized in. When used, deauthorizes the channel this command was used in previously");
                    helpEmbed.AddField("__Usage:__", "**!setTarget**\nthis command only has one set of options");
                    helpEmbed.AddField("__Parameters:__", "this command has no parameters");
                    helpEmbed.AddField("__Examples:__", "!setTarget");
                    break;
                case "seteventtarget":
                    helpEmbed.WithDescription("Sets the channel this command is used in as being the channel event commands and event announcments are authorized in. When used, deauthorizes the channel this command was used in previously");
                    helpEmbed.AddField("__Usage:__", "**!setEventTarget**\nthis command has only one set of options");
                    helpEmbed.AddField("__Parameters:__", "this command has no parameters");
                    helpEmbed.AddField("__Examples:__", "!setEventTarget");
                    break;
                case "editevent":
                    helpEmbed.WithDescription("Modifies a specified field of the specified event. Warning: event names are case sensitive");
                    helpEmbed.AddField("__Usage:__", "**!editEvent <event name> <field to change> <changes>**this command has only one set of options\n");
                    helpEmbed.AddField("__Parameters:__", "**<event name>**\nthe case senstive name of the event you wish to edit. Must be wrapped in quotation marks if more than one word\n\n**<field to change>**\nthe name of the field you wish to change\n\n**<changes>**\nthe new value you want to assign to the field");
                    helpEmbed.AddField("__Examples:__", "!editEvent \"Test Event\" description this is the new description\n!editEvent \"Test Event\" start 12/12/2019 12:30");
                    break;
                case "rsvp":
                    helpEmbed.WithDescription("Adds or removes your RSVP from the specified event. Warning: event names are case sensitive");
                    helpEmbed.AddField("__Usage:__", "**!rsvp <event name>**\nthis command has no other options");
                    helpEmbed.AddField("__Parameters:__", "**<event name>**\nThe case sensitive name of the event you would like to add or remove your rsvp for");
                    helpEmbed.AddField("__Examples:__", "!rsvp Test Event");
                    break;
                case "whois":
                    helpEmbed.WithDescription("Displays the specified person's FFXIV Character Data, shows an error if they have never used !iam");
                    helpEmbed.AddField("__Usage:__", "**!whoIs <user name>**\nthis command has no other options");
                    helpEmbed.AddField("__Parameters:__", "**<user name>**\nthe name of the user you would like to view character data for. It is reccomended that you mention them to ensure the name is properly passed");
                    helpEmbed.AddField("__Examples:__", "!whoIs @person");
                    break;
                case "maint":
                    helpEmbed.WithDescription("Displays the remaining time until maintenance starts or ends if maintenance has been scheduled");
                    helpEmbed.AddField("__Usage:__", "**!maint**\nthis command has no other options");
                    helpEmbed.AddField("__Parameters:__", "This command has no parameters");
                    helpEmbed.AddField("__Examples:__", "!maint");
                    break;
                case "mycode":
                    helpEmbed.WithDescription("Adds your \"Friend Code\" for the specified platform to the code database. Throws an error if the platform is unsupported");
                    helpEmbed.AddField("__Usage:__", "**!myCode <platform> <code>**\nthis command has no other options");
                    helpEmbed.AddField("__Parameters:__", "**<platform>**\nthe platform you would like to record your code for. Available platforms: PSN, XBOX, Switch, nin3ds, steam\n\n**<code>**\nthe code you would like to record");
                    helpEmbed.AddField("__Examples:__", "!myCOde psn myPSNid");
                    break;
                case "codes":
                    helpEmbed.WithDescription("Displays all friend codes for the FC or the specified user");
                    helpEmbed.AddField("__Usage:__", "**!codes**\n!codes <user>");
                    helpEmbed.AddField("__Parameters:__", "**[user]**\nif provided, returns all codes for the user named. If absent, returns every code for every saved user");
                    helpEmbed.AddField("__Examples:__", "!codes\n!codes @person");
                    break;
                case "mycodes":
                    helpEmbed.WithDescription("Displays all your saved friend codes");
                    helpEmbed.AddField("__Usage:__", "**!myCodes**\nthis command has no other options");
                    helpEmbed.AddField("__Parameters:__", "this command has no parameters");
                    helpEmbed.AddField("__Examples:__", "!myCodes");
                    break;
                case "addmaint":
                    helpEmbed.WithDescription("Adds maitenence to the maint command. Admin use only");
                    helpEmbed.AddField("__Usage:__", "**!addMaint <start> <end> <patch>**\nthis command has no other options");
                    helpEmbed.AddField("__Parameters:__", "**<start>**\nthe start date and time of maintenance. must be wrapped in quotations\n\n**<end>**\nthe end date and time of maintenance. must be wrapped in quotations\n\n**<patch>**\nthe number of the patch being applied");
                    helpEmbed.AddField("__Examples:__", "!addMaint \"12/29/2019 00:00\" \"12/29/2019 06:00\" 5.1");
                    break;
                default:
                    helpEmbed.WithDescription("Error: no command found with that name");
                    break;
            }

            helpEmbed.WithFooter("Legend: <required> [optional]");
            helpEmbed.WithCurrentTimestamp();
            helpEmbed.WithColor(Color.Blue);

            //display the help embed to the user
            await Context.Channel.SendMessageAsync("Displaying Help on a specific command", false, helpEmbed.Build()).ConfigureAwait(false);

        }
    }

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
            if (Context.Channel.Id == globals.targetEventChannel)
            {
                Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {"Command triggered: showEvent"} by {Context.User.Username}");
            await retrieveEvent(reqEvent, "Displaying Event").ConfigureAwait(false);
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
            if (Context.Channel.Id == globals.targetEventChannel)
            {
                if (globals.eventCalendar.ContainsKey(name))
                {
                    await ReplyAsync("Error: An active event with that name already exists, choose a different name, delete the exisiting event, or wait for the existing event to pass").ConfigureAwait(false);
                    Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {"Event already exists error thrown"} by {Context.User.Username}");
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {"Command triggered: newEvent"} by {Context.User.Username}");
                    await createEvent(name, start, end, type, mentions, description).ConfigureAwait(false);
                }
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
            if (Context.Channel.Id == globals.targetEventChannel)
            {
                if (globals.eventCalendar.ContainsKey(name))
                {
                    Console.WriteLine($"{System.DateTime.Now.ToLongTimeString(),-8} {"Command triggered: rsvp"}-{name} by {Context.User.Username}");
                    await toggleRSVP(name).ConfigureAwait(false);                    
                }
                else
                {
                    await ReplyAsync("Error: no event found with that name").ConfigureAwait(false);
                    Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {"No event found error"} by {Context.User.Username}");
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
                //add the event to the xml tree
                globals.storeEvent(storedEvent);
                //display the created event to the user
                await retrieveEvent(storedEvent.name, "Event Created").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                await Context.Channel.SendMessageAsync("Error: An unknown error has occured").ConfigureAwait(false);
                throw;
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
                await Context.Channel.SendMessageAsync("Error: No command found with that name").ConfigureAwait(false);
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
                switch (field.ToLower())
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
                        await Context.Channel.SendMessageAsync("Error: No field found with that name").ConfigureAwait(false);
                        break;
                }
                globals.commandStorage.Save(globals.storageFilePath);
            }
            else
            {
                await Context.Channel.SendMessageAsync("Error: No command found with that name").ConfigureAwait(false);
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

                    rsvp.Value = retrievedEvent.saveRSVPs();

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

                    rsvp.Value = retrievedEvent.saveRSVPs();

                    globals.commandStorage.Save(globals.storageFilePath);
                    await Context.Channel.SendMessageAsync($"You have successfully RSVP'd for the event {name}").ConfigureAwait(false);
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("Error: No command found with that name").ConfigureAwait(false);
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
                Title  = "Active Events"
            };
            
            foreach (KeyValuePair<string, DateTime> item in globals.eventCalendar)
            {
                listEmbed.AddField(item.Key, $"Starts at: {item.Value}");
            }
            await Context.Channel.SendMessageAsync("displaying all events", false, listEmbed.Build()).ConfigureAwait(false);
        }
    }

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
            Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {"Admin Command triggered: setTarget"} by {Context.User.Username}");
            await storeTarget((Context.Channel.Id)).ConfigureAwait(false);
        }

        /// <summary>
        /// Stores and assigns the specified channel id as the new target channel
        /// </summary>
        /// <param name="target">the channel id to be set as the new target</param>
        /// <returns>returns task complete</returns>
        public async Task storeTarget(ulong target)
        {
            await Task.Run(() => globals.changeTarget(target)).ConfigureAwait(false);
            await ReplyAsync("This channel will now recieve event announcments.").ConfigureAwait(false);
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
            Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {"Admin Command triggered: setEventTarget"} by {Context.User.Username}");
            await storeEventTarget((Context.Channel.Id)).ConfigureAwait(false);
        }

        /// <summary>
        /// Stores and assigns the specified channel id as the new target event channel
        /// </summary>
        /// <param name="target">the channel id to be set as the new target</param>
        /// <returns>returns task complete</returns>
        public async Task storeEventTarget(ulong target)
        {
            await Task.Run(() => globals.changeEventTarget(target)).ConfigureAwait(false);
            await ReplyAsync("This channel will now recieve event announcments.").ConfigureAwait(false);
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

            await ReplyAsync("Maitenance has successfully been added").ConfigureAwait(false);
            Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {"Command triggered: setmaint"} by {Context.User.Username}");
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
            Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {"Admin Command triggered: setStatus"} by {Context.User.Username}");
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
            await ReplyAsync("The bot status has been changed").ConfigureAwait(false);
        }

        /// <summary>
        /// Triggers an embed announcing bot downtime
        /// </summary>
        /// <returns></returns>
        [Command("startBotDowntime")]
        [Summary("triggers an embed announcing bot downtime")]
        public async Task showDownTime()
        {
            Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} {"Admin Command triggered: startbotdowntime"} by {Context.User.Username}");
            await Task.Run(() => havocBotClass.showDownTime()).ConfigureAwait(false);
        }

    }
}
