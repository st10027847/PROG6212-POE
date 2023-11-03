using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
using MySql.Data.MySqlClient;
using PROG6212_CustomClassLib;

namespace PROG6212_POE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        // Class Fields and Constructors
        private const string connection_string = "Server=127.0.0.1;Database=Prog6212PoEDB;UserId=root;pwd=Joshh2003&;";
        private int currentUserId;
        private List<Module> modules = new List<Module>();
        private int numberOfWeeks;
        private DateTime startDate;
        private MySqlConnection connection;

        public MainWindow(MySqlConnection connection)
        {
            this.connection = connection;
        }

        public MainWindow()
        {
            InitializeComponent();
            connection = new MySqlConnection(connection_string);
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        // Event Handler for Add Module Button when Clicked
        private void AddModuleButton_Click(object sender, RoutedEventArgs e)
        {
                // Logic for adding a module
                Module module = new Module
                {
                    Code = ModuleCodeTextBox.Text,
                    Name = ModuleNameTextBox.Text,
                    Credits = int.Parse(ModuleCreditsTextBox.Text),
                    ClassHoursPerWeek = int.Parse(ModuleClassHourstextBox.Text)
                };

            // Save the module to the database
            SaveModuleToDatabase(module);

            modules.Add(module);

            // Clearing the respective text fields
            ModuleCodeTextBox.Clear();
            ModuleNameTextBox.Clear();
            ModuleCreditsTextBox.Clear();
            ModuleClassHourstextBox.Clear();

            UpdateModuleList();
            MessageBox.Show("Module Added Successfully!");
        }

        // Method to save module details to the database
        private void SaveModuleToDatabase(Module module)
        {
            // Logic for saving data to database
            using (MySqlConnection connection = new MySqlConnection(connection_string))
            {
                connection.Open();

                string insertModuleQuery = "insert into Modules (UserId, Code, Name, Credits, ClassHoursPerWeek) values (@UserId, @Code, @Name, @Credits, @ClassHoursPerWeek)";
                MySqlCommand insertModuleCommand = new MySqlCommand(insertModuleQuery, connection);
                insertModuleCommand.Parameters.AddWithValue("@UserId", 1);
                insertModuleCommand.Parameters.AddWithValue("@Code", module.Code);
                insertModuleCommand.Parameters.AddWithValue("@Name", module.Name);
                insertModuleCommand.Parameters.AddWithValue("@Credits", module.Credits);
                insertModuleCommand.Parameters.AddWithValue("@ClassHoursPerWeek", module.ClassHoursPerWeek);
                insertModuleCommand.ExecuteNonQuery();

                connection.Close();
            }
        }

        // Method to authenticate user (set to boolean value [true or false] for return)
        public bool AuthenticateUser(string username, string password)
        {
            // Logic for authenticating user
            using (MySqlConnection connection = new MySqlConnection(connection_string))
            {
                connection.Open();

                // Check if the username and password has match in the Users table
                string query = "select UserId from Users where Username = @Username and PasswordHash = @PasswordHash";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@PasswordHash", HashPassword(password));
                object result = command.ExecuteScalar();

                connection.Close();

                if(result != null && result != DBNull.Value)
                {
                    currentUserId = Convert.ToInt32(result);
                    return true;
                }

                return false;
            }
        }
        
        // Method used to hash the given password using SHA-256 algorithm
        private string HashPassword(string password)
        {
            // Creating SHA-256 hashing algorithm instance
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Create to store hashed bytes as hexadecimal characters
                StringBuilder builder = new StringBuilder();
                for(int i = 0; i < hashedBytes.Length; i++)
                {
                    builder.Append(hashedBytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        
        // Method to update the ModulesListView with the modules data
        private void UpdateModuleList()
        {
            // Clear existing items in the ModulesListView
            ModulesListView.Items.Clear();

            foreach(Module module in modules)
            {
                ListViewItem item = new ListViewItem()
                {
                    Content = $"{module.Code} - {module.Name} ({module.Credits} credits, {module.ClassHoursPerWeek} class hours/week)"
                };
                ModulesListView.Items.Add(item);
            }
        }

        // Event Handler for text change within WeeksTextBox.
        private void WeeksTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Logic for text changed
            if(int.TryParse(WeeksTextBox.Text, out int weeks))
            {
                numberOfWeeks = weeks;
                CalculateSelfStudyHours();
            }
        }

        // Event Handler for StartDateDatePicker when the selected date changes
        private void StartDateDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // Logic for handling start date input
            startDate = StartDateDatePicker.SelectedDate ?? DateTime.Now;
            CalculateSelfStudyHours();
        }

        // Method to calculate self-study hours for modules
        private void CalculateSelfStudyHours()
        {
            // Logic
            foreach(Module module in modules)
            {
                int selfStudyHoursPerWeek = (module.Credits * 10) - module.ClassHoursPerWeek;
                int totalSelfStudyHours = selfStudyHoursPerWeek * numberOfWeeks;
                module.SelfStudyHours = totalSelfStudyHours;
            }
            UpdateModuleList();
        }

        // Event Handler for the Record Hours Button when clicked
        private void RecordHoursButton_Click(object sender, RoutedEventArgs e)
        {
            // Logic
            if(ModulesListView.SelectedItem is ListViewItem selectedModuleItem)
            {
                Module selectedModule = modules[ModulesListView.Items.IndexOf(selectedModuleItem)];
                if(int.TryParse(HoursTextBox.Text, out int hours))
                {
                    selectedModule.RecordedHours += hours;
                    UpdateModuleList();
                    CalculateRemainingHours();
                }
            }
            UpdateModuleList();
        }

        // Method to calculate remaining hours for modules in the current week
        private void CalculateRemainingHours()
        {
            // Logic for calculating remaining hours
            DateTime currentDate = DateTime.Now;
            DateTime startOfWeek = startDate.AddDays(-(int)startDate.DayOfWeek);
            DateTime endOfWeek = startOfWeek.AddDays(6);

            foreach(Module module in modules)
            {
                int recordedHoursThisWeek = module.GetRecordedHoursForWeek(startOfWeek, endOfWeek);
                int remainingHoursThisWeek = module.SelfStudyHours - recordedHoursThisWeek;
                module.RemainingHoursThisWeek = remainingHoursThisWeek;
            }

            UpdateRemainingHoursList();
        }

        // Method to update the Remaining Hours ListView
        private void UpdateRemainingHoursList()
        {
            // Logic for updating Remaining Hours ListView
            RemainingHoursListView.Items.Clear();

            foreach(Module module in modules)
            {
                ListViewItem item = new ListViewItem
                {
                    Content = $"{module.Code} - Remaining Hours This Week: {module.RemainingHoursThisWeek}"
                };
                RemainingHoursListView.Items.Add(item);
            }
        }

        // Event Handler for MainWindow loaded event
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Logic
            startDate = DateTime.Now;
            numberOfWeeks = 0;
            UpdateModuleList();
        }

        // Event Handler for MainWindow closed event
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // Handle cleanup here, if needed
            // For example, you can save the data or perform other cleanup tasks
        }
    }
}
