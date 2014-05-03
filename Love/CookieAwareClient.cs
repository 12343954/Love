using Microsoft.Phone.Shell;
using System;
using System.Net;

namespace Love
{
    class CookieAwareClient : WebClient
    {
        private CookieContainer m_container = new CookieContainer();


        [System.Security.SecuritySafeCritical]
        public CookieAwareClient()
            : base()
        {
            m_container = App.g_CookieContainer;
        }


        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = m_container;
                request.Headers["User-Agent"] = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
                request.Headers["Accept-Encoding"] = "gzip,deflate,sdche";
                request.Headers["Accept-Language"] = "zh-CN,zh;q=0.8";
                request.Headers["Referer"] = "http://love.163.com/home";
                request.Headers["Origin"] = "http://love.163.com";
                request.Headers["Host"] = "love.163.com";
                request.Method = "POST";
            }
            return request;
        }
        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse response = base.GetWebResponse(request, result);

            if (response is HttpWebResponse)
            {
                // Perform any custom actions with the response ...
                response.Headers["Cache-Control"] = "no-cache";
                response.Headers["Content-Language"] = "zh-CN";
                response.Headers["Content-Type"] = "application/json; charset=UTF-8";
                response.Headers["Pragma"] = "No-cache";

            }
            return response;

        }
    }
}

/*
CookieAwareClient cookieClient = new CookieAwareClient;
 
cookieClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(cookieAttempt_DownloadStringCompleted);

cookieClient.DownloadStringAsync(new Uri(“http://yourlogin.com/login?username&pass));
 */
