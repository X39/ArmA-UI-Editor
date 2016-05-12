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
using System.IO;

namespace ArmA_UI_Editor.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ListView_Initialized(object sender, EventArgs e)
        {
            ListView lw = sender as ListView;
            lw.Items.Add(new { Text = "FOOBAR", Object = "FOO" });
            lw.Items.Add(new { Text = "FOOBAR", Object = "FOO" });
        }

        private void ConfigTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                MemoryStream memStream = new MemoryStream();
                //ToDo: Read ConfigTextbox content into memStream
                memStream.Seek(0, SeekOrigin.Begin);
                
                SQF.ClassParser.File.load(memStream);
                //ToDo: Build UI from Config


            }
            catch(Exception ex)
            {
                StatusText.Text = ex.Message;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown();
            
        }
    }
}
