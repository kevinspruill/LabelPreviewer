using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace NiceLabelReader.NetFramework
{
    /// <summary>
    /// Dialog for configuring print settings
    /// </summary>
    public class PrintSettingsDialog : Window
    {
        private ComboBox cmbDpi;
        private CheckBox chkFitToPage;
        private CheckBox chkCenterOnPage;
        private Button btnOK;
        private Button btnCancel;

        public int SelectedDpi { get; private set; } = 300;
        public bool FitToPage { get; private set; } = false;
        public bool CenterOnPage { get; private set; } = true;

        public PrintSettingsDialog()
        {
            this.Title = "Print Settings";
            this.Width = 400;
            this.Height = 250;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.ResizeMode = ResizeMode.NoResize;

            // Create the main layout
            Grid grid = new Grid { Margin = new Thickness(10) };
            this.Content = grid;

            // Define rows
            for (int i = 0; i < 5; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Add a title
            TextBlock titleText = new TextBlock
            {
                Text = "Print Settings",
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 15)
            };
            Grid.SetColumnSpan(titleText, 2);
            grid.Children.Add(titleText);

            // DPI Setting
            Label dpiLabel = new Label { Content = "Print Resolution:" };
            Grid.SetRow(dpiLabel, 1);
            Grid.SetColumn(dpiLabel, 0);
            grid.Children.Add(dpiLabel);

            cmbDpi = new ComboBox { Margin = new Thickness(0, 5, 0, 5) };
            cmbDpi.Items.Add("150 DPI");
            cmbDpi.Items.Add("200 DPI");
            cmbDpi.Items.Add("300 DPI");
            cmbDpi.Items.Add("600 DPI");
            cmbDpi.SelectedIndex = 2; // Default to 300 DPI
            Grid.SetRow(cmbDpi, 1);
            Grid.SetColumn(cmbDpi, 1);
            grid.Children.Add(cmbDpi);

            // Fit to page
            chkFitToPage = new CheckBox
            {
                Content = "Fit to printer page",
                Margin = new Thickness(0, 10, 0, 5),
                IsChecked = false
            };
            Grid.SetRow(chkFitToPage, 2);
            Grid.SetColumnSpan(chkFitToPage, 2);
            grid.Children.Add(chkFitToPage);

            // Center on page
            chkCenterOnPage = new CheckBox
            {
                Content = "Center on page",
                Margin = new Thickness(0, 5, 0, 10),
                IsChecked = true
            };
            Grid.SetRow(chkCenterOnPage, 3);
            Grid.SetColumnSpan(chkCenterOnPage, 2);
            grid.Children.Add(chkCenterOnPage);

            // Buttons
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 15, 0, 0)
            };
            Grid.SetRow(buttonPanel, 4);
            Grid.SetColumnSpan(buttonPanel, 2);
            grid.Children.Add(buttonPanel);

            btnOK = new Button
            {
                Content = "OK",
                Width = 80,
                Height = 25,
                Margin = new Thickness(0, 0, 10, 0),
                IsDefault = true
            };
            btnOK.Click += BtnOK_Click;
            buttonPanel.Children.Add(btnOK);

            btnCancel = new Button
            {
                Content = "Cancel",
                Width = 80,
                Height = 25,
                IsCancel = true
            };
            buttonPanel.Children.Add(btnCancel);
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            // Parse DPI from selected item
            string dpiString = cmbDpi.SelectedItem as string;
            if (dpiString != null)
            {
                int.TryParse(dpiString.Split(' ')[0], out int dpi);
                SelectedDpi = dpi;
            }

            FitToPage = chkFitToPage.IsChecked ?? false;
            CenterOnPage = chkCenterOnPage.IsChecked ?? true;

            DialogResult = true;
            Close();
        }
    }
}
