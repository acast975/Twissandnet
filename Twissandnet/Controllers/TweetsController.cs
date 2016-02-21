using Cassandra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Twissandnet.Models;

namespace Twissandnet.Controllers
{
    public class TweetsController : ApiController
    {
        ISession session = Twissandnet.Cassandra.SessionManager.GetSession();
        static PreparedStatement
                    insertTweet = null,
                    updateUserTweets = null,
                    getUserTweets = null,
                    getTweets = null,
                    getHashTweets = null,
                    getAllTweets = null;
        /// <summary>
        /// Returns tweets for current user
        /// </summary>
        /// <returns></returns>
        public async Task<Response> GetUserFeed()
        {
            if (!SessionUtil.IsLoggedIn())
            {
                return new Response() { status = Response.StatusCode.UserNotLoggedIn };
            }

            Response friends = await AccountsController.GetUserFriends(SessionUtil.GetAccount().username);
            ((List<string>)friends.result).Add(SessionUtil.GetAccount().username);
            List<Guid> tweetIds = new List<Guid>();
            foreach (var friend in (List<string>)friends.result)
            {
                tweetIds.AddRange(await GetUserTweetIds(friend));
            }

            return new Response()
            {
                status = Response.StatusCode.OK,
                result = await GetTweetsById(tweetIds)
            };
        }
        /// <summary>
        /// Returns all tweets 
        /// </summary>
        /// <returns></returns>
        public async Task<Response> GetPublicTweets()
        {
            List<Tweet> tweets = new List<Tweet>();

            if (getAllTweets == null)
            {
                getAllTweets = await session.PrepareAsync("select username, originaltext, timestamp from tweets");
            }
            return new Response()
            {
                status = Response.StatusCode.OK,
                result = await LoadTweets(getAllTweets)
            };
        }

        /// <summary>
        /// Gets tweets posted by given user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<Response> GetUserTweets(string user)
        {
            List<Tweet> tweets = new List<Tweet>();

            List<Guid> tweetIds = await GetUserTweetIds(user);
            tweets = await GetTweetsById(tweetIds);
            return new Response()
            {
                status = Response.StatusCode.OK,
                result = tweets
            };
        }

        private async Task<List<Guid>> GetUserTweetIds(string user)
        {
            List<Guid> tweetIds = new List<Guid>();
            if (getUserTweets == null)
            {
                getUserTweets = await session.PrepareAsync("select tweets from user_tweets where username = ?");
            }
            var stmt = getUserTweets.Bind(user);
            RowSet results = await session.ExecuteAsync(stmt);
            Row row = results.FirstOrDefault();
            if (row != null)
            {
                tweetIds = ((IEnumerable<Guid>)row["tweets"]).ToList();
            }
            return tweetIds;
        }

        public async Task<Response> PostTweet(Tweet tweet)
        {
            if (!SessionUtil.IsLoggedIn())
            {
                return new Response() { status = Response.StatusCode.UserNotLoggedIn };
            }
            if (insertTweet == null)
            {
                insertTweet = await session.PrepareAsync("insert into tweets (id, username, originaltext, timestamp, hashtags) values (?, ?, ?, ?, ?)" +
                    "USING TIMESTAMP ?");
            }
            Guid tweetId = Guid.NewGuid();
            long timestamp = GetTimestamp();
            var stmt = insertTweet.Bind(tweetId, SessionUtil.GetAccount().username, tweet.originalText, timestamp, tweet.hashtags);
            await session.ExecuteAsync(stmt);

            if (updateUserTweets == null)
            {
                updateUserTweets = await session.PrepareAsync("UPDATE user_tweets SET tweets = tweets + ? WHERE username = ?");
            }

            stmt = updateUserTweets.Bind(new List<Guid>() { tweetId }, SessionUtil.GetAccount().username);
            await session.ExecuteAsync(stmt);
            return new Response() { status = Response.StatusCode.OK };
        }
        /// <summary>
        /// Gets all tweets containing hashtag
        /// </summary>
        /// <param name="hashtag"></param>
        /// <returns></returns>
        public async Task<Response> GetHashtagTweets(string hashtag)
        {
            List<Tweet> tweets = new List<Tweet>();
            if (getHashTweets == null)
            {
                getHashTweets = await session.PrepareAsync("select username, originaltext, timestamp from tweets where hashtags contains ?");
            }
            return new Response()
            {
                status = Response.StatusCode.OK,
                result = await LoadTweets(getHashTweets, hashtag)
            };
        }


        private async Task<List<Tweet>> GetTweetsById(List<Guid> ids)
        {
            if (ids.Count == 0)
                return new List<Tweet>();

            if (getTweets == null)
            {
                getTweets = await session.PrepareAsync("select username, originaltext, timestamp from tweets where id in ?");
            }
            return await LoadTweets(getTweets, ids);
        }

        private async Task<List<Tweet>> LoadTweets(PreparedStatement prepStmt, params object[] values)
        {
            var stmt = prepStmt.Bind(values);
            var result = await session.ExecuteAsync(stmt);
            var tweets = new List<Tweet>();
            foreach (Row row in result)
            {
                Tweet t = new Tweet()
                {
                    username = row["username"].ToString(),
                    originalText = row["originaltext"].ToString(),
                    timestamp = GetTimestamp(DateTime.Parse(row["timestamp"].ToString()))
                };
                tweets.Add(t);
            }

            return tweets;
        }

        private long GetTimestamp(DateTime dt)
        {
            long ts = (long)dt.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

            return ts;
        }
        private long GetTimestamp()
        {
            return GetTimestamp(DateTime.UtcNow);
        }
    }
}
