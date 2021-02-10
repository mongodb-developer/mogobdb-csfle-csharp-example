using System;
using System.IO;
using MongoDB.Driver;

namespace EnvoyMedSys
{
    public enum KmsKeyLocation
    {
        Local,
    }
    class Program
    {
        public static void Main()
        {
            var connectionString = "PASTE YOUR MONGODB CONNECTION STRING/ATLAS URI HERE";
            var keyVaultNamespace = CollectionNamespace.FromFullName("encryption.__keyVault");

            var kmsKeyHelper = new KmsKeyHelper(
                connectionString: connectionString,
                keyVaultNamespace: keyVaultNamespace);
            var autoEncryptHelper = new AutoEncryptHelper(
                connectionString: connectionString,
                keyVaultNamespace: keyVaultNamespace);

            string kmsKeyIdBase64;

            if (!File.Exists("../../../master-key.txt"))
            {
                kmsKeyHelper.GenerateLocalMasterKey();
            }

            kmsKeyIdBase64 = kmsKeyHelper.CreateKeyWithLocalKmsProvider();
            autoEncryptHelper.EncryptedWriteAndReadAsync(kmsKeyIdBase64, KmsKeyLocation.Local);

            // Comparison query on non-encrypting client
            autoEncryptHelper.QueryWithNonEncryptedClient();

            Console.ReadKey();
        }
    }
}
