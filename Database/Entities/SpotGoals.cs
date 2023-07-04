using Database.DTOs.ChannelDTO;
using Database.DTOs.MediaPlanTermDTO;
using Database.DTOs.SpotDTO;
using System;

namespace Database.Entities
{
    public class SpotGoals
    {

        private int insertations;
        private double grp;
        private double budget;

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
        public SpotGoals()
        {
            Insertations = 0;
            Grp = 0;
            Budget = 0;
        }

        
    }
}
