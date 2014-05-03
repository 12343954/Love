using Love.Model;
using Love.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
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
using System.Windows.Media.Imaging;

namespace Love
{
    public partial class ME : PhoneApplicationPage
    {
        ObservableCollection<Trend> TrendListCollection;
        ObservableCollection<Profile> InfoListCollection;
        ObservableCollection<Profile> HuatianListCollection;

        string pageToken = "";

        public string UserID, UserName, UserUrl, UserSex, title, MeUrl;
        public string strTrendID = "", Response = "";

        public ME()
        {
            InitializeComponent();
            this.Loaded += ME_Loaded;
        }

        void ME_Loaded(object sender, RoutedEventArgs e)
        {
            if (UserID.Equals(App.MyID))
                title = "我";
            else
                title = UserSex;

            header_trend.Text = title + "的动态";
            header_info.Text = title + "的资料";
            //header_info.Text = title + "资料";
        }

        #region 获取网页我的资料GetUserInfo
        public async void GetUserInfo()
        {
            await Task.Run(() =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    ApplicationBar.Mode = ApplicationBarMode.Minimized;

                    progressBar1.IsIndeterminate = true;
                    if (list_info.Items.Count > 0)
                    {
                        list_info.ScrollIntoView(list_info.Items[0]);
                        list_info.UpdateLayout();
                    }
                });

                var url = "http://love.163.com/" + UserUrl;
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                //webRequest.CookieContainer = App.g_CookieContainer;
                webRequest.CookieContainer = new CookieContainer();
                webRequest.CookieContainer = App.g_CookieContainer;
                webRequest.Method = "GET";
                //webRequest.Headers["Referer"] = "http://love.163.com/home";// +MeUrl;

                webRequest.BeginGetResponse(new AsyncCallback(GetHTMLCallback), webRequest);
            });
        }

        private void GetHTMLCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest webRequest = null;
            HttpWebResponse response = null;
            try
            {
                webRequest = (HttpWebRequest)asynchronousResult.AsyncState;

                response = (HttpWebResponse)webRequest.EndGetResponse(asynchronousResult);
                //response.Headers[HttpRequestHeader.ContentType] = "application/json; charset=UTF-8";

                Stream streamResponse = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(streamResponse);
                Response = streamReader.ReadToEnd();

                MatchCollection mc = Regex.Matches(Response, "<span class=\"profile-label\">(.|\n)*?</span>(.|\n)*?</li>", RegexOptions.IgnoreCase);
                InfoListCollection = new ObservableCollection<Profile>();

                foreach (Match mcc in mc)
                {
                    string[] arr = Regex.Split(mcc.Value, "</span>");

                    var pro = new Profile();
                    if (arr.Length >= 1)
                        pro.Title = arr[0].Replace("<span class=\"profile-label\">", "");
                    if (arr.Length >= 2)
                        pro.Value = AppGlobalStatic.ClearAhref(arr[1].Replace("\r\n", "").Replace("</li>", ""));


                    InfoListCollection.Add(pro);
                }

                Dispatcher.BeginInvoke(() =>
                {
                    list_info.ItemsSource = InfoListCollection;
                });


                streamResponse.Close();
                streamReader.Close();
                response.Close();

            }
            catch (WebException e)
            {

            }
            finally
            {
                if (webRequest != null) webRequest.Abort();
                if (response != null) response.Close();
            }

            Dispatcher.BeginInvoke(() =>
            {
                progressBar1.IsIndeterminate = false;
            });
        }
        #endregion

        #region GetUserTimeline()

        public async void GetUserTimeline()
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
            var url = AppGlobalStatic.MessagesURL["TA的动态"];
            // Create the web request object
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            webRequest.Headers["X-Requested-With"] = "XMLHttpRequest";
            webRequest.CookieContainer = App.g_CookieContainer;

            // Start the request
            webRequest.BeginGetRequestStream(new AsyncCallback(GetUserTimelineStreamCallback), webRequest);
        }

        private void GetUserTimelineStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
            Stream postStream = webRequest.EndGetRequestStream(asynchronousResult);

            //pageToken = AppGlobalStatic.ConvertLocalToTimestamp(DateTime.Now).ToString();

            string postData = string.Format("size=20&pageToken={0}&userId={1}", pageToken, UserID);

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            postStream.Write(byteArray, 0, byteArray.Length);
            postStream.Close();

            webRequest.BeginGetResponse(new AsyncCallback(GetUserTimelineCallback), webRequest);
        }

        private void GetUserTimelineCallback(IAsyncResult asynchronousResult)
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
                {
                    pageToken = (string)json["page"]["pageToken"];
                    TrendListCollection.Add(new Trend() { IsMoreButtonTrend = true });
                }

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

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationContext.QueryString.TryGetValue("userid", out UserID);
            NavigationContext.QueryString.TryGetValue("username", out UserName);
            NavigationContext.QueryString.TryGetValue("userurl", out UserUrl);
            NavigationContext.QueryString.TryGetValue("usersex", out UserSex);

            //这里要先于pageLoad执行，所以，在这里初始化pageToken
            pageToken = AppGlobalStatic.ConvertLocalToTimestamp(DateTime.Now).ToString();

            if (!UserID.Equals(App.MyID))
            {
                if (pivot.Items.Count > 2)
                    pivot.Items.RemoveAt(2);
            }

        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            if (pivot.SelectedIndex != 0) pivot.SelectedIndex = 0;

            pageToken = AppGlobalStatic.ConvertLocalToTimestamp(DateTime.Now).ToString();
            GetUserTimeline();
        }

        private void btn_HeadImage_Click(object sender, RoutedEventArgs e)
        {

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

        private void list_trend_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btn_more_Click(object sender, RoutedEventArgs e)
        {
            GetUserTimeline();
        }

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

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Popup_ShowImage.IsOpen)
            {
                e.Cancel = true;
                Popup_ShowImage.IsOpen = false;
                ApplicationBar.IsVisible = true;
                //ApplicationBar.Mode = ApplicationBarMode.Default;
                return;
            }
        }

        private void pivot_LoadedPivotItem(object sender, PivotItemEventArgs e)
        {
            switch (e.Item.Name.ToLower())
            {
                case "pivot_trend":
                    if (ApplicationBar.Mode != ApplicationBarMode.Default)
                        ApplicationBar.Mode = ApplicationBarMode.Default;
                    if (TrendListCollection == null)
                        GetUserTimeline();
                    break;
                case "pivot_info":
                    ApplicationBar.Mode = ApplicationBarMode.Minimized;
                    if (App.MyLove == null)
                    {
                        list_info.Items.Clear();
                        list_info.Items.Add(new Profile { Title = "错误：", Value = "读取有误，请下次登录后重试" });
                        return;
                    }
                    if (InfoListCollection == null)
                        GetUserInfo();
                    break;
                case "pivot_huatian":
                    if (HuatianListCollection == null)
                    {
                        progressBar1.IsIndeterminate = true;
                        var http = new Http();
                        http.StartRequest(AppGlobalStatic.MessagesURL["数据统计"], "POST", true, App.g_CookieContainer, null,
                            result =>
                            {
                                //处理返回结果result
                                dynamic json = JsonConvert.DeserializeObject(result);
                                HuatianListCollection = new ObservableCollection<Profile>();
                                HuatianListCollection.Add(new Profile()
                                {
                                    Title = string.Format(@"{0}，{1}来到花田。", (string)json["nickName"],
    (string)json["createTime"]),
                                    Value = ""
                                });

                                HuatianListCollection.Add(new Profile()
                                {
                                    Title = string.Format(@"已上传 {0} 张照片，
已发表 {1} 个动态，
回答了 {2} 个QA。",
             (string)json["photoCount"],
    (string)json["lovenoteCount"],
    (string)json["qaCount"]),
                                    Value = ""
                                });
                                HuatianListCollection.Add(new Profile()
                                {
                                    Title = string.Format(@"我访问了 {0} 个人，相当于一个学校的女生。
{1} 人访问了我，当中一定有很多人暗恋您。",
                       (string)json["visitedCount"],
    (string)json["visitorCount"]),
                                    Value = ""
                                });
                                HuatianListCollection.Add(new Profile()
                                {
                                    Title = string.Format(@"跟 {0} 个人打过招呼，
第一个人是 {1}", (string)json["sayHiCount"],
    (string)json["firstSayHiUserNickname"]),
                                    Value = ""
                                });
                                HuatianListCollection.Add(new Profile()
                                {
                                    Title = string.Format(@"跟 {0} 个人有过接触，
聊的最多的是 {1}", (string)json["sessionCount"],
    (string)json["maxSessionTotalUserNickname"]),
                                    Value = ""
                                });
                                HuatianListCollection.Add(new Profile()
                                {
                                    Title = string.Format(@"{0} 人访问了你，
一共点赞 {1} 次，
被赞 {2} 次！", (string)json["visitorCount"],
    (string)json["praisedCount"],
    (string)json["praiseCount"]),
                                    Value = ""
                                });

                                Dispatcher.BeginInvoke(() =>
                                {
                                    list_huatian.ItemsSource = HuatianListCollection;
                                    progressBar1.IsIndeterminate = false;
                                });
                            });
                    }
                    break;
            }
        }
    }
}