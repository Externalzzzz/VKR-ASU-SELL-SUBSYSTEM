using System.Windows;
using System.ComponentModel;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using Npgsql;
using System.Data;

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
        static string[] headers = new string[] 
        { "Код", "Название", "Размер", "Класс прочности", 
        "Марка морозостойкости", "Единица измерения", "Стоимость(руб.)" };
        static string[,] data = new string[,]
            {
                {"100",     "Блоки D600",   "625х250x200",    "В5",      "F100", "Шт.", "5600"},
                {"101",     "Блоки D300",   "300х250х625",    "В1,5",    "F100", "Шт.", "3300"},
                {"102",     "Блоки D100",   "600x250x200",    "B0,5",    "F35",  "Шт.", "1000"},
                {"103",     "Блоки D200",   "600x100x250",    "В0,5",    "F35",  "Шт.", "2300"},
                {"104",     "Блоки D200",   "600x200x250",    "В0,5",    "F35",  "Шт.", "2700"},
                {"105",     "Блоки D400",   "600x400x250",    "В2",      "F100", "Шт.", "6000"},
                {"106",     "Блоки D300",   "300х250х625",    "В1,5",    "F100", "Шт.", "2300"},
                {"107",     "Блоки D300",   "300х250х625",    "В1,5",    "F100", "Шт.", "3300"},
                {"108",     "Блоки D100",   "600x250x200",    "B0,5",    "F35",  "Шт.", "1000"},
                {"109",     "Блоки D200",   "600x100x250",    "В0,5",    "F35",  "Шт.", "2300"},
                {"110",     "Блоки D200",   "600x200x250",    "В0,5",    "F35",  "Шт.", "2700"},
                {"111",     "Блоки D300",   "300х250х625",    "В1,5",    "F100", "Шт.", "2300"},
            };

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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //items = new BindingList<Item>();
            //for (int i = 0; i < data.GetLength(0); i++)
            //    items.Add(new Item() { ItemCode = data[i, 0], ItemName = data[i, 1], Size = data[i, 2], ItemClass1 = data[i, 3], ItemClass2 = data[i, 4], Edinica = data[i, 5], Price = data[i, 6] });
            //GRIDTable.ItemsSource = items;
            List<List<string>> collection = SendQuery(query);

            List<List<string>> columnsCollection = SendQuery(queryColumns);
            DataTable dt = new DataTable();
            for (int i = 0; i < collection[0].Count; i++)
            //dt.Columns.Add(new DataColumn(columnsCollection[i][0], columnsCollection[i][1] == "integer"?typeof(int):typeof(string)));
            {
                dt.Columns.Add(new DataColumn(attributes[columnsCollection[i][0]], typeof(string)));
                
            }

            for (int i = 0; i < collection.Count;i++)
            {
                DataRow dr = dt.NewRow();
                for (int j = 0; j < collection[i].Count; j++)
                {
                    //if (dt.Columns[j].DataType == typeof(string))
                    //    dr[j] = collection[i][j];
                    //else 
                    //    dr[j] = int.Parse(collection[i][j]);
                    dr[j] = collection[i][j];
                }
                dt.Rows.Add(dr);
            }
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
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Данные успешно сохранены!", "Изменение данных в справочнике", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
