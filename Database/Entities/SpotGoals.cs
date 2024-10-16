using Database.DTOs.ChannelDTO;
using Database.DTOs.MediaPlanTermDTO;
using Database.DTOs.SpotDTO;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Database.Entities
{
    public class SpotGoals : INotifyPropertyChanged
    {

        private int insertations;
        private decimal grp;
        private decimal grp2;
        private decimal grp3;
        private decimal budget;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Insertations
        {
            get { return insertations; }
            set 
            { 
                insertations = value; 
                OnPropertyChanged();
            }
        }

        public decimal Grp
        {
            get { return grp; }
            set 
            { 
                grp = value; 
                OnPropertyChanged();
            }
        }
        public decimal Grp2
        {
            get { return grp2; }
            set
            {
                grp2 = value;
                OnPropertyChanged();
            }
        }
        public decimal Grp3
        {
            get { return grp3; }
            set
            {
                grp3 = value;
                OnPropertyChanged();
            }
        }

        public decimal Budget
        {
            get { return budget; }
            set 
            { 
                budget = value; 
                OnPropertyChanged();
            }
        }
        public SpotGoals()
        {
            Insertations = 0;
            Grp = 0;
            Grp2 = 0;
            Grp3 = 0;
            Budget = 0;
        }

        public void ResetValues()
        {
            Insertations = 0;
            Grp = 0;
            Grp2 = 0;
            Grp3 = 0;
            Budget = 0;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

        public static SpotGoals operator +(SpotGoals a, SpotGoals b)
        {
            if (a == null || b == null)
            {
                throw new ArgumentNullException("Cannot add null SpotGoals.");
            }

            return new SpotGoals
            {
                Insertations = a.Insertations + b.Insertations,
                Grp = a.Grp + b.Grp,
                Grp2 = a.Grp2 + b.Grp2,
                Grp3 = a.Grp3 + b.Grp3,
                Budget = a.Budget + b.Budget
            };
        }
    }
}
