using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
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
using System.Xml.Linq;
using System.Device.Location;
using System.ComponentModel;
using CefSharp;
using CefSharp.Wpf;

namespace Apteka
{
    /// <summary>
    /// Логика взаимодействия для User.xaml
    /// </summary>
    public partial class User : Window
    {
        private int userId;
        private int pharmacyId;
        OracleConnection ConnectionToOracle;
        string connectionString = "DATA SOURCE=localhost:1521/orcl.mshome.net;TNS_ADMIN=C:\\Users\\oracledatabase\\Oracle\\network\\admin;PERSIST SECURITY INFO=True;USER ID=PROGRAMMER;PASSWORD=12345";

        public User(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            Loaded += User_Loaded;
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

        private void User_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayUserData(userId);
            DisplayUserFavorites(userId);
            DisplayUserReservation(userId);

        }

        private void Change_info_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Phone_number.Text) || string.IsNullOrEmpty(Email.Text) || string.IsNullOrEmpty(Password.Text))
            {
                MessageBox.Show("Заполните поля, в том что хотите изменить.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
           
            try
            {
                string phoneNumber = Phone_number.Text;
                string email = Email.Text;
                string password = Password.Text;

                UpdateUserInfo(userId, phoneNumber, email, password);
                MessageBox.Show("Информация пользователя успешно обновлена.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                DisplayUserData(userId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Результат: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void UpdateUserInfo(int userId, string phoneNumber, string email, string password)
        {
            try
            {
                using (OracleCommand cmd = new OracleCommand("Update_User_Info", ConnectionToOracle))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_user_id", OracleDbType.Int32).Value = userId;
                    cmd.Parameters.Add("p_phone_number", OracleDbType.Varchar2).Value = phoneNumber;
                    cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = email;
                    cmd.Parameters.Add("p_password", OracleDbType.Varchar2).Value = password;
                    cmd.Parameters.Add("p_error", OracleDbType.Varchar2, ParameterDirection.Output).Size = 200;

                    cmd.ExecuteNonQuery();

                    string errorMsg = cmd.Parameters["p_error"].Value.ToString();
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        MessageBox.Show(errorMsg, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (OracleException ex)
            {
                throw new Exception("Ошибка Oracle: " + ex.Message);
            }
        }
        private int GetDrugId(string drugName, string dosage)                                       
        {
            int drugId = -1;

            try
            {
                string sqlQuery = "SELECT DrugID FROM Drugs WHERE Name = :drugName AND Dosage = :dosage";

                using (OracleCommand cmd = new OracleCommand(sqlQuery, ConnectionToOracle))
                {
                    cmd.Parameters.Add("drugName", OracleDbType.Varchar2).Value = drugName;
                    cmd.Parameters.Add("dosage", OracleDbType.Varchar2).Value = dosage;

                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        drugId = Convert.ToInt32(result);
                    }
                    else
                    {
                        MessageBox.Show("Лекарство с указанным именем и дозировкой не найдено.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении DrugID: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            return drugId;
        }
        private void Make_review_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Drug_name.Text) || string.IsNullOrEmpty(Drug_dosage.Text) || string.IsNullOrEmpty(Review_text.Text))
            {
                MessageBox.Show("Введите название и дозировку лекарства. Добавьте текст своего отзыва.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                string drugName = Drug_name.Text; 
                string dosage = Drug_dosage.Text; 
                int drugId = GetDrugId(drugName, dosage); 

                if (drugId != -1) 
                {
                    string content = Review_text.Text;

                    AddReview(userId, drugId, content); 
                }
                else
                {
                    MessageBox.Show("Не удалось найти лекарство с указанным названием и дозировкой.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Результат: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void AddReview(int userId, int drugId, string content)
        {
            try
            {
                using (OracleCommand cmd = new OracleCommand("Add_Review", ConnectionToOracle))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_UserID", OracleDbType.Int32).Value = userId;
                    cmd.Parameters.Add("p_DrugID", OracleDbType.Int32).Value = drugId;
                    cmd.Parameters.Add("p_Content", OracleDbType.Varchar2).Value = content;
                    cmd.Parameters.Add("p_error", OracleDbType.Varchar2, ParameterDirection.Output).Size = 200;

                    cmd.ExecuteNonQuery();

                    string errorMsg = cmd.Parameters["p_error"].Value.ToString();
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        MessageBox.Show(errorMsg, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Отзыв успешно добавлен.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
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
        private void Add_fav_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Drug_name.Text) || string.IsNullOrEmpty(Drug_dosage.Text))
            {
                MessageBox.Show("Чтобы добавить лекарство в избранное, введите название и дозировку.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                string drugName = Drug_name.Text;
                string dosage = Drug_dosage.Text;
            
                int drugId = GetDrugId(drugName, dosage);
                if (drugId != -1)
                {
                    AddToFavorites(drugId, userId);
                    DisplayUserFavorites(userId);
                }
                else
                {
                    MessageBox.Show("Не удалось найти лекарство с указанным названием и дозировкой.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void AddToFavorites(int drugId, int userId)
        {
            try
            {
                using (OracleCommand cmd = new OracleCommand("Add_To_Favorites", ConnectionToOracle))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_DrugId", OracleDbType.Int32).Value = drugId;
                    cmd.Parameters.Add("p_UserId", OracleDbType.Int32).Value = userId;
                    cmd.Parameters.Add("p_FavoriteId", OracleDbType.Int32).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("p_Error", OracleDbType.Varchar2, ParameterDirection.Output).Size = 200;

                    cmd.ExecuteNonQuery();

                    string errorMsg = cmd.Parameters["p_Error"].Value.ToString();
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        MessageBox.Show(errorMsg, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        int favoriteId = Convert.ToInt32(cmd.Parameters["p_FavoriteId"].Value);
                        MessageBox.Show("Запись успешно добавлена в избранное. FavoriteID: " + favoriteId, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
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
        private void Make_reservation_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Drug_name.Text) || string.IsNullOrEmpty(Drug_dosage.Text) || string.IsNullOrEmpty(Pharmacy_name.Text) 
                || string.IsNullOrEmpty(Pharmacy_adres.Text) || string.IsNullOrEmpty(Reservation_quantity.Text) || string.IsNullOrEmpty(Reservation_data.Text))
            {
                MessageBox.Show("Для бронирования необходимо ввести название и дозировку, а так же заполнить остальные поля.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                string drugName = Drug_name.Text;
                string dosage = Drug_dosage.Text;
                string name = Pharmacy_name.Text;
                string address = Pharmacy_adres.Text;
                int drugId = GetDrugId(drugName, dosage);
                SearchPharmacy(name,address); 

                
                if (userId != -1 && drugId != -1 && pharmacyId != -1)
                {
                  
                    int quantity = Convert.ToInt32(Reservation_quantity.Text);
                    DateTime orderDate = DateTime.Parse(Reservation_data.Text);

                    AddReservation(userId, drugId, pharmacyId, quantity, orderDate, "Ожидание");
                    DisplayUserReservation(userId);
                }
                else
                {
                    MessageBox.Show("Не удалось выполнить бронирование. Пожалуйста, убедитесь, что все необходимые данные заполнены верно.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при бронировании: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void AddReservation(int userId, int drugId, int pharmacyId, int quantity, DateTime orderDate, string status)
        {
            try
            {
                using (OracleCommand cmd = new OracleCommand("Add_Reservation", ConnectionToOracle))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_UserID", OracleDbType.Int32).Value = userId;
                    cmd.Parameters.Add("p_DrugID", OracleDbType.Int32).Value = drugId;
                    cmd.Parameters.Add("p_PharmacyID", OracleDbType.Int32).Value = pharmacyId;
                    cmd.Parameters.Add("p_Quantity", OracleDbType.Int32).Value = quantity;
                    cmd.Parameters.Add("p_Order_Date", OracleDbType.Date).Value = orderDate;
                    cmd.Parameters.Add("p_Status", OracleDbType.Varchar2).Value = status;
                    cmd.Parameters.Add("p_OrderID", OracleDbType.Int32, ParameterDirection.Output);
                    cmd.Parameters.Add("p_Error", OracleDbType.Varchar2, ParameterDirection.Output).Size = 200;

                    cmd.ExecuteNonQuery();

                    string errorMsg = cmd.Parameters["p_Error"].Value.ToString();
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        MessageBox.Show(errorMsg, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        int orderId = Convert.ToInt32(cmd.Parameters["p_OrderID"].Value);
                        MessageBox.Show("Бронь успешно добавлена. OrderID: " + orderId, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
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
        private void SearchPharmacy(string name, string address)
        {
            try
            {
                using (OracleCommand cmd = new OracleCommand("SearchPharmacy", ConnectionToOracle))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_name", OracleDbType.Varchar2).Value = name;
                    cmd.Parameters.Add("p_address", OracleDbType.Varchar2).Value = address;
                    cmd.Parameters.Add("p_pharmacy_id", OracleDbType.Int32).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("p_error_msg", OracleDbType.Varchar2, 200).Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();

                    OracleDecimal pharmacyIdOracle = (OracleDecimal)cmd.Parameters["p_pharmacy_id"].Value;
                    int? pharmacyId = null;
                    if (!pharmacyIdOracle.IsNull)
                    {
                        pharmacyId = pharmacyIdOracle.ToInt32();
                    }
                    string errorMsg = cmd.Parameters["p_error_msg"].Value.ToString();

                    if (pharmacyId != null)
                    {
                        MessageBox.Show("Найдена аптека с PharmacyID: " + pharmacyId, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.pharmacyId = pharmacyId.Value;

                    }
                    else
                    {
                        MessageBox.Show(errorMsg, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
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
        private void Drop_fav_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int drugId;
                if (int.TryParse(DrugId.Text, out drugId))
                {
                    if (userId != -1 && drugId != -1)
                    {
                        DeleteFavoriteByUserIdAndDrugId(userId, drugId);
                        DisplayUserFavorites(userId);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось получить UserId или DrugId.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("DrugId должен быть числом.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void DeleteFavoriteByUserIdAndDrugId(int userId, int drugId)
        {
            try
            {
                using (OracleCommand cmd = new OracleCommand("Delete_Favorite", ConnectionToOracle))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_UserId", OracleDbType.Int32).Value = userId;
                    cmd.Parameters.Add("p_DrugId", OracleDbType.Int32).Value = drugId;
                    cmd.Parameters.Add("p_Error", OracleDbType.Varchar2, ParameterDirection.Output).Size = 200;

                    cmd.ExecuteNonQuery();

                    string errorMsg = cmd.Parameters["p_Error"].Value.ToString();
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        MessageBox.Show(errorMsg, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Запись в избранном успешно удалена.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
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
        private void Drop_reserv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int orderID;
                if (int.TryParse(Reserv_idv.Text, out orderID))
                {
                    string errorMsg;
                    using (OracleCommand cmd = new OracleCommand("Delete_Reservation", ConnectionToOracle))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_OrderID", OracleDbType.Int32).Value = orderID;
                        cmd.Parameters.Add("p_error", OracleDbType.Varchar2, ParameterDirection.Output).Size = 200;

                        cmd.ExecuteNonQuery();

                        errorMsg = cmd.Parameters["p_error"].Value.ToString();
                    }
                    MessageBox.Show(errorMsg, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    DisplayUserReservation(userId);
                }
                else
                {
                    MessageBox.Show("OrderID должен быть числом.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Drug_name.Text) || string.IsNullOrEmpty(Drug_dosage.Text))
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
                MessageBox.Show("Ошибка Oracle: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private void DisplayUserReservation(int userId)
        {
            try
            {
                string sqlQuery = "SELECT * FROM UserReservationView WHERE UserID = :userId";
                using (OracleCommand cmd = new OracleCommand(sqlQuery, ConnectionToOracle))
                {

                    cmd.Parameters.Add(new OracleParameter("userId", userId));
                    OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    Reservation_Data.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при заполнении данных: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void DisplayUserData(int userId)
        {
            try
            {
                string sqlQuery = "SELECT * FROM T_USERS WHERE UserId = :userId";
                using (OracleCommand cmd = new OracleCommand(sqlQuery, ConnectionToOracle))
                {

                    cmd.Parameters.Add(new OracleParameter("userId", userId));
                    OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    User_Data.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при заполнении данных: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void DisplayUserFavorites(int userId)
        {
            try
            {
                string sqlQuery = "SELECT * FROM UserFavoritesView WHERE UserID = :userId";
                using (OracleCommand cmd = new OracleCommand(sqlQuery, ConnectionToOracle))
                {
                    cmd.Parameters.Add(new OracleParameter("userId", userId));
                    OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    Favorite_Data.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при отображении избранных лекарств: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
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
