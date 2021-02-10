using System;
using System.Collections.Generic;
using System.IO;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Encryption;

namespace EnvoyMedSys
{
    public class AutoEncryptHelper
    {
        private static readonly string __localMasterKeyPath = "../../../master-key.txt";
        private static readonly string __sampleNameValue = "Takeshi Kovacs";
        private static readonly int __sampleSsnValue = 213238414;

        private static readonly BsonDocument __sampleDocFields =
            new BsonDocument
            {
                { "name", __sampleNameValue },
                { "ssn", __sampleSsnValue },
                { "bloodType", "AB-" },
                {
                    "medicalRecords",
                    new BsonArray(new []
                    {
                        new BsonDocument("weight", 180),
                        new BsonDocument("bloodPressure", "120/80")
                    })
                },
                {
                    "insurance",
                    new BsonDocument
                    {
                        { "policyNumber", 211241 },
                        { "provider", "EnvoyHealth" }
                    }
                }
            };

        private readonly string _connectionString;
        private readonly CollectionNamespace _keyVaultNamespace;
        private readonly CollectionNamespace _medicalRecordsNamespace;

        public AutoEncryptHelper(string connectionString, CollectionNamespace keyVaultNamespace)
        {
            _connectionString = connectionString;
            _keyVaultNamespace = keyVaultNamespace;
            _medicalRecordsNamespace = CollectionNamespace.FromFullName("medicalRecords.patients");
        }

        public async void EncryptedWriteAndReadAsync(string keyIdBase64, KmsKeyLocation kmsKeyLocation)
        {
            // Construct a JSON Schema
            var schema = JsonSchemaCreator.CreateJsonSchema(keyIdBase64);

            // Construct an auto-encrypting client
            var autoEncryptingClient = CreateAutoEncryptingClient(
                kmsKeyLocation,
                _keyVaultNamespace,
                schema);

            // Set our working database and collection to medicalRecords.patients
            var collection = autoEncryptingClient
                .GetDatabase(_medicalRecordsNamespace.DatabaseNamespace.DatabaseName)
                .GetCollection<BsonDocument>(_medicalRecordsNamespace.CollectionName);

            var ssnQuery = Builders<BsonDocument>.Filter.Eq("ssn", __sampleSsnValue);

            // Upsert (update document if found, otherwise create it) a document into the collection
            var medicalRecordUpdateResult = await collection
                .UpdateOneAsync(ssnQuery, new BsonDocument("$set", __sampleDocFields), new UpdateOptions() { IsUpsert = true });

            if (!medicalRecordUpdateResult.UpsertedId.IsBsonNull) 
            {
                Console.WriteLine("Successfully upserted the sample document!");
            }

            // Query by SSN field with auto-encrypting client
            var result = collection.Find(ssnQuery).Single();

            Console.WriteLine($"Encrypted client query by the SSN (deterministically-encrypted) field:\n {result}\n");
        }

        public void QueryWithNonEncryptedClient()
        {
            var nonAutoEncryptingClient = new MongoClient(_connectionString);
            var collection = nonAutoEncryptingClient
              .GetDatabase(_medicalRecordsNamespace.DatabaseNamespace.DatabaseName)
              .GetCollection<BsonDocument>(_medicalRecordsNamespace.CollectionName);
            var ssnQuery = Builders<BsonDocument>.Filter.Eq("ssn", __sampleSsnValue);

            var result = collection.Find(ssnQuery).FirstOrDefault();
            if (result != null)
            {
                throw new Exception("Expected no document to be found but one was found.");
            }

            // Query by name field with a normal non-auto-encrypting client
            var nameQuery = Builders<BsonDocument>.Filter.Eq("name", __sampleNameValue);
            result = collection.Find(nameQuery).FirstOrDefault();
            if (result == null)
            {
                throw new Exception("Expected the document to be found but none was found.");
            }

            Console.WriteLine($"Query by name (non-encrypted field) using non-auto-encrypting client returned:\n {result}\n");
        }


        private IMongoClient CreateAutoEncryptingClient(
            KmsKeyLocation kmsKeyLocation,
            CollectionNamespace keyVaultNamespace,
            BsonDocument schema)
        {
            var kmsProviders = new Dictionary<string, IReadOnlyDictionary<string, object>>();

            // Specify the local master encryption key
            if (kmsKeyLocation == KmsKeyLocation.Local)
            {
                var localMasterKeyBase64 = File.ReadAllText(__localMasterKeyPath);
                var localMasterKeyBytes = Convert.FromBase64String(localMasterKeyBase64);
                var localOptions = new Dictionary<string, object>
                    {
                        { "key", localMasterKeyBytes }
                    };
                kmsProviders.Add("local", localOptions);
            }

            // 
            var schemaMap = new Dictionary<string, BsonDocument>();
            schemaMap.Add(_medicalRecordsNamespace.ToString(), schema);

            // Specify location of mongocryptd binary, if necessary
            var extraOptions = new Dictionary<string, object>()
            {
                // uncomment the following line if you are running mongocryptd manually
                // { "mongocryptdBypassSpawn", true }
            };

            // Create CSFLE-enabled MongoClient
            // The addition of the automatic encryption settings are what 
            // change this from a standard MongoClient to a CSFLE-enabled one
            var clientSettings = MongoClientSettings.FromConnectionString(_connectionString);
            var autoEncryptionOptions = new AutoEncryptionOptions(
                keyVaultNamespace: keyVaultNamespace,
                kmsProviders: kmsProviders,
                schemaMap: schemaMap,
                extraOptions: extraOptions);
            clientSettings.AutoEncryptionOptions = autoEncryptionOptions;
            return new MongoClient(clientSettings);
        }
    }
}