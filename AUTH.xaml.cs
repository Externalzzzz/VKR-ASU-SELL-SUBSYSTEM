using System.Collections.Generic;
using System.Windows;

namespace asu
{
    /// <summary>
    /// Логика взаимодействия для AUTH.xaml
    /// </summary>
    public partial class AUTH : Window
    {
       public AUTH()
        {
            InitializeComponent();
        }

        private static Dictionary<string, string> Auth_Pass = new Dictionary<string, string>()
        {
            { "admin","admin"},
            { "БорисякМА","123"}
        };
        private bool checkAuth (string login, string password)
        {
            Auth_Pass.TryGetValue(login, out string result);
            return result==password;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (checkAuth(textLogin.Text, textPass.Password))
            {
                MainWindow win1 = new MainWindow(textLogin.Text);
                Close();
                win1.Show();

            }
            else 
                MessageBox.Show("Вы ввели некорректные данные!", "Ошибка ввода данных", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
