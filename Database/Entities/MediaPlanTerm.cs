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
        private string? _added;
        private string? _deleted;

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
                    OnPropertyChanged(nameof(Status));
                }
            }
        }
        public string? Added
        {
            get { return _added; }
            set
            {
                if (_added != value)
                {
                    _added = value;
                    OnPropertyChanged(nameof(Added));
                }
            }
        }
        public string? Deleted
        {
            get { return _deleted; }
            set
            {
                if (_deleted != value)
                {
                    _deleted = value;
                    OnPropertyChanged(nameof(Deleted));
                }
            }
        }

        // 0 - No changes
        // 1 - Added
        // 2 - Deleted
        // 3 - Added and Deleted - Modified
        public int Status
        {
            get
            {
                if (string.IsNullOrEmpty(_added) && string.IsNullOrEmpty(_deleted))
                    return 0;
                else if (string.IsNullOrEmpty(_added) && !string.IsNullOrEmpty(_deleted))
                    return 1;
                else if (!string.IsNullOrEmpty(_added) && string.IsNullOrEmpty(_deleted))
                    return 2;
                else 
                    return 3;

            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
