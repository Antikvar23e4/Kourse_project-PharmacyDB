using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
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
    /// Логика взаимодействия для Pharma.xaml
    /// </summary>
    public partial class Pharma : Window
    {
        private int userId;
        OracleConnection ConnectionToOracle;
        string connectionString = "DATA SOURCE=localhost:1521/orcl.mshome.net;TNS_ADMIN=C:\\Users\\oracledatabase\\Oracle\\network\\admin;PERSIST SECURITY INFO=True;USER ID=PROGRAMMER;PASSWORD=12345";
        private int pharmacyId;

        public Pharma(int userId)
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

                        using (OracleCommand checkCmd = new OracleCommand("SELECT 1 FROM Pharmacy_representatives WHERE UserID = :userId", ConnectionToOracle))
                        {
                            checkCmd.Parameters.Add("userId", OracleDbType.Int32).Value = userId;

                            OracleDataReader reader = checkCmd.ExecuteReader();
                            bool userExists = reader.Read();
                            reader.Close();

                            if (userExists)
                            {
                                
                                UpdatePharmacyRepresentative(userId, pharmacyId.Value);
                            }
                            else
                            {
                               
                                AddPharmacyRepresentative(userId, pharmacyId.Value);
                            }
                        }
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

        private void AddPharmacyRepresentative(int userId, int pharmacyId)
        {
            try
            {
                using (OracleCommand cmd = new OracleCommand("Add_Pharmacy_Representative", ConnectionToOracle))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_UserID", OracleDbType.Int32).Value = userId;
                    cmd.Parameters.Add("p_PharmacyID", OracleDbType.Int32).Value = pharmacyId;
                    cmd.Parameters.Add("p_error", OracleDbType.Varchar2, ParameterDirection.Output).Size=200;

                    cmd.ExecuteNonQuery();

                    string errorMsg = cmd.Parameters["p_error"].Value.ToString();
                    MessageBox.Show(errorMsg, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void UpdatePharmacyRepresentative(int userId, int pharmacyId)
        {
            try
            {
                using (OracleCommand cmd = new OracleCommand("Update_Pharmacy_Representative", ConnectionToOracle))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_UserID", OracleDbType.Int32).Value = userId;
                    cmd.Parameters.Add("p_PharmacyID", OracleDbType.Int32).Value = pharmacyId;
                    cmd.Parameters.Add("p_error", OracleDbType.Varchar2, ParameterDirection.Output).Size = 200;

                    cmd.ExecuteNonQuery();

                    string errorMsg = cmd.Parameters["p_error"].Value.ToString();
                    MessageBox.Show(errorMsg, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void Upd_pharma_repres_Click(object sender, RoutedEventArgs e)
        {
            string pharmacyName = Pharma_name.Text;
            string pharmacyAddress = Pharma_adress.Text;
            if (!string.IsNullOrWhiteSpace(pharmacyName) && !string.IsNullOrWhiteSpace(pharmacyAddress))
            {
                SearchPharmacy(pharmacyName, pharmacyAddress);
            }
            else
            {
                MessageBox.Show("Пожалуйста, заполните название и адрес аптеки.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show("Пожалуйста, заполните все поля для  лекарства.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Пожалуйста, заполните все поля для  лекарства.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ConnectionToOracle.Close();
            MainWindow newWindow = new MainWindow();
            this.Close();
            newWindow.Show();
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

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении DrugID: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            return drugId;
        }

        private void AddAvailability(int drugId, int quantity, decimal price)
        {
            try
            {
                using (OracleCommand cmd = new OracleCommand("Add_Availability", ConnectionToOracle))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_DrugID", OracleDbType.Int32).Value = drugId;
                    cmd.Parameters.Add("p_PharmacyID", OracleDbType.Int32).Value = pharmacyId;
                    cmd.Parameters.Add("p_Quantity", OracleDbType.Int32).Value = quantity;
                    cmd.Parameters.Add("p_Price", OracleDbType.Decimal).Value = price;
                    cmd.Parameters.Add("p_error", OracleDbType.Varchar2, ParameterDirection.Output).Size = 200;

                    cmd.ExecuteNonQuery();

                    string errorMsg = cmd.Parameters["p_error"].Value.ToString();
                    MessageBox.Show(errorMsg, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void AddAvailabilityFromTextBoxes()
        {
            if (string.IsNullOrWhiteSpace(Drug_name_Quantity.Text) || string.IsNullOrWhiteSpace(Dosage_Quantity.Text) || string.IsNullOrWhiteSpace(Items_Quantity.Text) ||
                 string.IsNullOrWhiteSpace(Price_Quantity.Text))
             {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                string drugName = Drug_name_Quantity.Text;
                string dosage = Dosage_Quantity.Text;
                int quantity = int.Parse(Items_Quantity.Text);
                decimal price = decimal.Parse(Price_Quantity.Text);
                int drugId = GetDrugId(drugName, dosage);

                if (drugId != -1)
                {
                    AddAvailability(drugId, quantity, price);
                }
                else
                {
                    MessageBox.Show("Ошибка: Не удалось найти соответствующий препарат.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Ошибка: Проверьте правильность введенных данных.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Create_Quantity_Click(object sender, RoutedEventArgs e)
        {
            AddAvailabilityFromTextBoxes();
        }

        private void Update_Quantity_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Drug_name_Quantity.Text) || string.IsNullOrWhiteSpace(Dosage_Quantity.Text))
            {
                MessageBox.Show("Пожалуйста, введите название и дозировку лекарства ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                string drugName = Drug_name_Quantity.Text;
                string dosage = Dosage_Quantity.Text;
                int quantity = int.Parse(Items_Quantity.Text);
                decimal price = decimal.Parse(Price_Quantity.Text);

                int drugId = GetDrugId(drugName, dosage);

                UpdateAvailability(drugId, quantity, price);
            }
            catch (FormatException)
            {
                MessageBox.Show("Ошибка: Проверьте правильность введенных данных.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UpdateAvailability(int drugId, int quantity, decimal price)
        {
            try
            {
                using (OracleCommand cmd = new OracleCommand("Update_Availability", ConnectionToOracle))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_DrugID", OracleDbType.Int32).Value = drugId;
                    cmd.Parameters.Add("p_PharmacyID", OracleDbType.Int32).Value = pharmacyId;
                    cmd.Parameters.Add("p_Quantity", OracleDbType.Int32).Value = quantity;
                    cmd.Parameters.Add("p_Price", OracleDbType.Decimal).Value = price;
                    cmd.Parameters.Add("p_error", OracleDbType.Varchar2, ParameterDirection.Output).Size = 200;

                    cmd.ExecuteNonQuery();

                    string errorMsg = cmd.Parameters["p_error"].Value.ToString();
                    MessageBox.Show(errorMsg, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void Delete_Quantity_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Drug_name_Quantity.Text) || string.IsNullOrWhiteSpace(Dosage_Quantity.Text))
            {
                MessageBox.Show("Пожалуйста, введите название и дозировку лекарства ", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                string drugName = Drug_name_Quantity.Text;
                string dosage = Dosage_Quantity.Text;
                int drugId = GetDrugId(drugName, dosage);
                if (drugId != -1)
                {
                    DeleteAvailability(drugId, pharmacyId, out string errorMsg);
                    MessageBox.Show(errorMsg, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Ошибка: Препарат не найден.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Ошибка: Проверьте правильность введенных данных.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteAvailability(int drugId, int pharmacyId, out string errorMsg)
        {
            try
            {
                using (OracleCommand cmd = new OracleCommand("Delete_Availability", ConnectionToOracle))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_DrugID", OracleDbType.Int32).Value = drugId;
                    cmd.Parameters.Add("p_PharmacyID", OracleDbType.Int32).Value = pharmacyId;
                    cmd.Parameters.Add("p_error", OracleDbType.Varchar2, ParameterDirection.Output).Size = 200;

                    cmd.ExecuteNonQuery();

                    errorMsg = cmd.Parameters["p_error"].Value.ToString();
                }
            }
            catch (OracleException ex)
            {
                errorMsg = "Ошибка Oracle: " + ex.Message;
            }
            catch (Exception ex)
            {
                errorMsg = "Произошла ошибка: " + ex.Message;
            }
        }

        private void Show_Quantity_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sqlQuery = "SELECT * FROM T_AVAILABILITY WHERE pharmacyId = :pharmacyId";

                using (OracleCommand cmd = new OracleCommand(sqlQuery, ConnectionToOracle))
                {
                    cmd.Parameters.Add(new OracleParameter("pharmacyId", pharmacyId));

                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        DataSet dataSet = new DataSet();
                        adapter.Fill(dataSet);
                        ResultData.ItemsSource = dataSet.Tables[0].DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при выполнении запроса: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Update_reservation_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(OrderId.Text) || string.IsNullOrWhiteSpace(Order_Status.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                int orderId = Convert.ToInt32(OrderId.Text);
                string newStatus = Order_Status.Text;

                using (OracleCommand cmd = new OracleCommand("UpdateReservationStatus", ConnectionToOracle))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_OrderID", OracleDbType.Int32).Value = orderId;
                    cmd.Parameters.Add("p_NewStatus", OracleDbType.Varchar2).Value = newStatus;
                    cmd.Parameters.Add("p_Error", OracleDbType.Varchar2, ParameterDirection.Output).Size = 200;

                    cmd.ExecuteNonQuery();

                    string errorMsg = cmd.Parameters["p_Error"].Value.ToString();
                    MessageBox.Show(errorMsg, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
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


        private void Show_reservation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sqlQuery = "SELECT * FROM T_RESERVATION";

                using (OracleCommand cmd = new OracleCommand(sqlQuery, ConnectionToOracle))
                {
                    cmd.Parameters.Add(new OracleParameter("pharmacyId", pharmacyId));

                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        DataSet dataSet = new DataSet();
                        adapter.Fill(dataSet);
                        ResultData.ItemsSource = dataSet.Tables[0].DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при выполнении запроса: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }       
}
   
