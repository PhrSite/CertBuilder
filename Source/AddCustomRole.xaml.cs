using System.Text.RegularExpressions;
using System.Windows;

namespace CertBuilder
{
    /// <summary>
    /// Interaction logic for AddCustomRole.xaml. This dialog box allows the user to enter a custom role.
    /// </summary>
    public partial class AddCustomRole : Window
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AddCustomRole()
        {
            InitializeComponent();
            CustomRole.Focus();
        }

        /// <summary>
        /// Output. If ShowDialog() returns true then this string contains the new role.
        /// </summary>
        public string strCustomRole { get; private set; } = null;

        private void OnOkClicked(object sender, RoutedEventArgs e)
        {
            strCustomRole = CustomRole.Text;
            if (string.IsNullOrEmpty(strCustomRole) == true)
            {
                MessageBox.Show("Enter a custom role", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 30 Sep 25 PHR -- This is actually not necessary
            // Test for any whitespace in the custom role
            //if (Regex.Match(strCustomRole, @"\s").Success == true)
            //{
            //    MessageBox.Show("Roles cannot contain whitespace", "Error", MessageBoxButton.OK, 
            //        MessageBoxImage.Error);
            //    return;
            //}

            DialogResult = true;
            Close();
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
