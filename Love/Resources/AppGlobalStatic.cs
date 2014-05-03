using Microsoft.Phone.Info;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace Love.Resources
{
    static class AppGlobalStatic
    {
        public static CookieContainer cookieContainer;
        /// <summary>
        /// 网易登录地址 
        /// 参数：{0}:username {1}:password
        /// </summary>
        public static string Netease_LoginURL = "https://reg.163.com/logins.jsp?url=&product=&savelogin=&outfoxer=&domains=&syscheckcode=034715eb7e1f16cd332e41f95e3303204ceadd7e&username={0}&password={1}&Submit=";

        /// <summary>
        /// 全局User-Agent
        /// </summary>
        public static string User_Agent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";

        /// <summary>
        /// 消息MessagesURL
        /// 用法：AppGlobalStatic.MessagesURL["花田"]
        /// </summary>
        public static Dictionary<string, string> MessagesURL = new Dictionary<string, string>
        { 
            {"花田","http://love.163.com/trend/list"}, //height=162-165&age=25-27&city=0&province=1&pageToken=3
                                                                                //province=1&city=0&age=25-27&height=162-170&education=3-1&salaryRequire=2-0&pageToken=2
            {"文字传情","http://love.163.com/trend/writeLoveNotes"},//templateId=1&content=aaaaaaaaaaaaaaaaaaaaaaa --XMLHttpRequest
            {"删除动态","http://love.163.com/trend/delete"},//trendId=3157662356479847904 --XMLHttpRequest
            {"新动态","http://love.163.com/home/newInfo"},//height=162-165&age=25-27&city=0&province=1&lastTrendTime=1376747837450 --XMLHttpRequest
            {"不感兴趣","http://love.163.com/relation/reject"},//userId=587800352364240787
            {"最近来访","http://love.163.com/messages/visitor"}, //html页面，硬解析
            {"最近赞过我的","http://love.163.com/messages/like/praiseList"},//pageNo=2
            {"收藏","http://love.163.com/following/list"},//pageNo=2
            {"最近收藏我的","http://love.163.com/messages/followers/list"},//pageNo=2
            {"取消收藏","http://love.163.com/relation/unfollow"},//userId=2975793562372206859  --XMLHttpRequest--Transfer-Encoding=chunked
            {"私信","http://love.163.com/messages/dmTimeline"},//withUserId=-7351731031069715476&pageNo=2
            {"消息器","http://love.163.com/newCount"},
            {"点赞","http://love.163.com/messages/like/praise"},//trendId=-2908161503669088367  --XMLHttpRequest
            {"发私信","http://love.163.com/messages/add"},//withUserId=-6657986368473697133&content=%E8%8A%B1%E7%94%B0%E6%8F&isSetTop=0 --&trendId:7914952496683816177 --XMLHttpRequest
            {"TA的动态","http://love.163.com/trend/usertimeline"},//userId=8669081578819327078&size=20&pageToken=1376004255461 --XMLHttpRequest
            {"全部消息","http://love.163.com/messages/notice"}//isUnread=0&isMatched=0&pageNo=1 
            ,{"数据统计","http://love.163.com/user/anniversaryData"}
        };


        /// <summary>
        /// 图片转换表情文字
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string ImgToEmotion(string content)
        {
            MatchCollection mc = Regex.Matches(content, "<img (.*?)>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            foreach (Match mcc in mc)
            {
                content = content.Replace(mcc.Value, " [" + Regex.Match(mcc.Value, "title=\"(.*)\"").Groups[1].ToString() + "]");
            }
            if (content.Equals("[]"))
                content = "[emoji]";

            return content;
        }

        //清除超链接，返回纯文本
        public static string ClearAhref(string content)
        {
            //@"<a\s*[^>]*>([\s\S]+?)</a>"
            MatchCollection mc = Regex.Matches(content, @"<a\s*[^>]*>(.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            foreach (Match mcc in mc)
            {
                content = Regex.Replace(content, @"<a\s*[^>]*>(.*?)</a>", "$1");
            }
            return content.Replace("<br /> <br />  —— 来自Windows·Phone", "").Replace("<br />", "\n");
        }

        /// <summary>
        /// 格式化来源
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string Source(string source, string richContent = "")
        {
            if (Regex.IsMatch(source, "iphone", RegexOptions.IgnoreCase))
                return "来自爱疯";
            if (Regex.IsMatch(source, "android", RegexOptions.IgnoreCase))
                return "来自安猪";

            if (richContent.Contains("来自Windows·Phone"))
                return "来自WP8";

            return "来自田里";
        }

        //学历，从1开始
        public static string[] Xueli = new string[] { "", "大专以下", "大专", "本科", "硕士", "博士", "" };

        //生肖
        public static string AgeToAnimal(string age)
        {
            //string[] Animal = new string[] { "鼠", "牛", "虎", "兔", "龙", "蛇", "马", "羊", "猴", "鸡", "狗", "猪" };
            //32%12=8 --2
            //31%12=7 --1
            //30%12=6 --0
            //29%12=5 --11
            //28%12=4 --10
            //27%12=3 --9
            //26%12=2 --8
            string[] Animal = new string[] { "猪", "狗", "鸡", "猴", "羊", "马", "蛇", "龙", "兔", "虎", "牛", "鼠" };

            int iAge = 0;

            int.TryParse(age, out iAge);

            iAge = iAge % 12;

            if (iAge == 6)
                return Animal[0];
            else if (iAge > 6)
                return Animal[iAge - 6];
            else
                return Animal[iAge + 6];

        }
        /// <summary>
        /// 时间差函数
        /// </summary>
        /// <param name="dateTime">对比时间</param>
        /// <returns>时间差</returns>
        public static string DateStringFromNow(string dateTime)
        {
            DateTime _dateTime;
            if (!DateTime.TryParse(dateTime, out _dateTime)) return dateTime;
            TimeSpan span = DateTime.Now - _dateTime;

            if (span.TotalSeconds < 1)
                return "刚刚";

            if (span.TotalMinutes < 1)
                return string.Format("{0}秒前", (int)Math.Floor(span.TotalSeconds));

            if (span.TotalHours < 1)
                return string.Format("{0}分钟前", (int)Math.Floor(span.TotalMinutes));

            if (span.TotalDays < 1)
                return string.Format("{0}小时前", (int)Math.Floor(span.TotalHours));

            if (span.TotalDays < 30)
                return string.Format("{0}天前", (int)Math.Floor(span.TotalDays));
            if (span.TotalDays < 60)
                return "1个月前";
            if (span.TotalDays < 90)
                return "2个月前";
            if (span.TotalDays < 120)
                return "3个月前";
            if (span.TotalDays < 150)
                return "4个月前";
            if (span.TotalDays < 180)
                return "5个月前";
            if (span.TotalDays < 210)
                return "半年前";
            if (span.TotalDays < 240)
                return "7个月前";
            if (span.TotalDays < 270)
                return "8个月前";
            if (span.TotalDays < 300)
                return "9个月前";
            if (span.TotalDays < 330)
                return "10个月前";
            if (span.TotalDays < 366)
                return "11个月前";
            if (span.TotalDays < 720)
                return "1年前";
            if (span.TotalDays < 1080)
                return "2年前";
            if (span.TotalDays < 1440)
                return "3年前";
            if (span.TotalDays < 1830)
                return "4年前";
            if (span.TotalDays < 2196)
                return "5年前";

            return _dateTime.ToString("yyyy-MM-dd hh:mm");


        }

        /// <summary>
        /// 数字格式化
        /// </summary>
        /// <param name="num"></param>
        /// <returns>返回 万计数单位</returns>
        public static string NumberFomat(string num)
        {
            //8231253 = 823.1万
            //  371910 = 37.2万
            //      1522  return
            if (num.Length < 5) return num;

            return num.Substring(0, num.Length - 4) + "." + num.Substring(num.Length - 4, 1) + "万";
        }


        #region 时间戳互转
        public static readonly DateTime UnixTimestampLocalZero = System.TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), TimeZoneInfo.Local);
        public static readonly DateTime UnixTimestampUtcZero = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        //本地时间 --> 时间戳
        public static long ConvertLocalToTimestamp(DateTime datetime)
        {
            return (long)(datetime - UnixTimestampLocalZero).TotalMilliseconds;
        }
        //UTC ==> 时间戳
        public static long ConvertUtcToTimestamp(DateTime datetime)
        {
            return (long)(datetime - UnixTimestampUtcZero).TotalMilliseconds;
        }
        //时间戳  ==> 本地时间
        public static DateTime ConvertLocalFromTimestamp(long timestamp)
        {
            return UnixTimestampLocalZero.AddMilliseconds(timestamp);
        }
        //时间戳 ==>  UTC
        public static DateTime ConvertUtcFromTimestamp(long timestamp)
        {
            return UnixTimestampUtcZero.AddMilliseconds(timestamp);
        }
        #endregion
    }
}
