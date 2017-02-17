using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace DataOne.BitUp
{
    public class Request
    {
        private string _base64Credentials;

        public Request(string user, string pass)
        {
            byte[] credentials = Encoding.UTF8.GetBytes(user + ":" + pass);
            _base64Credentials = Convert.ToBase64String(credentials);
        }

        public JObject GetResponse(string url)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = WebRequestMethods.Http.Get;
            request.Headers.Add("Authorization", "Basic " + _base64Credentials);

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            string result;
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                result = reader.ReadToEnd();
            }
            response.Close();

            return JObject.Parse(result);
        }

        public Stream GetResponseFileStream(string url)
        {
            HttpWebResponse response;
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = WebRequestMethods.Http.Post;
            request.Headers.Add("Authorization", "Basic " + _base64Credentials);

            response = request.GetResponse() as HttpWebResponse;
            Stream responseStream = response.GetResponseStream();
            return responseStream;
        }
    }
}
