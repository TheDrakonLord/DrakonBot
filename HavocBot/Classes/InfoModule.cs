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
            if (Context.Channel.Id == globals.guildSettings[$"g{Context.Guild.Id}"][0])
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                using (Context.Channel.EnterTypingState()) globals.lodestoneAPI.getCharacter(name, server, Context);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                globals.logMessage(Context,Properties.strings.catCmdTrigger,Properties.strings.cmdIAm);
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
            if (Context.Channel.Id == globals.guildSettings[$"g{Context.Guild.Id}"][0])
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                using (Context.Channel.EnterTypingState()) globals.lodestoneAPI.showCharacter(Context);
                globals.logMessage(Context,Properties.strings.catCmdTrigger,Properties.strings.cmdWhoAmI);

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
            if (Context.Channel.Id == globals.guildSettings[$"g{Context.Guild.Id}"][0])
            {
#pragma warning disable CA1307 // Specify StringComparison
                string trimmedId = userid.Replace("<", "");
                trimmedId = trimmedId.Replace(">", "");
                trimmedId = trimmedId.Replace("@", "");
#pragma warning restore CA1307 // Specify StringComparison

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                using (Context.Channel.EnterTypingState()) globals.lodestoneAPI.showCharacter(Context, trimmedId);
                globals.logMessage(Context, Properties.strings.catCmdTrigger,Properties.strings.cmdWhoIs);

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
            if (Context.Channel.Id == globals.guildSettings[$"g{Context.Guild.Id}"][0])
            {
                await showMaint().ConfigureAwait(false);
                globals.logMessage(Context, Properties.strings.catCmdTrigger,Properties.strings.cmdMaint);
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
                await ReplyAsync(Properties.strings.msgNoCurrentMaint).ConfigureAwait(false);
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
            if (Context.Channel.Id == globals.guildSettings[$"g{Context.Guild.Id}"][0])
            {
                globals.logMessage(Context,Properties.strings.catCmdTrigger,Properties.strings.cmdMyCode);
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
            if (globals.commandStorage.Element("codes").Element($"g{Context.Guild.Id}").Elements(Context.User.Username).Any())
            {
                switch (platform.ToLower())
                {

                    case "psn":
                    case "playstation":
                        globals.commandStorage.Element("codes").Element($"g{Context.Guild.Id}").Element(Context.User.Id.ToString()).Element("psn").Value = code;
                        await Context.Channel.SendMessageAsync(Properties.strings.codeSuccess).ConfigureAwait(false);
                        break;
                    case "xbox":
                    case "xbl":
                        globals.commandStorage.Element("codes").Element($"g{Context.Guild.Id}").Element(Context.User.Id.ToString()).Element("xbox").Value = code;
                        await Context.Channel.SendMessageAsync(Properties.strings.codeSuccess).ConfigureAwait(false);
                        break;
                    case "switch":
                    case "sw":
                        globals.commandStorage.Element("codes").Element($"g{Context.Guild.Id}").Element(Context.User.Id.ToString()).Element("switch").Value = code;
                        await Context.Channel.SendMessageAsync(Properties.strings.codeSuccess).ConfigureAwait(false);
                        break;
                    case "nin3ds":
                        globals.commandStorage.Element("codes").Element($"g{Context.Guild.Id}").Element(Context.User.Id.ToString()).Element("nin3ds").Value = code;
                        await Context.Channel.SendMessageAsync(Properties.strings.codeSuccess).ConfigureAwait(false);
                        break;
                    case "steam":
                        globals.commandStorage.Element("codes").Element($"g{Context.Guild.Id}").Element(Context.User.Username).Element("steam").Value = code;
                        await Context.Channel.SendMessageAsync(Properties.strings.codeSuccess).ConfigureAwait(false);
                        break;
                    default:
                        await Context.Channel.SendMessageAsync(Properties.strings.codePlatformError).ConfigureAwait(false);
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
                            globals.commandStorage.Element("codes").Element($"g{Context.Guild.Id}").Add(new XElement(Context.User.Username,
                            new XElement("psn", code),
                            new XElement("xbox", "N/A"),
                            new XElement("switch", "N/A"),
                            new XElement("nin3ds", "N/A"),
                            new XElement("steam", "N/A")
                            ));
                            await Context.Channel.SendMessageAsync(Properties.strings.codeSuccess).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            globals.logMessage(ex.ToString());
                            throw;
                        }

                        break;
                    case "xbox":
                    case "xbl":
                        globals.commandStorage.Element("codes").Element($"g{Context.Guild.Id}").Add(new XElement(Context.User.Username,
                            new XElement("psn", "N/A"),
                            new XElement("xbox", code),
                            new XElement("switch", "N/A"),
                            new XElement("nin3ds", "N/A"),
                            new XElement("steam", "N/A")
                            ));
                        await Context.Channel.SendMessageAsync(Properties.strings.codeSuccess).ConfigureAwait(false);
                        break;
                    case "switch":
                    case "sw":
                        globals.commandStorage.Element("codes").Element($"g{Context.Guild.Id}").Add(new XElement(Context.User.Username,
                            new XElement("psn", "N/A"),
                            new XElement("xbox", "N/A"),
                            new XElement("switch", code),
                            new XElement("nin3ds", "N/A"),
                            new XElement("steam", "N/A")
                            ));
                        await Context.Channel.SendMessageAsync(Properties.strings.codeSuccess).ConfigureAwait(false);
                        break;
                    case "nin3ds":
                        globals.commandStorage.Element("codes").Element($"g{Context.Guild.Id}").Add(new XElement(Context.User.Username,
                            new XElement("psn", "N/A"),
                            new XElement("xbox", "N/A"),
                            new XElement("switch", "N/A"),
                            new XElement("nin3ds", code),
                            new XElement("steam", "N/A")
                            ));
                        await Context.Channel.SendMessageAsync(Properties.strings.codeSuccess).ConfigureAwait(false);
                        break;
                    case "steam":
                        globals.commandStorage.Element("codes").Element($"g{Context.Guild.Id}").Add(new XElement(Context.User.Username,
                            new XElement("psn", "N/A"),
                            new XElement("xbox", "N/A"),
                            new XElement("switch", "N/A"),
                            new XElement("nin3ds", "N/A"),
                            new XElement("steam", code)
                            ));
                        await Context.Channel.SendMessageAsync(Properties.strings.codeSuccess).ConfigureAwait(false);
                        break;
                    default:
                        await Context.Channel.SendMessageAsync(Properties.strings.codePlatformError).ConfigureAwait(false);
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
            if (Context.Channel.Id == globals.guildSettings[$"g{Context.Guild.Id}"][0])
            {
                globals.logMessage(Context,Properties.strings.catCmdTrigger,Properties.strings.cmdCodesNull);
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
            if (Context.Channel.Id == globals.guildSettings[$"g{Context.Guild.Id}"][0])
            {
                globals.logMessage(Context,Properties.strings.catCmdTrigger,Properties.strings.cmdCodesUser);
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
            if (Context.Channel.Id == globals.guildSettings[$"g{Context.Guild.Id}"][0])
            {
                globals.logMessage(Context,Properties.strings.catCmdTrigger,Properties.strings.cmdMyCodes);
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

            codeDatabase = from el in codeDatabase.Elements($"g{Context.Guild.Id}")
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

            codeDatabase = from el in codeDatabase.Elements($"g{Context.Guild.Id}")
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
            await Context.Channel.SendMessageAsync(null, false, codelist.Build()).ConfigureAwait(false);
        }

        /// <summary>
        /// Shows a list of commands available in the current context with brief descriptions of each
        /// </summary>
        /// <returns></returns>
        [Command("help")]
        [Summary("Displays a list of available commands")]
        public async Task showHelpAsync()
        {
            globals.logMessage(Context,Properties.strings.catCmdTrigger,Properties.strings.cmdHelp);
            var helpEmbed = new EmbedBuilder()
            {
                Title = "Available Commands"
            };

            if (Context.Channel.Id == globals.guildSettings[$"g{Context.Guild.Id}"][1])
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
                if (Context.Channel.Id == globals.guildSettings[$"g{Context.Guild.Id}"][0])
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

            helpEmbed.WithFooter(Properties.strings.helpFooter);
            helpEmbed.WithCurrentTimestamp();
            helpEmbed.WithColor(Color.Blue);

            //display the help embed to the user
            await Context.Channel.SendMessageAsync(Properties.strings.msgHelp, false, helpEmbed.Build()).ConfigureAwait(false);
        }

        /// <summary>
        /// Displays changes since last patch
        /// </summary>
        /// <returns></returns>
        [Command("patchNotes")]
        [Summary("Displays changes since last patch")]
        public async Task showPatchNotes()
        {
            globals.logMessage(Context,Properties.strings.catCmdTrigger,Properties.strings.cmdPatchNotes);
            var patchEmbed = new EmbedBuilder();
            patchEmbed.WithTitle(globals.versionID);

            patchEmbed.AddField(" * *Changes * *", globals.patchnotes);

            patchEmbed.WithFooter(globals.patchID);
            patchEmbed.WithTimestamp(globals.patchDate);
            patchEmbed.WithColor(Color.Gold);

            await Context.Channel.SendMessageAsync(Properties.strings.msgPatchNotes, false, patchEmbed.Build()).ConfigureAwait(false);
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
            globals.logMessage(Context,Properties.strings.catCmdTrigger,Properties.strings.cmdHelp);
            var helpEmbed = new EmbedBuilder()
            {
                Title = $"Help: {cmdName}"
            };

#pragma warning disable CA1304 // Specify CultureInfo
            switch (cmdName.ToLower())
#pragma warning restore CA1304 // Specify CultureInfo
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

            helpEmbed.WithFooter(Properties.strings.helpFooter);
            helpEmbed.WithCurrentTimestamp();
            helpEmbed.WithColor(Color.Blue);

            //display the help embed to the user
            await Context.Channel.SendMessageAsync(Properties.strings.msgHelpSpecific, false, helpEmbed.Build()).ConfigureAwait(false);

        }
    }
}
