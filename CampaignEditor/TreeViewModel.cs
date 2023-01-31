using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace TreeViewModels
{
    public class TreeViewModel : INotifyPropertyChanged
    {
        public TreeViewModel(string name)
        {
            Name = name;
            Children = new List<TreeViewModel>();
        }

        #region Properties

        public string Name { get; private set; }
        public List<TreeViewModel> Children { get; private set; }
        public bool IsInitiallySelected { get; private set; }

        bool? _isChecked = false;
        TreeViewModel _parent;
        
        #region IsChecked

        public bool? IsChecked
        {
            get { return _isChecked; }
            set { SetIsChecked(value, true, true); }
        }

        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked) return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue) Children.ForEach(c => c.SetIsChecked(_isChecked, true, false));

            if (updateParent && _parent != null) _parent.VerifyCheckedState();

            NotifyPropertyChanged("IsChecked");
        }

        void VerifyCheckedState()
        {
            bool? state = null;

            for (int i = 0; i < Children.Count; ++i)
            {
                bool? current = Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                if (state != current)
                {
                    state = null;
                    break;
                }
            }

            SetIsChecked(state, false, true);
        }

        #endregion

        #endregion


        #region Initialization
        public void Initialize()
        {
            foreach (TreeViewModel child in Children)
            {
                child._parent = this;
                child.Initialize();
            }
        }
        private static Dictionary<string, List<string>> GetNodes()
        {
            Dictionary<string, List<string>> nodes = new Dictionary<string, List<string>>
            {
                { "Sex", new List<string>{ "male", "female" }},
                { "Age Groups", new List<string>{ "kids", "teens", "adults" }},
                { "Region", new List<string>{ "Serbia", "others" }},
                { "Working Status", new List<string>{ "employed", "unemployed" }}
            };

            return nodes;

        }

        public static List<TreeViewModel> SetTree()
        {
            Dictionary<string, List<string>> nodes = GetNodes();

            List<TreeViewModel> treeViewList = new List<TreeViewModel>();

            foreach (string nodeName in nodes.Keys)
            {
                TreeViewModel tv1 = new TreeViewModel(nodeName);
                treeViewList.Add(tv1);

                foreach (string subNodeName in nodes[nodeName])
                {
                    tv1.Children.Add(new TreeViewModel(subNodeName));
                }

            }
            foreach (TreeViewModel tv in treeViewList)
            {
                tv.Initialize();
            }

            return treeViewList;
        }

        #endregion


        public static List<string> GetSelected(TreeViewModel tv)
        {
            List<string> selected = new List<string>();
            
            foreach (TreeViewModel child in tv.Children)
            {
                if (child.IsChecked != false)
                    selected.Add(child.Name);
            }

            return selected;

            //***********************************************************
            //From your window capture selected your treeview control like:   TreeViewModel root = (TreeViewModel)TreeViewControl.Items[0];
            //                                                                List<string> selected = new List<string>(TreeViewModel.GetTree());
            //***********************************************************
        }

        #region INotifyPropertyChanged Members

        public void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
