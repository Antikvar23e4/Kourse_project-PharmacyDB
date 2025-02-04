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
using Oracle.ManagedDataAccess.Types;


namespace Apteka
{
    /// <summary>
    /// Логика взаимодействия для Manager.xaml
    /// </summary>
    public partial class Manager : Window
    {
        private int userId;
        OracleConnection ConnectionToOracle;
        string connectionString = "DATA SOURCE=localhost:1521/orcl.mshome.net;TNS_ADMIN=C:\\Users\\oracledatabase\\Oracle\\network\\admin;PERSIST SECURITY INFO=True;USER ID=PROGRAMMER;PASSWORD=12345";

        public Manager(int userId)
        {
            InitializeComponent();
            this.userId = userId;
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

            Loaded += Manager_Loaded;
        }

        private void Manager_Loaded(object sender, RoutedEventArgs e)
        {
            CheckAndAddOrUpdateManager(userId);
        }

        private void CheckAndAddOrUpdateManager(int userId)
        {
            try
            {
                {
                    int managerCount;
                    using (OracleCommand checkCommand = new OracleCommand("SELECT COUNT(*) FROM T_MANAGERS WHERE UserID = :userId", ConnectionToOracle))
                    {
                        checkCommand.Parameters.Add(":userId", OracleDbType.Int32).Value = userId;
                        managerCount = Convert.ToInt32(checkCommand.ExecuteScalar());
                    }

                    if (managerCount > 0)
                    {
                        UpdateManagerStatus(userId, "Active");
                    }
                    else
                    {
                        AddManager(userId, "Active");
                    }
                }
            }
            catch (OracleException ex)
            {
                MessageBox.Show("Ошибка Oracle: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
        private void AddManager(int userId, string status)
        {
            try
            {
                {
                    using (OracleCommand command = new OracleCommand("Add_Manager", ConnectionToOracle))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("p_UserID", OracleDbType.Int32).Value = userId;
                        command.Parameters.Add("p_Status", OracleDbType.Varchar2).Value = status;
                        command.ExecuteNonQuery();
                        MessageBox.Show("Менеджер добавлен.");
                    }
                }
            }
            catch (OracleException ex)
            {
                MessageBox.Show("Ошибка Oracle: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void UpdateManagerStatus(int userId, string status)
        {
            try
            {
                {

                    using (OracleCommand command = new OracleCommand("Update_Manager_Status", ConnectionToOracle))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("p_UserID", OracleDbType.Int32).Value = userId;
                        command.Parameters.Add("p_Status", OracleDbType.Varchar2).Value = status;
                        command.ExecuteNonQuery();
                        MessageBox.Show("Менеджер в сети.");
                    }
                }
            }
            catch (OracleException ex)
            {
                MessageBox.Show("Ошибка Oracle: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string status2 = "Offline";
            UpdateManagerStatus2(userId, status2);
            ConnectionToOracle.Close();
            MainWindow newWindow = new MainWindow();
            this.Close();
            newWindow.Show();
        }

        private void UpdateManagerStatus2(int userId, string status2)
        {
            try
            {
                using (OracleCommand command = new OracleCommand("Update_Manager_Status", ConnectionToOracle))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("p_UserID", OracleDbType.Int32).Value = userId;
                    command.Parameters.Add("p_Status", OracleDbType.Varchar2).Value = status2;
                    command.ExecuteNonQuery();
                    MessageBox.Show("Вы вышли из системы.");
                }
            }
            catch (OracleException ex)
            {
                MessageBox.Show("Ошибка Oracle: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void Create_Drug_button_Click(object sender, RoutedEventArgs e)
        {
            string name = Drug_name.Text;
            string manufacturer = Manufacturer.Text;
            string instruction = Instruction.Text;
            string applicationFeatures = Application_Features.Text;
            string activeSubstance = Active_Substance.Text;
            string indications = Indications.Text;
            string contraindications = Contraindications.Text;
            string dosage = Dosage.Text;
            string sideEffect = Side_Effects.Text;

            string resultMessage = null;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(dosage) || string.IsNullOrEmpty(manufacturer) || string.IsNullOrEmpty(instruction) || string.IsNullOrEmpty(sideEffect)
            || string.IsNullOrEmpty(applicationFeatures) || string.IsNullOrEmpty(activeSubstance) || string.IsNullOrEmpty(indications) || string.IsNullOrEmpty(contraindications))
            { 
                MessageBox.Show("Пожалуйста, заполните все поля для  лекарства.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                using (OracleCommand command = new OracleCommand("Add_Drug", ConnectionToOracle))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("p_Name", OracleDbType.Varchar2).Value = name;
                    command.Parameters.Add("p_Manufacturer", OracleDbType.Varchar2).Value = manufacturer;
                    command.Parameters.Add("p_Instruction", OracleDbType.Varchar2).Value = instruction;
                    command.Parameters.Add("p_Application_Features", OracleDbType.Varchar2).Value = applicationFeatures;
                    command.Parameters.Add("p_Active_Substance", OracleDbType.Varchar2).Value = activeSubstance;
                    command.Parameters.Add("p_Indications", OracleDbType.Varchar2).Value = indications;
                    command.Parameters.Add("p_Contraindications", OracleDbType.Varchar2).Value = contraindications;
                    command.Parameters.Add("p_Dosage", OracleDbType.Varchar2).Value = dosage;
                    command.Parameters.Add("p_Side_Effects", OracleDbType.Varchar2).Value = sideEffect;

                    command.Parameters.Add("p_result", OracleDbType.Varchar2, ParameterDirection.Output).Size = 200;

                    command.ExecuteNonQuery();

                    resultMessage = command.Parameters["p_result"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                resultMessage = "Ошибка при выполнении запроса: " + ex.Message;
            }

            if (!string.IsNullOrEmpty(resultMessage))
            {
                MessageBox.Show(resultMessage, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            else
            {
                MessageBox.Show("Не удалось получить сообщение.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Update_Drug_button_Click(object sender, RoutedEventArgs e)
        {
            string name = Drug_name.Text;
            string dosage = Dosage.Text;
            string manufacturer = Manufacturer.Text;
            string instruction = Instruction.Text;
            string applicationFeatures = Application_Features.Text;
            string activeSubstance = Active_Substance.Text;
            string indications = Indications.Text;
            string contraindications = Contraindications.Text;
            string sideEffect = Side_Effects.Text;

            string resultMessage = null;
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(dosage) || string.IsNullOrEmpty(manufacturer) || string.IsNullOrEmpty(instruction) || string.IsNullOrEmpty(sideEffect)
             || string.IsNullOrEmpty(applicationFeatures) || string.IsNullOrEmpty(activeSubstance) || string.IsNullOrEmpty(indications) || string.IsNullOrEmpty(contraindications))
            {
                MessageBox.Show("Пожалуйста, заполните все поля для  лекарства.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                using (OracleCommand command = new OracleCommand("Update_Drug", ConnectionToOracle))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("p_Name", OracleDbType.Varchar2).Value = name;
                    command.Parameters.Add("p_Dosage", OracleDbType.Varchar2).Value = dosage;
                    command.Parameters.Add("p_Manufacturer", OracleDbType.Varchar2).Value = manufacturer;
                    command.Parameters.Add("p_Instruction", OracleDbType.Varchar2).Value = instruction;
                    command.Parameters.Add("p_Application_Features", OracleDbType.Varchar2).Value = applicationFeatures;
                    command.Parameters.Add("p_Active_Substance", OracleDbType.Varchar2).Value = activeSubstance;
                    command.Parameters.Add("p_Indications", OracleDbType.Varchar2).Value = indications;
                    command.Parameters.Add("p_Contraindications", OracleDbType.Varchar2).Value = contraindications;
                    command.Parameters.Add("p_Side_Effects", OracleDbType.Varchar2).Value = sideEffect;

                    command.Parameters.Add("p_result", OracleDbType.Varchar2, ParameterDirection.Output).Size = 200;

                    command.ExecuteNonQuery();

                    resultMessage = command.Parameters["p_result"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                resultMessage = "Ошибка при выполнении запроса: " + ex.Message;
            }

            if (!string.IsNullOrEmpty(resultMessage))
            {
                MessageBox.Show(resultMessage, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                Drug_name.Text = "";
                Manufacturer.Text = "";
                Instruction.Text = "";
                Application_Features.Text = "";
                Active_Substance.Text = "";
                Indications.Text = "";
                Contraindications.Text = "";
                Dosage.Text = "";
                Side_Effects.Text = "";
            }
            else
            {
                MessageBox.Show("Не удалось получить сообщение.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void Delete_Drug_button_Click(object sender, RoutedEventArgs e)
        {
            string name = Drug_name.Text;
            string dosage = Dosage.Text;

            string resultMessage = null;
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(dosage))
            {
                MessageBox.Show("Пожалуйста, заполните поля названия и дозировки лекарства.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                using (OracleCommand command = new OracleCommand("Delete_Drug", ConnectionToOracle))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("p_Name", OracleDbType.Varchar2).Value = name;
                    command.Parameters.Add("p_Dosage", OracleDbType.Varchar2).Value = dosage;
                    command.Parameters.Add("p_result", OracleDbType.Varchar2, ParameterDirection.Output).Size = 200;

                    command.ExecuteNonQuery();

                    resultMessage = command.Parameters["p_result"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                resultMessage = "Ошибка при выполнении запроса: " + ex.Message;
            }

            if (!string.IsNullOrEmpty(resultMessage))
            {
                MessageBox.Show(resultMessage, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                Drug_name.Text = "";
                Manufacturer.Text = "";
                Instruction.Text = "";
                Application_Features.Text = "";
                Active_Substance.Text = "";
                Indications.Text = "";
                Contraindications.Text = "";
                Dosage.Text = "";
                Side_Effects.Text = "";
            }
            else
            {
                MessageBox.Show("Не удалось получить сообщение.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        public class SdoPointType
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        private void Create_Institution_button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Institution_name.Text) || string.IsNullOrWhiteSpace(Adress.Text) || string.IsNullOrWhiteSpace(Phone_namber.Text) ||
        string.IsNullOrWhiteSpace(Email.Text) || string.IsNullOrWhiteSpace(Workinh_hours.Text) ||string.IsNullOrWhiteSpace(Coordinates.Text))
    {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            double latitude;
            double longitude;
            if (double.TryParse(Coordinates.Text.Split(',')[0], out latitude) && double.TryParse(Coordinates.Text.Split(',')[1], out longitude))
            {
                try
                {
                    SdoPointType point = new SdoPointType { Latitude = latitude, Longitude = longitude };

                    using (OracleCommand command = new OracleCommand("Add_Institution", ConnectionToOracle))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("p_Pharmacy_Name", OracleDbType.Varchar2).Value = Institution_name.Text;
                        command.Parameters.Add("p_Location_Latitude", OracleDbType.Double).Value = point.Latitude;
                        command.Parameters.Add("p_Location_Longitude", OracleDbType.Double).Value = point.Longitude;
                        command.Parameters.Add("p_Pharma_Adress", OracleDbType.Varchar2).Value = Adress.Text;
                        command.Parameters.Add("p_Phone_Number", OracleDbType.Varchar2).Value = Phone_namber.Text;
                        command.Parameters.Add("p_Email", OracleDbType.Varchar2).Value = Email.Text;
                        command.Parameters.Add("p_Working_Hours", OracleDbType.Varchar2).Value = Workinh_hours.Text;
                        command.Parameters.Add("p_error", OracleDbType.Varchar2, ParameterDirection.Output).Size = 200;

                        command.ExecuteNonQuery();

                        string errorMessage = command.Parameters["p_error"].Value.ToString();
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            MessageBox.Show(errorMessage, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Запись успешно добавлена.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при выполнении запроса: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Введите корректные координаты в формате \"широта, долгота\".", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Update_Institution_button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Institution_name.Text) || string.IsNullOrWhiteSpace(Adress.Text))
            {
                MessageBox.Show("Пожалуйста, введите название и адресс аптеки. Поля доступные изменению:номер телефона, почта, часы работы.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            string pharmacyName = Institution_name.Text;
            string pharmacyAddress = Adress.Text;
            string newPhoneNumber = Phone_namber.Text;
            string newEmail = Email.Text;
            string newWorkingHours = Workinh_hours.Text;

            try
            {
                using (OracleCommand command = new OracleCommand("Update_Institution", ConnectionToOracle))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("p_Pharmacy_Name", OracleDbType.Varchar2).Value = pharmacyName;
                    command.Parameters.Add("p_Pharma_Adress", OracleDbType.Varchar2).Value = pharmacyAddress;
                    command.Parameters.Add("p_New_Phone_Number", OracleDbType.Varchar2).Value = newPhoneNumber;
                    command.Parameters.Add("p_New_Email", OracleDbType.Varchar2).Value = newEmail;
                    command.Parameters.Add("p_New_Working_Hours", OracleDbType.Varchar2).Value = newWorkingHours;
                    command.Parameters.Add("p_error", OracleDbType.Varchar2, ParameterDirection.Output).Size = 200;

                    command.ExecuteNonQuery();

                    string errorMessage = command.Parameters["p_error"].Value.ToString();
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        MessageBox.Show(errorMessage, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Информация об аптеке успешно обновлена.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при выполнении запроса: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Delete_Institution_button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Institution_name.Text) || string.IsNullOrWhiteSpace(Adress.Text))
            {
                MessageBox.Show("Пожалуйста, введите название и адресс аптеки.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            string institutionName = Institution_name.Text;
            string address = Adress.Text;

            try
            {
                using (OracleCommand command = new OracleCommand("Delete_Institution", ConnectionToOracle))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("p_Pharmacy_Name", OracleDbType.Varchar2).Value = institutionName;
                    command.Parameters.Add("p_Pharma_Adress", OracleDbType.Varchar2).Value = address;
                    command.Parameters.Add("p_error", OracleDbType.Varchar2, ParameterDirection.Output).Size = 200;

                    command.ExecuteNonQuery();

                    string errorMessage = command.Parameters["p_error"].Value.ToString();
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        MessageBox.Show(errorMessage, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Аптека успешно удалена.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при выполнении запроса: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Show_users_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                string sqlQuery = "SELECT * FROM T_USERS";
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

        private void Show_pharma_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sqlQuery = "SELECT PharmacyID, Pharmacy_Name, Pharma_Adress, Phone_Number, Email, Working_Hours FROM T_INSTITUTION";
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
    
        private void Show_drugs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sqlQuery = "SELECT * FROM T_DRUGS ORDER BY NAME ";
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

        private void Show_review_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sqlQuery = "SELECT * FROM T_REVIEWS";
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

        private void Delete_user_button_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(UserId.Text, out int userId2))
            {
                try
                {
                    using (OracleCommand cmd = new OracleCommand("Delete_User", ConnectionToOracle))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new OracleParameter("p_UserId", userId2));
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Пользователь успешно удален.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении пользователя: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Введите корректный идентификатор пользователя.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
    
}
