using System.Windows;
using System.ComponentModel;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using Npgsql;
using System.Data;
using System.Linq;

namespace asu
{
    /// <summary>
    /// Строки
    /// </summary>
    public class Item
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Size { get; set; }
        public string ItemClass1 { get; set; }
        public string ItemClass2 { get; set; }
        public string Edinica{ get; set; }
        public string Price{ get; set; }
    }


    /// <summary>
    /// Логика взаимодействия для Catalog.xaml
    /// </summary>
    /// 
    public partial class Catalog : Window
    {
        

        //private BindingList<Item> items;
        private Dictionary<string, string> attributes = new Dictionary<string, string>
        {
            {"product_id",                      "Код" },
            { "product_price",                  "Стоимость" },
            { "product_cubic_to_block",         "Перевод в кубометры"},
            { "product_thermal_conduct_class",  "Класс теплопроводности"},
            { "product_frost_resist_class"  ,   "Класс морозостокойсти"},
            { "product_unit_measure"    ,       "Единица измерения"},
            { "product_strength"    ,           "Класс прочности"},
            { "product_name"    ,               "Название"},
            { "product_size",                   "Размеры" }

        };
        public Catalog()
        {
            InitializeComponent();
        }
        const string query = @"SELECT product_id, 
                                      product_name, 
                                      product_size, 
                                      product_thermal_conduct_class, 
                                      product_frost_resist_class, 
                                      product_unit_measure, product_price, 
                                      product_strength, 
                                      product_cubic_to_block 
                                        FROM dbschema.products
                                        ORDER BY product_id ASC;";
        const string queryColumns = @"SELECT
                                            column_name,
                                            data_type
                                        FROM
                                            information_schema.columns
                                        WHERE
                                            table_name = 'products'
                                        ORDER BY ordinal_position ASC;";
        DataTable DTDB = null;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<List<string>> collection = SendQuery(query);

            List<List<string>> columnsCollection = SendQuery(queryColumns);
            DataTable dt = new DataTable();
            for (int i = 0; i < collection[0].Count; i++)
            {
                dt.Columns.Add(new DataColumn(attributes[columnsCollection[i][0]], typeof(string)));
                
            }

            for (int i = 0; i < collection.Count;i++)
            {
                DataRow dr = dt.NewRow();
                for (int j = 0; j < collection[i].Count; j++)
                {
                    dr[j] = collection[i][j];
                }
                dt.Rows.Add(dr);
            }
            DTDB = dt;
            GRIDTable.ItemsSource = new DataView(dt);

        }
        static List<List<string>> SendQuery(string query)
        {
            Debug.Write(Connecting.DefaultConnetion);

            NpgsqlConnection conn = new NpgsqlConnection(Connecting.DefaultConnetion);
            NpgsqlCommand comm = new NpgsqlCommand(query, conn);
            List<List<string>> res = new List<List<string>>();
            try
            {
                conn.Open();
                var reader = comm.ExecuteReader();
                while (reader.Read())
                {
                    int i = 0;
                    try
                    {
                        List<string> temp = new List<string>();
                        for (int j = 0; j < reader.FieldCount; j++)
                        {
                            temp.Add(reader[j].ToString());

                        }
                        res.Add(temp);
                        i++;
                    }
                    catch { }

                }
                Debug.WriteLine("SQL query res:");
                foreach (var item in res)
                {
                    foreach (var item2 in item)
                        Debug.Write($"{item2}\t");
                    Debug.Write("\n");
                }
                conn.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            return res;
        }
        private void sendQuery()
        {
            NpgsqlConnection conn = new NpgsqlConnection(Connecting.DefaultConnetion);
            string queryTrunc = "TRUNCATE dbschema.products;";
            NpgsqlCommand comm = new NpgsqlCommand(queryTrunc, conn);
            conn.Open();
            comm.ExecuteReader();
            conn.Wait();
            conn.Close();
        }
            
        private void sendQuery(string query)
        {
            NpgsqlConnection conn = new NpgsqlConnection(Connecting.DefaultConnetion);
            NpgsqlCommand comm = new NpgsqlCommand(query, conn);
            List<List<string>> res = new List<List<string>>();
            try
            {
                conn.Open();
                var reader = comm.ExecuteReader();
                while (reader.Read())
                {
                    int i = 0;
                    try
                    {
                        List<string> temp = new List<string>();
                        for (int j = 0; j < reader.FieldCount; j++)
                        {
                            temp.Add(reader[j].ToString());

                        }
                        res.Add(temp);
                        i++;
                    }
                    catch { }

                }
                Debug.WriteLine("SQL query res:");
                foreach (var item in res)
                {
                    foreach (var item2 in item)
                        Debug.Write($"{item2}\t");
                    Debug.Write("\n");
                }
                conn.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            return;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = ((DataView)GRIDTable.ItemsSource).ToTable();
            //sendQuery();
            MainWindow.DebugData(dt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string queryAdd = $@"UPDATE
                                dbschema.products set product_id = {int.Parse(dt.Rows[i][0].ToString())}, product_name = '{dt.Rows[i][1]}', product_size = '{dt.Rows[i][2].ToString()}', product_thermal_conduct_class = '{dt.Rows[i][3]}',
                            product_frost_resist_class = '{dt.Rows[i][4].ToString()}', product_unit_measure = '{dt.Rows[i][5].ToString()}', product_price = {int.Parse(dt.Rows[i][6].ToString())}, product_strength = '{dt.Rows[i][7].ToString()}', product_cubic_to_block = {int.Parse(dt.Rows[i][8].ToString())} 
                                WHERE product_id = {int.Parse(dt.Rows[i][0].ToString())};";
                Debug.WriteLine($"edited- {queryAdd}");
                sendQuery(queryAdd);
            }
            MessageBox.Show("Данные успешно сохранены!", "Изменение данных в справочнике", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
