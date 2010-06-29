using System.Windows;
using System.Windows.Controls;

namespace LogViewer
{
    /// <summary>
    /// Interaction logic for FilterActivity.xaml
    /// </summary>
    public partial class FilterActivity
    {
        public FilterActivity()
        {
            InitializeComponent();
            VisualStateManager.GoToState(this, "NotFiltered", false);
        }

        private bool filtered;

        public bool IsFiltered
        {
            get { return filtered; }
            set
            {
                filtered = value;
                VisualStateManager.GoToState(this, filtered ? "Filtered" : "NotFiltered", true);
            }
        }
    }
}