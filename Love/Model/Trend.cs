using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Love.Model
{
    public class Trend : INotifyPropertyChanged
    {
        string _id, _content, _richContent, _timestamp, _letterTemplateId, _letterContent, _source;
        User _user;
        int _diggCount;
        bool _isDigged, _isLoveNotes, _isQaTrend, _isPhotoTrend, _isMoreButtonTrend;
        List<Photo> _photolist;
        Question _question;

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
        public string Content
        {
            get { return _content; }
            set
            {
                if (_content != value)
                {
                    _content = value;
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
        public string Timestamp
        {
            get { return _timestamp; }
            set
            {
                if (_timestamp != value)
                {
                    _timestamp = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int DiggCount
        {
            get { return _diggCount; }
            set
            {
                if (_diggCount != value)
                {
                    _diggCount = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool IsDigged
        {
            get { return _isDigged; }
            set
            {
                if (_isDigged != value)
                {
                    _isDigged = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool IsLoveNotes
        {
            get { return _isLoveNotes; }
            set
            {
                if (_isLoveNotes != value)
                {
                    _isLoveNotes = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool IsQaTrend
        {
            get { return _isQaTrend; }
            set
            {
                if (_isQaTrend != value)
                {
                    _isQaTrend = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool IsPhotoTrend
        {
            get { return _isPhotoTrend; }
            set
            {
                if (_isPhotoTrend != value)
                {
                    _isPhotoTrend = value;
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
        public string LetterTemplateId
        {
            get { return _letterTemplateId; }
            set
            {
                if (_letterTemplateId != value)
                {
                    _letterTemplateId = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string LetterContent
        {
            get { return _letterContent.Replace("<br /> <br /> —— 来自Windows·Phone", "").Replace("<br> ", "\n"); }
            set
            {
                if (_letterContent != value)
                {
                    _letterContent = value;
                    NotifyPropertyChanged();
                }
            }
        }

        //Android客户端
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

        public List<Photo> Photolist
        {
            get { return _photolist; }
            set
            {
                if (_photolist != value)
                {
                    _photolist = value;
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
        public Question QA
        {
            get { return _question; }
            set
            {
                if (_question != value)
                {
                    _question = value;
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
