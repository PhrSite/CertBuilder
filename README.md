# Introduction
This application is a Windows WPF desktop application for Windows 10 or Windows 11.

This application provides a simple graphical user interface to help you build self-signed or signed X.509 digital certificates for testing purposes. This application can be used to create the following types of X.509 certificates.

1.	Self-signed Certificate Authority (CA) certificates
2.	Signed Intermediate Certificate Authority (ICA) certificates
3.	Signed or self-signed end-entity certificates

This application allows you to build certificates using the RSA signing algorithm or the ECDSA signing algorithm. RSA certificates are created using a fixed key length of 2048 bits and SHA512 for the hash function. ECDSA certificates are created using a fixed key length of 521 bits using the P521 elliptic curve with SHA-512 for the hash algorithm.

Certificates are saved as files using a file name that you can specify in a location that you can specify. The application creates two types of files. The file with a file name extension of “cer” contains only a public key. The file with a file name extension of “pfx” contains both the public key and a private key. The PFX file is password protected. This file can be used by client and server applications that use Transport Layer Security (TLS and/or HTTPS) or have a need to digitally sign digital documents.

This application also allows you to build X.509 certificates for testing NG9-1-1 applications (see Reference 1). The PSAP Credentialing Agency (PCA, see Reference 2) is responsible for issuing certificates to be used by functional elements, services, agencies and agents within a NG9-1-1 emergency services network. Certificates issued by the PCA contain the following information in the otherName sequence of the Subject Alternate Name certificate extension (see Section 4.2.1.6 of RFC 5280 [3] and Section 7.1.2.11 of the PCA Certificate Policy [2]).

1.	ID Type (identifies the entity such as: Element, Service, Agent or Agency)
2.	ID of the entity
3.	Roles assigned to the entity
4.	Owner of the certificate assigned to the entity

This application allows you to build certificates that include the above information in the otherName sequence of the Subject Alternate Name certificate extension.

## References

1. [NENA i3 Standard for Next Generation 9-1-1](https://cdn.ymaws.com/www.nena.org/resource/resmgr/standards/nena-sta-010.3f-2021_i3_stan.pdf). National Emergency Number Association (NENA) 911 Core Services Committee, i3 Architecture Working Group, NENA-STA-010.3f, October 7, 2021.
2. [Public Safety Answering Point (PSAP) Credentialing Agency (PCA) Certificate Policy](https://ng911ioc.org/wp-content/uploads/2025/03/NG9-1-1-PKI-CP-v1.2_March2025.pdf). NG9-1-1 Interoperability Oversight Commission (NIOC), V1.2, June 26, 2024.
3. Internet X.509 Public Key Infrastructure Certificate and Certificate Revocation List (CRL) Profile, IETF, [RFC 5280](https://www.rfc-editor.org/rfc/rfc5280), May 2008.

# Installation
You can download the self-extracting EXE file for the CertBuilder application from [here](https://1drv.ms/u/c/4f6607f8bc331ae0/ERped4jK2LpLtelyCRSSrkoBBd52WFEihIoAmQj3a1tbfQ?e=ZhqATG).

**Note**: The self-extracting EXE installation file has not been digitally signed with a valid code signing certificate.

# Dependancies
The CertBuilder project uses the following NG9-1-1 related NuGet package.
1. Ng911Lib (2.0.1)

# Project Structure

| Directory | Description |
|--------|--------|
| Source | Contains the Visual Studio project and source code files for the CertBuilder application |
| CertBuilderSetup | Contains the Visual Studio project files for the project that builds a Windows MSI installation file. |

# Building an Installation File
The steps to build an installation file for the CertBuilder application are:
1. Open the CertBuilder.sln solution in Visual Studio, open the project properties and change the Assembly version and the File version.
2. Select Release build and build the project.
3. Publish the project
4. Open the CertBuilderSetup solution
5. Make sure that all of the files in the CertBuilder/Publish directory are included in the Application Folder.
6. Make sure that the Debug build is selected.
7. Modify the Version in the project properties. Visual Studio will prompt you to change the Product Code. Select Yes.
8. Build the project. 
9. Build a self-extracting EXE file as described in the following section.

## Creating a Self-Extracting EXE Installation File
The Windows iexpress application can be used to build a self-extracting EXE file from the setup.exe and the CertBuilder.msi and setup.exe files produced by the CertBuilderSetup project.

The CertBuilderSetup directory contains a configuration file for iexpress called CertBuilderSetup.SED.

The iexpress application does not support relative file paths so this file must be modified as follows.

1. Modify the TargetName property (shown highlighted below) to specify the path of the output EXE file for the path on the development computer that you are using.
2. Change the version number of the CertBuilder.X.X.exe file name in the TargetName property to the correct release version. 
3. Modify the SourceFiles0 property (shown highlighted below) to specify the path to the source files for the path on the development computer that you are using. 

The highlighted text lines shown below are the ones in the CertBuilderSetup.SED file that need to be modified.

> [Version]
Class=IEXPRESS
SEDVersion=3
[Options]
PackagePurpose=InstallApp
ShowInstallProgramWindow=0
HideExtractAnimation=0
UseLongFileName=1
InsideCompressed=0
CAB_FixedSize=0
CAB_ResvCodeSigning=0
RebootMode=I
InstallPrompt=%InstallPrompt%
DisplayLicense=%DisplayLicense%
FinishMessage=%FinishMessage%
TargetName=%TargetName%
FriendlyName=%FriendlyName%
AppLaunched=%AppLaunched%
PostInstallCmd=%PostInstallCmd%
AdminQuietInstCmd=%AdminQuietInstCmd%
UserQuietInstCmd=%UserQuietInstCmd%
SourceFiles=SourceFiles
[Strings]
InstallPrompt=
DisplayLicense=
FinishMessage=
<mark>TargetName=C:\_MyProjects\CertBuilder\InstallationPackages\CertBuilder1.0.0.exe</mark>
FriendlyName=CertBuilder
AppLaunched=setup.exe
PostInstallCmd=<None>
AdminQuietInstCmd=
UserQuietInstCmd=
FILE0="CertBuilderSetup.msi"
FILE1="setup.exe"
[SourceFiles]
<mark>SourceFiles0=C:\_MyProjects\CertBuilder\CertBuilderSetup\Debug\</mark>
[SourceFiles0]
%FILE0%=
%FILE1%=

Once the CertBuilderSetup.SED file has been modified and saved, you can use it with iexpress to produce a self-extracting EXE file by following these steps:
1. Open a command prompt window
1. Type: iexpress
1. Select Open existing Self Extraction Directive file
1. Browse to the CertBuilderSetup directory and select the CertBuilderSetup.SED file
1. Click on the Next button
1. Make sure that the Create Package option is selected and click on Next

The CertBuilderX.X.X.exe file will be created in the InstallationPackages directory.


