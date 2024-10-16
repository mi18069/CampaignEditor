﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Database.Entities
{
    public class MediaPlanHist : INotifyPropertyChanged
    {
        public bool Active
        {
            get { return active; }
            set
            {
                active = value;
                OnPropertyChanged();
            }
        }

        public int xmphistid { get; set; }
        public int xmpid { get; set; }
        public int schid { get; set; }
        public int chid { get; set; }
        public string name { get; set; }
        public string position { get; set; }
        public string stime { get; set; }
        public string? etime { get; set; }
        public DateOnly date { get; set; }
        public float progcoef { get; set; }
        public decimal amr1 { get; set; }
        public decimal amr2 { get; set; }
        public decimal amr3 { get; set; }
        public decimal amrsale { get; set; }
        public decimal amrp1 { get; set; }
        public decimal amrp2 { get; set; }
        public decimal amrp3 { get; set; }
        public decimal amrpsale { get; set; }
        public bool active { get; set; }
        public bool outlier { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
