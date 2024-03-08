
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Database.Entities
{
    public class Channel : INotifyPropertyChanged
    {
        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set 
            { 
                _isSelected = value;
                OnPropertyChanged();
            }
        }
        public int chid { get; set; }
        public bool chactive { get; set; }
        public string chname { get; set; }
        public int chrdsid { get; set; }
        public string chsname { get; set; }
        public int shid { get; set; }
        public int chrfid { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
    }
}
