using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Database.Entities
{
    public class MPGoals : INotifyCollectionChanged, INotifyPropertyChanged
    {
        private ObservableCollection<MediaPlan> mediaPlans;
        private double grp;
        private int insertations;
        private double budget;

        public double Grp
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
        public double Budget
        {
            get { return Math.Round(budget, 2); }
            set
            {
                budget = value;
                OnPropertyChanged(nameof(Budget));
            }
        }

        public MPGoals()
        {
            mediaPlans = new ObservableCollection<MediaPlan>();
        }

        public ObservableCollection<MediaPlan> MediaPlans
        {
            get { return mediaPlans; }
            set
            {
                if (mediaPlans != value)
                {
                    if (mediaPlans != null)
                    {
                        // Unsubscribe from the PropertyChanged event of existing MediaPlan instances
                        foreach (var mediaPlan in mediaPlans)
                        {
                            mediaPlan.PropertyChanged -= MediaPlan_PropertyChanged;
                        }
                        mediaPlans.CollectionChanged -= MediaPlans_CollectionChanged;
                    }

                    mediaPlans = value;

                    if (mediaPlans != null)
                    {
                        // Subscribe to the PropertyChanged event of new MediaPlan instances
                        foreach (var mediaPlan in mediaPlans)
                        {
                            mediaPlan.PropertyChanged += MediaPlan_PropertyChanged;
                        }
                        mediaPlans.CollectionChanged += MediaPlans_CollectionChanged;
                    }

                    CalculateGoals();
                    OnPropertyChanged(nameof(MediaPlans));
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }
        }

        private void MediaPlans_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CalculateGoals();
            OnCollectionChanged(e);
            OnPropertyChanged(nameof(Budget));
            OnPropertyChanged(nameof(Insertations));
            OnPropertyChanged(nameof(Grp));

        }

        private void CalculateGoals()
        {
            var budget = 0.0;
            var grp = 0.0;
            var insertations = 0;

            foreach (var mediaPlan in mediaPlans)
            {
                budget += mediaPlan.Price;
                insertations += mediaPlan.Insertations;
                grp += mediaPlan.Insertations * (mediaPlan.Amrp1 + mediaPlan.Amrp2 + mediaPlan.Amrp3);
            }

            Budget = budget;
            Grp = grp;
            Insertations = insertations;
        }


        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MediaPlan_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Handle property changes of MediaPlan instances
            CalculateGoals();
            OnPropertyChanged(nameof(MediaPlans));
        }
    }
}
