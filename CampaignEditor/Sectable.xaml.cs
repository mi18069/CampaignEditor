using CampaignEditor.Controllers;
using Database.DTOs.CampaignDTO;
using Database.DTOs.SectableDTO;
using Database.DTOs.SectablesDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CampaignEditor
{
    public partial class Sectable : Window
    {
        private SectablesController _sectablesController;
        private SectableController _sectableController;

        public bool success = false;
        private bool modifiedSectables = false;
        private bool modifiedSectable = false;

        private SectableDTO _sectable = null;
        private CampaignDTO _campaign = null;

        public SectableDTO Sec { get { return _sectable; } }

        ObservableCollection<Tuple<int, double>> dgList = new ObservableCollection<Tuple<int, double>>();

        public Sectable(ISectableRepository sectableRepository,
            ISectablesRepository sectablesRepository)
        {
            InitializeComponent();

            _sectablesController = new SectablesController(sectablesRepository);
            _sectableController = new SectableController(sectableRepository);
            if (MainWindow.user.usrlevel <= 0)
            {
                cbGlobal.Visibility = Visibility.Visible;
            }
        }

        public async void Initialize(CampaignDTO campaign, SectableDTO sectable = null)
        {
            if (sectable != null)
            {
                _sectable = sectable;
                await FillBySctAsync(_sectable);
            }
            _campaign = campaign;
        }

        #region Filling fields

        private async Task FillBySctAsync(SectableDTO sectable)
        {
            dgList.Clear();
            tbName.Text = sectable.sctname.Trim();
            cbActive.IsChecked = sectable.sctactive;
            cbGlobal.IsChecked = sectable.ownedby == 0 ? true : false;

            List<SectablesDTO> seccoefs = (List<SectablesDTO>)await _sectablesController.GetSectablesById(sectable.sctid);
            seccoefs = seccoefs.OrderBy(s => s.sec).ToList();
            foreach (var seccoef in seccoefs)
            {
                dgList.Add(Tuple.Create(seccoef.sec, seccoef.coef));
            }
            FillDGSectables();
        }

        private void FillDGSectables()
        {
            dgSectables.ItemsSource = dgList;
        }

        #endregion

        #region Text Boxes mechanism
        private void tbAddFrom_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            tbAddTo.Text = tbAddFrom.Text;
        }

        private void tbDeleteFrom_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            tbDeleteTo.Text = tbDeleteFrom.Text;
        }

        private void tbIntegerOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            int num = 0;
            bool success = int.TryParse(e.Text, out num);
            e.Handled = !success;
        }

        private void tbName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            modifiedSectable = true;
        }

        private void cb_Changed(object sender, RoutedEventArgs e)
        {
            modifiedSectable = true;
        }

        #endregion

        #region Add and Delete Buttons

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            int from = 0;
            int to = 0;
            double coef = 0;

            if (int.TryParse(tbAddFrom.Text, out from) &&
                int.TryParse(tbAddTo.Text, out to) &&
                double.TryParse(tbCoef.Text, out coef))
            {
                InsertRange(from, to, coef);
                modifiedSectables = true;
                FillDGSectables();
            }
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            int from = 0;
            int to = 0;

            if (int.TryParse(tbDeleteFrom.Text, out from) &&
                int.TryParse(tbDeleteTo.Text, out to))
            {
                DeleteRange(from, to);
                modifiedSectables = true;
                FillDGSectables();
            }
        }
        private void InsertRange(int startRange, int endRange, double coef)
        {
            // Find the correct position for the first number in the range using binary search
            int low = 0;
            int high = dgList.Count() - 1;
            while (low <= high)
            {
                int mid = (low + high) / 2;
                if (dgList[mid].Item1 < startRange)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            // Insert or override the numbers in the range
            int currentValue = startRange;
            int currentIndex = low;
            while (currentValue <= endRange)
            {
                if (currentIndex < dgList.Count() && dgList[currentIndex].Item1 == currentValue)
                {
                    // Override the existing number
                    dgList[currentIndex] = Tuple.Create(currentValue, coef);
                }
                else
                {
                    // Insert the new number
                    dgList.Add(Tuple.Create(0, coef));
                    for (int i = dgList.Count() - 1; i > currentIndex; i--)
                    {
                        dgList[i] = dgList[i - 1];
                    }
                    dgList[currentIndex] = Tuple.Create(currentValue, coef);
                }

                currentValue++;
                currentIndex++;
            }

        }
        private void DeleteRange(int startRange, int endRange)
        {
            // Find the correct position for the first number in the range using binary search
            int low = 0;
            int high = dgList.Count() - 1;
            while (low <= high)
            {
                int mid = (low + high) / 2;
                if (dgList[mid].Item1 < startRange)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            // Check if the numbers in the range exist in the list, and remove them if they do
            int currentValue = startRange;
            int currentIndex = low;
            while (currentValue <= endRange)
            {
                if (currentIndex < dgList.Count() && dgList[currentIndex].Item1 == currentValue)
                {
                    // Remove the number from the list
                    dgList.RemoveAt(currentIndex);
                }
                else
                {
                    // Number not found in the list, do nothing
                    currentValue++;
                }

            }

        }

        #endregion

        #region Save and Cancel Buttons
        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (modifiedSectable == false && modifiedSectables == false)
            {
                this.Close();
            }
            else
            {
                if (_sectable == null)
                {
                    _sectable = await _sectableController.CreateSectable(new CreateSectableDTO
                        (tbName.Text.Trim(), false, (bool)cbActive.IsChecked, _campaign.clid));
                }
                int id = _sectable.sctid;

                if (modifiedSectables)
                {
                    await _sectablesController.DeleteSectablesById(id);
                    foreach (var sectables in dgList)
                    {
                        await _sectablesController.CreateSectables(new CreateSectablesDTO(
                            id, sectables.Item1, sectables.Item2));
                    }
                }
                if (modifiedSectable)
                {
                    int owner = (bool)cbGlobal.IsChecked ? 0 : _campaign.clid;
                    await _sectableController.UpdateSectable(new UpdateSectableDTO(id, tbName.Text.Trim(), false, (bool)cbActive.IsChecked, owner));
                }
                success = true;
            }
            this.Close();
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        
    }
}

