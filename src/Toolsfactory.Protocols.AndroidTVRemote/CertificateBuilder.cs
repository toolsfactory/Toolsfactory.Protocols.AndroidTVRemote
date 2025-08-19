using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System.Security.Cryptography.X509Certificates;

namespace Toolsfactory.Protocols.AndroidTVRemote
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class CertificateBuilder
    {
        private readonly Org.BouncyCastle.X509.X509Certificate _certificate;
        private readonly AsymmetricKeyParameter _privateKey;

        #region Constructors
        public CertificateBuilder(CertificateNameOptions nameOptions, CertificateOptions certificateOptions)
        {
            var random = GenerateSecureRandom();
            var subjectKeyPair = GenerateKeyPair(certificateOptions, random);
            var issuerKeyPair = subjectKeyPair;
            var signatureFactory = new Asn1SignatureFactory(certificateOptions.SignatureAlgorithm, issuerKeyPair.Private, random);
            var name = GenerateName(nameOptions);

            var certificateGenerator = new X509V3CertificateGenerator();
            certificateGenerator.SetNotBefore(certificateOptions.NotBefore);
            certificateGenerator.SetNotAfter(certificateOptions.NotAfter);
            certificateGenerator.SetSubjectDN(name);
            certificateGenerator.SetIssuerDN(name);
            certificateGenerator.SetPublicKey(subjectKeyPair.Public);
            certificateGenerator.SetSerialNumber(GenerateSerial(random));

            if (certificateOptions.ExtendedClientAuthentication)
                AddExtendedClientAuthentication(certificateGenerator);

            _certificate = certificateGenerator.Generate(signatureFactory);
            _privateKey = issuerKeyPair.Private;
        }
        #endregion

        #region PEM Export Methods
        public string CertificateAsPEM()
        {
            using var textWriter = new StringWriter();
            using (PemWriter pemWriter = new PemWriter(textWriter))
            {
                pemWriter.WriteObject(_certificate);
            }
            return textWriter.ToString();
        }

        public string PrivateKeyAsPEM()
        {
            var pkcs8 = new Pkcs8Generator(_privateKey);
            using var textWriter = new StringWriter();
            using (PemWriter pemWriter = new PemWriter(textWriter))
            {
                pemWriter.WriteObject(pkcs8);
            }
            return textWriter.ToString();
        }
        #endregion

        #region Private Helpers
        private static BigInteger GenerateSerial(SecureRandom random)
        {
            // Serial number is required, generate it randomly
            byte[] serial = new byte[20];
            random.NextBytes(serial);
            serial[0] = 1;
            var bigSerial = new BigInteger(serial);
            return bigSerial;
        }

        private static void AddExtendedClientAuthentication(X509V3CertificateGenerator certificateGenerator)
        {
            var keyUsage = new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyEncipherment);
            certificateGenerator.AddExtension(X509Extensions.KeyUsage, false, keyUsage.ToAsn1Object());

            var extendedKeyUsage = new ExtendedKeyUsage(new[] { KeyPurposeID.id_kp_clientAuth });
            certificateGenerator.AddExtension(X509Extensions.ExtendedKeyUsage, true, extendedKeyUsage.ToAsn1Object());
        }

        private static AsymmetricCipherKeyPair GenerateKeyPair(CertificateOptions certificateOptions, SecureRandom random)
        {
            var keyGenerationParameters = new KeyGenerationParameters(random, certificateOptions.KeyStrength);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            return keyPairGenerator.GenerateKeyPair();
        }

        private static SecureRandom GenerateSecureRandom()
        {
            var randomGenerator = new CryptoApiRandomGenerator();
            var random = new SecureRandom(randomGenerator);
            return random;
        }

        private X509Name GenerateName(CertificateNameOptions nameOptions)
        {
            var nameOids = new List<DerObjectIdentifier>
            {
                X509Name.CN,
                X509Name.O,
                X509Name.OU,
                X509Name.ST,
                X509Name.C,
                X509Name.L,
                X509Name.E
            };

            var nameValues = new Dictionary<DerObjectIdentifier, string>()
            {
                { X509Name.CN, nameOptions.Name },
                { X509Name.O, nameOptions.Organisation },
                { X509Name.OU, nameOptions.OrganisationUnit },
                { X509Name.ST, nameOptions.State },
                { X509Name.C, nameOptions.Country },
                { X509Name.L, nameOptions.Locality },
                { X509Name.E, nameOptions.Email }
            };

            return new X509Name(nameOids, nameValues);
        }
        #endregion

        #region Static Helpers
        public static X509Certificate2 LoadCertificateFromPEM(string pem)
        {
            using var textReader = new StringReader(pem);
            using PemReader reader = new PemReader(textReader);
            Org.BouncyCastle.X509.X509Certificate certificate = null!;
            AsymmetricKeyParameter privateKey = null!;

            while (reader.ReadPemObject() is { } read)
            {
                switch (read.Type)
                {
                    case "CERTIFICATE": certificate = new Org.BouncyCastle.X509.X509Certificate(read.Content); break;
                    case "PRIVATE KEY": privateKey = PrivateKeyFactory.CreateKey(read.Content); break;
                    default: throw new NotSupportedException(read.Type);
                }
            }

            if (certificate == null || privateKey == null)
                throw new Exception("Unable to load certificate with the private key from the PEM!");

            return GetX509CertificateWithPrivateKey(certificate, privateKey);
        }

        private static X509Certificate2 GetX509CertificateWithPrivateKey(Org.BouncyCastle.X509.X509Certificate bouncyCastleCert, AsymmetricKeyParameter privateKey)
        {
            // This workaround is needed to fill in the Private Key in the X509Certificate2
            string alias = bouncyCastleCert.SubjectDN.ToString();
            Pkcs12Store store = new Pkcs12StoreBuilder().Build();

            X509CertificateEntry certEntry = new X509CertificateEntry(bouncyCastleCert);
            store.SetCertificateEntry(alias, certEntry);

            AsymmetricKeyEntry keyEntry = new AsymmetricKeyEntry(privateKey);
            store.SetKeyEntry(alias, keyEntry, [certEntry]);

            byte[] certificateData;
            string password = Guid.NewGuid().ToString();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                store.Save(memoryStream, password.ToCharArray(), new SecureRandom());
                memoryStream.Flush();
                certificateData = memoryStream.ToArray();
            }

            return new X509Certificate2(certificateData, password, X509KeyStorageFlags.Exportable);
        }
        #endregion
    }

    #region Record Definitions
    public abstract record CertificateNameOptions(
        string Name,
        string Country = "",
        string State = "",
        string Locality = "",
        string Organisation = "",
        string OrganisationUnit = "",
        string Email = ""
    );

    public abstract record CertificateOptions(
        DateTime NotBefore,
        DateTime NotAfter,
        bool ExtendedClientAuthentication = true,
        int KeyStrength = 2048,
        string SignatureAlgorithm = "SHA256WITHRSA"
    );
    #endregion
}
