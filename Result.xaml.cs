using System.Windows;
using System.Windows.Xps.Packaging;
using System.IO;
using System.Diagnostics;
using System;
//using Microsoft.Office.Interop.Word;

namespace asu
{
    /// <summary>
    /// Логика взаимодействия для Result.xaml
    /// </summary>
    /// 
    public partial class Result : Window
    {


        private string _dir = "";

        public Result(string directory)
        {
            InitializeComponent();
             _dir = directory;
            XpsDocument doc = new XpsDocument(_dir, FileAccess.ReadWrite);
            docViewer.Document = doc.GetFixedDocumentSequence();
            doc.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Данные сохранены", "Результаты анализа успешно сохранены в базе данных", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string dir = _dir.Substring(0, _dir.Length - 3) + "docx";
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                // Set filter for file extension and default file extension
                DefaultExt = ".docx",
                Filter = "DOCX Files (*.docx)|*.docx|DOC Files (*.doc)|*.doc|XPS Files (*.XPS)|*.xps"
            };
            Nullable<bool> result = dlg.ShowDialog();
            // Get the selected file name and display in a TextBox
            string filename = "";
            int changer = 0;
            if (result == true)
            {
                // Open document
                filename = dlg.FileName;
                if (dlg.FileName[dlg.FileName.Length - 1] == 's')
                    changer = 3;
                switch(dlg.FileName[dlg.FileName.Length - 1])
                {
                    case 's': changer = 3; break;
                    case 'c': changer = 2; break;
                    case 'x': changer = 1; break;
                }
                Debug.WriteLine($"filename - {filename}");
            }

            WordSolver ws = new WordSolver(dir);
            ws.Saver(filename, changer);
            Debug.WriteLine(dir);
        }
    }
}
