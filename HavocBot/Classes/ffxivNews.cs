using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using HtmlAgilityPack;
using System.IO;
using Discord;

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

        readonly HtmlWeb _web;
        
        /// <summary>
        /// Basic constructor.
        /// Initializes the web client
        /// </summary>
        public ffxivNews()
        {
            _web = new HtmlWeb();
            
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

            string xpathRoot = "/html/body/div[4]/div[2]/div[1]/div/ul[2]";
            string xpathTarget = $"/li[{tId}]";

            topic retTopic = new topic
            {
                title = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/header/p/a")[0].InnerText,
                image = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/div/a/img")[0].Attributes[0].Value,
                desc = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/div/p[2]")[0].InnerText,
                link = siteRoot + doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/header/p/a")[0].Attributes[0].Value,
                //retTopic.time = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/header/time")[0].InnerText;
                id = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/header/time")[0].FirstChild.Attributes[0].Value
            };

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
            int intUL;
            Uri uriResult;
            switch (type)
            {
                case newsType.Notice:
                    Uri.TryCreate(siteRoot + noticesRoot, UriKind.RelativeOrAbsolute, out uriResult);
                    doc = _web.Load(uriResult);
                    intUL = 3;
                    break;
                case newsType.Update:
                    Uri.TryCreate(siteRoot + updatesRoot, UriKind.RelativeOrAbsolute, out uriResult);
                    doc = _web.Load(uriResult);
                    intUL = 2;
                    break;
                case newsType.Status:
                    Uri.TryCreate(siteRoot + statusRoot, UriKind.RelativeOrAbsolute, out uriResult);
                    doc = _web.Load(uriResult);
                    intUL = 2;
                    break;
                default:
                    Uri.TryCreate(siteRoot + noticesRoot, UriKind.RelativeOrAbsolute, out uriResult);
                    doc = _web.Load(uriResult);
                    intUL = 3;
                    break;
            }
            

            string xpathRoot = $"/html/body/div[4]/div[2]/div[1]/div/ul[{intUL}]";
            string xpathTarget = $"/li[{tId}]";

            news retNews = new news
            {
                title = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/a/div/p")[0].InnerText,
                link = siteRoot + doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/a")[0].Attributes[0].Value,
                //retNews.time = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/header/time")[0].InnerText;
                id = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/a/div/time")[0].FirstChild.Attributes[0].Value
            };

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

            string xpathRoot = "/html/body/div[4]/div[2]/div[1]/div/ul[3]";
            string xpathTarget = $"/li[{tId}]";

            maintNews retMaint = new maintNews
            {
                title = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/a/div/p")[0].InnerText,
                link = siteRoot + doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/a")[0].Attributes[0].Value,
                //retMaint.time = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/header/time")[0].InnerText;
                id = doc.DocumentNode.SelectNodes(xpathRoot + xpathTarget + "/a/div/time")[0].FirstChild.Attributes[0].Value
            };

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

            return retMaint;
        }

        /// <summary>
        /// Generates an embed for the requested news item
        /// </summary>
        /// <param name="tId">the news item to be retrieve</param>
        /// <param name="type">when using s = 1, the type of news to be retrieved</param>
        public EmbedBuilder generateEmbed(int tId, newsType type = newsType.Notice)
        {
            EmbedBuilder newsEmbed = new EmbedBuilder();
            switch (type)
            {
                case newsType.Topic:
                    topic retTop = getTopic(tId);
                    newsEmbed.Title = retTop.title;
                    newsEmbed.ImageUrl = retTop.image;
                    newsEmbed.Description = retTop.desc;
                    newsEmbed.Url = retTop.link;
                    //newsEmbed.Timestamp = retTop.time;
                    newsEmbed.WithAuthor("Topic", _topicImg);
                    newsEmbed.WithColor(Color.DarkOrange);
                    return newsEmbed;
                case newsType.Notice:
                case newsType.Status:
                case newsType.Update:
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
                    news retNews = getNews(tId, type);
                    newsEmbed.WithTitle(retNews.title);
                    newsEmbed.WithUrl(retNews.link);
                    //newsEmbed.Timestamp = retNews.time;
                    newsEmbed.WithAuthor(type.ToString(), iconUrl);
                    return newsEmbed;
                case newsType.Maintenance:
                    maintNews retMaint = getMaint(tId);
                    newsEmbed.Title = retMaint.title;
                    //newsEmbed.Description = retMaint.desc;
                    newsEmbed.Url = retMaint.link;
                    //newsEmbed.Timestamp = retMaint.time;
                    newsEmbed.AddField("Start", retMaint.start, true);
                    newsEmbed.AddField("End", retMaint.end, true);
                    newsEmbed.WithAuthor("Maintenance", _maintImg);
                    newsEmbed.WithColor(Color.Gold);
                    return newsEmbed;
                default:
                    throw new ArgumentException("Invalid News Type integer");
            }
        }
    }
}
