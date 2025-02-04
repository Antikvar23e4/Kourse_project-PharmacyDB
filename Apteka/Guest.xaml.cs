using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Shapes;

namespace Apteka
{
    /// <summary>
    /// Логика взаимодействия для Guest.xaml
    /// </summary>
    public partial class Guest : Window
    {
        OracleConnection ConnectionToOracle;
        string connectionString = "DATA SOURCE=localhost:1521/orcl.mshome.net;TNS_ADMIN=C:\\Users\\oracledatabase\\Oracle\\network\\admin;PERSIST SECURITY INFO=True;USER ID=PROGRAMMER;PASSWORD=12345";

        public Guest()
        {
            InitializeComponent();
            try
            {
                ConnectionToOracle = new OracleConnection(connectionString);
                ConnectionToOracle.Open();
            }
            catch (OracleException ex)
            {
                MessageBox.Show("Ошибка при подключении к базе данных: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Drug_name.Text) || string.IsNullOrEmpty(Drug_dosage.Text) )
            {
                MessageBox.Show("Введите название и дозировку лекарства для поиска.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                string name = Drug_name.Text;
                string dosage = Drug_dosage.Text;

                string sqlQuery = "SELECT * FROM Drug_Search_View WHERE Name LIKE '%' || :name ||  '%' AND Dosage LIKE '%' || :dosage || '%'";

                using (OracleCommand cmd = new OracleCommand(sqlQuery, ConnectionToOracle))
                {
                    cmd.Parameters.Add("name", OracleDbType.Varchar2).Value = name;
                    cmd.Parameters.Add("dosage", OracleDbType.Varchar2).Value = dosage;

                    OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("Ничего не найдено.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        ResultData.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (OracleException ex)
            {
                MessageBox.Show("Ошибка Oracle: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Sort_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string drugName = Drug_name.Text;
                string dosage = Drug_dosage.Text;
                string sqlQuery = "SELECT * FROM Drug_Availability_View WHERE Drug_Name = :drugName AND Dosage = :dosage";

                using (OracleCommand cmd = new OracleCommand(sqlQuery, ConnectionToOracle))
                {
                    cmd.Parameters.Add("drugName", OracleDbType.Varchar2).Value = drugName;
                    cmd.Parameters.Add("dosage", OracleDbType.Varchar2).Value = dosage;

                    OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    ResultData.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (OracleException ex)
            {
                MessageBox.Show("Ошибка Oracle: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Reviewes_on_drug_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string drugName = Drug_name.Text;
                string dosage = Drug_dosage.Text;
                string sqlQuery = "SELECT * FROM DrugReviewsView WHERE DrugName = :drugName AND Dosage = :dosage";

                using (OracleCommand cmd = new OracleCommand(sqlQuery, ConnectionToOracle))
                {
                    cmd.Parameters.Add(new OracleParameter("drugName", drugName));
                    cmd.Parameters.Add(new OracleParameter("dosage", dosage));

                    OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    if (dataTable.Rows.Count == 0)
                    {
                        MessageBox.Show("На это лекарство еще нет отзывов.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        ResultData.ItemsSource = dataTable.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при выполнении запроса: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Find_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string coordinates = user_coord.Text;
                string[] parts = coordinates.Split(',');

                if (parts.Length == 2)
                {
                    double latitude;
                    double longitude;

                    if (double.TryParse(parts[0], out latitude) && double.TryParse(parts[1], out longitude))
                    {
                        using (OracleCommand command = new OracleCommand("Find_Closest_Pharmacies", ConnectionToOracle))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.Add("user_latitude", OracleDbType.Double).Value = latitude;
                            command.Parameters.Add("user_longitude", OracleDbType.Double).Value = longitude;
                            command.Parameters.Add("cur_pharmacies", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                            OracleDataAdapter adapter = new OracleDataAdapter(command);
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            ResultData.ItemsSource = dt.DefaultView;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Введите корректные координаты.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Введите корректные координаты в формате 'широта, долгота'.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при выполнении запроса: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void All_drugs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sqlQuery = "SELECT * FROM T_DRUGS";
                using (OracleDataAdapter adapter = new OracleDataAdapter(sqlQuery, ConnectionToOracle))
                {
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet);
                    ResultData.ItemsSource = dataSet.Tables[0].DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при выполнении запроса: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void All_Pharmacy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sqlQuery = "SELECT Pharmacy_Name, Pharma_Adress, Phone_Number, Email, Working_Hours FROM T_INSTITUTION";
                using (OracleDataAdapter adapter = new OracleDataAdapter(sqlQuery, ConnectionToOracle))
                {
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet);
                    ResultData.ItemsSource = dataSet.Tables[0].DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при выполнении запроса: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ConnectionToOracle.Close();
            MainWindow newWindow = new MainWindow();
            this.Close();
            newWindow.Show();
        }
    }
}
