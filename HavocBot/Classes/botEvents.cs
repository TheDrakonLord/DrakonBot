/* Title: botEvents.cs
 * Author: Neal Jamieson
 * Version: 0.0.0.0
 * 
 * Description:
 *     This class provides the means to store events in a organized manner.
 * 
 * Dependencies:
 * 
 * Dependent on:
 *      
 * References:
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace HavocBot
{
    /// <summary>
    /// This class provides a data structure to store and load events
    /// </summary>
    public class botEvents
    {
        //storage id
        private Guid _sid = Guid.NewGuid();
        //Name
        private string _name;
        //Types
        /// <summary>
        /// En umeration for the various types of events.
        /// </summary>
        /// <remarks>Public to enable use of enum as a property</remarks>
        public enum types
        {
            /// <summary>The Default case. enables the use of OtherTypes poperty which accepts custom user input</summary>
            other,
            /// <summary>Events based around FFXIV dungeons</summary>
            dungeon,
            /// <summary>Events based around FFXIV trials</summary>
            trial,
            /// <summary>Events based around FFXIV raids</summary>
            raid,
            /// <summary>EVents based around FFXIV alliance raids</summary>
            alliance,
            /// <summary>Events based around FFXIV extreme trials</summary>
            extreme,
            /// <summary>Events based around FFXIV savage raids</summary>
            savage,
            /// <summary>Events based around FFXIV ultimate content</summary>
            ultimate,
            /// <summary>Events based around FFXIV treasure maps</summary>
            maps,
            /// <summary>Events based around FFXIV hunts</summary>
            hunts,
            /// <summary>Events based around FFXIV Deep Dungeons</summary>
            deepDungeon,
            /// <summary>Events based around FFXIV FATES</summary>
            fates,
            /// <summary>Events based around FFXIV GATES</summary>
            gates,
            /// <summary>Events involving the Jackbox Party Pack by Jackbox Games</summary>
            jackbox,
            /// <summary>Events involving movies, tv shows, or anime</summary>
            movie
        };
        // Event type
        private types _type = types.other;
        // custom string for "other" type events
        private string _otherType = "Other";
        //load image paths
        private IEnumerable<XElement> _imageRetrieve;
        // file path of the image for the current event type
        private string _typeImagePath;
        //Description
        private string _description = "N/A";
        //Start Date
        private DateTime _startDate;
        //end date
        private DateTime _endDate;
        //reminder
        private int _reminder = 0;
        //repeat
        private enum repeatTypes
        {
            none,
            hourly,
            daily,
            weekly,
            monthly,
            quarterly,
            semiannually,
            annually,
            biannually
        }
        private repeatTypes _repeat = repeatTypes.none;
        //mentions
        private enum mentionOptions
        {
            none,
            rsvp,
            fcMembers,
            Everyone
        }
        private mentionOptions _mentions = mentionOptions.none;
        //rsvps
        private List<string> _rsvps = new List<string>();
        private List<string> _rsvpID = new List<string>();
        //author
        private string _author = "HavocBot";
        //author Avatar
        private string _authorURL = null;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="name">The user specified name for the event. Case sensitive. Must be unique</param>
        /// <param name="start">the start date for the event. Must be parsed into datetime format</param>
        /// <param name="end">the end date for the event. Must be parsed into datetime format</param>
        /// <param name="cStore">Passes the current command storage tree</param>
        public botEvents(string name, DateTime start, DateTime end, XElement cStore)
        {
            _name = name;
            _startDate = start;
            _endDate = end;

            _imageRetrieve = from el in cStore.Elements("images")
                            select el;

            _imageRetrieve = from el in _imageRetrieve.Elements("eventType")
                            select el;

        }

        /// <summary>
        /// read only property for storage id
        /// </summary>
        public Guid storageID => _sid;

        /// <summary>
        /// property for name
        /// </summary>
        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Property for type.
        /// </summary>
        /// <remarks>Accepts a string value for setter</remarks>
        /// <returns>a string representation of the type</returns>
        public string type
        {
            get
            {
                switch (_type)
                {
                    case types.other:
                        return _otherType;
                    default:
                        return _type.ToString();
                }
            }

            set
            {
                switch (value)
                {
                    case "dungeon":
                        _type = types.dungeon;
                        _typeImagePath = (string)
                            (from el in _imageRetrieve.Descendants("dungeon")
                             select el).First();
                        break;
                    case "trial":
                        _type = types.trial;
                        _typeImagePath = (string)
                            (from el in _imageRetrieve.Descendants("trial")
                             select el).First();
                        break;
                    case "raid":
                        _type = types.raid;
                        _typeImagePath = (string)
                            (from el in _imageRetrieve.Descendants("raid")
                             select el).First();
                        break;
                    case "alliance":
                        _type = types.alliance;
                        _typeImagePath = (string)
                            (from el in _imageRetrieve.Descendants("HighEnd")
                             select el).First();
                        break;
                    case "extreme":
                        _type = types.extreme;
                        _typeImagePath = (string)
                            (from el in _imageRetrieve.Descendants("HighEnd")
                             select el).First();
                        break;
                    case "savage":
                        _type = types.savage;
                        _typeImagePath = (string)
                            (from el in _imageRetrieve.Descendants("HighEnd")
                             select el).First();
                        break;
                    case "ultimate":
                        _type = types.ultimate;
                        _typeImagePath = (string)
                            (from el in _imageRetrieve.Descendants("HighEnd")
                             select el).First();
                        break;
                    case "maps":
                        _type = types.maps;
                        _typeImagePath = (string)
                            (from el in _imageRetrieve.Descendants("maps")
                             select el).First();
                        break;
                    case "hunts":
                        _type = types.hunts;
                        _typeImagePath = (string)
                            (from el in _imageRetrieve.Descendants("hunts")
                             select el).First();
                        break;
                    case "deepDungeon":
                        _type = types.deepDungeon;
                        _typeImagePath = (string)
                            (from el in _imageRetrieve.Descendants("potd")
                             select el).First();
                        break;
                    case "fates":
                        _type = types.fates;
                        _typeImagePath = (string)
                            (from el in _imageRetrieve.Descendants("fates")
                             select el).First();
                        break;
                    case "gates":
                        _type = types.gates;
                        _typeImagePath = (string)
                            (from el in _imageRetrieve.Descendants("gates")
                             select el).First();
                        break;
                    case "jackbox":
                        _type = types.jackbox;
                        _typeImagePath = (string)
                            (from el in _imageRetrieve.Descendants("jackbox")
                             select el).First();
                        break;
                    case "movie":
                        _type = types.movie;
                        _typeImagePath = (string)
                            (from el in _imageRetrieve.Descendants("movie")
                             select el).First();
                        break;
                    default:
                        _type = types.other;
                        _otherType = value;
                        _typeImagePath = (string)
                            (from el in _imageRetrieve.Descendants("other")
                             select el).First();
                        break;
                }
            }
        }

        /// <summary>
        /// Property for types
        /// </summary>
        /// <remarks>Accepts types enums</remarks>
        /// <returns>Returns types enums</returns>
        public types TypeEnum
        {
            get { return _type; }
            set { _type = value; }
        }

        /// <summary>
        /// property for description
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// property for start date
        /// </summary>
        public DateTime StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        }

        /// <summary>
        /// property for end date
        /// </summary>
        public DateTime EndDate
        {
            get { return _endDate; }
            set { _endDate = value; }
        }

        /// <summary>
        /// property for getting and setting reminder time in minutes
        /// </summary>
        public int ReminderMinutes
        {
            get { return _reminder; }
            set { _reminder = value; }
        }

        /// <summary>
        /// property for getting and setting reminder time converted to/from hours
        /// </summary>
        public double ReminderHours
        {
            get { return _reminder / 60; }
            set { _reminder = (int)value * 60; }
        }

        /// <summary>
        /// property for repeat
        /// </summary>
        public string Repeat
        {
            get { return _repeat.ToString(); }
            set
            {
                switch (value)
                {
                    case "hourly":
                        _repeat = repeatTypes.hourly;
                        break;
                    case "daily":
                        _repeat = repeatTypes.daily;
                        break;
                    case "weekly":
                        _repeat = repeatTypes.weekly;
                        break;
                    case "monthly":
                        _repeat = repeatTypes.monthly;
                        break;
                    case "quarterly":
                        _repeat = repeatTypes.quarterly;
                        break;
                    case "semiannually":
                        _repeat = repeatTypes.semiannually;
                        break;
                    case "annually":
                        _repeat = repeatTypes.annually;
                        break;
                    case "biannually":
                        _repeat = repeatTypes.biannually;
                        break;
                    default:
                        _repeat = repeatTypes.none;
                        break;
                }
            }
        }

        /// <summary>
        /// property for mentions
        /// </summary>
        public string Mentions
        {
            get
            {
                switch (_mentions)
                {
                    case mentionOptions.none:
                        return "N/A";
                    case mentionOptions.rsvp:
                        return "rsvp";
                    case mentionOptions.fcMembers:
                        return "<@&473181362879332382>";
                    case mentionOptions.Everyone:
                        return "<@&236955200311525377>";
                    default:
                        return "N/A";
                }
            }
            set
            {
                switch (value.ToLower())
                {
                    case "none":
                    case "N/A":
                        _mentions = mentionOptions.none;
                        break;
                    case "everyone":
                        _mentions = mentionOptions.Everyone;
                        break;
                    case "fc":
                    case "fc members":
                    case "fcmembers":
                        _mentions = mentionOptions.fcMembers;
                        break;
                    case "rsvp":
                    case "rsvps":
                        _mentions = mentionOptions.rsvp;
                        break;
                    default:
                        _mentions = mentionOptions.none;
                        break;
                }
            }
        }

        /// <summary>
        /// Property for assigning or retrieving the entire list of RSVPs
        /// </summary>
        public List<string> RSVPs
        {
            get { return _rsvps; }
            set { _rsvps = value; }
        }

        /// <summary>
        /// Generates a string listing all RSVPs
        /// </summary>
        /// <returns>A list of all RSVPs separated by commas</returns>
        public string allRSVPs()
        {
            string strOutput = "N/A";

            if (_rsvps.Any())
            {
                strOutput = String.Join(", ", _rsvps.ToArray());
            }

            return strOutput;
        }

        /// <summary>
        /// Generates a string listing all RSVPs without extra spaces
        /// </summary>
        /// <returns>A list of all RSVPs separated by commas</returns>
        public string saveRSVPs()
        {
            string strOutput = "N/A";

            if (_rsvps.Any())
            {
                strOutput = String.Join(",", _rsvps.ToArray());
            }

            return strOutput;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string allRSVPIDs()
        {
            string strOutput = "N/A";

            if (_rsvpID.Any())
            {
                strOutput = String.Join(", ", _rsvpID.ToArray());
            }

            return strOutput;
        }

        /// <summary>
        /// Takes a string with all RSVPs and converts them to a list
        /// </summary>
        /// <param name="rsvpList">the string list of all RSVPs</param>
        /// <param name="idList">the ids of the users</param>
        public void importRSVPs(string rsvpList, string idList)
        {
            if (!rsvpList.Equals("N/A"))
            {
                string tempString = rsvpList;
                _rsvps = tempString.Split(',').ToList();
                tempString = "";
                tempString = idList;
                tempString = tempString.Replace(" ", "");
                _rsvpID = tempString.Split(',').ToList();
            }
        }

        /// <summary>
        /// Adds a single RSVP to the list
        /// </summary>
        /// <param name="nickname">The nickname of the user to be added as a RSVP</param>
        /// <param name="id">the id of the user</param>
        public void addRSVP(string nickname, string id)
        {
            _rsvps.Add(nickname);
            _rsvpID.Add(id);
        }

        /// <summary>
        /// Removes a single RSVP from the list
        /// </summary>
        /// <param name="nickname">The nickname of the user to be removed as a RSVP</param>
        /// <param name="id">the id of the user</param>
        public void removeRSVP(string nickname, string id)
        {
            _rsvps.Remove(nickname);
            _rsvpID.Remove(id);
        }

        /// <summary>
        /// sets or gets the image path for the type image icons
        /// </summary>
        public string TypeImagePath
        {
            get { return _typeImagePath; }
            set { _typeImagePath = value; }
        }

        /// <summary>
        /// Sets or gets the author of the event
        /// </summary>
        public string Author
        {
            get { return _author; }
            set { _author = value; }
        }

        /// <summary>
        /// sets or gets the URL of the author's avatar
        /// </summary>
        public string AuthorURL
        {
            get { return _authorURL; }
            set { _authorURL = value; }
        }

        /// <summary>
        /// advances the start and end date of the event based on the repeat
        /// </summary>
        public void repeatDate()
        {
            switch (_repeat)
            {
                case repeatTypes.none:
                    break;
                case repeatTypes.hourly:
                    _startDate = _startDate.AddHours(1);
                    _endDate = _endDate.AddHours(1);
                    break;
                case repeatTypes.daily:
                    _startDate = _startDate.AddDays(1);
                    _endDate = _endDate.AddDays(1);
                    break;
                case repeatTypes.weekly:
                    _startDate = _startDate.AddDays(7);
                    _endDate = _endDate.AddDays(7);
                    break;
                case repeatTypes.monthly:
                    _startDate = _startDate.AddMonths(1);
                    _endDate = _endDate.AddMonths(1);
                    break;
                case repeatTypes.quarterly:
                    _startDate = _startDate.AddMonths(3);
                    _endDate = _endDate.AddMonths(3);
                    break;
                case repeatTypes.semiannually:
                    _startDate = _startDate.AddMonths(6);
                    _endDate = _endDate.AddMonths(6);
                    break;
                case repeatTypes.annually:
                    _startDate = _startDate.AddYears(1);
                    _endDate = _endDate.AddYears(1);
                    break;
                case repeatTypes.biannually:
                    _startDate = _startDate.AddYears(2);
                    _endDate = _endDate.AddYears(2);
                    break;
                default:
                    break;
            }
        }
    }
}
