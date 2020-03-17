using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreTweet;
using CoreTweet.Streaming;

namespace HavocBot
{
    /// <summary>
    /// 
    /// </summary>
    public class ffxivTwitter
    {
        OAuth.OAuthSession _session;
        readonly Tokens _token;

        /// <summary>
        /// 
        /// </summary>
        public ffxivTwitter()
        {
            _session = OAuth.Authorize("key", "secret");
            _token = _session.GetTokens("pin");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task twitterStream()
        {
            foreach (var m in _token.Streaming.Filter(follow => "FFXIV_EN")
                    .OfType<StatusMessage>()
                    .Select(x => x.Status)
                    .Take(1))
                Console.WriteLine("about tea by {0}: {1}", m.User.ScreenName, m.Text);
        }
    }
}
