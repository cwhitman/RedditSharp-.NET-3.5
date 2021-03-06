using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace RedditSharp
{
    public interface IWebAgent
    {
        CookieContainer Cookies { get; set; }
        string AuthCookie { get; set; }
        string AccessToken { get; set; }
        bool UseProxy { get; set; }
        WebProxy Proxy { get; set; }
        HttpWebRequest InjectProxy(HttpWebRequest request);
        HttpWebRequest CreateRequest(string url, string method);
        HttpWebRequest CreateGet(string url);
        HttpWebRequest CreatePost(string url);
        HttpWebRequest CreatePut(string url);
        HttpWebRequest CreateDelete(string url);
        string GetResponseString(Stream stream);
        void WritePostBody(Stream stream, object data, params string[] additionalFields);
        JToken CreateAndExecuteRequest(string url);
        JToken ExecuteRequest(HttpWebRequest request);
    }
}
