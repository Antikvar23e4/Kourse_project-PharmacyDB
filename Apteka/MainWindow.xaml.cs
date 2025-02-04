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
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace Apteka
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int userId;
        OracleConnection ConnectionToOracle;
        string connectionString = "DATA SOURCE=localhost:1521/orcl.mshome.net;TNS_ADMIN=C:\\Users\\oracledatabase\\Oracle\\network\\admin;PERSIST SECURITY INFO=True;USER ID=PROGRAMMER;PASSWORD=12345";
        public MainWindow()
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ConnectionToOracle.Close();
            Frame frame = new Frame();
            Regisration registrationPage = new Regisration();
            frame.Content = registrationPage;
            this.Content = frame;

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string username = UserNameTextBox.Text;
            string password = PasswordTextBox.Text;
           
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return; 
            }

            try
            {
                using (OracleCommand command = new OracleCommand("Authenticate_User", ConnectionToOracle))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("p_Username", OracleDbType.Varchar2).Value = username;
                    command.Parameters.Add("p_Password", OracleDbType.Varchar2).Value = password;
                    command.Parameters.Add("p_SuccessMsg", OracleDbType.Varchar2, ParameterDirection.Output).Size=200;
                    command.Parameters.Add("p_ErrorMsg", OracleDbType.Varchar2, ParameterDirection.Output).Size = 200;

                    command.ExecuteNonQuery();

                    string successMsg = command.Parameters["p_SuccessMsg"].Value.ToString();
                    string errorMsg = command.Parameters["p_ErrorMsg"].Value.ToString();

                    if (!string.IsNullOrEmpty(successMsg) && successMsg != "null")
                    {
                        MessageBox.Show(successMsg);
                        userId = GetUserId(username, password);
                        int roleId = GetUserRoleId(username, password);
                        OpenPageBasedOnRoleId(roleId);
                    }
                    else
                    {
                        MessageBox.Show(errorMsg);
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

        private int GetUserRoleId(string username, string password)
        {
            int roleId = 0;

            try
            {
                using (OracleCommand command = new OracleCommand("Get_User_RoleId", ConnectionToOracle))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("p_Username", OracleDbType.Varchar2).Value = username;
                    command.Parameters.Add("p_Password", OracleDbType.Varchar2).Value = password;
                    command.Parameters.Add("p_RoleId", OracleDbType.Int32, ParameterDirection.Output);

                    command.ExecuteNonQuery();

                    OracleDecimal roleIdOracle = (OracleDecimal)command.Parameters["p_RoleId"].Value;
                    roleId = roleIdOracle.ToInt32();
                }
            }
            catch (OracleException ex)
            {
                MessageBox.Show("Ошибка Oracle: " + ex.Message);
            }

            return roleId;
        }

        private void OpenPageBasedOnRoleId(int roleId)
        {
            switch (roleId)
            {
                case 1:
                    ConnectionToOracle.Close();
                    User userWindow = new User(this.userId);
                    userWindow.Show();
                    break;
                case 2:
                    ConnectionToOracle.Close();
                    Manager managerWindow = new Manager(this.userId);
                    managerWindow.Show();
                    break;
                case 3:
                    ConnectionToOracle.Close();
                    Pharma pharmaWindow = new Pharma(this.userId);
                    pharmaWindow.Show();
                    break;
                default:
                    MessageBox.Show("Не удалось определить роль пользователя.");
                    break;
            }
            this.Close();
        }
 
        private int GetUserId(string username, string password)
        {
            int userId = 0;

            try
            {
                using (OracleCommand command = new OracleCommand("Get_User_Id", ConnectionToOracle))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("p_Username", OracleDbType.Varchar2).Value = username;
                    command.Parameters.Add("p_Password", OracleDbType.Varchar2).Value = password;
                    command.Parameters.Add("p_UserId", OracleDbType.Int32, ParameterDirection.Output);

                    command.ExecuteNonQuery();
                    OracleDecimal userIdOracle = (OracleDecimal)command.Parameters["p_UserId"].Value;
                    userId = userIdOracle.ToInt32();
                    MessageBox.Show("UserId: " + userId);
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

            return userId;
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ConnectionToOracle.Close();
            Guest guestWindow = new Guest();
            guestWindow.Show();
            this.Close();
        }
    }
}
