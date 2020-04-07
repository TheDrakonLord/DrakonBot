/* Title: lodestone.cs
 * Author: Neal Jamieson
 * Version: 0.0.0.0
 * 
 * Description:
 *     This class handles the requests made to the Final Fantasy XIV Lodestone API
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
 *     Xml.linq
 *     XIVAPI
 *     newtonsoft.json
 *      
 * References:
 *     This code is adapted from the instructions provided by the XIVAPI Documentation found at:
 *         https://xivapi.com/
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Discord.Commands;
using System.Xml.Linq;
using Discord;
using Discord.WebSocket;

namespace HavocBot
{
    /// <summary>
    /// Class for accessing the FFXIV lodestone API
    /// </summary>
    public class lodestone
    {
        private readonly HttpClient _client;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public lodestone()
        {
            _client = new HttpClient();
        }

        /// <summary>
        /// Searchers for a specified character and saves it to a users profile
        /// </summary>
        /// <param name="name">the name of the character to be displayed</param>
        /// <param name="server">the characters server</param>
        /// <param name="context">context in which the command was called</param>
        /// <returns></returns>
        public async Task getCharacter(string name, string server, SocketCommandContext context)
        {
            System.Diagnostics.Contracts.Contract.Requires(name != null);
            System.Diagnostics.Contracts.Contract.Requires(context != null);
            
            string userID = $"u{context.User.Id}";
            try
            {


#pragma warning disable CA1307 // Specify StringComparison
                string strName = name.Replace(" ", "+");
#pragma warning restore CA1307 // Specify StringComparison
                Uri.TryCreate($"https://xivapi.com/character/search?name={strName}&server={server}", UriKind.RelativeOrAbsolute, out Uri uriResult);
            HttpResponseMessage req = await _client.GetAsync(uriResult).ConfigureAwait(false);
            dynamic character = JsonConvert.DeserializeObject(
            req.Content.ReadAsStringAsync().Result
            );
            //store the data in the xml file
            globals.commandStorage.Element("characters").Add(new XElement(userID,
                        new XElement("id", (string)character.Results[0].ID),
                        new XElement("name", (string)character.Results[0].Name),
                        new XElement("server", (string)character.Results[0].Server)
                        ));
            //Save the tree to the file
            globals.commandStorage.Save(globals.storageFilePath);

            //create an embed based off of the loaded event
            var charEmbed = new EmbedBuilder()
            {
                Title = "Success!"
            };
            charEmbed.WithDescription("Character successfully saved! \n\n You can now view your character with !whoami");
            charEmbed.WithCurrentTimestamp();
            charEmbed.WithColor(Color.Green);

            //display the loaded event to the user
            await context.Channel.SendMessageAsync(Properties.strings.msgCharSave, false, charEmbed.Build()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                globals.logMessage(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Access the users character data and displays the users character info
        /// </summary>
        /// <param name="context">context in which the command was called</param>
        /// <returns></returns>
        public async Task showCharacter(SocketCommandContext context)
        {
            System.Diagnostics.Contracts.Contract.Requires(context != null);
            string charName;
            string charServer;
            string charTitle;
            string charRace;
            string charTribe;
            string charLevel;
            string charJob;
            string charFC;
            string charAvatar;
            string charPortrait;
            string userID = $"u{context.User.Id}";

            try
            {
                //find and load the requested character
                IEnumerable<XElement> charRetrieve =
                 from el in globals.commandStorage.Elements("characters")
                 select el;

                charRetrieve = from el in charRetrieve.Elements(userID)
                                select el;

                string strCharId = (string)
                 (from el in charRetrieve.Descendants("id")
                  select el).Last();

                if (ulong.TryParse(strCharId, out ulong charId)) { }
                
                charName = (string)
                 (from el in charRetrieve.Descendants("name")
                  select el).Last();

                charServer = (string)
                 (from el in charRetrieve.Descendants("server")
                  select el).Last();

                Uri.TryCreate($"https://xivapi.com/character/{charId}?extended=1&data=FC,CJ", UriKind.RelativeOrAbsolute, out Uri uriResult);
                HttpResponseMessage req = await _client.GetAsync(uriResult).ConfigureAwait(false);
                dynamic character = JsonConvert.DeserializeObject(
                req.Content.ReadAsStringAsync().Result
                );

                charTitle = character.Character.Title.Name;
                charRace = character.Character.Race.Name;
                charTribe = character.Character.Tribe.Name;
                try
                {
                    charJob = character.Character.ActiveClassJob.Job.Name;
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    charJob = character.Character.ActiveClassJob.Class.Name;
                }
                charLevel = character.Character.ActiveClassJob.Level;
                charAvatar = character.Character.Avatar;
                charFC = character.FreeCompany.Name;
                charServer = character.FreeCompany.Server;
                charPortrait = character.Character.Portrait;

                

                //create an embed based off of the loaded event
                var charEmbed = new EmbedBuilder()
                {
                };
                charEmbed.WithDescription($"{charRace} {charTribe}\nLevel {charLevel} {charJob}\n<{charFC}> on {charServer}");
                charEmbed.WithAuthor($"{charName}, {charTitle}", charAvatar, $"https://na.finalfantasyxiv.com/lodestone/character/{charId}/");
                charEmbed.WithImageUrl(charPortrait);
                charEmbed.WithCurrentTimestamp();
                charEmbed.WithColor(Color.Red);

                //display the loaded event to the user
                await context.Channel.SendMessageAsync(Properties.strings.msgCharDisplay, false, charEmbed.Build()).ConfigureAwait(false);

            }
            catch (Exception ex)
            {
                await context.Channel.SendMessageAsync(Properties.strings.errorMissingChar, false).ConfigureAwait(false);
                globals.logMessage(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Access the users character data and displays the users character info
        /// </summary>
        /// <param name="context">context in which the command was called</param>
        /// <param name="userid">User id to be looked up</param>
        /// <returns></returns>
        public async Task showCharacter(SocketCommandContext context, string userid)
        {
            System.Diagnostics.Contracts.Contract.Requires(context != null);
            string charName;
            string charServer;
            string charTitle;
            string charRace;
            string charLevel;
            string charJob;
            string charFC;
            string charFcId;
            string charAvatar;
            string charTitleId;
            string charRaceId;
            string charPortrait;
            SocketGuildUser user = null;
            string uID = $"u{context.User.Id}";

            if (ulong.TryParse(userid, out ulong ulongId))
            {
                
                user = context.Guild.GetUser(ulongId);
            }
            
            

            try
            {
                //find and load the requested character
                IEnumerable<XElement> charRetrieve =
                 from el in globals.commandStorage.Elements("characters")
                 select el;

                charRetrieve = from el in charRetrieve.Elements(uID)
                               select el;

                string strCharId = (string)
                 (from el in charRetrieve.Descendants("id")
                  select el).First();

                if (ulong.TryParse(strCharId, out ulong charId)) { }

                charName = (string)
                 (from el in charRetrieve.Descendants("name")
                  select el).First();

                charServer = (string)
                 (from el in charRetrieve.Descendants("server")
                  select el).First();

                Uri.TryCreate($"https://xivapi.com/character/{charId}", UriKind.RelativeOrAbsolute, out Uri uriResult);
                HttpResponseMessage req = await _client.GetAsync(uriResult).ConfigureAwait(false);
                dynamic character = JsonConvert.DeserializeObject(
                req.Content.ReadAsStringAsync().Result
                );

                charTitleId = character.Character.Title;
                charRaceId = character.Character.Race;
                charLevel = character.Character.ActiveClassJob.Level;
                charJob = character.Character.ActiveClassJob.Name;
                charAvatar = character.Character.Avatar;
                charFcId = character.Character.FreeCompanyId;
                charPortrait = character.Character.Portrait;

                Uri.TryCreate($"https://xivapi.com/freecompany/{charFcId}", UriKind.RelativeOrAbsolute, out uriResult);

                req = await _client.GetAsync(uriResult).ConfigureAwait(false);
                character = JsonConvert.DeserializeObject(
                req.Content.ReadAsStringAsync().Result
                );

                charFC = character.FreeCompany.Name;

                Uri.TryCreate($"https://xivapi.com/title/{charTitleId}", UriKind.RelativeOrAbsolute, out uriResult);
                req = await _client.GetAsync(uriResult).ConfigureAwait(false);
                character = JsonConvert.DeserializeObject(
                req.Content.ReadAsStringAsync().Result
                );

                charTitle = character.Name;

                Uri.TryCreate($"https://xivapi.com/race/{charRaceId}", UriKind.RelativeOrAbsolute, out uriResult);
                req = await _client.GetAsync(uriResult).ConfigureAwait(false);
                character = JsonConvert.DeserializeObject(
                req.Content.ReadAsStringAsync().Result
                );

                charRace = character.Name;

                //create an embed based off of the loaded event
                var charEmbed = new EmbedBuilder()
                {
                };
                charEmbed.WithDescription($"{charRace} \nLevel {charLevel} {charJob}\n<{charFC}> on {charServer}");
                charEmbed.WithAuthor($"{charName}, {charTitle}", charAvatar, $"https://na.finalfantasyxiv.com/lodestone/character/{charId}/");
                charEmbed.WithImageUrl(charPortrait);
                charEmbed.WithCurrentTimestamp();
                charEmbed.WithColor(Color.Red);

                //display the loaded event to the user
                await context.Channel.SendMessageAsync(Properties.strings.msgCharDisplay, false, charEmbed.Build()).ConfigureAwait(false);

            }
            catch (Exception ex)
            {
                await context.Channel.SendMessageAsync(Properties.strings.errorMissingChar, false).ConfigureAwait(false);
                globals.logMessage(ex.Message);
                throw;
            }
        }
    }
}
