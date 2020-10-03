using Microsoft.Toolkit.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MyPushBox
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.LayoutDesign(5, 5);
        }

        private void LayoutDesign(int rowCount, int colCount)
        {
            Grid g = grid_table;

            for (int i = 0; i < rowCount; i++)
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = new GridLength(1, GridUnitType.Star);
                g.RowDefinitions.Add(rd);
            }

            for (int i = 0; i < colCount; i++)
            {
                ColumnDefinition rd = new ColumnDefinition();
                rd.Width = new GridLength(1, GridUnitType.Star);
                g.ColumnDefinitions.Add(rd);
            }

            Thickness t = new Thickness(10);
            
            Uri uri = new Uri("ms-appx:///Assets/Block-Ground.jpg", UriKind.RelativeOrAbsolute);
            BitmapImage bit = new BitmapImage(uri);
            

            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    Image img = new Image();
                    img.Source = bit;
                    grid_table.Children.Add(img);
                    Grid.SetColumn(img, c);
                    Grid.SetRow(img, r);
                }
            }

            /*
            //Create Stackpanel for ListBox Control and its description
            StackPanel DeptStackPanel = new StackPanel();
            DeptStackPanel.Margin = new Thickness(10);

            LayoutRoot.Children.Add(DeptStackPanel);
            Grid.SetColumn(DeptStackPanel, 1);
            Grid.SetRow(DeptStackPanel, 1);

            TextBlock DeptListHeading = new TextBlock();
            DeptListHeading.Text = "Department";

            ListBox DeptList = new ListBox();
            DeptList.Items.Add("Finance");
            DeptList.Items.Add("Marketing");
            DeptList.Items.Add("Human Resources");
            DeptList.Items.Add("Payroll");

            DeptStackPanel.Children.Add(DeptListHeading);
            //DeptStackPanel.Children.Add(DeptList);

            //Create StackPanel for buttons
            StackPanel ButtonsStackPanel = new StackPanel();
            ButtonsStackPanel.Margin = new Thickness(10);
            ButtonsStackPanel.Orientation = Orientation.Horizontal;
            ButtonsStackPanel.HorizontalAlignment = HorizontalAlignment.Center;

            LayoutRoot.Children.Add(ButtonsStackPanel);
            Grid.SetColumn(ButtonsStackPanel, 0);
            Grid.SetRow(ButtonsStackPanel, 2);
            Grid.SetColumnSpan(ButtonsStackPanel, 2);

            Button BackButton = new Button();
            BackButton.Content = "Back";
            BackButton.Width = 100;

            Button CancelButton = new Button();
            CancelButton.Content = "Cancel";
            CancelButton.Width = 100;

            Button NextButton = new Button();
            NextButton.Content = "Next";
            NextButton.Width = 100;

            ButtonsStackPanel.Children.Add(BackButton);
            ButtonsStackPanel.Children.Add(CancelButton);
            ButtonsStackPanel.Children.Add(NextButton);

            BackButton.Margin = new Thickness(10);
            CancelButton.Margin = new Thickness(10);
            NextButton.Margin = new Thickness(10);
            */
        }
    }
}
