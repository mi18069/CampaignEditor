
using System.ComponentModel;

namespace Database.Entities
{
    public class MediaPlanTuple : INotifyPropertyChanged
    {
        private ObservableArray<MediaPlanTerm?> _terms;
        public MediaPlan MediaPlan { get; set; }

        public MediaPlanTuple(MediaPlan mediaPlan, ObservableArray<MediaPlanTerm?> terms)
        {
            MediaPlan = mediaPlan;
            Terms = terms;
        }

        public ObservableArray<MediaPlanTerm?> Terms
        {
            get => _terms;
            set
            {
                if (_terms != value)
                {
                    _terms = value;
                    OnPropertyChanged(nameof(Terms));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
