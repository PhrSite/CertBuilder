/////////////////////////////////////////////////////////////////////////////////////
//  File:   NewCertPasswordWindow.cs                                1 Mar 23 PHR
/////////////////////////////////////////////////////////////////////////////////////

using System.Windows;

namespace CertBuilder
{
    /// <summary>
    /// Window class that allows the user to enter a password for a new certificate.
    /// Interaction logic for NewCertPasswordWindow.xaml
    /// </summary>
    public partial class NewCertPasswordWindow : Window
    {
        public string CertificatePassword;

        public NewCertPasswordWindow()
        {
            InitializeComponent();
            CertPw.Focus();
        }

        private void OnOkClicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CertPw.Password) == true)
            {
                MessageBox.Show("Please enter a password", "Error", MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                return;
            }

            if (CertPw.Password != ConfPw.Password)
            {
                MessageBox.Show("The passwords do not match", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            CertificatePassword = CertPw.Password;
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
