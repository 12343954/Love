using System;
using System.IO;
using System.Net;
using System.Text;

namespace Love.Resources
{
    public class Http
    {
        public delegate void HandleResult(string result);
        private HandleResult handle;
        string _postData = string.Empty;

        public void StartRequest(string Url, string method, bool isJSON, CookieContainer cookie, string PostData, HandleResult handle)
        {
            if (string.IsNullOrEmpty(method)) method = "GET";
            this._postData = PostData;

            this.handle = handle;
            var webRequest = (HttpWebRequest)WebRequest.Create(Url);
            if (isJSON)
            {
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                webRequest.Headers["X-Requested-With"] = "XMLHttpRequest";
            }
            if (cookie != null)
                webRequest.CookieContainer = cookie;
            webRequest.Method = method;
            try
            {
                webRequest.BeginGetResponse(new AsyncCallback(HandleResponse), webRequest);
            }
            catch
            {
            }
        }

        public void HandleResponse(IAsyncResult asyncResult)
        {
            HttpWebRequest httpRequest = null;
            HttpWebResponse httpResponse = null;
            string result = string.Empty;
            try
            {
                httpRequest = (HttpWebRequest)asyncResult.AsyncState;
                httpResponse = (HttpWebResponse)httpRequest.EndGetResponse(asyncResult);
                if (!string.IsNullOrEmpty(_postData))
                {
                    Stream postStream = httpRequest.EndGetRequestStream(asyncResult);
                    byte[] byteArray = Encoding.UTF8.GetBytes(_postData);
                    postStream.Write(byteArray, 0, byteArray.Length);
                    postStream.Close();
                }

                using (var reader = new StreamReader(httpResponse.GetResponseStream(), Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                    reader.Close();
                }
            }
            catch
            {

            }
            finally
            {
                if (httpRequest != null) httpRequest.Abort();
                if (httpResponse != null) httpResponse.Close();
            }
            handle(result);
        }
    }
}
