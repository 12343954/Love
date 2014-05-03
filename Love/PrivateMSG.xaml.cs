using Love.Model;
using Love.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Love
{
    public partial class PrivateMSG : PhoneApplicationPage
    {
        string URL, UserID, UserName, strLeaveMSG, TrendID;//默认，私信类型

        int PageIndex_Message = 1;
        ObservableCollection<Message> MessageListCollection;

        public PrivateMSG()
        {
            InitializeComponent();
            this.Loaded += PrivateMSG_Loaded;
        }

        void PrivateMSG_Loaded(object sender, RoutedEventArgs e)
        {

            username.Text = UserName;
            URL = AppGlobalStatic.MessagesURL["私信"];
            strLeaveMSG = "";
            GetMessage();
        }

        #region GetMessage 获取消息
        public async void GetMessage()
        {
            await Task.Run(() =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    ApplicationBar.Mode = ApplicationBarMode.Minimized;

                    progressBar1.IsIndeterminate = true;
                    if (list_message.Items.Count > 0)
                    {
                        list_message.ScrollIntoView(list_message.Items[0]);
                        list_message.UpdateLayout();
                    }
                });

                //var url = AppGlobalStatic.MessagesURL["全部消息"];
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(URL);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                webRequest.Headers["X-Requested-With"] = "XMLHttpRequest";
                webRequest.CookieContainer = App.g_CookieContainer;

                webRequest.BeginGetRequestStream(new AsyncCallback(GetMessageStreamCallback), webRequest);
            });
        }
        private void GetMessageStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
            Stream postStream = webRequest.EndGetRequestStream(asynchronousResult);

            string postData = string.Format("withUserId={0}&pageNo={1}", UserID, PageIndex_Message);

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            postStream.Write(byteArray, 0, byteArray.Length);
            postStream.Close();

            webRequest.BeginGetResponse(new AsyncCallback(GetMessageCallback), webRequest);
        }
        private void GetMessageCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
                HttpWebResponse response;

                // End the get response operation
                response = (HttpWebResponse)webRequest.EndGetResponse(asynchronousResult);
                response.Headers[HttpRequestHeader.ContentType] = "application/json; charset=UTF-8";

                Stream streamResponse = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(streamResponse);
                var Response = streamReader.ReadToEnd();

                dynamic json = JsonConvert.DeserializeObject(Response);

                MessageListCollection = new ObservableCollection<Message>();

                foreach (var item in json["list"])
                {
                    var message = new Message();
                    message.ID = Convert.ToString(item["id"]);
                    message.RichContent = (string)item["richContent"];
                    message.Source = AppGlobalStatic.Source((string)item["source"]);

                    message.PrettyTime = AppGlobalStatic.DateStringFromNow(((string)item["prettyTime"]).Replace("今天", DateTime.Now.ToString("yyyy-MM-dd")));

                    message.User = new User();
                    message.User.ID = Convert.ToString(item["user"]["id"]);
                    message.User.NickName = Convert.ToString(item["user"]["nickName"]);
                    message.User.Sex = ("人妖,他,她").Split(',')[(int)item["user"]["sex"]];

                    message.ToUser = new User();
                    message.ToUser.Url = Convert.ToString(item["toUser"]["url"]);
                    message.ToUser.NickName = Convert.ToString(item["toUser"]["nickName"]);
                    message.ToUser.Sex = ("人妖,他,她").Split(',')[(int)item["toUser"]["sex"]];

                    message.RelatedData = item["relatedData"] as JObject;


                    message.Type = (string)item["type"];
                    if (message.Type.Equals("1"))
                    {
                        if (message.User.ID.Equals(App.MyLove["id"].ToString()))
                            message.RichContent = "我赞了" + message.ToUser.Sex + "发布的动态";
                        else
                            message.RichContent = message.User.Sex + " 赞了我发布的动态";
                    }
                    else
                    {
                        message.RichContent = AppGlobalStatic.ImgToEmotion(message.RichContent);
                    }
                    if (message.RelatedData["trend"] != null)
                    {
                        message.RichContent += "\n-    -    -    -    -    -    -    -   -\n" + AppGlobalStatic.ImgToEmotion(Convert.ToString(message.RelatedData["trend"]["richContent"]));
                    }

                    message.RichContent = AppGlobalStatic.ClearAhref(message.RichContent);

                    message.User.Url = Convert.ToString(item["user"]["url"]);
                    message.User.Age = Convert.ToString(item["user"]["age"]) + " " + Love.Resources.AppGlobalStatic.AgeToAnimal((string)item["user"]["age"]);

                    MessageListCollection.Add(message);
                }
                int pageCount = (int)json["page"]["pageCount"];
                int pageNo = (int)json["page"]["pageNo"];

                if (MessageListCollection.Count > 0)
                    if (pageCount != pageNo)
                        MessageListCollection.Add(new Message() { IsMoreButtonTrend = true });

                PageIndex_Message++;
                Dispatcher.BeginInvoke(() =>
                {
                    list_message.ItemsSource = MessageListCollection;
                });

                streamResponse.Close();
                streamReader.Close();
                response.Close();

            }
            catch (WebException e)
            {

            }

            Dispatcher.BeginInvoke(() =>
            {
                progressBar1.IsIndeterminate = false;
                ApplicationBar.Mode = ApplicationBarMode.Default;
            });
        }
        #endregion

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationContext.QueryString.TryGetValue("userid", out UserID);
            NavigationContext.QueryString.TryGetValue("username", out UserName);
            NavigationContext.QueryString.TryGetValue("trendId", out TrendID);
        }

        private async void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            var button = (ApplicationBarIconButton)sender;
            switch (button.Text)
            {
                case "刷新":
                    PageIndex_Message = 1;
                    GetMessage();
                    break;
                case "回复":
                    if (txt_writemsg.Visibility != System.Windows.Visibility.Visible)
                        txt_writemsg.Visibility = System.Windows.Visibility.Visible;

                    if (txt_writemsg.Text.Trim().Length == 0)
                    {
                        txt_writemsg.Text = "回复";
                    }
                    if (txt_writemsg.Text.Equals("回复"))
                    {
                        txt_writemsg.SelectionStart = 0;
                        txt_writemsg.SelectionLength = 2;
                    }
                    else
                    {
                        txt_writemsg.SelectionStart = (txt_writemsg.Text.Length < 1) ? 0 : txt_writemsg.Text.Length;
                    }
                    txt_writemsg.Focus();

                    button.IconUri = new Uri("/Assets/AppBar/check.png", UriKind.Relative);
                    button.Text = "发送";

                    break;
                case "发送":
                    txt_writemsg.Visibility = System.Windows.Visibility.Collapsed;
                    button.IconUri = new Uri("/Assets/AppBar/edit.png", UriKind.Relative);
                    button.Text = "回复";
                    if (!txt_writemsg.Text.Equals("回复"))
                    {
                        strLeaveMSG = txt_writemsg.Text;
                        txt_writemsg.Text = "回复";
                        SendMessage();
                    }
                    break;
            }
        }

        #region SendMessage 发送私信
        public async void SendMessage()
        {
            if (string.IsNullOrEmpty(strLeaveMSG)) return;
            await Task.Run(() =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    ApplicationBar.Mode = ApplicationBarMode.Minimized;

                    progressBar1.IsIndeterminate = true;
                    if (list_message.Items.Count > 0)
                    {
                        list_message.ScrollIntoView(list_message.Items[0]);
                        list_message.UpdateLayout();
                    }
                });

                var url = AppGlobalStatic.MessagesURL["发私信"];
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                webRequest.Headers["X-Requested-With"] = "XMLHttpRequest";
                webRequest.Headers["User-Agent"] = AppGlobalStatic.User_Agent;
                webRequest.Headers["Referer"] = "http://love.163.com/home";
                webRequest.CookieContainer = App.g_CookieContainer;

                webRequest.BeginGetRequestStream(new AsyncCallback(SendMessageStreamCallback), webRequest);
            });
        }

        private void SendMessageStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
            Stream postStream = webRequest.EndGetRequestStream(asynchronousResult);

            string postData = string.Format("withUserId={0}&content={1}&isSetTop=0", UserID, HttpUtility.UrlEncode(strLeaveMSG));

            if (!string.IsNullOrEmpty(TrendID)) postData += "&trendId=" + TrendID;

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            postStream.Write(byteArray, 0, byteArray.Length);
            postStream.Close();
            TrendID = null;//只用一次，就释放掉，防止多次引用消息trend
            webRequest.BeginGetResponse(new AsyncCallback(SendMessageCallback), webRequest);
        }

        private void SendMessageCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
                HttpWebResponse response;

                // End the get response operation
                response = (HttpWebResponse)webRequest.EndGetResponse(asynchronousResult);
                response.Headers[HttpRequestHeader.ContentType] = "application/json; charset=UTF-8";

                Stream streamResponse = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(streamResponse);
                var Response = streamReader.ReadToEnd();

                dynamic json = JsonConvert.DeserializeObject(Response);

                streamResponse.Close();
                streamReader.Close();
                response.Close();

                if (json != null)
                {
                    Message message = new Message();
                    message.ID = Convert.ToString(json["id"]);
                    message.RichContent = (string)json["richContent"];
                    message.Source = AppGlobalStatic.Source((string)json["source"]);

                    message.PrettyTime = AppGlobalStatic.DateStringFromNow(((string)json["prettyTime"]).Replace("今天", DateTime.Now.ToString("yyyy-MM-dd")));

                    message.User = new User();
                    message.User.ID = Convert.ToString(json["user"]["id"]);
                    message.User.NickName = Convert.ToString(json["user"]["nickName"]);
                    message.User.Sex = ("人妖,他,她").Split(',')[(int)json["user"]["sex"]];

                    message.ToUser = new User();
                    message.ToUser.Url = Convert.ToString(json["toUser"]["url"]);
                    message.ToUser.NickName = Convert.ToString(json["toUser"]["nickName"]);
                    message.ToUser.Sex = ("人妖,他,她").Split(',')[(int)json["toUser"]["sex"]];

                    message.RelatedData = json["relatedData"] as JObject;


                    message.Type = (string)json["type"];
                    if (message.Type.Equals("1"))
                    {
                        if (message.User.ID.Equals(App.MyLove["id"].ToString()))
                            message.RichContent = "我赞了" + message.ToUser.Sex + "发布的动态";
                        else
                            message.RichContent = message.User.Sex + " 赞了我发布的动态";
                    }
                    else
                    {
                        message.RichContent = AppGlobalStatic.ImgToEmotion(message.RichContent);
                    }
                    if (message.RelatedData["trend"] != null)
                        message.RichContent += "\n-    -    -    -    -    -    -    -   -\n" + AppGlobalStatic.ImgToEmotion(Convert.ToString(message.RelatedData["trend"]["richContent"]));

                    message.User.Url = Convert.ToString(json["user"]["url"]);
                    message.User.Age = Convert.ToString(json["user"]["age"]) + " " + Love.Resources.AppGlobalStatic.AgeToAnimal((string)json["user"]["age"]);

                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageListCollection.Insert(0, message);
                    });
                }
                json = null;

            }
            catch (WebException e)
            {

            }

            Dispatcher.BeginInvoke(() =>
            {
                progressBar1.IsIndeterminate = false;
                ApplicationBar.Mode = ApplicationBarMode.Default;
            });
        }
        #endregion

        private void btn_more_Click(object sender, RoutedEventArgs e)
        {
            GetMessage();
        }

        private void list_message_PrepareContainerForItem(DependencyObject element, object item)
        {
            var Item = element as ListBoxItem;
            Message o = item as Message;
            if (o.IsMoreButtonTrend)
            {
                Item.ContentTemplate = this.Resources["ButtonMoreTemplate"] as DataTemplate;
                return;
            }
            if (o.User.ID.Equals(App.MyID))
                Item.ContentTemplate = this.Resources["Template_ME"] as DataTemplate;
            else
                Item.ContentTemplate = this.Resources["Template_YOU"] as DataTemplate;
        }
    }
}