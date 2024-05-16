using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Database.Entities
{
    public class MPGoals : INotifyCollectionChanged, INotifyPropertyChanged
    {
        private ObservableCollection<MediaPlan> mediaPlans;
        private decimal grp;
        private int insertations;
        private decimal budget;

        public decimal Grp
        {
            get { return grp; }
            set
            {
                grp = value;
                OnPropertyChanged(nameof(Grp));
                OnPropertyChanged(nameof(GrpRounded));
            }
        }

        public decimal GrpRounded
        {
            get { return Math.Round(grp, 2); }
            set
            {
                grp = value;
                OnPropertyChanged(nameof(GrpRounded));
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
            get { return budget; }
            set
            {
                budget = value;
                OnPropertyChanged(nameof(Budget));
                OnPropertyChanged(nameof(BudgetRounded));
            }
        }

        public decimal BudgetRounded
        {
            get { return Math.Round(budget, 2); }
            set
            {
                budget = value;
                OnPropertyChanged(nameof(BudgetRounded));
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
            decimal budget = 0.0M;
            decimal grp = 0.0M;
            int insertations = 0;

            foreach (var mediaPlan in mediaPlans)
            {
                budget += mediaPlan.Price;
                insertations += mediaPlan.Insertations;
                grp += mediaPlan.Insertations * mediaPlan.Amrp1;
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
