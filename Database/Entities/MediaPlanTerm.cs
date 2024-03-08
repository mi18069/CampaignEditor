using System;
using System.ComponentModel;

namespace Database.Entities
{
    public class MediaPlanTerm : INotifyPropertyChanged
    {
        private int _xmptermid;
        private int _xmpid;
        private DateOnly _date;
        private string? _spotcode;

        public int Xmptermid
        {
            get { return _xmptermid; }
            set
            {
                _xmptermid = value;
            }
        }
        public int Xmpid
        {
            get { return _xmpid; }
            set
            {
                _xmpid = value;
            }
        }
        public DateOnly Date
        {
            get { return _date; }
            set
            {
                _date = value;
            }
        }
        public string? Spotcode
        {
            get { return _spotcode; }
            set
            {
                if (_spotcode != value)
                {
                    _spotcode = value;
                    OnPropertyChanged(nameof(Spotcode));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
