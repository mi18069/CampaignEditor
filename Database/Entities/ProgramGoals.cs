using Database.DTOs.ChannelDTO;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Database.Entities
{
    public class ProgramGoals : INotifyPropertyChanged
    {
        private ChannelDTO channel;
        private int insertations;
        private double grp;
        private double budget;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ChannelDTO Channel
        {
            get { return channel; }
            set { channel = value; }
        }

        public int Insertations
        {
            get { return insertations; }
            set { insertations = value; }
        }

        public double Grp
        {
            get { return Math.Round(grp, 2); }
            set { grp = value; }
        }

        public double Budget
        {
            get { return Math.Round(budget, 2); }
            set { budget = value; }
        }
        public ProgramGoals(ChannelDTO channel)
        {
            this.channel = channel;
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
