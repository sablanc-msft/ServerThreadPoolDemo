using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Threading;

namespace ThreadPoolDemoClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            ThreadPool.SetMinThreads(1000, 1000);

            InitializeComponent();

            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
            this.DataContext = mainWindowViewModel;
        }

        private void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            System.Collections.IList addedItems = e.AddedItems;

            if (addedItems.Count > 0)
            {
                ListBox listBox = (ListBox)sender;

                listBox.ScrollIntoView(addedItems[0]);


            }
        }
    }
}
