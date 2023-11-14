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
        public float progcoef { get; set; }
        public DateOnly created { get; set; }
        public DateOnly? modified { get; set; }
        public double amr1 { get; set; }
        public int amr1trim { get; set; }
        public double amr2 { get; set; }
        public int amr2trim { get; set; }
        public double amr3 { get; set; }
        public int amr3trim { get; set; }
        public double amrsale { get; set; }
        public int amrsaletrim { get; set; }
        public double amrp1 { get; set; }
        public double amrp2 { get; set; }
        public double amrp3 { get; set; }
        public double amrpsale { get; set; }
        public double dpcoef { get; set; }
        public double seascoef { get; set; }
        public double seccoef { get; set; }
        public double price { get; set; }
        public bool active { get; set; }

        public double pps { get; set; }

        private double cpp;

        private int _length;

        private int _insertations;

        public double Affinity
        {
            get { return Amrpsale != 0 ? (Amrp1 / Amrpsale) * 100 : 0; }
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

        public double Amr1
        {
            get { return amr1 * ((double)amr1trim / 100); }
            set
            {
                amr1 = value * (100 / (double)amr1trim);
                OnPropertyChanged();
                //OnPropertyChanged(nameof(Amrp1));
            }
        }

        public double Amr2
        {
            get { return amr2 * ((double)amr2trim / 100); }
            set
            {
                amr2 = value * (100 / (double)amr2trim);
                OnPropertyChanged();
                //OnPropertyChanged(nameof(Amrp2));

            }
        }

        public double Amr3
        {
            get { return amr3 * ((double)amr3trim / 100); }
            set
            {
                amr3 = value * (100 / (double)amr3trim);
                OnPropertyChanged();
                //OnPropertyChanged(nameof(Amrp3));
            }
        }

        public double Amrsale
        {
            get { return amrsale * ((double)amrsaletrim / 100); }
            set
            {
                amrsale = value * (100 / (double)amrsaletrim);
                OnPropertyChanged();
                //OnPropertyChanged(nameof(Amrpsale));
            }
        }

        public double Amrp1
        {
            get { return amrp1 * ((double)amr1trim / 100); }
            set
            {
                amrp1 = value * (100 / (double)amr1trim);
                OnPropertyChanged();
                OnPropertyChanged(nameof(Affinity));
                OnPropertyChanged(nameof(PricePerSecond));
                OnPropertyChanged(nameof(Price));
            }
        }

        public double Amrp2
        {
            get { return amrp2 * ((double)amr2trim / 100); }
            set
            {
                amrp2 = value * (100 / (double)amr2trim);
                OnPropertyChanged();
                OnPropertyChanged(nameof(Affinity));
                OnPropertyChanged(nameof(PricePerSecond));
                OnPropertyChanged(nameof(Price));
            }
        }

        public double Amrp3
        {
            get { return amrp3 * ((double)amr3trim / 100); }
            set
            {
                amrp3 = value * (100 / (double)amr3trim);
                OnPropertyChanged();
                OnPropertyChanged(nameof(Affinity));
                OnPropertyChanged(nameof(PricePerSecond));
                OnPropertyChanged(nameof(Price));
            }
        }

        public double Amrpsale
        {
            get { return amrpsale * ((double)amrsaletrim /100); }
            set
            {
                amrpsale = value * (100 / (double)amrsaletrim);
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

        public double Dpcoef
        {
            get { return dpcoef; }
            set
            {
                dpcoef = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PricePerSecond));
            }
        }

        public float Progcoef
        {
            get { return progcoef; }
            set
            {
                progcoef = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Price));
            }
        }

        public double Seascoef
        {
            get { return seascoef; }
            set
            {
                seascoef = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Price));
            }
        }

        public double Seccoef
        {
            get { return seccoef; }
            set
            {
                seccoef = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Price));
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

        public double AvgLength
        {
            get { return (double)_length / (Insertations == 0 ? 1 : Insertations) ; }
        }

        public double Cpp 
        { 
            get { return cpp; }
            set 
            { 
                cpp = value;
                OnPropertyChanged();
            } 
        }

        public double Price
        {
            get { return price; }
            set 
            { 
                price = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Cpp));
            }
        }

        public double PricePerSecond
        {
            get { return pps; }
            set
            {
                pps = value;
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
