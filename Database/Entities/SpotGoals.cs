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
        private double grp;
        private double budget;

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

        public double Grp
        {
            get { return grp; }
            set 
            { 
                grp = value; 
                OnPropertyChanged();
            }
        }

        public double Budget
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
            Budget = 0;
        }

        public void ResetValues()
        {
            Insertations = 0;
            Grp = 0;
            Budget = 0;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
    }
}
