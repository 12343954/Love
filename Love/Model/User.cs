using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Love.Model
{
    public class User : INotifyPropertyChanged
    {
        string _id, _nickName, _url, _avatar, _age, _height, _education, _salary, _province, _city, _albumId,
            _school, _company, _isOnline, _sex;
        bool _isAvatarAudit, _following, _followed;
        int _photoCount;

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

        public string NickName
        {
            get { return _nickName; }
            set
            {
                if (_nickName != value)
                {
                    _nickName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Url
        {
            get { return _url; }
            set
            {
                if (_url != value)
                {
                    _url = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Avatar
        {
            get { return _avatar; }
            set
            {
                if (_avatar != value)
                {
                    _avatar = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Age
        {
            get { return _age; }
            set
            {
                if (_age != value)
                {
                    _age = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Height
        {
            get { return _height; }
            set
            {
                if (_height != value)
                {
                    _height = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Education
        {
            get { return _education; }
            set
            {
                if (_education != value)
                {
                    _education = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Salary
        {
            get { return _salary; }
            set
            {
                if (_salary != value)
                {
                    _salary = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Province
        {
            get { return _province; }
            set
            {
                if (_province != value)
                {
                    _province = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string City
        {
            get { return _city; }
            set
            {
                if (_city != value)
                {
                    _city = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string AlbumID
        {
            get { return _albumId; }
            set
            {
                if (_albumId != value)
                {
                    _albumId = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string School
        {
            get { return _school; }
            set
            {
                if (_school != value)
                {
                    _school = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Company
        {
            get { return _company; }
            set
            {
                if (_company != value)
                {
                    _company = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string IsOnline
        {
            get { return _isOnline; }
            set
            {
                if (_isOnline != value)
                {
                    _isOnline = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool IsAvatarAudit
        {
            get { return _isAvatarAudit; }
            set
            {
                if (_isAvatarAudit != value)
                {
                    _isAvatarAudit = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool Following
        {
            get { return _following; }
            set
            {
                if (_following != value)
                {
                    _following = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool Followed
        {
            get { return _followed; }
            set
            {
                if (_followed != value)
                {
                    _followed = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Sex
        {
            get { return _sex; }
            set
            {
                if (_sex != value)
                {
                    _sex = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int PhotoCount
        {
            get { return _photoCount; }
            set
            {
                if (_photoCount != value)
                {
                    _photoCount = value;
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
