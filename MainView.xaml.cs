using Autodesk.Revit.UI;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RevitAPITotalRoomNuumber
{
    /// <summary>
    /// Логика взаимодействия для MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public string NumberPrefix;
        public string StartFrom;
        public string SelectedNumberingDirection;
        public DelegateCommand SaveCommand { get; set; }

        public MainView()
        {
            SaveCommand = new DelegateCommand(onSaveCommand);
            InitializeComponent();
        }

        private void onSaveCommand()
        {
            NumberPrefix = textBox_NumberPrefix.Text;
            StartFrom = textBox_StartFrom.Text;
            SelectedNumberingDirection = (groupBox_NumberingDirection.Content as System.Windows.Controls.Grid)
                .Children.OfType<RadioButton>()
                .FirstOrDefault(rb => rb.IsChecked.Value == true)
                .Name;
            DialogResult = true;
            Close();
        }

        private void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            NumberPrefix = textBox_NumberPrefix.Text;
            StartFrom = textBox_StartFrom.Text;
            SelectedNumberingDirection = (groupBox_NumberingDirection.Content as System.Windows.Controls.Grid)
                .Children.OfType<RadioButton>()
                .FirstOrDefault(rb => rb.IsChecked.Value == true)
                .Name;
            DialogResult = true;
            Close();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void RoomNumeratorWPF_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                NumberPrefix = textBox_NumberPrefix.Text;
                StartFrom = textBox_StartFrom.Text;
                SelectedNumberingDirection = (groupBox_NumberingDirection.Content as System.Windows.Controls.Grid)
                    .Children.OfType<RadioButton>()
                    .FirstOrDefault(rb => rb.IsChecked.Value == true)
                    .Name;
                DialogResult = true;
                Close();
            }

            else if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }
    }
}
