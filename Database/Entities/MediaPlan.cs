﻿using Database.DTOs.DayPartDTO;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Database.Entities
{
    public class MediaPlan : INotifyPropertyChanged
    {

        public int xmpid { get; set; }
        public int schid { get; set; }
        public int cmpid { get; set; }
        public int chid { get; set; }
        public string name { get; set; }
        public int version { get; set; }
        public string position { get; set; }
        public string stime { get; set; }
        public string? etime { get; set; }
        public string? blocktime { get; set; }
        public string days { get; set; }
        public string type { get; set; }
        public bool special { get; set; }
        public DateOnly sdate { get; set; }
        public DateOnly? edate { get; set; }
        public decimal progcoef { get; set; }
        public DateOnly created { get; set; }
        public DateOnly? modified { get; set; }
        public decimal amr1 { get; set; }
        public int amr1trim { get; set; }
        public decimal amr2 { get; set; }
        public int amr2trim { get; set; }
        public decimal amr3 { get; set; }
        public int amr3trim { get; set; }
        public decimal amrsale { get; set; }
        public int amrsaletrim { get; set; }
        public decimal amrp1 { get; set; }
        public decimal amrp2 { get; set; }
        public decimal amrp3 { get; set; }
        public decimal amrpsale { get; set; }
        public decimal dpcoef { get; set; }
        public decimal seascoef { get; set; }
        public decimal seccoef { get; set; }
        public decimal price { get; set; }
        public bool active { get; set; }

        public decimal pps { get; set; }
        public decimal coefA { get; set; }
        public decimal coefB { get; set; }
        public decimal cbrcoef { get; set; }

        private decimal cpp;

        private int _length;

        private int _insertations;

        private DayPartDTO _dayPart;
        public decimal chcoef = 1.0M;

        public int Affinity
        {
            get { return (int)Math.Round(Amrpsale != 0 ? (Amrp1 / Amrpsale) * 100 : 0); }
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        public string Position
        {
            get { return position; }
            set
            {
                position = value;
                OnPropertyChanged();
            }
        }

        public string Stime
        {
            get { return stime; }
            set
            {
                stime = value;
                OnPropertyChanged();
            }
        }

        public string? Etime
        {
            get { return etime; }
            set
            {
                etime = value;
                OnPropertyChanged();
            }
        }

        public string? Blocktime
        {
            get { return blocktime; }
            set
            {
                blocktime = value;
                OnPropertyChanged();
            }
        }

        public string Type
        {
            get { return type; }
            set
            {
                type = value;
                OnPropertyChanged();
            }
        }

        public bool Special
        {
            get { return special; }
            set
            {
                special = value;
                OnPropertyChanged();
            }
        }

        public decimal Amr1
        {
            get { return amr1 * (amr1trim / 100.0M); }
            set
            {
                amr1 = value * (100.0M / amr1trim);
                OnPropertyChanged();
                //OnPropertyChanged(nameof(Amrp1));
            }
        }

        public decimal Amr2
        {
            get { return amr2 * ((decimal)amr2trim / 100); }
            set
            {
                amr2 = value * (100 / (decimal)amr2trim);
                OnPropertyChanged();
                //OnPropertyChanged(nameof(Amrp2));

            }
        }

        public decimal Amr3
        {
            get { return amr3 * ((decimal)amr3trim / 100); }
            set
            {
                amr3 = value * (100 / (decimal)amr3trim);
                OnPropertyChanged();
                //OnPropertyChanged(nameof(Amrp3));
            }
        }

        public decimal Amrsale
        {
            get { return amrsale * ((decimal)amrsaletrim / 100); }
            set
            {
                amrsale = value * (100 / (decimal)amrsaletrim);
                OnPropertyChanged();
                //OnPropertyChanged(nameof(Amrpsale));
            }
        }

        public decimal Amrp1
        {
            get { return amrp1 * ((decimal)amr1trim / 100); }
            set
            {
                amrp1 = value * (100 / (decimal)amr1trim);
                OnPropertyChanged();
                OnPropertyChanged(nameof(Affinity));
                OnPropertyChanged(nameof(PricePerSecond));
                OnPropertyChanged(nameof(Price));
            }
        }

        public decimal Amrp2
        {
            get { return amrp2 * ((decimal)amr2trim / 100); }
            set
            {
                amrp2 = value * (100 / (decimal)amr2trim);
                OnPropertyChanged();
                OnPropertyChanged(nameof(Affinity));
                OnPropertyChanged(nameof(PricePerSecond));
                OnPropertyChanged(nameof(Price));
            }
        }

        public decimal Amrp3
        {
            get { return amrp3 * ((decimal)amr3trim / 100); }
            set
            {
                amrp3 = value * (100 / (decimal)amr3trim);
                OnPropertyChanged();
                OnPropertyChanged(nameof(Affinity));
                OnPropertyChanged(nameof(PricePerSecond));
                OnPropertyChanged(nameof(Price));
            }
        }

        public decimal Amrpsale
        {
            get { return amrpsale * ((decimal)amrsaletrim /100); }
            set
            {
                amrpsale = value * (100 / (decimal)amrsaletrim);
                OnPropertyChanged();
                OnPropertyChanged(nameof(Affinity));
                OnPropertyChanged(nameof(PricePerSecond));
                OnPropertyChanged(nameof(Price));
            }
        }

        public int Amr1trim
        {
            get { return amr1trim; }
            set
            {
                amr1trim = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Amr1));
                OnPropertyChanged(nameof(Amrp1));
            }
        }

        public int Amr2trim
        {
            get { return amr2trim; }
            set
            {
                amr2trim = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Amr2));
                OnPropertyChanged(nameof(Amrp2));

            }
        }

        public int Amr3trim
        {
            get { return amr3trim; }
            set
            {
                amr3trim = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Amr3));
                OnPropertyChanged(nameof(Amrp3));
            }
        }

        public int Amrsaletrim
        {
            get { return amrsaletrim; }
            set
            {
                amrsaletrim = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Amrsale));
                OnPropertyChanged(nameof(Amrpsale));
            }
        }

        public int Insertations
        {
            get { return _insertations; }
            set
            {
                _insertations = value;
                OnPropertyChanged();
            }
        }

        public decimal Dpcoef
        {
            get { return dpcoef; }
            set
            {
                dpcoef = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PricePerSecond));
            }
        }

        public decimal Progcoef
        {
            get { return progcoef; }
            set
            {
                progcoef = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Price));
            }
        }

        public decimal Seascoef
        {
            get { return seascoef; }
            set
            {
                seascoef = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Price));
            }
        }

        public decimal Seccoef
        {
            get { return seccoef; }
            set
            {
                seccoef = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Price));
            }
        }

        public decimal CoefA
        {
            get { return coefA; }
            set
            {
                coefA = value;
                OnPropertyChanged();
            }
        }

        public decimal CoefB
        {
            get { return coefB; }
            set
            {
                coefB = value;
                OnPropertyChanged();
            }
        }

        public decimal Cbrcoef
        {
            get { return cbrcoef; }
            set
            {
                cbrcoef = value;
                OnPropertyChanged();
            }
        }

        public int Length
        {
            get { return _length; }
            set 
            { 
                _length = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Price));
                OnPropertyChanged(nameof(PricePerSecond));
                OnPropertyChanged(nameof(AvgLength));
            }
        }

        public decimal AvgLength
        {
            get { return (decimal)_length / (Insertations == 0 ? 1 : Insertations) ; }
        }

        public decimal Cpp 
        { 
            get { return cpp; }
            set 
            { 
                cpp = value;
                OnPropertyChanged();
            } 
        }

        public decimal Grp
        {
            get { return Insertations * Amrp1; }
        }

        public decimal Price
        {
            get { return price; }
            set 
            { 
                price = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Cpp));
            }
        }

        public decimal PricePerSecond
        {
            get { return pps; }
            set
            {
                pps = value;
                OnPropertyChanged();
            }
        }

        public DayPartDTO DayPart
        {
            get { return _dayPart; }
            set { _dayPart = value; }
        }

        public decimal Chcoef
        {
            get { return chcoef; }
            set
            {
                chcoef = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
    }
}
