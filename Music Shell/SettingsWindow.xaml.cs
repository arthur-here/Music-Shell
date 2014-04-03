using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Music_Shell
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void btnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.path == null)
                Properties.Settings.Default.path = tbPath.Text;

            Properties.Settings.Default.Save();
        }

        private void pathButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Properties.Settings.Default.path = folderBrowserDialog1.SelectedPath;
            }
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.path != null)
                tbPath.Text = Properties.Settings.Default.path;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
