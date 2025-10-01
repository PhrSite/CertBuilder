/////////////////////////////////////////////////////////////////////////////////////
//  File:   CertBuilderSettings.cs                                  27 Feb 23 PHR
/////////////////////////////////////////////////////////////////////////////////////

using Ng911CertUtils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CertBuilder
{
    /// <summary>
    /// Model class for the settings for the CertBuilder application
    /// </summary>
    public class CertBuilderSettings : INotifyPropertyChanged 
    {
        /// <summary>
        /// If true then create a self-signed certificate. If false then create a signed certificate.
        /// </summary>
        public bool selfSigned { get; set; } = true;

        /// <summary>
        /// If true, then create a certificate that can sign other certificates.
        /// </summary>
        public bool caCertificate { get; set; } = true;

        /// <summary>
        /// The selected index of the Signing Algorithm combo box. 0 = RSA, 1 = ECDSA.
        /// </summary>
        public int signingAlgorithmIdx { get; set; } = 0;

        /// <summary>
        /// Specifies the file path for the certificate file to sign new cerfificates with.
        /// Required if selfSigned is false.
        /// </summary>
        public string caCertFile
        {
            get { return _caCertFile; }
            set
            {
                _caCertFile = value;
                OnPropertyChanged();
            }
        }

        private string _caCertFile;

        /// <summary>
        /// Specifies the number of years before the certificate expires. The minimum value
        /// is 1. This only applies to self-signed certificates.
        /// </summary>
        public int expiresYears
        {
            get { return _expiresYears; }
            set 
            {
                _expiresYears = value;
                OnPropertyChanged();
            }
        }

        private int _expiresYears = 10;

        /// <summary>
        /// Specifies the directory in which to store created certificates.
        /// </summary>
        public string destinationDirectory 
        {  get { return _destinationDirectory; }
            set
            {
                _destinationDirectory = value;
                OnPropertyChanged();
            }
        }
        private string _destinationDirectory;

        /// <summary>
        /// Specifies the file name with no extension for a new certificate
        /// </summary>
        public string fileNameNoExtension { get; set; }

        /// <summary>
        /// If true then add the NG9-1-1 Subject Alternate Name extensions to the new certificate.
        /// </summary>
        public bool addNg911SubjectAltName { get; set; } = true;

        /// <summary>
        /// Contains the parameters for building the distinguished name for the Subject certificate
        /// extension.
        /// </summary>
        public DistinguishedNameParams nameParams { get; set; } = new DistinguishedNameParams();

        /// <summary>
        /// Contains the key usage parameters for the new extension.
        /// </summary>
        public KeyUsageParams usageParams { get; set; } = new KeyUsageParams();

        /// <summary>
        /// Contains the parameters for the NG9-1-1 Subject Alternate Name extension to add
        /// to the new certificate.
        /// </summary>
        public Ng911SanParams sanParams { get; set; } = new Ng911SanParams();

        [JsonIgnore]
        public List<string> idTypes { get; } = new List<string>()
        {
            "ElementId",
            "ServiceId",
            "AgencyId",
            "AgentId",
            "CAId"
        };

        /// <summary>
        /// Default constructor
        /// </summary>
        public CertBuilderSettings()
        {
            sanParams.idType = "ElementId";
        }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
