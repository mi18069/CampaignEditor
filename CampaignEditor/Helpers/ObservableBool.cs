using System.ComponentModel;

namespace CampaignEditor.Helpers
{
    public class ObservableBool : INotifyPropertyChanged
    {
        private bool _value;

        public bool Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public ObservableBool(bool value)
        {
            Value = value;
        }



        // Implement INotifyPropertyChanged interface with OnPropertyChanged method
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
