using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using System.Collections.Generic;

namespace CampaignEditor.UserControls
{
    public class ListBoxAddButton : ListBox
    {
        private Object objectToAdd = null;

        // For Plus Icon
        private string appPath = Directory.GetCurrentDirectory();
        private string imgGreenPlusPath = "\\images\\GreenPlus.png";

        public ListBoxAddButton()
        {
            this.SizeChanged += listBox_SizeChanged;
        }

        public void Initialize(object objToAdd)
        {
            objectToAdd = objToAdd;
            Type objectType = objectToAdd.GetType();
            ConstructorInfo constructor = objectType.GetConstructor(Type.EmptyTypes);
            object newObject = constructor.Invoke(new object[] { });
            this.Items.Add(newObject);

            Button addButton = MakeAddButton();
            this.Items.Add(addButton);

            ResizeItems(this.Items);
        }

        private Button MakeAddButton()
        {
            Button btnAddDP = new Button();
            btnAddDP.Click += new RoutedEventHandler(btnAddDP_Click);
            Image imgGreenPlus = new Image();
            imgGreenPlus.Source = new BitmapImage(new Uri(appPath + imgGreenPlusPath));
            btnAddDP.Content = imgGreenPlus;
            btnAddDP.Width = 50;
            btnAddDP.Height = 50;
            btnAddDP.Background = Brushes.White;
            btnAddDP.BorderThickness = new Thickness(0);
            btnAddDP.HorizontalAlignment = HorizontalAlignment.Right;

            return btnAddDP;
        }

        private void btnAddDP_Click(object sender, RoutedEventArgs e)
        {
            Type objectType = objectToAdd.GetType();
            ConstructorInfo constructor = objectType.GetConstructor(Type.EmptyTypes);
            object newObject = constructor.Invoke(new object[] { });
            this.Items.Insert(this.Items.Count - 1, newObject);
            ResizeItems(this.Items);
        }

        private void listBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeItems(this.Items);
        }

        private void ResizeItems(ItemCollection items)
        {
            int n = items.Count;
            for (int i = 0; i < n - 1; i++)
            {
                if (items[i] is FrameworkElement element)
                {
                    element.Width = this.ActualWidth;
                }
            }

            if (items[n - 1] is FrameworkElement button)
            {
                button.Width = this.ActualWidth;
            }
        }
    }
}
