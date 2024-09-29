using System.ComponentModel;

namespace CampaignEditor.Helpers
{
    public class SharedColumnsWidthViewModel : INotifyPropertyChanged
    {
        public SharedExpectedColumnsWidthViewModel ExpectedColumnWidthViewModel { get; set; }
        public SharedRealizedColumnsWidthViewModel RealizedColumnWidthViewModel { get; set; }

        public SharedColumnsWidthViewModel()
        {
            ExpectedColumnWidthViewModel = new SharedExpectedColumnsWidthViewModel();
            RealizedColumnWidthViewModel = new SharedRealizedColumnsWidthViewModel();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SharedExpectedColumnsWidthViewModel : INotifyPropertyChanged
    {
        private double[] _columnWidths;

        public double[] ColumnWidths
        {
            get => _columnWidths;
            set
            {
                if (_columnWidths != value)
                {
                    _columnWidths = value;
                    OnPropertyChanged(nameof(ColumnWidths));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SharedRealizedColumnsWidthViewModel : INotifyPropertyChanged
    {
        private double[] _columnWidths;

        public double[] ColumnWidths
        {
            get => _columnWidths;
            set
            {
                if (_columnWidths != value)
                {
                    _columnWidths = value;
                    OnPropertyChanged(nameof(ColumnWidths));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
