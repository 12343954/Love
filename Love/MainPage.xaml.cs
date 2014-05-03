using Love.Model;
using Love.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Love
{
    public partial class MainPage : PhoneApplicationPage
    {
        int PageIndex_Trend = 1;
        int PageIndex_Message = 1;
        ObservableCollection<Trend> TrendListCollection;
        ObservableCollection<Message> MessageListCollection;
        Trend LoveNote;
        string strLoveNote = "";// 发表文字
        string strTrendID = ""; //点赞时的trend ID
        // 构造函数
        public MainPage()
        {
            InitializeComponent();

            Match mc = Regex.Match(App.MyHomeHTML, "<script id=\"data_loginUser\" type=\"application/json\">(.|\n)*?</script>", RegexOptions.IgnoreCase);
            if (mc.Success)
                App.MyLove = JsonConvert.DeserializeObject(mc.Value.Replace("<script id=\"data_loginUser\" type=\"application/json\">", "").Replace("</script>", "")) as JObject;

            // 用于本地化 ApplicationBar 的示例代码
            //BuildLocalizedApplicationBar();
        }

        #region 简单的post
        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    progressBar1.IsIndeterminate = true;
                });

                SendPost();
                //Thread.Sleep(2000);
                //for (int i = 0; i < 200; i++)
                //{
                //    Thread.Sleep(10);
                //}

                Dispatcher.BeginInvoke(() =>
                {
                    progressBar1.IsIndeterminate = false;
                });
            });

            return;

            CookieAwareClient cookieClient = new CookieAwareClient();

            //cookieClient.Headers["X-Requested-With"] = "XMLHttpRequest";
            //cookieClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(cookieAttempt_DownloadStringCompleted);
            //cookieClient.DownloadStringAsync(new Uri("http://love.163.com/operation/activities"));


            cookieClient.Headers["Content-Type"] = "application/x-www-form-urlencoded; charset=UTF-8;";
            cookieClient.UploadStringAsync(new Uri("http://love.163.com/home/newInfo"), "POST", HttpUtility.UrlEncode(string.Format("height=162-165&age=25-27&city=0&province=1&lastTrendTime={0}", Convert.ToInt64(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)))));
            cookieClient.UploadStringCompleted += cookieClient_UploadStringCompleted;

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
        }
        #endregion

        #region SendPost 获取动态
        public async void SendPost()
        {
            await Task.Run(() =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    ApplicationBar.Mode = ApplicationBarMode.Minimized;

                    progressBar1.IsIndeterminate = true;
                    if (list_trend.Items.Count > 0)
                    {
                        list_trend.ScrollIntoView(list_trend.Items[0]);
                        list_trend.UpdateLayout();
                    }
                });


                //var url = "http://love.163.com/home/newInfo";
                var url = "http://love.163.com/trend/list";
                // Create the web request object
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                webRequest.Headers["X-Requested-With"] = "XMLHttpRequest";
                webRequest.CookieContainer = App.g_CookieContainer;

                // Start the request
                webRequest.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), webRequest);
            });
        }

        public void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
            Stream postStream = webRequest.EndGetRequestStream(asynchronousResult);

            string postData = string.Format(App.Search + "&pageToken={0}", PageIndex_Trend);

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            postStream.Write(byteArray, 0, byteArray.Length);
            postStream.Close();

            webRequest.BeginGetResponse(new AsyncCallback(GetResponseCallback), webRequest);
        }

        public void GetResponseCallback(IAsyncResult asynchronousResult)
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

                TrendListCollection = new ObservableCollection<Trend>();

                foreach (var item in json["list"])
                {
                    var trend = new Trend();
                    trend.ID = Convert.ToString(item["id"]);
                    //trend.Content = Convert.ToString(item["richContent"]);
                    trend.Timestamp = AppGlobalStatic.DateStringFromNow(AppGlobalStatic.ConvertLocalFromTimestamp(long.Parse((string)item["timestamp"])).ToString());
                    trend.DiggCount = Convert.ToInt32((string)item["diggCount"]);
                    trend.User = new User();
                    trend.User.ID = Convert.ToString(item["user"]["id"]);
                    trend.User.NickName = Convert.ToString(item["user"]["nickName"]);
                    trend.User.Url = Convert.ToString(item["user"]["url"]);
                    trend.User.Sex = ("人妖,他,她").Split(',')[(int)item["user"]["sex"]];
                    trend.User.Avatar = Convert.ToString(item["user"]["avatar"]);
                    trend.User.IsOnline = ((string)item["user"]["isOnline"]).Equals("1") ? "在线" : "离线";
                    trend.User.Age = Convert.ToString(item["user"]["age"]) + " " + Love.Resources.AppGlobalStatic.AgeToAnimal((string)item["user"]["age"]);
                    trend.User.Height = Convert.ToString(item["user"]["height"]);
                    trend.User.Education = AppGlobalStatic.Xueli[int.Parse((string)item["user"]["education"])];

                    if (item["isLoveNotes"] != null)
                    {
                        trend.IsLoveNotes = true;
                        trend.Content = (string)item["letterContent"];
                        trend.LetterTemplateId = (string)item["letterTemplateId"];
                    }
                    else if (item["isQaTrend"] != null)
                    {
                        trend.IsQaTrend = true;
                        trend.Content = (string)item["content"];
                        trend.QA = new Question();
                        trend.QA.ID = (string)item["mediaInfo"]["question"]["id"];
                        trend.QA.Title = (string)item["mediaInfo"]["question"]["title"];
                        trend.QA.AnswerID = (int)item["mediaInfo"]["question"]["answerId"];
                        trend.QA.Options = new List<QuestionOption>();
                        int _index = 1;
                        foreach (var ss in item["mediaInfo"]["question"]["options"])
                        {
                            QuestionOption qo = new QuestionOption();
                            qo.Checked = (_index == trend.QA.AnswerID);
                            qo.Text = (string)ss["text"];
                            trend.QA.Options.Add(qo);
                            _index++;
                        }
                    }
                    else
                    {
                        trend.IsPhotoTrend = true;
                        trend.Content = (string)item["richContent"];
                        trend.Photolist = new List<Photo>();

                        foreach (var ss in item["mediaInfo"]["photoList"])
                        {
                            trend.Photolist.Add(new Photo() { ID = (string)ss["id"], URL = (string)ss["url"] });
                        }
                    }
                    trend.Source = AppGlobalStatic.Source((string)item["source"], trend.Content);
                    trend.Content = AppGlobalStatic.ClearAhref(trend.Content);

                    TrendListCollection.Add(trend);
                }


                if (TrendListCollection.Count > 0)
                    TrendListCollection.Add(new Trend() { IsMoreButtonTrend = true });

                PageIndex_Trend++;
                Dispatcher.BeginInvoke(() =>
                {
                    list_trend.ItemsSource = TrendListCollection;
                    //textblock1.Text = Response;
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

                var url = AppGlobalStatic.MessagesURL["全部消息"];
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
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

            string postData = string.Format("isUnread=0&isMatched=0&pageNo={0}", PageIndex_Message);

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
                    message.RichContent = AppGlobalStatic.ImgToEmotion((string)item["richContent"]);

                    message.PrettyTime = AppGlobalStatic.DateStringFromNow(((string)item["prettyTime"]).Replace("今天", DateTime.Now.ToString("yyyy-MM-dd")));

                    message.Type = (string)item["type"];
                    switch (message.Type.ToLower())
                    {
                        case "visitor":
                        case "heed":
                        case "digg":
                            message.User = new User();
                            message.User.Avatar = "/Assets/Image/" + message.Type + ".png";
                            message.NewUsersCount = Convert.ToInt32((string)item["newUsersCount"]);
                            break;
                        case "message":
                        case "admin":
                            message.NewCount = Convert.ToInt32((string)item["newCount"]);
                            message.TotalCount = Convert.ToInt32((string)item["totalCount"]);
                            message.User = new User();
                            message.User.ID = Convert.ToString(item["user"]["id"]);
                            message.User.NickName = Convert.ToString(item["user"]["nickName"]);
                            message.User.Url = Convert.ToString(item["user"]["url"]);
                            message.User.Sex = ("人妖,他,她").Split(',')[(int)item["user"]["sex"]];
                            message.User.Avatar = Convert.ToString(item["user"]["avatar"]);
                            message.User.IsOnline = ((string)item["user"]["isOnline"]).Equals("1") ? "在线" : "离线";
                            message.User.Age = Convert.ToString(item["user"]["age"]) + " " + Love.Resources.AppGlobalStatic.AgeToAnimal((string)item["user"]["age"]);
                            break;
                    }

                    MessageListCollection.Add(message);
                }

                if (MessageListCollection.Count > 0)
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


        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (gird_wirtenote.Visibility == System.Windows.Visibility.Visible)
            {
                e.Cancel = true;
                ApplicationBarIconButton button = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
                button.IconUri = new Uri("/Assets/AppBar/edit.png", UriKind.Relative);
                button.Text = "发表";
                gird_wirtenote.Visibility = System.Windows.Visibility.Collapsed;
                ApplicationBar.Mode = ApplicationBarMode.Default;
                return;
            }
            if (Popup_ShowImage.IsOpen)
            {
                e.Cancel = true;
                Popup_ShowImage.IsOpen = false;
                ApplicationBar.IsVisible = true;
                //ApplicationBar.Mode = ApplicationBarMode.Default;
                return;
            }

            if (pivot.SelectedIndex != 0)
            {
                e.Cancel = true;
                pivot.SelectedIndex = 0;
                return;
            }
            MessageBoxResult r = MessageBox.Show("", "确定退出花田", MessageBoxButton.OKCancel);
            if (r == MessageBoxResult.OK)
            {
                Application.Current.Terminate();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void list_PrepareContainerForItem(DependencyObject element, object item)
        {
            var Item = element as ListBoxItem;
            Trend o = item as Trend;
            if (o.IsLoveNotes)
                Item.ContentTemplate = this.Resources["ListBoxDataTemplate"] as DataTemplate;
            else if (o.IsQaTrend)
                Item.ContentTemplate = this.Resources["ListBoxQATemplate"] as DataTemplate;
            else if (o.IsPhotoTrend)
                Item.ContentTemplate = this.Resources["ListBoxImageTemplate"] as DataTemplate;
            else if (o.IsMoreButtonTrend)
                Item.ContentTemplate = this.Resources["ButtonMoreTemplate"] as DataTemplate;
        }

        private async void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            var button = (ApplicationBarIconButton)sender;
            switch (button.Text)
            {
                case "刷新":
                    if (pivot.SelectedIndex == 0)
                    {
                        PageIndex_Trend = 1;
                        SendPost();
                    }
                    else
                    {
                        PageIndex_Message = 1;
                        GetMessage();
                    }
                    break;
                case "发表":
                    //ApplicationBar.Mode = ApplicationBarMode.Minimized;
                    if (pivot.SelectedIndex != 0) pivot.SelectedIndex = 0;
                    button.IconUri = new Uri("/Assets/AppBar/check.png", UriKind.Relative);
                    button.Text = "发送";
                    txt_Note.Text = "#文字传情#";
                    txt_Note.SelectionStart = 0;
                    txt_Note.SelectionLength = txt_Note.Text.Length;
                    txt_Note.Select(0, txt_Note.Text.Length);
                    txt_Note.Focus();

                    gird_wirtenote.Visibility = System.Windows.Visibility.Visible;

                    break;
                case "发送":
                    button.IconUri = new Uri("/Assets/AppBar/edit.png", UriKind.Relative);
                    button.Text = "发表";
                    if (txt_Note.Text.Trim().Length > 0 && !txt_Note.Text.Equals("#文字传情#"))
                    {
                        if (txt_Note.Text.Length > 200)
                        {
                            MessageBox.Show("不能超过200字，请酌情删减后再发！", "提示", MessageBoxButton.OK);
                        }
                        else
                        {
                            ApplicationBar.Mode = ApplicationBarMode.Minimized;
                            gird_wirtenote.Visibility = System.Windows.Visibility.Collapsed;
                            strLoveNote = txt_Note.Text;
                            SendNote();
                        }
                    }
                    else
                    {
                        button.IconUri = new Uri("/Assets/AppBar/edit.png", UriKind.Relative);
                        button.Text = "发表";
                        gird_wirtenote.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    break;
            }
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            var button = (ApplicationBarMenuItem)sender;
            switch (button.Text)
            {
                case "我的动态":
                    if (App.MyLove == null)
                        MessageBox.Show("信息读取有误，下次登录再试，请原谅", "抱歉", MessageBoxButton.OK);
                    else
                        NavigationService.Navigate(new System.Uri(string.Format("/ME.xaml?userid={0}&username={1}&userurl={2}&usersex={3}", new string[] { App.MyLove["id"].ToString(), App.MyLove["nickName"].ToString(), App.MyLove["url"].ToString(), App.MyLove["sex"].ToString() }), UriKind.Relative));
                    break;
                case "打分":
                    MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
                    marketplaceReviewTask.Show();
                    break;
                case "关于":
                    NavigationService.Navigate(new System.Uri("/About.xaml", UriKind.Relative));
                    break;
            }
        }

        private async void btn_more_Click(object sender, RoutedEventArgs e)
        {
            if (pivot.SelectedIndex == 0)
                SendPost();
            else
                GetMessage();
        }

        private void list_message_PrepareContainerForItem(DependencyObject element, object item)
        {
            var Item = element as ListBoxItem;
            Message o = item as Message;
            if (o.IsMoreButtonTrend)
                Item.ContentTemplate = this.Resources["ButtonMoreTemplate"] as DataTemplate;
            else
                Item.ContentTemplate = this.Resources["MessageTemplate"] as DataTemplate;
        }

        private void pivot_LoadedPivotItem(object sender, PivotItemEventArgs e)
        {
            switch (e.Item.Name)
            {
                case "pivot_trend":
                    if (list_trend.Items.Count < 1)
                    {
                        PageIndex_Trend = 1;
                        SendPost();
                    }
                    break;
                case "pivot_Message":
                    if (list_message.Items.Count < 1)
                    {
                        PageIndex_Message = 1;
                        GetMessage();
                    }
                    break;
            }
        }

        private void list_message_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (list_message == null || list_message.SelectedItem == null) return;

            var selectedItem = (Message)list_message.SelectedItem;
            string url = "";
            switch (selectedItem.Type.ToLower())
            {
                case "message":
                case "admin":
                    url = string.Format("/PrivateMSG.xaml?type={0}&userid={1}&username={2}", selectedItem.Type, selectedItem.User.ID, selectedItem.User.NickName);
                    break;
                default:
                    url = string.Format("/OtherMSG.xaml?type={0}", selectedItem.Type);

                    break;
            }

            NavigationService.Navigate(new System.Uri(url, UriKind.Relative));
            list_message.SelectedItem = null;
        }

        private void list_trend_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            return;
            if (list_trend == null || list_trend.SelectedItem == null) return;
            var selectedItem = (Trend)list_trend.SelectedItem;
            NavigationService.Navigate(new System.Uri(string.Format("/PrivateMSG.xaml?type={0}&userid={1}&username={2}", "message", selectedItem.User.ID, selectedItem.User.NickName), UriKind.Relative));
            list_trend.SelectedItem = null;
        }

        private void txt_Note_GotFocus(object sender, RoutedEventArgs e)
        {
            txt_Note.Background = new SolidColorBrush(Colors.White);
            txt_Note.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void txt_Note_TextChanged(object sender, TextChangedEventArgs e)
        {
            txt_Count.Text = txt_Note.Text.Length.ToString();
        }

        private void btn_SendNote_Click(object sender, RoutedEventArgs e)
        {
            if (Popup_ShowImage.IsOpen)
            {
                Popup_ShowImage.IsOpen = false;
                ApplicationBar.Mode = ApplicationBarMode.Default;
                return;
            }
            if (txt_Note.Text.Trim().Length < 10)
            {
                txt_Count.Text = "至少输入10个字";
                return;
            }
            strLoveNote = txt_Note.Text;
            SendNote();
        }

        #region SendNote 发文字传情
        public async void SendNote()
        {
            await Task.Run(() =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    ApplicationBar.Mode = ApplicationBarMode.Minimized;
                    progressBar1.IsIndeterminate = true;
                });

                var url = AppGlobalStatic.MessagesURL["文字传情"];
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                webRequest.Headers["X-Requested-With"] = "XMLHttpRequest";
                webRequest.Headers["User-Agent"] = AppGlobalStatic.User_Agent;
                webRequest.Headers["Referer"] = "http://love.163.com/home";
                webRequest.CookieContainer = App.g_CookieContainer;

                webRequest.BeginGetRequestStream(new AsyncCallback(SendNoteStreamCallback), webRequest);
            });
        }
        private void SendNoteStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
            Stream postStream = webRequest.EndGetRequestStream(asynchronousResult);

            string postData = "";

            //postData = "templateId=1&content=%E9%9A%8F%E4%BE%BF%E8%AF%B4%E7%82%B9%E6%83%85%E8%AF%9D%2C%2C%2C%2C";
            postData = string.Format("templateId={0}&content={1}", "1", HttpUtility.UrlEncode(strLoveNote + "\n\r\n\r —— 来自Windows·Phone"));

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            postStream.Write(byteArray, 0, byteArray.Length);
            postStream.Close();

            webRequest.BeginGetResponse(new AsyncCallback(SendNoteCallback), webRequest);
        }
        private void SendNoteCallback(IAsyncResult asynchronousResult)
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

                LoveNote = new Trend();
                if (json != null)
                {
                    LoveNote.ID = (string)json["id"];
                    LoveNote.IsLoveNotes = (bool)json["isLoveNotes"];
                    LoveNote.Timestamp = (string)json["timestamp"];
                    if (LoveNote.IsLoveNotes)
                    {
                        LoveNote.Content = (string)json["letterContent"];
                    }
                    else
                    {
                        LoveNote.Content = (string)json["richContent"];
                    }


                    LoveNote.Source = AppGlobalStatic.Source((string)json["source"], LoveNote.Content);
                    //LoveNote.IsLoveNotes = (bool)json["isLoveNotes"];
                    //LoveNote.IsLoveNotes = (bool)json["isLoveNotes"];
                }
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
                //Popup_WriteNote.IsOpen = false;
                ApplicationBar.Mode = ApplicationBarMode.Default;
            });
        }
        #endregion

        private void btn_HeadImage_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as Button).DataContext as Trend;
            if (data != null)
            {
                App.UserVisit = data.User;
                NavigationService.Navigate(new System.Uri(string.Format("/ME.xaml?userid={0}&username={1}&userurl={2}&usersex={3}", new string[] { data.User.ID, data.User.NickName, data.User.Url, data.User.Sex }), UriKind.Relative));
                return;
            }

            var data2 = (sender as Button).DataContext as Message;
            if (data2 != null)
            {
                switch (data2.User.Avatar)
                {
                    case "/Assets/Image/visitor.png":
                        NavigationService.Navigate(new System.Uri("/OtherMSG.xaml?type=visitor", UriKind.Relative));
                        break;
                    case "/Assets/Image/heed.png":
                        NavigationService.Navigate(new System.Uri("/OtherMSG.xaml?type=heed", UriKind.Relative));
                        break;
                    case "/Assets/Image/digg.png":
                        NavigationService.Navigate(new System.Uri("/OtherMSG.xaml?type=digg", UriKind.Relative));
                        break;
                    default://人头像
                        App.UserVisit = data2.User;
                        NavigationService.Navigate(new System.Uri(string.Format("/ME.xaml?userid={0}&username={1}&userurl={2}&usersex={3}", new string[] { data2.User.ID, data2.User.NickName, data2.User.Url, data2.User.Sex }), UriKind.Relative));
                        break;
                }
            }


            //var trend = list_trend.ItemContainerGenerator.ContainerFromItem(data) as ListBoxItem;
            //((Love.Model.Trend)(trend.Item)).User.Url
        }

        private void btn_like_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var data = button.DataContext as Trend;
            if (data != null)
            {
                button.Content = (int.Parse(button.Content.ToString()) + 1).ToString();
                strTrendID = data.ID;
                LikePraise();
            }
        }

        #region 点赞 http://love.163.com/messages/like/praise"
        public async void LikePraise()
        {
            await Task.Run(() =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    progressBar1.IsIndeterminate = true;
                });

                var url = AppGlobalStatic.MessagesURL["点赞"];
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                webRequest.Headers["X-Requested-With"] = "XMLHttpRequest";
                webRequest.Headers["User-Agent"] = AppGlobalStatic.User_Agent;
                webRequest.Headers["Referer"] = "http://love.163.com/home";
                webRequest.CookieContainer = App.g_CookieContainer;

                webRequest.BeginGetRequestStream(new AsyncCallback(LikePraiseStreamCallback), webRequest);
            });
        }

        private void LikePraiseStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
            Stream postStream = webRequest.EndGetRequestStream(asynchronousResult);

            string postData = "";

            postData = string.Format("trendId={0}", strTrendID);
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            postStream.Write(byteArray, 0, byteArray.Length);
            postStream.Close();

            webRequest.BeginGetResponse(new AsyncCallback(LikePraiseCallback), webRequest);
        }

        private void LikePraiseCallback(IAsyncResult asynchronousResult)
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

            }
            catch (WebException e)
            {

            }

            Dispatcher.BeginInvoke(() =>
            {
                progressBar1.IsIndeterminate = false;
            });
        }

        #endregion

        private void btn_gotoMessage_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as Button).DataContext as Trend;
            if (data != null)
            {
                NavigationService.Navigate(new System.Uri(string.Format("/PrivateMSG.xaml?type={0}&userid={1}&username={2}&userurl={3}&trendId={4}"
                    , new string[] { "message", data.User.ID, data.User.NickName, data.User.Url, data.ID }), UriKind.Relative));
            }
        }

        private void btn_popImage_Click(object sender, RoutedEventArgs e)
        {
            var photo = (sender as Button).DataContext as Photo;
            if (photo != null)
            {
                ApplicationBar.IsVisible = false;
                Popup_ShowImage.IsOpen = true;
                gird_Image.Width = Application.Current.Host.Content.ActualWidth;
                gird_Image.Height = Application.Current.Host.Content.ActualHeight;
                IMG_ShowPop.Source = new BitmapImage(new Uri(photo.URL, UriKind.Absolute));
            }
        }


        // 用于生成本地化 ApplicationBar 的示例代码
        //private void BuildLocalizedApplicationBar()
        //{
        //    // 将页面的 ApplicationBar 设置为 ApplicationBar 的新实例。
        //    ApplicationBar = new ApplicationBar();

        //    // 创建新按钮并将文本值设置为 AppResources 中的本地化字符串。
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // 使用 AppResources 中的本地化字符串创建新菜单项。
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}