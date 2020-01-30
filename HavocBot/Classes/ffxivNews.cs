using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Net;
using HtmlAgilityPack;
using System.IO;
using Discord;
using System.Globalization;

namespace HavocBot
{
    /// <summary>
    /// Enum for identifying the type of a news item
    /// </summary>
    public enum newsType
    {
        /// <summary>
        /// news items originating from the notices section on the FFXIV Lodestone
        /// </summary>
        Notice,
        /// <summary>
        /// news items originating from the Updates section on the FFXIV Lodestone
        /// </summary>
        Update,
        /// <summary>
        /// news items originating from the Statuses section on the FFXIV Lodestone
        /// </summary>
        Status,
        /// <summary>
        /// news items originating from the Topics section on the FFXIV Lodestone
        /// </summary>
        Topic,
        /// <summary>
        /// news items originating from the Maintenance section on the FFXIV Lodestone
        /// </summary>
        Maintenance,
    }

    /// <summary>
    /// Structure for containing the data obtained from topic news items on the ffxiv lodestone
    /// </summary>
    public struct topic
    {
        /// <summary>
        /// Variable for storing the title of the news item
        /// </summary>
        public string title;

        /// <summary>
        /// Variable for storing the image of the topic news item
        /// </summary>
        public string image;

        /// <summary>
        /// Variable for storing the description of the news item
        /// </summary>
        public string desc;

        /// <summary>
        /// Variable for storing the link of the news item
        /// </summary>
        public string link;

        /// <summary>
        /// Variable for storing the unique identifier of the news item
        /// </summary>
        public string id;
    }

    /// <summary>
    /// Structure for containing the data obtained from generic news items on the ffxiv lodestone
    /// </summary>
    public struct news
    {
        /// <summary>
        /// Variable for storing the title of the news item
        /// </summary>
        public string title;

        /// <summary>
        /// Variable for storing the link of the news item
        /// </summary>
        public string link;

        /// <summary>
        /// Variable for storing the unique identifier of the news item
        /// </summary>
        public string id;
    }

    /// <summary>
    /// Structure for containing the data obtained from maintenance news items on the ffxiv lodestone
    /// </summary>
    public struct maintNews
    {
        /// <summary>
        /// Variable for storing the title of the news item
        /// </summary>
        public string title;

        /// <summary>
        /// Variable for storing the description of the news item
        /// </summary>
        public string desc;

        /// <summary>
        /// Variable for storing the link of the news item
        /// </summary>
        public string link;

        /// <summary>
        /// Variable for storing the unique identifier of the news item
        /// </summary>
        public string id;

        /// <summary>
        /// Variable for storing the start date of the maintenance news item
        /// </summary>
        public DateTime start;

        /// <summary>
        /// Variable for storing the end date of the maintenance news item
        /// </summary>
        public DateTime end;
    }

    /// <summary>
    /// Class for obtaining and handling ffxiv news items from the ffxiv lodestone website
    /// </summary>
    public class ffxivNews
    {
        /// <summary>
        /// Constant string containing the first portion of the ffxiv lodestone URL
        /// </summary>
        public const string siteRoot = "https://na.finalfantasyxiv.com";

        /// <summary>
        /// Constant string containing the portion of the URL leading to topic news items
        /// </summary>
        public const string topicsRoot = "/lodestone/topics";

        /// <summary>
        /// Constant string containing the portion of the URL leading to a list of all the latest news items
        /// </summary>
        public const string latestRoot = "/lodestone/news/";

        /// <summary>
        /// Constant string containing the portion of the URL leading to notice news items
        /// </summary>
        public const string noticesRoot = "/lodestone/news/category/1";

        /// <summary>
        /// Constant string containing the portion of the URL leading to maintenance news items
        /// </summary>
        public const string maintRoot = "/lodestone/news/category/2";

        /// <summary>
        /// Constant string containing the portion of the URL leading to update news items
        /// </summary>
        public const string updatesRoot = "/lodestone/news/category/3";

        /// <summary>
        /// Constant string containing the portion of the URL leading to status news items
        /// </summary>
        public const string statusRoot = "/lodestone/news/category/4";

        private const string _noticeImg = "https://i.imgur.com/rRYRqbf.png";

        private const string _topicImg = "https://i.imgur.com/yJumUoG.png";

        private const string _updateImg = "https://i.imgur.com/bVvGZQX.png";

        private const string _maintImg = "https://i.imgur.com/eEjvRhp.png";

        private const string _statusImg = "https://i.imgur.com/sFu0C1x.png";

        private string _lastNoticeID;

        private string _lastTopicID;

        private string _lastUpdateID;

        private string _lastMaintID;

        private string _lastStatusID;

        readonly HtmlWeb _web;

        /// <summary>
        /// Basic constructor.
        /// Initializes the web client
        /// </summary>
        public ffxivNews()
        {
            _web = new HtmlWeb();
            IEnumerable<XElement> settingsRetrieve =
                 from el in globals.commandStorage.Elements("news")
                 select el;


            _lastTopicID = (string)
                 (from el in settingsRetrieve.Descendants("topics")
                  select el).First();

            _lastNoticeID = (string)
                 (from el in settingsRetrieve.Descendants("notices")
                  select el).First();

            _lastMaintID = (string)
                 (from el in settingsRetrieve.Descendants("maint")
                  select el).First();

            _lastUpdateID = (string)
                 (from el in settingsRetrieve.Descendants("updates")
                  select el).First();

            _lastStatusID = (string)
                 (from el in settingsRetrieve.Descendants("status")
                  select el).First();
        }

        /// <summary>
        /// Obtains the needed data from a topic news item
        /// </summary>
        /// <param name="tId">the news item to retrieve. 1 is the newest. Each increment is one older news item. must be a positive integer</param>
        /// <returns>Returns a topic structure containing the requested data</returns>
        public topic getTopic(int tId)
        {

            //Topics
            Uri.TryCreate(siteRoot + topicsRoot, UriKind.RelativeOrAbsolute, out Uri uriResult);
            HtmlDocument doc = _web.Load(uriResult);
            topic retTopic = new topic();

            try
            {

                string xpathRoot = "/html/body/div[4]/div[2]/div[1]/div/ul[2]";
                string xpathTarget = $"/li[{tId}]";


                retTopic.title = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/header/p/a")[0].InnerText;
                retTopic.image = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/div/a/img")[0].Attributes[0].Value;
                retTopic.desc = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/div/p[2]")[0].InnerText;
                retTopic.link = siteRoot + doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/header/p/a")[0].Attributes[0].Value;
                //retTopic.time = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/header/time")[0].InnerText;
                
                retTopic.id = retTopic.link.Substring(retTopic.link.LastIndexOf('/') + 1);
            }
            catch (NullReferenceException)
            {
                try
                {
                    Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} Error in Topic retrieval, Defaulting to alternate execution.");
                    string xpathRoot = "/html/body/div[4]/div[2]/div[1]/div/ul[3]";
                    string xpathTarget = $"/li[{tId}]";


                    retTopic.title = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/header/p/a")[0].InnerText;
                    retTopic.image = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/div/a/img")[0].Attributes[0].Value;
                    retTopic.desc = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/div/p[2]")[0].InnerText;
                    retTopic.link = siteRoot + doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/header/p/a")[0].Attributes[0].Value;
                    //retTopic.time = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/header/time")[0].InnerText;
                    
                    retTopic.id = retTopic.link.Substring(retTopic.link.LastIndexOf('/') + 1);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} Error in Topic retrieval, Alternate execution failed, skipping topic retrieval.");
                }
            }

            return retTopic;
        }

        /// <summary>
        /// Obtains the needed data from a Notice, Update, or Status news item
        /// </summary>
        /// <param name="tId">the news item to retrieve. 1 is the newest. Each increment is one older news item. must be a positive integer</param>
        /// <param name="type">The type of news item being requested. must be a newsType enum value</param>
        /// <returns>Returns a news structure containing the requested data</returns>
        public news getNews(int tId, newsType type)
        {
            HtmlDocument doc;
            Uri uriResult;
            news retNews = new news();

            switch (type)
            {
                case newsType.Notice:
                    Uri.TryCreate(siteRoot + noticesRoot, UriKind.RelativeOrAbsolute, out uriResult);
                    doc = _web.Load(uriResult);
                    break;
                case newsType.Update:
                    Uri.TryCreate(siteRoot + updatesRoot, UriKind.RelativeOrAbsolute, out uriResult);
                    doc = _web.Load(uriResult);
                    break;
                case newsType.Status:
                    Uri.TryCreate(siteRoot + statusRoot, UriKind.RelativeOrAbsolute, out uriResult);
                    doc = _web.Load(uriResult);
                    break;
                default:
                    throw new Exception("Unexpected NewsType in paramater");
            }

            try
            {
                string xpathRoot = $"/html/body/div[4]/div[2]/div[1]/div/ul[2]";
                string xpathTarget = $"/li[{tId}]";

                retNews.title = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/a/div/p")[0].InnerText;
                retNews.link = siteRoot + doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/a")[0].Attributes[0].Value;
                //retNews.time = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/header/time")[0].InnerText;
                
                retNews.id = retNews.link.Substring(retNews.link.LastIndexOf('/') + 1);

                
            }
            catch (NullReferenceException)
            {
                try
                {
                    Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} Error in {type.ToString()} retrieval, Defaulting to alternate execution.");
                    string xpathRoot = $"/html/body/div[4]/div[2]/div[1]/div/ul[3]";
                    string xpathTarget = $"/li[{tId}]";


                    retNews.title = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/a/div/p")[0].InnerText;
                    retNews.link = siteRoot + doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/a")[0].Attributes[0].Value;
                    //retNews.time = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/header/time")[0].InnerText;
                    
                    retNews.id = retNews.link.Substring(retNews.link.LastIndexOf('/') + 1);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} Error in {type.ToString()} retrieval, Alternate execution failed, skipping {type.ToString()} retrieval.");

                }
                
            }
            return retNews;
        }

        /// <summary>
        /// Obtains the needed data from a Maintenance news item
        /// </summary>
        /// <param name="tId">the news item to retrieve. 1 is the newest. Each increment is one older news item. must be a positive integer</param>
        /// <returns>Returns a maintNews structure containing the requested data</returns>
        public maintNews getMaint(int tId)
        {

            Uri.TryCreate(siteRoot + maintRoot, UriKind.RelativeOrAbsolute, out Uri uriResult);
            HtmlDocument doc = _web.Load(uriResult);
            maintNews retMaint = new maintNews();

            try
            {
                string xpathRoot = "/html/body/div[4]/div[2]/div[1]/div/ul[2]";
                string xpathTarget = $"/li[{tId}]";


                retMaint.title = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/a/div/p")[0].InnerText;
                retMaint.link = siteRoot + doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/a")[0].Attributes[0].Value;
                //retMaint.time = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/header/time")[0].InnerText;

                retMaint.id = retMaint.link.Substring(retMaint.link.LastIndexOf('/') + 1);

                Uri.TryCreate(retMaint.link, UriKind.RelativeOrAbsolute, out uriResult);
                doc = _web.Load(uriResult);

                retMaint.desc = doc.DocumentNode.SelectNodes("/html/body/div[4]/div[2]/div[1]/article/div[1]")[0].InnerText;

                string tempStart;
                string tempEnd;
                retMaint.desc = retMaint.desc.Substring(retMaint.desc.IndexOf(']') + 1);
                retMaint.desc = retMaint.desc.Substring(0, retMaint.desc.IndexOf('('));
                retMaint.desc = retMaint.desc.Trim();
                tempStart = retMaint.desc.Substring(0, retMaint.desc.IndexOf('t'));
                tempEnd = retMaint.desc.Remove(retMaint.desc.IndexOf(tempStart), tempStart.Length);
                tempEnd = tempEnd.Substring(tempEnd.IndexOf('o') + 1);
                tempEnd = tempEnd.Replace(".", "");
                tempEnd = tempEnd.Trim();
                tempStart = tempStart.Replace(".", "");
                tempStart = tempStart.Trim();

                retMaint.start = DateTime.Parse(tempStart);
                retMaint.end = DateTime.Parse(tempEnd);
                CultureInfo enUS = new CultureInfo("en-US");
                if (DateTime.TryParseExact(tempEnd, "h:mm tt", enUS, DateTimeStyles.None, out var result2))
                {
                    retMaint.end = retMaint.start.Date + retMaint.end.TimeOfDay;
                }
            }
            catch (NullReferenceException)
            {
                try
                {
                    string xpathRoot = "/html/body/div[4]/div[2]/div[1]/div/ul[3]";
                    string xpathTarget = $"/li[{tId}]";


                    retMaint.title = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/a/div/p")[0].InnerText;
                    retMaint.link = siteRoot + doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/a")[0].Attributes[0].Value;
                    //retMaint.time = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/header/time")[0].InnerText;

                    retMaint.id = retMaint.link.Substring(retMaint.link.LastIndexOf('/') + 1);

                    Uri.TryCreate(retMaint.link, UriKind.RelativeOrAbsolute, out uriResult);
                    doc = _web.Load(uriResult);

                    retMaint.desc = doc.DocumentNode.SelectNodes("/html/body/div[4]/div[2]/div[1]/article/div[1]")[0].InnerText;

                    string tempStart;
                    string tempEnd;
                    retMaint.desc = retMaint.desc.Substring(retMaint.desc.IndexOf(']') + 1);
                    retMaint.desc = retMaint.desc.Substring(0, retMaint.desc.IndexOf('('));
                    retMaint.desc = retMaint.desc.Trim();
                    tempStart = retMaint.desc.Substring(0, retMaint.desc.IndexOf('t'));
                    tempEnd = retMaint.desc.Remove(retMaint.desc.IndexOf(tempStart), tempStart.Length);
                    tempEnd = tempEnd.Substring(tempEnd.IndexOf('o') + 1);
                    tempEnd = tempEnd.Replace(".", "");
                    tempEnd = tempEnd.Trim();
                    tempStart = tempStart.Replace(".", "");
                    tempStart = tempStart.Trim();

                    retMaint.start = DateTime.Parse(tempStart);
                    retMaint.end = DateTime.Parse(tempEnd);
                    CultureInfo enUS = new CultureInfo("en-US");
                    if (DateTime.TryParseExact(tempEnd, "h:mm tt", enUS, DateTimeStyles.None, out var result2))
                    {
                        retMaint.end = retMaint.start.Date + retMaint.end.TimeOfDay;
                    }
                    Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} Error in Maintenance retrieval, Defaulting to alternate execution.");

                }
                catch (NullReferenceException)
                {
                    Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} Error in Maintenance retrieval, Alternate execution failed, skipping Maintenance retrieval.");
                }
                
            }
            
            
            return retMaint;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="retTop"></param>
        /// <returns></returns>
        public EmbedBuilder generateEmbed(topic retTop)
        {
            EmbedBuilder newsEmbed = new EmbedBuilder
            {
                Title = retTop.title,
                ImageUrl = retTop.image,
                Description = retTop.desc,
                Url = retTop.link
            };
            //newsEmbed.Timestamp = retTop.time;
            newsEmbed.WithAuthor("Topic", _topicImg);
            newsEmbed.WithColor(Color.DarkOrange);
            return newsEmbed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="retNews"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public EmbedBuilder generateEmbed(news retNews, newsType type)
        {
            EmbedBuilder newsEmbed = new EmbedBuilder();
            string iconUrl;
            if (type == newsType.Notice)
            {
                iconUrl = _noticeImg;
                newsEmbed.WithColor(Color.LighterGrey);
            }
            else if (type == newsType.Status)
            {
                iconUrl = _statusImg;
                newsEmbed.WithColor(Color.Red);
            }
            else
            {
                iconUrl = _updateImg;
                newsEmbed.WithColor(Color.Green);
            }
            newsEmbed.WithTitle(retNews.title);
            newsEmbed.WithUrl(retNews.link);
            //newsEmbed.Timestamp = retNews.time;
            newsEmbed.WithAuthor(type.ToString(), iconUrl);
            return newsEmbed;
           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="retMaint"></param>
        /// <returns></returns>
        public EmbedBuilder generateEmbed(maintNews retMaint)
        {
            EmbedBuilder newsEmbed = new EmbedBuilder
            {
                Title = retMaint.title,
                //newsEmbed.Description = retMaint.desc;
                Url = retMaint.link
            };
            //newsEmbed.Timestamp = retMaint.time;
            newsEmbed.AddField("Start", retMaint.start, true);
            newsEmbed.AddField("End", retMaint.end, true);
            newsEmbed.WithAuthor("Maintenance", _maintImg);
            newsEmbed.WithColor(Color.Gold);
            return newsEmbed;
        }

        /// <summary>
        /// Searches through the lodestone news pages and determines how many new news items there are
        /// </summary>
        /// <returns>Returns an integer array representing how many new news items are present
        /// Each index corresponds to a news type
        /// 0: Topic, 1: Notice, 2: Update, 3: Status, 4: Maint
        /// Value equals number of new items</returns>
        public void refresh()
        {
            // Each index corresponds to a news type
            // 0: Topic, 1: Notice, 2: Update, 3: Status, 4: Maint 
            // Value equals number of new items
            const int startSearch = 1;
            const int maxSearch = 5;

            // topics
            for (int i = startSearch; i < maxSearch; i++)
            {
                topic retTop = getTopic(i);
                if (retTop.id.Equals(_lastTopicID))
                {
                    break;
                }
                else
                {
                    havocBotClass.showNewsEmbed(retTop);
                    Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} Posted New Topic {retTop.title}");
                }
            }

            // notices
            for (int i = startSearch; i < maxSearch; i++)
            {
                news retNews = getNews(i,newsType.Notice);
                if (retNews.id.Equals(_lastNoticeID))
                {
                    break;
                }
                else
                {
                    havocBotClass.showNewsEmbed(retNews, newsType.Notice);
                    Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} Posted New Notice {retNews.title}");
                }

            }

            // updates
            for (int i = startSearch; i < maxSearch; i++)
            {
                news retNews = getNews(i, newsType.Update);
                if (retNews.id.Equals(_lastUpdateID))
                {
                    break;
                }
                else
                {
                    havocBotClass.showNewsEmbed(retNews, newsType.Update);
                    Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} Posted New Update {retNews.title}");
                }

            }

            // status
            for (int i = startSearch; i < maxSearch; i++)
            {
                news retNews = getNews(i, newsType.Status);
                if (retNews.id.Equals(_lastStatusID))
                {
                    break;
                }
                else
                {
                    havocBotClass.showNewsEmbed(retNews, newsType.Status);
                    Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} Posted New Status {retNews.title}");
                }

            }

            // maintenance
            for (int i = startSearch; i < maxSearch; i++)
            {
                maintNews retMaint = getMaint(i);
                if (retMaint.id.Equals(_lastMaintID))
                {
                    break;
                }
                else
                {
                    havocBotClass.showNewsEmbed(retMaint);
                    Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} Posted New Maintenance {retMaint.title}");
                    if (retMaint.title.Contains("Lodestone"))
                    {
                        globals.lodeMaintStart = retMaint.start;
                        globals.lodeMaintEnd = retMaint.end;
                        IEnumerable<XElement> maintRetrieve =
                           from el in globals.commandStorage.Elements("maintenance")
                           select el;

                        maintRetrieve = from el in maintRetrieve.Elements("lodeMaint")
                                        select el;

                        XElement changeTarget = (from el in maintRetrieve.Descendants("start")
                                                 select el).First();

                        changeTarget.Value = retMaint.start.ToString();

                        changeTarget = (from el in maintRetrieve.Descendants("end")
                                        select el).First();

                        changeTarget.Value = retMaint.end.ToString();

                        globals.commandStorage.Save(globals.storageFilePath);
                        Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} Logged new lodestone maintenance from {retMaint.start} to {retMaint.end}");
                    }
                    else if (retMaint.title.Contains("All"))
                    {
                        IEnumerable<XElement> maintRetrieve =
                            from el in globals.commandStorage.Elements("maintenance")
                            select el;

                        maintRetrieve = from el in maintRetrieve.Elements("maint")
                                        select el;

                        XElement changeTarget = (from el in maintRetrieve.Descendants("start")
                                                 select el).First();

                        changeTarget.Value = retMaint.start.ToString();

                        changeTarget = (from el in maintRetrieve.Descendants("end")
                                        select el).First();

                        changeTarget.Value = retMaint.end.ToString();

                        globals.commandStorage.Save(globals.storageFilePath);
                        Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} Logged new All Worlds maintenance from {retMaint.start} to {retMaint.end}");
                    }
                    else if (retMaint.title.Contains("Exodus"))
                    {
                        IEnumerable<XElement> maintRetrieve =
                            from el in globals.commandStorage.Elements("maintenance")
                            select el;

                        maintRetrieve = from el in maintRetrieve.Elements("maint")
                                        select el;

                        XElement changeTarget = (from el in maintRetrieve.Descendants("start")
                                                 select el).First();

                        changeTarget.Value = retMaint.start.ToString();

                        changeTarget = (from el in maintRetrieve.Descendants("end")
                                        select el).First();

                        changeTarget.Value = retMaint.end.ToString();

                        globals.commandStorage.Save(globals.storageFilePath);
                        Console.WriteLine($"{DateTime.Now.ToShortDateString(),-11}{System.DateTime.Now.ToLongTimeString(),-8} Logged new Exodus world maintenance from {retMaint.start} to {retMaint.end}");
                    }
                }

            }

            topic retTop2 = getTopic(1);
            _lastTopicID = retTop2.id;
            globals.commandStorage.Element("news").Element("topics").SetValue(_lastTopicID);

            news retNews2 = getNews(1, newsType.Notice);
            _lastNoticeID = retNews2.id;
            globals.commandStorage.Element("news").Element("notices").SetValue(_lastNoticeID);

            retNews2 = getNews(1, newsType.Update);
            _lastUpdateID = retNews2.id;
            globals.commandStorage.Element("news").Element("updates").SetValue(_lastUpdateID);

            retNews2 = getNews(1, newsType.Status);
            _lastStatusID = retNews2.id;
            globals.commandStorage.Element("news").Element("status").SetValue(_lastStatusID);

            maintNews retMaint2 = getMaint(1);
            _lastMaintID = retMaint2.id;
            globals.commandStorage.Element("news").Element("maint").SetValue(_lastMaintID);

            globals.commandStorage.Save(globals.storageFilePath);
        }
    }
}
