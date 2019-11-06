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
        private HttpClient _client;

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
            try
            {

           
            string strName = name.Replace(" ", "+");
            HttpResponseMessage req = await _client.GetAsync($"https://xivapi.com/character/search?name={strName}&server={server}");
            dynamic character = JsonConvert.DeserializeObject(
            req.Content.ReadAsStringAsync().Result
            );
            //store the data in the xml file
            globals.commandStorage.Element("characters").Add(new XElement(context.User.Username,
                    new XAttribute("userId", context.User.Id.ToString()),
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
            await context.Channel.SendMessageAsync("Saving Character", false, charEmbed.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
            ulong charId;
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

            try
            {
                //find and load the requested character
                IEnumerable<XElement> charRetrieve =
                 from el in globals.commandStorage.Elements("characters")
                 select el;

                charRetrieve = from el in charRetrieve.Elements(context.User.Username)
                                where (string)el.Attribute("userId") == context.User.Id.ToString()
                                select el;

                string strCharId = (string)
                 (from el in charRetrieve.Descendants("id")
                  select el).Last();

                if (ulong.TryParse(strCharId, out charId)) { }
                
                charName = (string)
                 (from el in charRetrieve.Descendants("name")
                  select el).Last();

                charServer = (string)
                 (from el in charRetrieve.Descendants("server")
                  select el).Last();

                HttpResponseMessage req = await _client.GetAsync($"https://xivapi.com/character/{charId}");
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

                req = await _client.GetAsync($"https://xivapi.com/freecompany/{charFcId}");
                character = JsonConvert.DeserializeObject(
                req.Content.ReadAsStringAsync().Result
                );

                charFC = character.FreeCompany.Name;

                req = await _client.GetAsync($"https://xivapi.com/title/{charTitleId}");
                character = JsonConvert.DeserializeObject(
                req.Content.ReadAsStringAsync().Result
                );

                charTitle = character.Name;

                req = await _client.GetAsync($"https://xivapi.com/race/{charRaceId}");
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
                await context.Channel.SendMessageAsync("Displaying Character", false, charEmbed.Build());

            }
            catch (Exception ex)
            {
                await context.Channel.SendMessageAsync("Error: No Character Found", false);
                Console.WriteLine(ex.Message);
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
            ulong charId;
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
            ulong ulongId;
            SocketGuildUser user = null;

            if (ulong.TryParse(userid, out ulongId))
            {
                user = context.Guild.GetUser(ulongId);
            }
            
            

            try
            {
                //find and load the requested character
                IEnumerable<XElement> charRetrieve =
                 from el in globals.commandStorage.Elements("characters")
                 select el;

                charRetrieve = from el in charRetrieve.Elements(user.Username)
                               where (string)el.Attribute("userId") == user.Id.ToString()
                               select el;

                string strCharId = (string)
                 (from el in charRetrieve.Descendants("id")
                  select el).First();

                if (ulong.TryParse(strCharId, out charId)) { }

                charName = (string)
                 (from el in charRetrieve.Descendants("name")
                  select el).First();

                charServer = (string)
                 (from el in charRetrieve.Descendants("server")
                  select el).First();

                HttpResponseMessage req = await _client.GetAsync($"https://xivapi.com/character/{charId}");
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

                req = await _client.GetAsync($"https://xivapi.com/freecompany/{charFcId}");
                character = JsonConvert.DeserializeObject(
                req.Content.ReadAsStringAsync().Result
                );

                charFC = character.FreeCompany.Name;

                req = await _client.GetAsync($"https://xivapi.com/title/{charTitleId}");
                character = JsonConvert.DeserializeObject(
                req.Content.ReadAsStringAsync().Result
                );

                charTitle = character.Name;

                req = await _client.GetAsync($"https://xivapi.com/race/{charRaceId}");
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
                await context.Channel.SendMessageAsync("Displaying Character", false, charEmbed.Build());

            }
            catch (Exception ex)
            {
                await context.Channel.SendMessageAsync("Error: No Character Found", false);
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
