using System;
using System.Collections.Generic;
using System.Windows;
using System.Data;
using System.ComponentModel;
using System.Diagnostics;
using Npgsql;
using System.Windows.Controls;
using System.Linq;

namespace asu
{

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.DataContext = this;
            InitializeComponent();
            Calendar_start.SelectedDate = new DateTime(01,01,2021);
        }
        public MainWindow(string authdata)
        {
            InitializeComponent();
            if (authdata == "admin")
                connectTab.IsEnabled = true;
            else connectTab.IsEnabled = false;

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Connect(object sender, RoutedEventArgs e)
        {
            Connection con = new Connection();
            con.Show();
        }
        private void OpenCatalog(object sender, RoutedEventArgs e)
        {
            Catalog cat1 = new Catalog();
            cat1.Show();

        }

        private void DATAgrid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int MonthsCount = 0;
            try
            {
                MonthsCount = int.Parse(MonthsBox.Text);

            }
            catch
            {
                MessageBox.Show("Пожалуйста, введите корректные целочисленные значения для ограничений.", "Ошибка ограничений", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DataTable dataTable = ((DataView)DATAgrid.ItemsSource).ToTable();
            const int staticCols = 5; //ID -  CODE - name - price - measure
            int sumRow = 0;
            while(dataTable.Columns.Count != staticCols + MonthsCount)
            {
                dataTable.Columns.RemoveAt(dataTable.Columns.Count - 1);
            }
            List<int> sumns = new List<int> ();
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                for(int j = 0 + staticCols; j < staticCols + MonthsCount; j++)
                {
                    sumRow += (int)dataTable.Rows[i][j];
                }
                sumns.Add (sumRow);
                sumRow = 0;
            }
            dataTable.Columns.Add(new DataColumn("Сумма", typeof(int)));
            for (int i = 0; i < dataTable.Rows.Count;i++)
                dataTable.Rows[i][staticCols+MonthsCount] = sumns[i];
            int fullSumOfProducts = sumns.Sum();
            Debug.Write($"\nсумма столбца - {fullSumOfProducts}\n");
            dataTable.Columns.Add(new DataColumn("Доля(%)", typeof(int)));
            for (int i = 0; i < dataTable.Rows.Count;i++)
            {
                dataTable.Rows[i][staticCols+MonthsCount+1] = ((float)sumns[i] / (float)fullSumOfProducts ) * 100;
                Debug.WriteLine(((float)sumns[i] / (float)fullSumOfProducts) * 100);
            }
            DataView dv1 = new DataView(dataTable);
            dv1.Sort = "Доля(%) DESC";
            dataTable = dv1.ToTable();
            dataTable.Columns.Add(new DataColumn("Накапливаемая доля (%)", typeof(int)));
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                if (i == 0)
                    dataTable.Rows[i][staticCols+MonthsCount+2] = dataTable.Rows[i][staticCols + MonthsCount + 1];
                else
                    dataTable.Rows[i][staticCols+MonthsCount+2] = (int)dataTable.Rows[i-1][staticCols + MonthsCount + 2] + (int)dataTable.Rows[i][staticCols + MonthsCount + 1];

            }
            dataTable.Columns.Add(new DataColumn("ABC-группа", typeof(string)));
            for (int i = 0;i < dataTable.Rows.Count;i++)
            {
                int tempRes = (int)dataTable.Rows[i][staticCols + MonthsCount + 2];
                char abcChar = ' ';
                if (tempRes < 80)
                    abcChar = 'A';
                else if (tempRes < 95)
                    abcChar = 'B';
                else abcChar = 'C';

                dataTable.Rows[i][staticCols + MonthsCount + 3] = abcChar;
            }    
            List<List<int>> pricevalues = new List<List<int>>();
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                List <int> price = new List<int>();
                for (int j = staticCols; j < staticCols + MonthsCount; j++)
                    price.Add((int)dataTable.Rows[i][j] * (int)dataTable.Rows[i][3]);
                pricevalues.Add(price);
            }
            List<int> sumPrice = new List<int>();
            for (int i = 0; i < pricevalues.Count; i++)
            {
                sumRow = 0;
                for (int j = 0; j < pricevalues[i].Count; j++)
                {
                    sumRow += pricevalues[i][j];
                }
                sumPrice.Add(sumRow);
            }
            Debug.WriteLine($"\tстолбец сумм: ");
            sumPrice.ForEach(x => Debug.WriteLine($"\t{x}"));
            List<double> DIFF_Result = new List<double>();

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                double averagePrice = (double)pricevalues[i].Average();
                double top = 0;
                for (int j = 0; j < MonthsCount; j++)
                {
                    top += Math.Pow((double)pricevalues[i][j] - averagePrice, 2);
                }
                double squareDiff = Math.Sqrt(top / (double)MonthsCount - 1);

                double Diff = (squareDiff / averagePrice) * 100;
                DIFF_Result.Add(Diff);
            }
            dataTable.Columns.Add(new DataColumn("Коэффициент вариации (%)",typeof(double)));
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                dataTable.Rows[i][dataTable.Columns.Count-1] = Math.Round(DIFF_Result[i],2);
            }
            dataTable.Columns.Add(new DataColumn("XYZ-группа", typeof(string)));
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                if (DIFF_Result[i] > 0 && DIFF_Result[i] <= 10)
                    dataTable.Rows[i][dataTable.Columns.Count - 1] = "X";
                else if (DIFF_Result[i] > 10 && DIFF_Result[i] <= 25)
                    dataTable.Rows[i][dataTable.Columns.Count - 1] = "Y";
                else 
                    dataTable.Rows[i][dataTable.Columns.Count - 1] = "Z";
            }
            dataTable.Columns.Add(new DataColumn("Совмещение результатов", typeof(string)));
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                dataTable.Rows[i][dataTable.Columns.Count - 1] = (string)dataTable.Rows[i][dataTable.Columns.Count - 4] + (string)dataTable.Rows[i][dataTable.Columns.Count - 2];
            }
            dataTable.Columns.Add(new DataColumn("Результат отбора", typeof(string)));
            dv1 = new DataView(dataTable);
            dv1.Sort = "Доля(%) DESC, Коэффициент вариации (%) ASC ";
            dataTable = dv1.ToTable();
            for (int i = dataTable.Rows.Count - 1; i >= 0; i--)
            {
                if (dataTable.Rows[i][dataTable.Columns.Count-2].ToString() == "CZ"|| dataTable.Rows[i][dataTable.Columns.Count - 2].ToString() == "BZ" || dataTable.Rows[i][dataTable.Columns.Count - 2].ToString() == "CY")
                {
                    dataTable.Rows[i][dataTable.Columns.Count - 1] = "Рекомендовано снизить объемы производства";
                }
                else
                    dataTable.Rows[i][dataTable.Columns.Count - 1] = "Рекомендовано оставить в таком же объеме";


            }
            DebugData(pricevalues);
            DebugData(dataTable);
            var helper = new WordSolver("result.docx");
            var items = new Dictionary<string, string>()
            {
                {"<DATE>", "01.06.2022" }
                //{"<ID>", dataTable.Rows[0][0].ToString()},
                //{"<NAME>", dataTable.Rows[0][1].ToString()},
                //{"<ABC>", dataTable.Rows[0][dataTable.Columns.Count-5].ToString()},
                //{"<XYZ>", dataTable.Rows[0][dataTable.Columns.Count-3].ToString()},
                //{"<ABCXYZ>", dataTable.Rows[0][dataTable.Columns.Count-2].ToString()},
                //{"<RESULT>", dataTable.Rows[0][dataTable.Columns.Count-1].ToString()}
            };
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                items["<ID" + (i+1).ToString() + ">"] = dataTable.Rows[i][1].ToString();
                items["<NAME" + (i + 1).ToString() + ">"] = dataTable.Rows[i][2].ToString();
                items["<ABC" + (i + 1).ToString() + ">"] = dataTable.Rows[i][dataTable.Columns.Count - 5].ToString();
                items["<XYZ" + (i + 1).ToString() + ">"] = dataTable.Rows[i][dataTable.Columns.Count - 3].ToString();
                items["<ABCXYZ" + (i + 1).ToString() + ">"] = dataTable.Rows[i][dataTable.Columns.Count - 2].ToString();
                items["<RESULT" + (i + 1).ToString() + ">"] = dataTable.Rows[i][dataTable.Columns.Count - 1].ToString();
                items["<PERIOD>"] = dataTable.Columns[5].ToString() + "г.  -  31" + dataTable.Columns[5+MonthsCount-1].ToString().Substring(2) + "г.";

            }
            string dir = helper.Process(items);
            Result res = new Result(dir);
            res.Show();
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                if (dataTable.Rows[i][dataTable.Columns.Count - 1].ToString() == "Рекомендовано снизить объемы производства")
                {
                    var helper1 = new WordSolver("delProd.docx");
                    var items2 = new Dictionary<string, string>()
                    {
                        {"<DATE>", "01.06.2022" },
                        {"<ID>", dataTable.Rows[i][1].ToString() },
                        {"<NAME>", dataTable.Rows[i][2].ToString()},
                        {"<COST>", dataTable.Rows[i][3].ToString()},
                        {"<PERIOD>", dataTable.Columns[5].ToString() + "г.  -  31" + dataTable.Columns[5+MonthsCount-1].ToString().Substring(2) + "г." }

                    };
                    var dir1 = helper1.Process(items2);
                    Result res2 = new Result(dir1);
                    res2.Show();

                }
            }
            DATAgrid.ItemsSource = new DataView(dataTable);
        }
        private void OpenHelp(object sender, RoutedEventArgs e)
        {
            Help cat1 = new Help();
            cat1.Show();
        }
        //private void QUERIES()
        //{
        //    Debug.Write(Connecting.DefaultConnetion);

        //    NpgsqlConnection conn = new NpgsqlConnection(Connecting.DefaultConnetion);
        //    string query = "SELECT * FROM dbschema.products ORDER BY product_id ASC";
        //    string query2 = "SELECT * FROM dbschema.contract_of_purchase_sale ORDER BY contract_id ASC ";
        //    string query3 = "SELECT * FROM dbschema.connection_contract_product ORDER BY contract_id ASC, product_id ASC ";
        //    NpgsqlCommand comm = new NpgsqlCommand(query, conn);
        //    NpgsqlCommand comm2 = new NpgsqlCommand(query2, conn);


        //    List<string> res = new List<string>();
        //    try
        //    {
        //        conn.Open();
        //        var reader = comm.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            try
        //            {
        //                string temp = "";
        //                for (int i = 0; i < reader.FieldCount; i++)
        //                {
        //                    temp += i > 0 ? "\t" : "";
        //                    temp += reader[i].ToString();

        //                }
        //                res.Add(temp);
        //            }
        //            catch { }

        //        }
        //        Debug.WriteLine("SQL query res:");
        //        foreach (var item in res)
        //            Debug.WriteLine(item);
        //        res.Clear();
        //        conn.Close();
        //        conn.Open();
        //        reader = comm2.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            try
        //            {
        //                string temp = "";
        //                for (int i = 0; i < reader.FieldCount; i++)
        //                {
        //                    temp += i > 0 ? "\t" : "";
        //                    temp += reader[i].ToString();

        //                }
        //                res.Add(temp);
        //            }
        //            catch { }

        //        }
        //        Debug.WriteLine("SQL query res:");
        //        foreach (var item in res)
        //            Debug.WriteLine(item);

        //        conn.Close();
        //        res.Clear();
        //        comm = new NpgsqlCommand(query3, conn);
        //        conn.Open();
        //        reader = comm.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            try
        //            {
        //                string temp = "";
        //                for (int i = 0; i < reader.FieldCount; i++)
        //                {
        //                    temp += i > 0 ? "\t" : "";
        //                    temp += reader[i].ToString();

        //                }
        //                res.Add(temp);
        //            }
        //            catch { }

        //        }
        //        Debug.WriteLine("SQL query res:");
        //        foreach (var item in res)
        //            Debug.WriteLine(item);

        //        conn.Close();


        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        
        //}

        public static void DebugData(DataTable dat)
        {

            foreach (DataRow item in dat.Rows)
            {
                foreach (var j in item.ItemArray)
                    Debug.Write($"{j}\t");
                Debug.Write("\n");
            }
        }
        static void DebugData(List<List <int>> dat)
        {

            foreach (var  item in dat)
            {
                foreach (var j in item)
                    Debug.Write($"{j}\t");
                Debug.Write("\n");
            }
        }
        bool CheckTable()
        {
            DataView dataView = (DataView)DATAgrid.ItemsSource;
            DataTable dataTable = dataView.ToTable();
            foreach (DataRow item in dataTable.Rows)
                foreach (var j in item.ItemArray)
                    if (Convert.ToString(j) == "")
                    {
                        Debug.WriteLine("NULL NAIDEN");
                        return false;
                    }
            return true;
        }
        static int ValidateMonth(string ResDate) => int.Parse(ResDate) <=12 ?0:(int.Parse(ResDate)/12);
        static string CorrectStringMonth (int month)
        {
            if (month <= 12)
                if (month < 10)
                    return "0" + month.ToString();
                else
                    return month.ToString();
            else
                month = month % 12;
            return month < 10? "0" + month.ToString():month.ToString();
        }
        static List<string> SendSingleQuery(string query)
        { return new List<string>(); }
        static List<List <string>> SendQuery(string query)
        {
            Debug.Write(Connecting.DefaultConnetion);

            NpgsqlConnection conn = new NpgsqlConnection(Connecting.DefaultConnetion);
            NpgsqlCommand comm = new NpgsqlCommand(query, conn);
            List<List <string>> res = new List<List <string>>();
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
            catch (Exception ex) {  MessageBox.Show(ex.Message); }
            return res;
        }
        static string CorrectStringToQueryDate(string date) => date.Substring(6, 4) + "-" + date.Substring(3, 2) + "-" + date.Substring(0, 2);
        public static DataTable dt = new DataTable();
        public DataView DV { get; set; }
        private bool CheckConnection()
        {
            NpgsqlConnection conn = new NpgsqlConnection(Connecting.DefaultConnetion);
            try
            {
                conn.Open();
                conn.Close();
            }
            catch { return false; };
            return true;
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            //QUERIES();
            if (!CheckConnection())
            {
                MessageBox.Show("При подключении к базе данных произошла ошибка. Пожалуйста, повторите подключение через пару секунд, если проблема осталась, обратитесь к системному администратору.", "Ошибка подключения к базе данных", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string startMonth ="";
            try
            {
                Debug.WriteLine(Calendar_start.SelectedDate.Value);
                startMonth = Calendar_start.SelectedDate.Value.ToString("dd/MM/yyyy");

            }
            catch 
            { MessageBox.Show("Пожалуйста, введите начальную дату и количество месяцев!", "Ошибка запроса данных", MessageBoxButton.OK,MessageBoxImage.Error); return; }

            int monthCount;
            try
            {
                monthCount = int.Parse(MonthsBox.Text);
                if (monthCount <= 0)
                {
                    MessageBox.Show("Пожалуйста, введите целочисленное значение для количества месяцев.\nНапример, 12.",
                    "Ошибка ввода диапазона", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            catch 
            { 
                MessageBox.Show("Пожалуйста, введите целочисленное значение для количества месяцев.\nНапример, 12.", 
                    "Ошибка ввода диапазона", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            dt = new DataTable();
            DataColumn dataCol1 = new DataColumn("№", typeof(int)); 
            dataCol1.ColumnName = "№";
            dataCol1.DataType = typeof(string);
            dt.Columns.Add(dataCol1);
            dataCol1 = new DataColumn("Код категории", typeof(string));
            dataCol1.ColumnName = "Код категории";
            dataCol1.DataType = typeof(string);
            dt.Columns.Add(dataCol1);

            dataCol1 = new DataColumn("Продукция", typeof(string));
            dataCol1.ColumnName = "Продукция";
            dataCol1.DataType = typeof(string);
            dt.Columns.Add(dataCol1);
            dt.Columns.Add(new DataColumn("Стоимость", typeof(int)));
            dt.Columns.Add(new DataColumn("Единица измерения", typeof(string)));
            List<string> columns    = new List<string> { "№", "Продукция"};
            for (int i = 0; i < monthCount; i++)
            {
                int tempMonth = int.Parse(startMonth.Substring(3, 2)) + i;
                int year = int.Parse(startMonth.Substring(6, 4)) + ValidateMonth(tempMonth.ToString());
                string ResDate = startMonth.Substring(0, 2)+ "-";
                ResDate += CorrectStringMonth(tempMonth) + "-";
                ResDate += year.ToString();
                DataColumn dataCol = new DataColumn(ResDate, typeof(string));
                dataCol.ColumnName = ResDate;
                dataCol.DataType = typeof(int);
                dt.Columns.Add(dataCol);
                columns.Add(ResDate);

            }
            //DataGrid dtt = new DataGrid();    МБ ТАК.... сдвиг 1 месяц от исходного
            //for (int j = 0; j < 5; j++)
            //{
            //    //object[] cols = new object[monthCount + 2];
            //    //cols[0] = null;
            //    //cols[1] = "Блок";
            //    //for (int i = 2; i < monthCount + 2; i++)
            //    //    cols[i] = "лол";


            //}
            foreach (DataColumn col in dt.Columns)
                Debug.Write($"{col}\t");
            Debug.WriteLine("");
            foreach (DataRow row in dt.Rows)
            {
                foreach (var cell in row.ItemArray)
                    Debug.Write($"{cell}\t");
                Debug.Write("\n");
            }
            //}
            //for (int i = 0; i < monthCount; i++)
            //{
            //    string newMonth = CorrectStringToQueryDate(dt.Columns[i + 2].ColumnName);
            //    //string query = $@"select  b.product_name, a.product_sell_amount, 
            //    //                a.product_res_cost, c.contract_sign_date
            //    //                FROM dbschema.connection_contract_product as a,
            //    //                dbschema.products as b,
            //    //        		dbschema.contract_of_purchase_sale as c
            //    //                WHERE a.product_id = b.product_id and c.contract_id = a.contract_id and c.contract_sign_date between date('{newMonth}') and date('{newMonth}') +interval'1 month' - interval '1 day'
            //    //                ORDER BY a.contract_id ASC, a.product_id ASC; ";
            //    string query = $@"select  b.product_name,
            //                    FROM dbschema.connection_contract_product as a,
            //                    dbschema.products as b,
            //            		dbschema.contract_of_purchase_sale as c
            //                    WHERE a.product_id = b.product_id and c.contract_id = a.contract_id and c.contract_sign_date between date('{newMonth}') and date('{newMonth}') +interval'1 month' - interval '1 day'
            //                    ORDER BY a.contract_id ASC, a.product_id ASC; ";

            //    string query2 = $@"select  b.product_name, sum(a.product_sell_amount), sum(a.product_res_cost)
            //                    FROM dbschema.connection_contract_product as a,
            //                            dbschema.products as b,
            //                      dbschema.contract_of_purchase_sale as c
            //                    WHERE a.product_id = b.product_id and c.contract_id = a.contract_id and c.contract_sign_date between date('2021-02-01') and  date('2021-12-01') + interval'1 month'  and b.product_name = 'Блоки D100'
            //                    group by b.product_name, c.contract_sign_date, a.contract_id
            //                    ORDER BY a.contract_id;";

            //    List<List<string>> PRODUCTS = SendQuery(query);
            //    List<List<string>> queryRes = SendQuery(query2);
            //    for (int j )
            //    //if (i == 0)
            //    //{   
            //    //    for (int j = 0; j < queryRes.Count;j++)
            //    //    {
            //    //        DataRow rw = dt.NewRow();
            //    //        rw[0] = j;
            //    //        rw[1] = queryRes[j][0];
            //    //        Debug.WriteLine($" LOL {queryRes[j][0]}");
            //    //        dt.Rows.Add(rw.ItemArray);
            //    //    }

            //    //    }
            //    //for (int j = 0; j < queryRes.Count; j++)
            //    //{
            //    //    //DataRow temptw = dt.Rows[j];
            //    //    dt.Rows[j][columns[i+2]] = queryRes[j][1];
            //    //    Debug.WriteLine($"{dt.Rows[j][i+2]} = {queryRes[j][1]}");
            //    //}

            //}
            string newMonth = CorrectStringToQueryDate(dt.Columns[5].ColumnName);
            string lastMonth = CorrectStringToQueryDate(dt.Columns[dt.Columns.Count-1].ColumnName);

            string query = $@"select  b.product_id, b.product_name, b.product_price,  b.product_unit_measure
                                    FROM dbschema.connection_contract_product as a,
                                    dbschema.products as b,
                            		dbschema.contract_of_purchase_sale as c
                                    WHERE a.product_id = b.product_id and c.contract_id = a.contract_id and c.contract_sign_date between date('{newMonth}') and date('{newMonth}') +interval'1 month' - interval '1 day'
                                    ORDER BY a.contract_id ASC, a.product_id ASC; ";

            List<string> PRODUCTS = SendQuery(query).Select(x => x[1]).ToList();
            List<string> CODES= SendQuery(query).Select(x => x[0]).ToList();
            if (PRODUCTS.Count == 0)
            {
                MessageBox.Show("В начальном месяце не было продаж, пожалуйста, выберите корректную дату", "Ошибка начальной даты", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }  

            List<string> COST = SendQuery(query).Select(x => x[2]).ToList();
            List<string> MEASUREMENT = SendQuery(query).Select(x => x[3]).ToList();
            for (int i = 0; i < PRODUCTS.Count; i++)
            {
                List<string> rowList = new List<string> { (i+1).ToString(),CODES[i], PRODUCTS[i], COST[i], MEASUREMENT[i] };
                string query2 = $@"select  b.product_name, sum(a.product_sell_amount), sum(a.product_res_cost)
                                FROM dbschema.connection_contract_product as a,
                                        dbschema.products as b,
                                    dbschema.contract_of_purchase_sale as c
                                WHERE a.product_id = b.product_id and c.contract_id = a.contract_id and c.contract_sign_date between date('{newMonth}') and  date('{lastMonth}') + interval'1 month'  and b.product_name = '{PRODUCTS[i]}'
                                group by b.product_name, c.contract_sign_date, a.contract_id
                                ORDER BY a.contract_id ASC;";
                List<List<string>> queryRes = SendQuery(query2);
                for (int j = 0; j < queryRes.Count; j++)
                    rowList.Add(queryRes[j][1]);
                DataRow dr = dt.NewRow();
                for (int j = 0; j < rowList.Count; j++)
                {
                    if (j >4)
                        dr[j] = int.Parse(rowList[j]);
                    else 
                        dr[j] = rowList[j];
                    
                }
                dt.Rows.Add( dr);
            }

                //DATAgrid.BindingGroup = new DataView(dt);
                //DATAgrid.ItemsSource = new DataView(dt);
                //DV = new DataView(dt);
                DV = new DataView(dt);
                //dv.Table = dt;
                DATAgrid.ItemsSource = DV;
            //DATAgrid.DataContext = DATAgrid;
            //this.DataContext = this;
            //DATAgrid.IsReadOnly = false;
            DebugData(dt);
            if (!CheckTable())
            {
                MessageBox.Show("Выберите диапазон месяцев, в которых известны объемы продаж", "Ошибка загрузки данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Solve.IsEnabled = true;
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (DATAgrid.SelectedIndex >= 0)

            {
                DataRow product = null;
                //remove the selectedItem from the collection source
                try
                {
                    product = ((DataRowView)DATAgrid.SelectedItem).Row;
                }
                catch { MessageBox.Show("Ошибка удаления, пустую строку невозможно удалить!"); return; }

                DataTable dt2 = ((DataView)(DATAgrid.ItemsSource)).ToTable();
                DebugData(dt2);
                foreach (DataRow row in dt2.Rows)
                {
                    Debug.WriteLine(row.ItemArray.ToString());
                    Debug.WriteLine("Выбранная строка");
                    for (int i = 0; i < product.ItemArray.Length; i++)
                        Debug.Write(product.ItemArray[i] + " ");
                    Debug.Write("\n");
                    
                    if (row.ItemArray[0] == product.ItemArray[0])
                    {
                        row.Delete();
                        break;
                    }

                }
                dt2.AcceptChanges();
                DATAgrid.ItemsSource = new DataView(dt2);
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            Button_Click_1(sender, e);
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            if (DATAgrid.ItemsSource == null)
            {
                DataTable freedt = new DataTable();
                DataColumn dataCol1 = new DataColumn("№", typeof(int));
                dataCol1.ColumnName = "№";
                dataCol1.DataType = typeof(string);
                freedt.Columns.Add(dataCol1);
                dataCol1 = new DataColumn("Код категории", typeof(string));
                dataCol1.ColumnName = "Код категории";
                dataCol1.DataType = typeof(string);
                freedt.Columns.Add(dataCol1);

                dataCol1 = new DataColumn("Продукция", typeof(string));
                dataCol1.ColumnName = "Продукция";
                dataCol1.DataType = typeof(string);
                freedt.Columns.Add(dataCol1);
                freedt.Columns.Add(new DataColumn("Стоимость", typeof(int)));
                freedt.Columns.Add(new DataColumn("Единица измерения", typeof(string)));
                List<string> columns = new List<string> { "№", "Продукция" };
                int monthCount;
                try
                {
                    monthCount = int.Parse(MonthsBox.Text);
                    if (monthCount <= 0)
                    {
                        MessageBox.Show("Пожалуйста, введите целочисленное значение для количества месяцев.\nНапример, 12.",
                        "Ошибка ввода диапазона", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                catch
                {
                    MessageBox.Show("Пожалуйста, введите целочисленное значение для количества месяцев.\nНапример, 12.",
                        "Ошибка ввода диапазона", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                string startMonth = "";
                try
                {
                    Debug.WriteLine(Calendar_start.SelectedDate.Value);
                    startMonth = Calendar_start.SelectedDate.Value.ToString("dd/MM/yyyy");

                }
                catch
                { MessageBox.Show("Пожалуйста, введите начальную дату и количество месяцев!", "Ошибка запроса данных", MessageBoxButton.OK, MessageBoxImage.Error); return; }

                for (int i = 0; i < monthCount; i++)
                {
                    int tempMonth = int.Parse(startMonth.Substring(3, 2)) + i;
                    int year = int.Parse(startMonth.Substring(6, 4)) + ValidateMonth(tempMonth.ToString());
                    string ResDate = startMonth.Substring(0, 2) + "-";
                    ResDate += CorrectStringMonth(tempMonth) + "-";
                    ResDate += year.ToString();
                    DataColumn dataCol = new DataColumn(ResDate, typeof(string));
                    dataCol.ColumnName = ResDate;
                    dataCol.DataType = typeof(int);
                    freedt.Columns.Add(dataCol);

                }
                DATAgrid.ItemsSource = new DataView(freedt);
            }

            return;
        }
    }
}
