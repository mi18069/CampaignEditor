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
        private decimal grp1;
        private decimal grp2;
        private decimal grp3;
        private decimal budget;

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

        public decimal Grp1
        {
            get { return grp1; }
            set { grp1 = value; }
        }
        public decimal Grp2
        {
            get { return grp2; }
            set { grp2 = value; }
        }
        public decimal Grp3
        {
            get { return grp3; }
            set { grp3 = value; }
        }

        public decimal Budget
        {
            get { return budget; }
            set { budget = value;}
        }
        public ProgramGoals(ChannelDTO channel)
        {
            this.channel = channel;
            Insertations = 0;
            Grp1 = 0;
            Grp2 = 0;
            Grp3 = 0;
            Budget = 0;
        }

        public void SetValues(ProgramGoals pg)
        {
            Insertations = pg.Insertations;
            Grp1 = pg.Grp1;
            Grp2 = pg.Grp2;
            Grp3 = pg.Grp3;
            Budget = pg.Budget;
            OnPropertyChanged();

        }

        public void OnPropertyChanged([CallerMemberName] string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
    }
}
