using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    public class Post : VotableThing
    {
        private const string CommentUrl = "/api/comment";
        private const string RemoveUrl = "/api/remove";
        private const string DelUrl = "/api/del";
        private const string GetCommentsUrl = "/comments/{0}.json";
        private const string ApproveUrl = "/api/approve";
        private const string EditUserTextUrl = "/api/editusertext";
        private const string HideUrl = "/api/hide";
        private const string UnhideUrl = "/api/unhide";
        private const string SetFlairUrl = "/api/flair";
        private const string MarkNSFWUrl = "/api/marknsfw";
        private const string UnmarkNSFWUrl = "/api/unmarknsfw";
        private const string ContestModeUrl = "/api/set_contest_mode";

        public Post Init(Reddit reddit, JToken post, IWebAgent webAgent)
        {
            CommonInit(reddit, post, webAgent);
            JsonConvert.PopulateObject(post["data"].ToString(), this, reddit.JsonSerializerSettings);
            return this;
        }

#if (_HAS_ASYNC_)
        public async Task<Post> InitAsync(Reddit reddit, JToken post, IWebAgent webAgent)
        {
            CommonInit(reddit, post, webAgent);
            await Task.Factory.StartNew(() => JsonConvert.PopulateObject(post["data"].ToString(), this, reddit.JsonSerializerSettings));
            return this;
        }
#endif

        private void CommonInit(Reddit reddit, JToken post, IWebAgent webAgent)
        {
            base.Init(reddit, webAgent, post);
            Reddit = reddit;
            WebAgent = webAgent;
        }

        [JsonProperty("author")]
        public string AuthorName { get; set; }

        [JsonIgnore]
        public RedditUser Author
        {
            get
            {
                return Reddit.GetUser(AuthorName);
            }
        }

        public Comment[] Comments
        {
            get
            {
                return ListComments().ToArray();
            }
        }

        [JsonProperty("approved_by")]
        public string ApprovedBy { get; set; }

        [JsonProperty("author_flair_css_class")]
        public string AuthorFlairCssClass { get; set; }

        [JsonProperty("author_flair_text")]
        public string AuthorFlairText { get; set; }

        [JsonProperty("banned_by")]
        public string BannedBy { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        // Reddit returns edited as "false" if it never has been touched, and a timestamp value if it has; Json will parse the false as a Jan 1, 1970 date.
        [JsonProperty("edited")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime Edited { get; set; }

        [JsonProperty("is_self")]
        public bool IsSelfPost { get; set; }

        [JsonProperty("link_flair_css_class")]
        public string LinkFlairCssClass { get; set; }

        [JsonProperty("link_flair_text")]
        public string LinkFlairText { get; set; }

        [JsonProperty("num_comments")]
        public int CommentCount { get; set; }

        [JsonProperty("over_18")]
        public bool NSFW { get; set; }

        [JsonProperty("permalink")]
        [JsonConverter(typeof(UrlParser))]
        public Uri Permalink { get; set; }

// This is handled by the base class VotableThing
//      [JsonProperty("score")]
//      public int Score { get; set; }

        [JsonProperty("selftext")]
        public string SelfText { get; set; }

        [JsonProperty("selftext_html")]
        public string SelfTextHtml { get; set; }

        [JsonProperty("thumbnail")]
        [JsonConverter(typeof(UrlParser))]
        public Uri Thumbnail { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("subreddit")]
        public string SubredditName { get; set; }

        [JsonIgnore]
        public Subreddit Subreddit
        {
            get
            {
                return Reddit.GetSubreddit("/r/" + SubredditName);
            }
        }

        [JsonProperty("url")]
        [JsonConverter(typeof(UrlParser))]
        public Uri Url { get; set; }

        [JsonProperty("num_reports")]
        public int? Reports { get; set; }

        public Comment Comment(string message)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = WebAgent.CreatePost(CommentUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
                {
                    text = message,
                    thing_id = FullName,
                    uh = Reddit.User.Modhash,
                    api_type = "json"
                });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JObject.Parse(data);
            if (json["json"]["ratelimit"] != null)
                throw new RateLimitException(TimeSpan.FromSeconds(json["json"]["ratelimit"].ValueOrDefault<double>()));
            return new Comment().Init(Reddit, json["json"]["data"]["things"][0], WebAgent, this);
        }

        private string SimpleAction(string endpoint)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = WebAgent.CreatePost(endpoint);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                id = FullName,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            return data;
        }

        private string SimpleActionToggle(string endpoint, bool value)
        {
            if (Reddit.User == null)
                throw new AuthenticationException("No user logged in.");
            var request = WebAgent.CreatePost(endpoint);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                id = FullName,
                state = value,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            return data;
        }

        public void Approve()
        {
            var data = SimpleAction(ApproveUrl);
        }

        public void Remove()
        {
            RemoveImpl(false);
        }

        public void RemoveSpam()
        {
            RemoveImpl(true);
        }

        private void RemoveImpl(bool spam)
        {
            var request = WebAgent.CreatePost(RemoveUrl);
            var stream = request.GetRequestStream();
            WebAgent.WritePostBody(stream, new
            {
                id = FullName,
                spam = spam,
                uh = Reddit.User.Modhash
            });
            stream.Close();
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
        }

        public void Del()
        {
            var data = SimpleAction(DelUrl);
        }

        public void Hide()
        {
            var data = SimpleAction(HideUrl);
        }

        public void Unhide()
        {
            var data = SimpleAction(UnhideUrl);
        }

        public void MarkNSFW()
        {
            var data = SimpleAction(MarkNSFWUrl);
        }

        public void UnmarkNSFW()
        {
            var data = SimpleAction(UnmarkNSFWUrl);
        }

        public void ContestMode(bool state)
        {
            var data = SimpleActionToggle(ContestModeUrl, state);
        }

        #region Obsolete Getter Methods

        [Obsolete("Use Comments property instead")]
        public Comment[] GetComments()
        {
            return Comments;
        }

        #endregion Obsolete Getter Methods

        /// <summary>
        /// Replaces the text in this post with the input text.
        /// </summary>
        /// <param name="newText">The text to replace the post's contents</param>
        public void EditText(string newText)
        {
            if (Reddit.User == null)
                throw new Exception("No user logged in.");
            if (!IsSelfPost)
                throw new Exception("Submission to edit is not a self-post.");

            var request = WebAgent.CreatePost(EditUserTextUrl);
            WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                text = newText,
                thing_id = FullName,
                uh = Reddit.User.Modhash
            });
            var response = request.GetResponse();
            var result = WebAgent.GetResponseString(response.GetResponseStream());
            JToken json = JToken.Parse(result);
            if (json["json"].ToString().Contains("\"errors\": []"))
                SelfText = newText;
            else
                throw new Exception("Error editing text.");
        }
        public void Update()
        {
            JToken post = Reddit.GetToken(this.Url);
            JsonConvert.PopulateObject(post["data"].ToString(), this, Reddit.JsonSerializerSettings);
        }

        public void SetFlair(string flairText, string flairClass)
        {
            if (Reddit.User == null)
                throw new Exception("No user logged in.");

            var request = WebAgent.CreatePost(SetFlairUrl);
            WebAgent.WritePostBody(request.GetRequestStream(), new
            {
                api_type = "json",
                css_class = flairClass,
                link = FullName,
                name = Reddit.User.Name,
                text = flairText,
                uh = Reddit.User.Modhash
            });
            var response = request.GetResponse();
            var result = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JToken.Parse(result);
            LinkFlairText = flairText;
        }

        /// <summary>
        /// Lists the comments and initializes More if found.
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public List<Comment> ListComments()
        {
           return ListComments(null);
        }

        /// <summary>
        /// Lists the comments and initializes More if found.
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public List<Comment> ListComments(int? limit)
        {
            var url = string.Format(GetCommentsUrl, Id);

            if (limit.HasValue)
            {
                var query = HttpUtility.ParseQueryString(string.Empty);
                query.Add("limit", limit.Value.ToString());
                url = string.Format("{0}?{1}", url, query);
            }

            var request = WebAgent.CreateGet(url);
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JArray.Parse(data);
            var postJson = json.Last()["data"]["children"];

            var comments = new List<Comment>();
            foreach (var comment in postJson)
            {
                Comment newComment = new Things.Comment().Init(Reddit, comment, WebAgent, this);


                if (newComment.Kind != "more")
                    comments.Add(newComment);
            }

            return comments;
        }

        /// <summary>
        /// Enumerate more comments.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Comment> EnumerateComments()
        {
            var url = string.Format(GetCommentsUrl, Id);
            var request = WebAgent.CreateGet(url);
            var response = request.GetResponse();
            var data = WebAgent.GetResponseString(response.GetResponseStream());
            var json = JArray.Parse(data);
            var postJson = json.Last()["data"]["children"];
            More moreComments = null;
            foreach (var comment in postJson)
            {
                Comment newComment = new Comment().Init(Reddit, comment, WebAgent, this);
                if (newComment.Kind == "more")
                {
                    moreComments = new More().Init(Reddit, comment, WebAgent);
                }
                else
                {
                    yield return newComment;
                }
            }


            if (moreComments != null)
            {
                IEnumerator<Thing> things = moreComments.Things().GetEnumerator();
                things.MoveNext();
                Thing currentThing = null;
                while (currentThing != things.Current)
                {
                    currentThing = things.Current;
                    if (things.Current is Comment)
                    {
                        Comment next = ((Comment)things.Current).PopulateComments(things);
                        yield return next;
                    }
                    if (things.Current is More)
                    {
                        More more = (More)things.Current;
                        if (more.ParentId != FullName) break;
                        things = more.Things().GetEnumerator();
                        things.MoveNext();
                    }
                }
            }
        }
    }
}
