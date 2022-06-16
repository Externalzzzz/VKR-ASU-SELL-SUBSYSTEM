using System.Collections.Generic;
using System.Windows;
using Npgsql;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Configuration;
namespace asu
{
    /// <summary>
    /// Логика взаимодействия для Connection.xaml
    /// </summary>
    /// 
    public static class Connecting
    {
        public static string DefaultConnetion = $@"Host={ConfigurationManager.AppSettings.Get("host")};
                                                Port={ConfigurationManager.AppSettings.Get("port")};
                                                Username={ConfigurationManager.AppSettings.Get("username")};
                                                Password={ConfigurationManager.AppSettings.Get("password")};
                                                Database={ConfigurationManager.AppSettings.Get("database")};";
    
    }

    public partial class Connection : Window
    {
        public Connection()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string conn_param = "Host="+ host.Text + ";Port=" +  port.Text + ";Username=" + login.Text 
                            + ";Password=" + pass.Password + ";Database=" + db.Text + ";";
            var config =
           ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = config.AppSettings.Settings;
            settings["host"].Value = host.Text;
            settings["port"].Value = port.Text;
            settings["username"].Value = login.Text;
            settings["password"].Value = pass.Password;
            settings["database"].Value = db.Text;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
            ConfigurationManager.RefreshSection("appSettings");
            Debug.WriteLine($"entered: {conn_param}\nneeded: {Connecting.DefaultConnetion}");
            string sql = "SELECT column_name, data_type FROM information_schema.columns WHERE table_name = 'products'; ";
            NpgsqlConnection conn = new NpgsqlConnection(conn_param);
            NpgsqlCommand comm = new NpgsqlCommand(sql, conn);
            List<string> result = new List<string>();
            Connecting.DefaultConnetion = conn_param;
            try
            {
                conn.Open();
                var reader = comm.ExecuteReader();
                while (reader.Read())
                {
                    try
                    {
                        result.Add(reader.GetString(0) + " " + reader.GetString(1));

                    }
                    catch { }

                }
                foreach (var item in result)
                    Debug.WriteLine(item);
                conn.Close();
            
                MessageBox.Show("Подключение к базе данных произведено успешно!", "Успешное подключение", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch
            { 
                MessageBox.Show("Подключение к базе данных не произведено, проверьте данные и попробуйе еще раз.", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Warning); 
            }
            
        }
    }
}
