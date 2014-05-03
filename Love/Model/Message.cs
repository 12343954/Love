using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Love.Model
{
    public class Message : INotifyPropertyChanged
    {
        string _id, _prettyTime, _richContent, _type, _source;
        bool isFromLoginUser, _lastTopMessage, _isMoreButtonTrend;
        int _newCount, _payType, _totalCount, _newUsersCount;
        User _user, _toUser;
        JObject _relatedData;

        public string ID
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string PrettyTime
        {
            get { return _prettyTime; }
            set
            {
                if (_prettyTime != value)
                {
                    _prettyTime = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string RichContent
        {
            get { return _richContent; }
            set
            {
                if (_richContent != value)
                {
                    _richContent = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Source
        {
            get { return _source; }
            set
            {
                if (_source != value)
                {
                    _source = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Type
        {
            get { return _type; }
            set
            {
                if (_type != value)
                {
                    _type = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool IsFromLoginUser
        {
            get { return isFromLoginUser; }
            set
            {
                if (isFromLoginUser != value)
                {
                    isFromLoginUser = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool LastTopMessage
        {
            get { return _lastTopMessage; }
            set
            {
                if (_lastTopMessage != value)
                {
                    _lastTopMessage = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool IsMoreButtonTrend
        {
            get { return _isMoreButtonTrend; }
            set
            {
                if (_isMoreButtonTrend != value)
                {
                    _isMoreButtonTrend = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int NewCount
        {
            get { return _newCount; }
            set
            {
                if (_newCount != value)
                {
                    _newCount = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int PayType
        {
            get { return _payType; }
            set
            {
                if (_payType != value)
                {
                    _payType = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int TotalCount
        {
            get { return _totalCount; }
            set
            {
                if (_totalCount != value)
                {
                    _totalCount = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int NewUsersCount
        {
            get { return _newUsersCount; }
            set
            {
                if (_newUsersCount != value)
                {
                    _newUsersCount = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public User User
        {
            get { return _user; }
            set
            {
                if (_user != value)
                {
                    _user = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public User ToUser
        {
            get { return _toUser; }
            set
            {
                if (_toUser != value)
                {
                    _toUser = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public JObject RelatedData {
            get { return _relatedData; }
            set
            {
                if (_relatedData != value)
                {
                    _relatedData = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }

        }
    }
}
