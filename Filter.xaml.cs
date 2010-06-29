using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WPF.Themes;

namespace LogViewer
{
    /// <summary>
    /// Interaction logic for Filter.xaml
    /// </summary>
    public partial class Filter
    {
        private readonly LogFilter logFilter;

        public Filter(IEnumerable<LogEntry> entries, LogFilter logFilter)
        {
            Opacity = 0d;
            InitializeComponent();
            PopulateDropDown(entries);
            this.logFilter = logFilter;
            MainGrid.DataContext = this.logFilter;
            this.ApplyTheme("ExpressionDark");
        }

        private void PopulateDropDown(IEnumerable<LogEntry> entries)
        {
            comboBoxLevel.ItemsSource = (from e in entries select e.Level).Distinct().ToList();
            comboBoxUserName.ItemsSource = (from e in entries select e.UserName).Distinct().OrderBy(e=>e).ToList();
            comboBoxThread.ItemsSource = (from e in entries select e.Thread).Distinct().OrderBy(e => e).ToList();
            comboBoxMachineName.ItemsSource = (from e in entries select e.MachineName).Distinct().OrderBy(e => e).ToList();
            comboBoxHostName.ItemsSource = (from e in entries select e.HostName).Distinct().OrderBy(e => e).ToList();
            comboBoxApplication.ItemsSource = (from e in entries select e.App).Distinct().OrderBy(e => e).ToList();
            comboBoxClass.ItemsSource = (from e in entries select e.Class).Distinct().OrderBy(e => e).ToList();
            comboBoxMethod.ItemsSource = (from e in entries select e.Method).Distinct().OrderBy(e => e).ToList();
            comboBoxFile.ItemsSource = (from e in entries select e.File).Distinct().OrderBy(e => e).ToList();
            comboLogFile.ItemsSource = (from e in entries select e.LogFile).Distinct().OrderBy(e => e).ToList();
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            logFilter.Clear();
            comboBoxLevel.SelectedIndex = -1;
            comboBoxUserName.SelectedIndex = -1;
            comboBoxThread.SelectedIndex = -1;
            comboBoxMachineName.SelectedIndex = -1;
            comboBoxHostName.SelectedIndex = -1;
            comboBoxApplication.SelectedIndex = -1;
            comboBoxClass.SelectedIndex = -1;
            comboBoxMethod.SelectedIndex = -1;
            comboBoxFile.SelectedIndex = -1;
            comboLogFile.SelectedIndex = -1;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
