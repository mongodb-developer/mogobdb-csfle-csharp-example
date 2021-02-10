# Client-Side Field Level Encryption with MongoDB & .NET Driver

This is the sample application created in the [Client-Side Field Level Encryption (CSFLE) in MongoDB with C# tutorial](https://developer.mongodb.com/how-to/client-side-field-level-encryption-mongodb-csharp) by [Adrienne Tacke](https://twitter.com/adriennetacke). Feel free to clone this repo and follow along with the steps in the tutorial.

## Dependencies

* A [MongoDB Atlas](https://www.mongodb.com/cloud/atlas>) cluster running MongoDB 4.2 (or later) OR [MongoDB 4.2 Enterprise Server](https://www.mongodb.com/try/download/enterprise>) (or later). Required for automatic encryption.
* [MongoDB .NET Driver 2.12.0-beta](https://www.nuget.org/packages/MongoDB.Driver/2.12.0-beta1) (or later)
* [Mongocryptd](https://docs.mongodb.com/manual/reference/security-client-side-encryption-appendix/#installation)* (only required if running locally)
* File system permissions (to start the mongocryptd process, if running locally)

## Running the application

1. Clone this repository:

   ```
     git clone https://github.com/adriennetacke/mongodb-csfle-csharp-demo.git
   ```

2. If running locally, start a `mongod` instance (Enterprise version >= 4.2) running on port 27017.

3. Before running, don't forget to change the `connectionString` in `Program.cs`. If you're running a local MongoDB instance and haven't changed any default settings, you can use the default connection string: ``mongodb://localhost:27017``. If using a MongoDB Atlas cluster, paste in your cluster's URI.

When running the first time, a a key file will be generated. Make sure that this key file is in the root of your execution environment (it will automatically do this when generated). If you move it, be sure to also update the key file path variables in the code.

## More MongoDB Tutorials

Check out these other tutorials from Adrienne:

- [Create a Multi-Cloud Cluster with MongoDB Atlas](https://developer.mongodb.com/how-to/setup-multi-cloud-cluster-mongodb-atlas/)
- [Using MongoDB Atlas on Heroku](https://developer.mongodb.com/how-to/use-atlas-on-heroku/)
- [How to Use the Union All Aggregation Pipeline Stage in MongoDB 4.4](https://developer.mongodb.com/how-to/use-union-all-aggregation-pipeline-stage/)

