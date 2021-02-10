using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Encryption;

namespace EnvoyMedSys
{
    public class KmsKeyHelper
    {
        private readonly static string __localMasterKeyPath = "../../../master-key.txt";

        private readonly string _mdbConnectionString;
        private readonly CollectionNamespace _keyVaultNamespace;

        public KmsKeyHelper(
            string connectionString,
            CollectionNamespace keyVaultNamespace)
        {
            _mdbConnectionString = connectionString;
            _keyVaultNamespace = keyVaultNamespace;
        }

        public void GenerateLocalMasterKey()
        {
            using (var randomNumberGenerator = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var bytes = new byte[96];
                randomNumberGenerator.GetBytes(bytes);
                var localMasterKeyBase64 = Convert.ToBase64String(bytes);
                Console.WriteLine(localMasterKeyBase64);
                File.WriteAllText(__localMasterKeyPath, localMasterKeyBase64);
            }
        }

        public string CreateKeyWithLocalKmsProvider()
        {
            // Read Master Key from file & Convert
            string localMasterKeyBase64 = File.ReadAllText(__localMasterKeyPath);
            var localMasterKeyBytes = Convert.FromBase64String(localMasterKeyBase64);

            // Set KMS Provider Settings
            // Client uses these settings to discover the master key
            var kmsProviders = new Dictionary<string, IReadOnlyDictionary<string, object>>();
            var localOptions = new Dictionary<string, object>
            {
                { "key", localMasterKeyBytes }
            };
            kmsProviders.Add("local", localOptions);

            // Create Data Encryption Key
            var clientEncryption = GetClientEncryption(kmsProviders);
            var dataKeyId = clientEncryption.CreateDataKey("local", new DataKeyOptions(), CancellationToken.None);
            clientEncryption.Dispose();
            Console.WriteLine($"Local DataKeyId [UUID]: {dataKeyId}");

            var dataKeyIdBase64 = Convert.ToBase64String(GuidConverter.ToBytes(dataKeyId, GuidRepresentation.Standard));
            Console.WriteLine($"Local DataKeyId [base64]: {dataKeyIdBase64}");

            ValidateKey(dataKeyId);
            return dataKeyIdBase64;
        }

        private ClientEncryption GetClientEncryption(
            Dictionary<string, IReadOnlyDictionary<string, object>> kmsProviders)
        {
            var keyVaultClient = new MongoClient(_mdbConnectionString);
            var clientEncryptionOptions = new ClientEncryptionOptions(
                keyVaultClient: keyVaultClient,
                keyVaultNamespace: _keyVaultNamespace,
                kmsProviders: kmsProviders);

            return new ClientEncryption(clientEncryptionOptions);
        }

        private void ValidateKey(Guid dataKeyId)
        {
            var client = new MongoClient(_mdbConnectionString);
            var collection = client
                .GetDatabase(_keyVaultNamespace.DatabaseNamespace.DatabaseName)
#pragma warning disable CS0618 // Type or member is obsolete
                        .GetCollection<BsonDocument>(_keyVaultNamespace.CollectionName, new MongoCollectionSettings { GuidRepresentation = GuidRepresentation.Standard });
#pragma warning restore CS0618 // Type or member is obsolete

            var query = Builders<BsonDocument>.Filter.Eq("_id", new BsonBinaryData(dataKeyId, GuidRepresentation.Standard));
            var keyDocument = collection
                .Find(query)
                .Single();

            Console.WriteLine(keyDocument);
        }
    }
}
