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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Apteka
{
    /// <summary>
    /// Логика взаимодействия для Regisration.xaml
    /// </summary>
    public partial class Regisration : Page
    {
        OracleConnection ConnectionToOracle;
        string connectionString = "DATA SOURCE=localhost:1521/orcl.mshome.net;TNS_ADMIN=C:\\Users\\oracledatabase\\Oracle\\network\\admin;PERSIST SECURITY INFO=True;USER ID=PROGRAMMER;PASSWORD=12345";

        public Regisration()
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FirstNameTextBox.Text) ||
         string.IsNullOrEmpty(LastNameTextBox.Text) ||
         string.IsNullOrEmpty(MiddleNameTextBox.Text) ||
         string.IsNullOrEmpty(PhoneNumberTextBox.Text) ||
         string.IsNullOrEmpty(EmailTextBox.Text) ||
         string.IsNullOrEmpty(RoleIdTextBox.Text) ||
         string.IsNullOrEmpty(PasswordTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            string firstName = FirstNameTextBox.Text;
            string lastName = LastNameTextBox.Text;
            string middleName = MiddleNameTextBox.Text;
            string phoneNumber = PhoneNumberTextBox.Text;
            string email = EmailTextBox.Text;
            int roleId = int.Parse(RoleIdTextBox.Text);
            string password = PasswordTextBox.Text;

            try
            {
                using (OracleCommand command = new OracleCommand("Register_User", ConnectionToOracle))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("p_First_Name", OracleDbType.Varchar2).Value = firstName;
                    command.Parameters.Add("p_Last_Name", OracleDbType.Varchar2).Value = lastName;
                    command.Parameters.Add("p_Middle_Name", OracleDbType.Varchar2).Value = middleName;
                    command.Parameters.Add("p_Phone_Number", OracleDbType.Varchar2).Value = phoneNumber;
                    command.Parameters.Add("p_Email", OracleDbType.Varchar2).Value = email;
                    command.Parameters.Add("p_RoleId", OracleDbType.Int32).Value = roleId;
                    command.Parameters.Add("p_Password", OracleDbType.Varchar2).Value = password;
                    command.Parameters.Add("p_ErrorMsg", OracleDbType.Varchar2, 200).Direction = System.Data.ParameterDirection.Output;
                    command.ExecuteNonQuery();

                    string errorMsg = command.Parameters["p_ErrorMsg"].Value.ToString();
                    MessageBox.Show(errorMsg, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);

                }
            }
            catch (OracleException ex)
            {
                MessageBox.Show("Ошибка Oracle: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ConnectionToOracle.Close();
            MainWindow mainWindow = new MainWindow();
            NavigationService navigationService = NavigationService.GetNavigationService(this);
            mainWindow.Show();
            navigationService.Navigate(null);
            Window.GetWindow(this).Close();
        }

    }
}

