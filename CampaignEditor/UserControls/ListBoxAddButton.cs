using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Collections.Generic;

namespace CampaignEditor.UserControls
{
    public class ListBoxAddButton : ListBox
    {
        protected Object objectToAdd = null;

        public ListBoxAddButton()
        {
            this.SizeChanged += listBox_SizeChanged;
        }

        public void Initialize(object objToAdd)
        {
           
            objectToAdd = objToAdd;
            this.Items.Add(objToAdd);

            Button addButton = MakeAddButton();
            this.Items.Add(addButton);

            ResizeItems(this.Items);
        }

        public List<object> GetItems()
        {
            List<object> itemsList = new List<object>();

            // Get the type of objectToAdd
            Type objectType = objectToAdd.GetType();

            // Iterate through the ListBox items
            foreach (object item in this.Items)
            {
                itemsList.Add(item);
            }

            // Remove button
            itemsList.RemoveAt(itemsList.Count-1);
            return itemsList;
        }

        private Button MakeAddButton()
        {
            Button btnAddDP = new Button();
            btnAddDP.Click += new RoutedEventHandler(btnAddDP_Click);
            Image imgGreenPlus = new Image();
            ImageSource imageSource = (ImageSource)Application.Current.FindResource("plus_icon");
            imgGreenPlus.Source = imageSource;
            btnAddDP.Content = imgGreenPlus;
            btnAddDP.Width = 50;
            btnAddDP.Height = 50;
            btnAddDP.Background = Brushes.White;
            btnAddDP.BorderThickness = new Thickness(0);
            btnAddDP.HorizontalAlignment = HorizontalAlignment.Right;

            return btnAddDP;
        }

        public event EventHandler BtnAddClicked;
        virtual protected void btnAddDP_Click(object sender, RoutedEventArgs e)
        {
            Type objectType = objectToAdd.GetType();
            ConstructorInfo constructor = objectType.GetConstructor(Type.EmptyTypes);
            object newObject = constructor.Invoke(new object[] { });
            this.Items.Insert(this.Items.Count - 1, newObject);
            ResizeItems(this.Items);
            BtnAddClicked?.Invoke(this, null);
        }


        private void listBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeItems(this.Items);
        }

        public void ResizeItems(ItemCollection items)
        {
            int n = items.Count;
            for (int i = 0; i < n - 1; i++)
            {
                if (items[i] is FrameworkElement element)
                {
                    element.Width = this.ActualWidth;
                }
            }

            if (n > 0)
            {
                if (items[n - 1] is FrameworkElement button)
                {
                    button.Width = this.ActualWidth;
                }
            }
        }
    }
}
