using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Database.Entities
{
    public class SpotGoalsSum : INotifyPropertyChanged
    {
        ObservableCollection<SpotGoals> SpotGoals { get; set; }

        public SpotGoalsSum(ObservableCollection<SpotGoals> spotGoals)
        {
            SpotGoals = spotGoals;
        }

        public int Insertations
        {
            get { return SpotGoals.Sum(sg => sg.Insertations); }
        }

        public decimal Grp
        {
            get { return SpotGoals.Sum(sg => sg.Grp); }
        }

        public decimal Budget
        {
            get { return SpotGoals.Sum(sg => sg.Budget); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

    }
}
