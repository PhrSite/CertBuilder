/////////////////////////////////////////////////////////////////////////////////////
//  File:   MainWindow.xaml.cs                                      24 Feb 23 PHR
//
//  Revised:    27 Jul 25 PHR
//              -- Changed to use X509CertificateLoader.LoadPkcs12().
/////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.IO;
using Ng911CertUtils;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Ng911Lib.Utilities;
using System.Diagnostics;
using System.Configuration;

namespace CertBuilder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml. Main window class for the application.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            string strVer = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            Title = Title + " (" + strVer + ")";

            settings = GetLastUsedSettings();
            MainGrid.DataContext = settings;
            ShowRoles();

            if (settings.selfSigned == true)
                CaStackPanel.Visibility= Visibility.Collapsed;
        }

        /// <summary>
        /// Filename to used for storing the last used settings.
        /// </summary>
        private const string SettingsFileName = "CertBuilderSettings.json";

        /// <summary>
        /// Directory for storing the last used settings
        /// </summary>
        private string SettingsDirectory = null;

        /// <summary>
        /// Full file path for the last used settings file
        /// </summary>
        private string SettingsFilePath = null;
        private CertBuilderSettings settings { get; set; }

        private CertBuilderSettings GetLastUsedSettings()
        {
            CertBuilderSettings Settings = new CertBuilderSettings();
            string MyDocmentsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            SettingsDirectory = @$"{MyDocmentsDir}\CertBuilder";
            SettingsFilePath = Path.Combine(SettingsDirectory, SettingsFileName);
            if (File.Exists(SettingsFilePath) == true)
            {
                string strSettings = File.ReadAllText(SettingsFilePath);
                Settings = JsonHelper.DeserializeFromString<CertBuilderSettings>(strSettings);
                if (Settings == null)
                    Settings = new CertBuilderSettings();   // An error occurred, use the default settings
            }

            return Settings;                
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// The user clicked on the Save Settings button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSaveSettings(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(SettingsDirectory) == false)
                Directory.CreateDirectory(SettingsDirectory);

            string strSettings = JsonHelper.SerializeToString(settings);
            File.WriteAllText(SettingsFilePath, strSettings);
            System.Windows.MessageBox.Show("The current settings have been saved.", "Settings Saved",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// The user clicked on the Create Certificate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCreateCertificate(object sender, RoutedEventArgs e)
        {
            if (VerifyGeneralSettings() == false)
                return;

            if (VerifySubjectDistinguishedNameSettings() == false)
                return;

            if (VerifyKeyUsageSettings() == false)
                return;

            if (VerifySubjectAltName() == false)
                return;

            X509Certificate2 CaCert = null;
            string CaCertPw = null;
            Exception Excpt = null;
            if (settings.selfSigned == false)
            {   // Get the X.509 certificate to use to sign the new certificate
                CaPasswordWindow Cpw = new CaPasswordWindow();
                bool? RetVal = Cpw.ShowDialog();
                if (RetVal == false)
                    return;

                CaCertPw = Cpw.CaPassword;
                try
                {
                    //CaCert = new X509Certificate2(settings.caCertFile, CaCertPw);
                    // 27 Jul 25 PHR
                    CaCert = X509CertificateLoader.LoadPkcs12(File.ReadAllBytes(settings.caCertFile), CaCertPw);
                }
                catch (CryptographicException Ce) { Excpt = Ce; }
                catch (Exception Ex) { Excpt = Ex; }

                if (Excpt != null)
                {
                    System.Windows.MessageBox.Show("An exception occurred when trying to load " +
                        "the CA certificate to use for signing. The exception message is: \n\n" +
                        $"{Excpt.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // Get the password for the new certificate
            NewCertPasswordWindow Npw = new NewCertPasswordWindow();
            bool? NpwResult = Npw.ShowDialog();
            if (NpwResult == false)
                return;

            string NewCertPw = Npw.CertificatePassword;

            Ng911SanParams ng911Sans = settings.addNg911SubjectAltName == true ?
                settings.sanParams : null;

            X509Certificate2 X509Cert = null;
            if (settings.signingAlgorithmIdx == 0)
            {   // Create an RSA certificate
                if (settings.selfSigned == true)
                {
                    X509Cert = CertUtils.CreateRsaSelfSignedCertificate(settings.nameParams,
                        settings.usageParams, ng911Sans, settings.expiresYears, NewCertPw,
                        settings.caCertificate, out Excpt);
                }
                else
                {
                    X509Cert = CertUtils.CreateRsaSignedCertificate(CaCert, settings.nameParams,
                        settings.usageParams, ng911Sans, NewCertPw, settings.caCertificate, out Excpt);
                }
            }
            else
            {   // Create an ECDSA certificate
                if (settings.selfSigned == true)
                {
                    X509Cert = CertUtils.CreateEcdsaSelfSignedCertificate(settings.nameParams,
                        settings.usageParams, ng911Sans, settings.expiresYears, NewCertPw,
                        settings.caCertificate, out Excpt);
                }
                else
                {
                    X509Cert = CertUtils.CreateEcdsaSignedCertificate(CaCert, settings.nameParams,
                        settings.usageParams, ng911Sans, settings.caCertificate, out Excpt);
                }
            }

            if (X509Cert != null) 
            {
                byte[] CertBytes = X509Cert.Export(X509ContentType.Pkcs12, NewCertPw);
                File.WriteAllBytes(Path.Combine(settings.destinationDirectory, settings.fileNameNoExtension +
                    ".pfx"), CertBytes);
                byte[] RawBytes = X509Cert.GetRawCertData();
                File.WriteAllBytes(Path.Combine(settings.destinationDirectory, settings.fileNameNoExtension +
                    ".cer"), RawBytes);

                System.Windows.MessageBox.Show("The new certificate was successfully created.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {   // If X509Cert is null, then Excpt will not be null.
                System.Windows.MessageBox.Show("An unexpected error occurred. The certificates " +
                    $"were not created. The exception message is:\n\n {Excpt.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool VerifyGeneralSettings()
        {
            bool Success = true;
            UIElement Uie = null;
            if (settings.selfSigned == false)
            {
                if (string.IsNullOrEmpty(settings.caCertFile) == true)
                {
                    System.Windows.MessageBox.Show("Self-Signed Certificate is not checked and a " +
                        "CA Certificate file has not been specified.", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Uie = CaCert;
                    Success = false;
                }

                // Make sure that the CA certificate file exists.
                if (Success == true && File.Exists(settings.caCertFile) == false)
                {
                    System.Windows.MessageBox.Show("The specified CA certificate file does " +
                        "not exist", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Uie = CaCert;
                    Success = false;
                }
            }

            if (Success == true && string.IsNullOrEmpty(settings.destinationDirectory) == true)
            {
                System.Windows.MessageBox.Show("The New Certificate Destination Directory " +
                    "must be specified.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Uie = DestDir;
                Success = false;
            }

            if (Success == true && Directory.Exists(settings.destinationDirectory) == false) 
            {
                System.Windows.MessageBox.Show("The New Certificate Destination Directory " +
                    "does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Uie = DestDir;
                Success = false;
            }

            if (Success == true)
            {
                if (settings.expiresYears < 1)
                {
                    System.Windows.MessageBox.Show("The Expires value must be greater than " +
                        "or equal to 1", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Uie = Expires;
                    Success = false;
                }
            }

            if (Success == false)
            {
                TabCtrl.SelectedIndex = 0;  // Display the General tab
                if (Uie != null)
                    Uie.Focus();
            }

            return Success;
        }

        private bool VerifySubjectDistinguishedNameSettings()
        {
            if (string.IsNullOrEmpty(settings.nameParams.commonName) == true)
            {
                System.Windows.MessageBox.Show("Common Name must be specified.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                TabCtrl.SelectedIndex = 1;  // Display the Subject Distinguished Name tab
                CommonName.Focus();
                return false;
            }

            // Only the Common Name is required

            return true;
        }

        private bool VerifyKeyUsageSettings()
        {
            bool Success = true;
            KeyUsageParams Kp = settings.usageParams;
            if (Kp.crlSign == false && Kp.dataEncipherment == false && Kp.decipherOnly == false &&
                Kp.digitalSignature == false && Kp.encipherOnly == false && Kp.keyAgreement == false &&
                Kp.keyCertSign == false && Kp.keyEncipherment && Kp.nonRepudiation == false &&
                Kp.clientAuthentication == false && Kp.serverAuthentication == false && Kp.codeSigning == false) 
            {
                System.Windows.MessageBox.Show("No Key Usage selections are checked. " +
                    "Please select some key usages and try again.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Success = false;
            }

            if (Kp.codeSigning == true && Kp.digitalSignature == false)
            {
                System.Windows.MessageBox.Show("Code Signing is selected but Digital Signature is not selected. " +
                    "Please select Digital Signature.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Success = false;
            }

            // TODO: There may need to be certain key usage combinations that should be checked.

            if (Success == false)
                TabCtrl.SelectedIndex = 2;  // Display the Key Usage tab

            return Success;
        }

        private bool VerifySubjectAltName()
        {
            bool Success = true;
            if (settings.addNg911SubjectAltName == false)
                return Success;

            if (string.IsNullOrEmpty(settings.sanParams.iD) == true)
            {
                System.Windows.MessageBox.Show("The ID parameter must be specified.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
               Success = false;
            }

            if (Success == true)
            {
                if (settings.sanParams.roles.Count == 0)
                {
                    System.Windows.MessageBox.Show("No roles are selected. Please select " +
                        "one or more roles and try again.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Success = false;
                }
            }

            if (Success == false)
                TabCtrl.SelectedIndex = 3;  // Show the SubAltName tab

            return Success;
        }

        private void OnDestDirBrowse(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog Dlg = new FolderBrowserDialog();
            DialogResult Dr = Dlg.ShowDialog();
            if (Dr == System.Windows.Forms.DialogResult.OK)
                settings.destinationDirectory = Dlg.SelectedPath;
        }

        /// <summary>
        /// Handler for the CaCertBrowse button. Allow the user to select a certificate file
        /// to sign a new certificate with.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCaCertBrowse(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Dlg = new OpenFileDialog();
            Dlg.Filter = "PFX Files|*.pfx";
            DialogResult Dr = Dlg.ShowDialog();
            if (Dr == System.Windows.Forms.DialogResult.OK)
                settings.caCertFile = Dlg.FileName;
        }

        /// <summary>
        /// The user clicked on the Add roles button in the SubAltName tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAddRole(object sender, RoutedEventArgs e)
        {
            List<string> RoleList;
            switch (IdTypeCombo.Text)
            {
                case "ElementId":
                    RoleList = Roles.ElementRoles;
                    break;
                case "ServiceId":
                    RoleList = Roles.ServiceRoles;
                    break;
                case "AgencyId":
                    RoleList = Roles.AgencyRoles;
                    break;
                case "AgentId":
                    RoleList = Roles.AgentRoles;
                    break;
                case "CAId":
                    RoleList = Roles.CaRoles;
                    break;
                default:
                    RoleList = Roles.ElementRoles;
                    break;
            }

            AddRoleWindow Arw = new AddRoleWindow(RoleList);
            bool? Result = Arw.ShowDialog();

            if (Result == true)
            {
                foreach (string str in Arw.SelectedRoles)
                {
                    if (settings.sanParams.roles.Contains(str) == false)    // 27 Jul 25 PHR
                        settings.sanParams.roles.Add(str);
                }

                ShowRoles();
            }
        }

        private void ShowRoles()
        {
            if (RolesLb == null)
                return;

            RolesLb.Items.Clear();

            foreach (string strRole in settings.sanParams.roles)
                RolesLb.Items.Add(strRole);
        }

        /// <summary>
        /// The user clicked on the Delete button in the SubAltName tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDeleteRole(object sender, RoutedEventArgs e)
        {
            if (RolesLb.SelectedItems.Count == 0)
            {
                System.Windows.MessageBox.Show("No items selected", "Error", MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                return;
            }

            foreach (string str in RolesLb.SelectedItems)
            {
                if (settings.sanParams.roles.Contains(str) == true)
                    settings.sanParams.roles.Remove(str);
            }

            ShowRoles();
        }

        /// <summary>
        /// The user clicked on the Clear button in the SubAltName tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClearRoles(object sender, RoutedEventArgs e)
        {
            settings.sanParams.roles.Clear();
            ShowRoles();
        }

        private void OnSelfSignedCheckBoxClicked(object sender, RoutedEventArgs e)
        {
            if (settings.selfSigned == true)
                CaStackPanel.Visibility = Visibility.Collapsed;
            else
                CaStackPanel.Visibility = Visibility.Visible;
        }

        private void OnHelpClicked(object sender, RoutedEventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = @".\OperatorsManual\CertBuilderOpMan.pdf";
            proc.Start();
        }
    }
}
