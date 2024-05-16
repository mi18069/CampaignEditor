using System;
using System.ComponentModel;

namespace Database.Entities
{
    public class SelectedMPGoals : INotifyPropertyChanged
    {
        private MediaPlan mediaPlan;
        private decimal grp;
        private int insertations;
        private decimal budget;

        public event PropertyChangedEventHandler? PropertyChanged;

        public decimal Grp
        {
            get { return Math.Round(grp, 2); }
            set
            {
                grp = value;
                OnPropertyChanged(nameof(Grp));
            }
        }
        public int Insertations
        {
            get { return insertations; }
            set
            {
                insertations = value;
                OnPropertyChanged(nameof(Insertations));
            }
        }
        public decimal Budget
        {
            get { return Math.Round(budget, 2); }
            set
            {
                budget = value;
                OnPropertyChanged(nameof(Budget));
            }
        }

        public SelectedMPGoals()
        {
        }

        public MediaPlan MediaPlan
        {
            get { return mediaPlan; }
            set
            {
                if (mediaPlan != null)
                {
                    mediaPlan.PropertyChanged -= MediaPlan_PropertyChanged;
                }

                mediaPlan = value;

                if (mediaPlan != null)
                {
                    mediaPlan.PropertyChanged += MediaPlan_PropertyChanged;
                }

                CalculateGoals();
                OnPropertyChanged(nameof(MediaPlan));
            }
        }
        private void CalculateGoals()
        {
            Budget = mediaPlan.Price;
            Insertations = mediaPlan.Insertations;
            Grp = mediaPlan.Grp;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MediaPlan_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Handle property changes of MediaPlan instances
            CalculateGoals();
            OnPropertyChanged(nameof(MediaPlan));
        }

    }
}
