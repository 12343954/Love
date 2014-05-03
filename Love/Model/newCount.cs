using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Love.Model
{
    public class newCount : INotifyPropertyChanged
    {
        public newCount() { }

        public string newDigg { get; set; }
        public string newFollow { get; set; }
        public string newMessage { get; set; }
        public string newNotice { get; set; }
        public string newVisitors { get; set; }
        public string userId { get; set; }

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
