using CampaignEditor.Helpers;
using Database.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CampaignEditor.UserControls
{
    /// <summary>
    /// Like spotGoalsSubGrid, but only with Total values
    /// </summary>
    public partial class SpotGoalsTotalSubGrid : UserControl
    {

        ObservableCollection<ObservableCollection<SpotGoals>> _valuesList;
        Dictionary<int, SpotGoals> _dictionary = new Dictionary<int, SpotGoals>();
        ObservableRangeCollection<SpotGoals> _values = new ObservableRangeCollection<SpotGoals>();

        public ObservableCollection<SpotGoals> Values
        {
            get { return _values; }
        }
        public SpotGoalsTotalSubGrid(ObservableCollection<ObservableCollection<SpotGoals>> valuesList)
        {
            InitializeComponent();

            _valuesList = valuesList;
            int n = 0;
            if (_valuesList.Count > 0)
            {
                n = _valuesList[0].Count;
            }
            for (int i=0; i<n; i++)
            {
                _dictionary.Add(i, new SpotGoals());
            }
            SumValues();
            SubscribeToDataGrids();
        }

        private void SumValues() 
        {
            ResetDictionaryValues();
            foreach (ObservableCollection<SpotGoals> spotGoalsList in _valuesList)
            {
                int n = spotGoalsList.Count;
                for (int i=0; i<n; i++)
                {
                    SpotGoals spotGoals = spotGoalsList[i];
                    _dictionary[i].Insertations += spotGoals.Insertations;
                    _dictionary[i].Grp += spotGoals.Grp;
                    _dictionary[i].Budget += spotGoals.Budget;
                }
            }
            _values.ReplaceRange(_dictionary.Values);
            dgGrid.ItemsSource = _values;

        }

        private void ResetDictionaryValues()
        {
            foreach (SpotGoals spotGoal in _dictionary.Values)
            {
                spotGoal.Grp = 0;
                spotGoal.Insertations = 0;
                spotGoal.Budget = 0;
            }
        }

        public void SubscribeToDataGrids()
        {
           
            foreach (ObservableCollection<SpotGoals> spotGoalsList in _valuesList)
            {
                spotGoalsList.CollectionChanged += spotGoalsList_CollectionChanged;
            }
        }


        private void spotGoalsList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SumValues();
        }
     
    }
}
