using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using HtmlAgilityPack;

namespace HavocBot
{
    /// <summary>
    /// 
    /// </summary>
    public enum newsType
    {
        /// <summary>
        /// 
        /// </summary>
        Notice,
        /// <summary>
        /// 
        /// </summary>
        Update,
        /// <summary>
        /// 
        /// </summary>
        Status,
    }

    /// <summary>
    /// 
    /// </summary>
    public struct topic
    {
        /// <summary>
        /// 
        /// </summary>
        public string title;
        /// <summary>
        /// 
        /// </summary>
        public string image;
        /// <summary>
        /// 
        /// </summary>
        public string desc;
        /// <summary>
        /// 
        /// </summary>
        public string link;
        /// <summary>
        /// 
        /// </summary>
        public string id;
    }
    /// <summary>
    /// 
    /// </summary>
    public struct news
    {
        /// <summary>
        /// 
        /// </summary>
        public string title;
        /// <summary>
        /// 
        /// </summary>
        public string link;
        /// <summary>
        /// 
        /// </summary>
        public string id;
    }
    /// <summary>
    /// 
    /// </summary>
    public struct maintNews
    {
        /// <summary>
        /// 
        /// </summary>
        public string title;
        /// <summary>
        /// 
        /// </summary>
        public string desc;
        /// <summary>
        /// 
        /// </summary>
        public string link;
        /// <summary>
        /// 
        /// </summary>
        public string id;
        /// <summary>
        /// 
        /// </summary>
        public DateTime start;
        /// <summary>
        /// 
        /// </summary>
        public DateTime end;
    }
    /// <summary>
    /// 
    /// </summary>
    public class ffxivNews
    {
        /// <summary>
        /// 
        /// </summary>
        public const string siteRoot = "https://na.finalfantasyxiv.com";
        /// <summary>
        /// 
        /// </summary>
        public const string topicsRoot = "/lodestone/topics";
        /// <summary>
        /// 
        /// </summary>
        public const string latestRoot = "/lodestone/news/";
        /// <summary>
        /// 
        /// </summary>
        public const string noticesRoot = "/lodestone/news/category/1";
        /// <summary>
        /// 
        /// </summary>
        public const string maintRoot = "/lodestone/news/category/2";
        /// <summary>
        /// 
        /// </summary>
        public const string updatesRoot = "/lodestone/news/category/3";
        /// <summary>
        /// 
        /// </summary>
        public const string statusRoot = "/lodestone/news/category/4";
        readonly HtmlWeb _web;
        
        /// <summary>
        /// 
        /// </summary>
        public ffxivNews()
        {
            _web = new HtmlWeb();
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tId"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="tId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
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
            //Notices, updates, and statuses

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
        /// 
        /// </summary>
        /// <param name="tId"></param>
        /// <returns></returns>
        public maintNews getMaint(int tId)
        {
            //maintenance
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
    }
}
