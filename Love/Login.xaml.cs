using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Browser;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using Love.Resources;

namespace Love
{
    public partial class Login : PhoneApplicationPage
    {
        bool httpResult = HttpWebRequest.RegisterPrefix("http://", WebRequestCreator.ClientHttp);

        public Login()
        {
            InitializeComponent();
            PageLoaded();
        }

        private void PageLoaded()
        {
            progressBar1.IsIndeterminate = true;
            btn_login.IsEnabled = false;
            btn_login.Content = "请稍等 ...";
            if (IsolatedStorageSettingHelper.IsolateStorageKeyIsExist("account"))
                UserName.Text = IsolatedStorageSettingHelper.GetIsolateStorageByStr("account");
            if (IsolatedStorageSettingHelper.IsolateStorageKeyIsExist("emailType"))
                EmailType.Text = IsolatedStorageSettingHelper.GetIsolateStorageByStr("emailType");
            if (IsolatedStorageSettingHelper.IsolateStorageKeyIsExist("password"))
            {
                Password.Password = IsolatedStorageSettingHelper.GetIsolateStorageByStr("password");
                Password2.Text = "密码已记录";
            }
            //UserName.Text = "";
            //EmailType.Text = "@163.com";
            //Password.Password = "";
            webbrowser1.Navigate(new Uri("http://love.163.com/logout?url=http://love.163.com/?from=wap"));
        }
        string boundary = DateTime.Now.Ticks.ToString();
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string username = UserName.Text.Trim(), password = Password.Password;
            if (username.Length == 0 || password.Length == 0)
            {
                MessageBox.Show("账号/密码 不能为空！", "提示", MessageBoxButton.OK);
                return;
            }

            if (!progressBar1.IsIndeterminate)
                progressBar1.IsIndeterminate = true;
            btn_login.Content = "登录中 ...";
            btn_login.IsEnabled = false;
            StringBuilder script = new StringBuilder("");

            script.Append("document.getElementsByName(\"username\")[0].value=\"").Append(username).Append(EmailType.Text).Append("\";")
                .Append("document.getElementsByName(\"password\")[0].value=\"").Append(password).Append("\";")
                .Append("document.forms[0].submit();");

            webbrowser1.InvokeScript("eval", script.ToString());

        }

        private void webbrowser1_LoadCompleted(object sender, NavigationEventArgs e)
        {

            if (!e.Uri.ToString().Contains("http://love.163.com/?from=wap"))
                return;
            btn_login.IsEnabled = true;
            btn_login.Content = "登录";

            progressBar1.IsIndeterminate = false;
        }

        private void webbrowser1_Navigated(object sender, NavigationEventArgs e)
        {
            CookieCollection cox = webbrowser1.GetCookies();
            CookieCollection cox_163com = new CookieCollection();
            CookieCollection cox_love163com = new CookieCollection();
            CookieCollection cox__love163com = new CookieCollection();

            if (e.Uri.ToString().Contains("http://love.163.com/home"))
            {
                App.MyLove = JObject.Parse("{\"id\": \"\",\"nickName\": \"\",\"url\": \"\",\"age\": 1,\"sex\": 1,\"avatar\": \"\",\"isAvatarAudit\": false,\"isVip\": false,\"isNormalVip\": false,\"isSuperVip\": false,\"albumId\": 1,\"isHasMate\": true}");

                foreach (Cookie c1 in cox)
                {
                    if (c1.Name.ToUpper().Equals("NETEASE_WDA_UID"))
                    {
                        App.MyID = Regex.Split(HttpUtility.UrlDecode(c1.Value), "#|#", RegexOptions.IgnoreCase)[0];
                    }
                    c1.Path = "/";
                    c1.Value = HttpUtility.UrlEncode(c1.Value);
                    switch (c1.Name.ToUpper())
                    {
                        case "NTES_SESS":
                        case "P_INFO":
                        case "S_INFO":
                        case "NTES_PASSPORT":
                        case "_NTES_NNID":
                        case "_NTES_NUID":
                            c1.Domain = ".163.com";
                            cox_163com.Add(c1);
                            break;
                        case "NTES_REPLY_NICKNAME":
                            string[] para = HttpUtility.UrlDecode(c1.Value).Split('|');
                            App.MyLove["id"] = para[2];
                            c1.Domain = ".163.com";
                            cox_163com.Add(c1);
                            break;
                        case "__UTMC":
                        case "NETEASE_WDA_UID":
                        case "__UTMZ":
                        case "__UTMA":
                        case "__UTMB":
                        case "FROM-PAGE":
                        case "USER-FROM":
                            c1.Domain = ".love.163.com";
                            cox_love163com.Add(c1);
                            break;
                        case "EMAIL":
                            c1.Domain = "love.163.com";
                            cox__love163com.Add(c1);
                            break;
                    }

                }

                App.g_CookieContainer = new CookieContainer();
                App.g_CookieContainer.Add(new Uri("http://love.163.com"), cox_love163com);
                App.g_CookieContainer.Add(new Uri("http://love.163.com"), cox_163com);
                App.g_CookieContainer.Add(new Uri("http://love.163.com"), cox__love163com);

                Thread.Sleep(300);

                App.MyHomeHTML = webbrowser1.SaveToString();
                Match mc = Regex.Match(App.MyHomeHTML, "<span.* id=\"feedFilterParams\" (.|\n)*?>");
                if (mc.Success)
                {
                    App.Search = mc.Value.Replace("<span ", "").Replace("class=\"feed-filter-title-text\" ", "").Replace("id=\"feedFilterParams\" ", "").Replace("data-", "").Replace("\"", "").Replace(" ", "&").Replace(">", "").Replace("salary-require", "salaryRequire");
                }
                else
                    App.Search = "height=162-165&age=25-27&city=0&province=1";

                progressBar1.IsIndeterminate = false;
                IsolatedStorageSettingHelper.AddIsolateStorageObj("account", UserName.Text);
                IsolatedStorageSettingHelper.AddIsolateStorageObj("emailType", EmailType.Text);
                IsolatedStorageSettingHelper.AddIsolateStorageObj("password", Password.Password);

                NavigationService.Navigate(new System.Uri("/MainPage.xaml", UriKind.Relative));
                return;
            }
            if (e.Uri.ToString().Contains("https://reg.163.com/logins.jsp"))
            {
                progressBar1.IsIndeterminate = false;
                if (cox.Count < 7)
                {
                    MessageBox.Show("登录失败，账号密码错误");
                    btn_login.IsEnabled = true;
                    btn_login.Content = "登录";
                }
            }
            if (e.Uri.ToString().Contains("http://love.163.com/?username="))
            {
                progressBar1.IsIndeterminate = false;
                btn_login.IsEnabled = false;
                btn_login.Content = "尚不能使用";
                MessageBoxResult result = MessageBox.Show("您需要登录网页端，近一步完成注册\n\n方可使用本花田客户端\n\n花田网址：http://love.163.com \n\n去完善资料点击\"确定\"，否则\"取消\"", "提示", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    WebBrowserTask wt = new WebBrowserTask();
                    wt.Uri = new Uri("http://love.163.com", UriKind.Absolute);
                    wt.Show();
                }
            }
        }

        private void webbrowser1_Navigating(object sender, NavigatingEventArgs e)
        {
            if (progressBar1.IsIndeterminate) return;

            progressBar1.IsIndeterminate = true;
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Popup_EmailType.IsOpen)
            {
                e.Cancel = true;
                Popup_EmailType.IsOpen = false;
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

        private void UserName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UserName.Text.Trim().Equals("网易通行证/手机号"))
                UserName.Text = "";
        }

        private void UserName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (UserName.Text.Trim().Length == 0)
                UserName.Text = "网易通行证/手机号";
        }

        private void Password_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void Password_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Password.Password.Trim().Length == 0)
            {
                Password2.Visibility = System.Windows.Visibility.Visible;
                Password.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void Password2_GotFocus(object sender, RoutedEventArgs e)
        {
            Password.Visibility = System.Windows.Visibility.Visible;
            Password2.Focus();
            Password2.Visibility = System.Windows.Visibility.Collapsed;
            Password2.Focus();
        }

        private void Password2_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void EmailType_GotFocus(object sender, RoutedEventArgs e)
        {
            list_EmailType.Focus();
            Popup_EmailType.IsOpen = true;

        }

        private void Popup_EmailType_LostFocus(object sender, RoutedEventArgs e)
        {
            Popup_EmailType.IsOpen = false;
        }

        private void list_EmailType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (list_EmailType == null || list_EmailType.SelectedItem == null) return;

            var selectedItem = ((System.Windows.Controls.ContentControl)(list_EmailType.SelectedItem)).Content;
            Popup_EmailType.IsOpen = false;

            EmailType.Text = selectedItem as string;
        }

        private void list_EmailType_LostFocus(object sender, RoutedEventArgs e)
        {
            Popup_EmailType.IsOpen = false;
        }

    }
}