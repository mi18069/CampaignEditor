using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Database.Entities
{
    public class MediaPlanRealized : INotifyPropertyChanged
    {
        private decimal? _price;
        private int? _status;

        public int? id { get; set; }
        public int? cmpid { get; set; }
        public string name { get; set; }
        public int? stime { get; set; }
        public int? etime { get; set; }
        public string stimestr { get; set; }
        public string etimestr { get; set; }
        public int? chid { get; set; }
        public int? dure { get; set; }
        public int? durf { get; set; }
        public string date { get; set; }
        public int? emsnum { get; set; }
        public int? posinbr { get; set; }
        public int? totalspotnum { get; set; }
        public int? breaktype { get; set; }
        public int? spotnum { get; set; }
        public int? brandnum { get; set; }
        public decimal amrp1 { get; set; }
        public decimal amrp2 { get; set; }
        public decimal amrp3 { get; set; }
        public decimal amrpsale { get; set; }
        public decimal? cpp { get; set; }
        public decimal? dpcoef { get; set; }
        public decimal? seascoef { get; set; }
        public decimal? seccoef { get; set; }
        public decimal? progcoef { get; set; }


        public decimal? price {
            get { return _price; }
            set { 
                _price = value;
                OnPropertyChanged();
            } 
        }
        public int? status { 
            get { return _status; } 
            set { 
                _status = value; 
                OnPropertyChanged(); 
            } 
        }

        public string? spotname { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
