/////////////////////////////////////////////////////////////////////////////////////
//  File:   CaPasswordWindow.cs                                     28 Feb 23 PHR
/////////////////////////////////////////////////////////////////////////////////////

using System.Windows;

namespace CertBuilder
{
    /// <summary>
    /// Window class for entering the password for the CA certificate to use for signing a new 
    /// X.509 certificate.
    /// Interaction logic for CaPasswordWindow.xaml
    /// </summary>
    public partial class CaPasswordWindow : Window
    {
        public CaPasswordWindow()
        {
            InitializeComponent();
            CertPw.Focus();
        }

        /// <summary>
        /// Contains the password entered by the user.
        /// </summary>
        public string CaPassword = null;

        private void OnOkClicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CertPw.Password) == true)
            {
                MessageBox.Show("Password not entered.", "Error", MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                return;
            }

            CaPassword = CertPw.Password;
            DialogResult = true;
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
