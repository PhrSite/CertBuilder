using System.Windows;
using System.Collections.Generic;

namespace CertBuilder
{
    /// <summary>
    /// Interaction logic for AddRoleWindow.xaml. This Window class is used as a pop-up dialog and
    /// it allows the user to select one or more roles.
    /// </summary>
    public partial class AddRoleWindow : Window
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="roles">Must include a list of roles that the user can select from.</param>
        public AddRoleWindow(List<string> roles)
        {
            Roles = roles;
            DataContext = this;

            InitializeComponent();

            // 27 Jul 25 PHR
            foreach (string role in Roles)
            {
                rolesListBox.Items.Add(role);
            }

        }

        private List<string> Roles { get; set; }

        /// <summary>
        /// Output. Contains a list of selected roles if ShowDialog() returns true. Will be empty if
        /// ShowDialog() returns false.
        /// </summary>
        public List<string> SelectedRoles = new List<string>();

        /// <summary>
        /// Event handler for the click event of the OK button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOk(object sender, RoutedEventArgs e)
        {
            if (rolesListBox.SelectedItems.Count == 0)
            {
                MessageBox.Show("No roles selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            foreach (string Lbi in rolesListBox.SelectedItems)
            {
                SelectedRoles.Add(Lbi);
            }

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Event handler for the click event of the Cancel button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Event handler for the Add Custom Role button. Displays the AddCustomRole dialog so the user
        /// can type in a new custom role to add.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAddCustom(object sender, RoutedEventArgs e)
        {
            AddCustomRole Acr = new AddCustomRole();
            bool? Result = Acr.ShowDialog();
            if (Result == true)
            {
                if (Roles.Contains(Acr.strCustomRole) == false)
                {
                    //Roles.Add(Acr.strCustomRole);
                    // 27 Jul 25 PHR
                    rolesListBox.Items.Add(Acr.strCustomRole);

                    rolesListBox.Items.Refresh();
                }
                else
                    MessageBox.Show($"The roles list already has a role called '{Acr.strCustomRole}'", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
