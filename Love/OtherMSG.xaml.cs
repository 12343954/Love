using Love.Model;
using Love.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Love
{
    public partial class OtherMSG : PhoneApplicationPage
    {
        string URL, UserID, UserName, MessageType = "message";//默认，私信类型

        int PageIndex_Message = 1;
        ObservableCollection<Message> MessageListCollection;

        public OtherMSG()
        {
            InitializeComponent();
            this.Loaded += PrivateMSG_Loaded;
        }

        void PrivateMSG_Loaded(object sender, RoutedEventArgs e)
        {
            if (list_message.Items.Count > 0) return;
            switch (MessageType)
            {
                case "visitor":
                    HeaderText.Text = "最近来访";
                    URL = AppGlobalStatic.MessagesURL["最近来访"];
                    break;
                case "heed":
                    HeaderText.Text = "收藏我的";
                    URL = AppGlobalStatic.MessagesURL["最近收藏我的"];
                    break;
                case "digg":
                    HeaderText.Text = "赞我的人";
                    URL = AppGlobalStatic.MessagesURL["最近赞过我的"];
                    break;
            }
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

                //var url = AppGlobalStatic.MessagesURL["最近赞过我的"];
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(URL);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                webRequest.Headers["X-Requested-With"] = "XMLHttpRequest";
                webRequest.Headers["Referer"] = "http://love.163.com/messages/like";

                webRequest.CookieContainer = App.g_CookieContainer;

                webRequest.BeginGetRequestStream(new AsyncCallback(GetMessageStreamCallback), webRequest);
            });



        }
        private void GetMessageStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
            Stream postStream = webRequest.EndGetRequestStream(asynchronousResult);

            string postData = "";
            switch (MessageType)
            {
                case "visitor"://最近来访
                    break;
                case "heed"://最近收藏我的
                case "digg"://最近赞过我的
                    postData = string.Format("pageNo={0}", PageIndex_Message);
                    break;
            }

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
                if (!MessageType.Equals("visitor"))
                {
                    response.Headers[HttpRequestHeader.ContentType] = "application/json; charset=UTF-8";
                }


                Stream streamResponse = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(streamResponse);
                var Response = streamReader.ReadToEnd();

                if (MessageType.Equals("visitor"))
                {
                    Match mc = Regex.Match(Response, "<script id=\"data_visitorList\" type=\"application/json\">(.|\n)*?</script>", RegexOptions.IgnoreCase);
                    if (mc.Success)
                    {
                        Response = mc.Value.Replace("<script id=\"data_visitorList\" type=\"application/json\">", "").Replace("</script>", "");
                    }
                    else
                        Response = "{list:[],page:{pageCount:0,pageNo:0}}";
                }

                dynamic json = JsonConvert.DeserializeObject(Response);

                MessageListCollection = new ObservableCollection<Message>();

                foreach (var item in json["list"])
                {
                    var message = new Message();
                    message.ID = Convert.ToString(item["id"]);

                    message.User = new User();
                    message.User.ID = Convert.ToString(item["user"]["id"]);
                    message.User.Avatar = (string)(item["user"]["avatar"]);
                    message.User.NickName = Convert.ToString(item["user"]["nickName"]);
                    message.User.Sex = ("人妖,他,她").Split(',')[(int)item["user"]["sex"]];
                    message.User.Height = (string)item["user"]["height"];
                    if (item["user"]["education"] != null)
                        message.User.Education = AppGlobalStatic.Xueli[int.Parse((string)item["user"]["education"])];
                    message.User.Url = Convert.ToString(item["user"]["url"]);
                    message.User.Age = Convert.ToString(item["user"]["age"]) + " " + Love.Resources.AppGlobalStatic.AgeToAnimal((string)item["user"]["age"]);

                    message.Type = (string)item["type"];
                    switch (MessageType)
                    {
                        case "visitor":
                            message.RichContent = message.User.Sex + "来看过你";
                            message.PrettyTime = AppGlobalStatic.DateStringFromNow(((string)item["visitTime"]).Replace("今天", DateTime.Now.ToString("yyyy-MM-dd")));
                            message.Source = message.User.Age + " " + message.User.Height + " " + message.User.Education;
                            break;
                        case "heed":
                            message.RichContent = (string)item["content"];
                            message.PrettyTime = AppGlobalStatic.DateStringFromNow(((string)item["followTime"]).Replace("今天", DateTime.Now.ToString("yyyy-MM-dd")));
                            message.Source = message.User.Age + " " + message.User.Height + " " + message.User.Education;// + " " + message.User.Sex + "收藏了你";
                            break;
                        case "digg":
                            message.RichContent = (string)item["richContent"];
                            message.PrettyTime = AppGlobalStatic.DateStringFromNow(((string)item["praiseTime"]).Replace("今天", DateTime.Now.ToString("yyyy-MM-dd")));
                            message.Source = message.User.Age + " " + message.User.Height + " " + message.User.Education;//message.User.Sex + " 赞了你";
                            break;
                    }

                    MessageListCollection.Add(message);
                }
                int pageCount = (int)json["page"]["pageCount"];
                int pageNo = (int)json["page"]["pageNo"];

                if (!MessageType.Equals("visitor"))
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

        #region GetLatestVisitor() //获取最新访问
        public async void GetLatestVisitor()
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

                CookieAwareClient cookieClient = new CookieAwareClient();

                //cookieClient.Headers["X-Requested-With"] = "XMLHttpRequest";
                cookieClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(cookieAttempt_DownloadStringCompleted);
                cookieClient.DownloadStringAsync(new Uri(URL, UriKind.Absolute));


                //cookieClient.Headers["Content-Type"] = "application/x-www-form-urlencoded; charset=UTF-8;";
                //cookieClient.UploadStringAsync(new Uri("http://love.163.com/home/newInfo"), "POST", HttpUtility.UrlEncode(string.Format("height=162-165&age=25-27&city=0&province=1&lastTrendTime={0}", Convert.ToInt64(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)))));
                //cookieClient.UploadStringCompleted += cookieClient_UploadStringCompleted;
            });

        }

        private void cookieClient_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            string body = e.Result;
            //textblock1.Text = e.Result;
        }

        private void cookieClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            Stream st = e.Result;
            byte[] data = new byte[st.Length];
            st.Read(data, 0, data.Length);
            //textblock1.Text = Encoding.UTF8.GetString(data, 0, data.Length);
        }

        private void cookieAttempt_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string body = e.Result;
            //textblock1.Text = e.Result;

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
            NavigationContext.QueryString.TryGetValue("type", out MessageType);
        }

        private async void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            var button = (ApplicationBarIconButton)sender;
            switch (button.Text)
            {
                case "刷新":
                    if (pivot.SelectedIndex == 0)
                    {
                        PageIndex_Message = 1;
                        GetMessage();
                    }
                    break;
            }
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
            Item.ContentTemplate = this.Resources["MessageTemplate"] as DataTemplate;
        }

        private void pivot_LoadedPivotItem(object sender, PivotItemEventArgs e)
        {
            //GetMessage();
        }

        private void btn_more_Click(object sender, RoutedEventArgs e)
        {
            GetMessage();
        }

        private void list_message_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (list_message == null || list_message.SelectedItem == null) return;

            var selectedItem = (Message)list_message.SelectedItem;
            string url = string.Format("/PrivateMSG.xaml?type={0}&userid={1}&username={2}", "message", selectedItem.User.ID, selectedItem.User.NickName);

            NavigationService.Navigate(new System.Uri(url, UriKind.Relative));
            list_message.SelectedItem = null;
        }

        private void btn_HeadImage_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as Button).DataContext as Message;
            NavigationService.Navigate(new System.Uri(string.Format("/ME.xaml?userid={0}&username={1}&userurl={2}&usersex={3}", new string[] { data.User.ID, data.User.NickName, data.User.Url, data.User.Sex }), UriKind.Relative));
        }
    }
}