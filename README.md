# Notice: Repository Deprecation
This repository is deprecated and no longer actively maintained. It contains outdated code examples or practices that do not align with current MongoDB best practices. While the repository remains accessible for reference purposes, we strongly discourage its use in production environments.
Users should be aware that this repository will not receive any further updates, bug fixes, or security patches. This code may expose you to security vulnerabilities, compatibility issues with current MongoDB versions, and potential performance problems. Any implementation based on this repository is at the user's own risk.
For up-to-date resources, please refer to the [MongoDB Developer Center](https://mongodb.com/developer).


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

3. In the `launchSettings.json` file, update the `MDB_URI` variable. 

- **MongoDB Atlas**: If using a MongoDB Atlas cluster, be sure to update the `<user>`, `<password>`, and `<yourcluster>` portions of the URI with your own credentials.
- **Local**: If running a local MongoDB instance and haven't changed any default settings, you can replace what's already in the file with the default connection string: ``mongodb://localhost:27017``.

Alternatively, you can add this variable to your host machine's environment variables as well. Note that you may have to restart Visual Studio for the variable to be registered.

💡 If you decide to use the `launchSettings.json` file, be sure to immediately add it to your `.gitignore` file so that you don't inadvertently expose your URI to the world! On a similar note, do not paste in your connection string directly into the code! *Well, why is there a `launchSettings.json` file in your repo, Adrienne?* I've deliberately left this file in to make development a bit easier should you decide to clone this repo. :) 

4. When running the first time, a a key file will be generated. Make sure that this key file is in the root of your execution environment (it will automatically do this when generated). If you move it, be sure to also update the key file path variables in the code.

## More MongoDB Tutorials

Check out these other tutorials from Adrienne:

- [Create a Multi-Cloud Cluster with MongoDB Atlas](https://developer.mongodb.com/how-to/setup-multi-cloud-cluster-mongodb-atlas/)
- [Using MongoDB Atlas on Heroku](https://developer.mongodb.com/how-to/use-atlas-on-heroku/)
- [How to Use the Union All Aggregation Pipeline Stage in MongoDB 4.4](https://developer.mongodb.com/how-to/use-union-all-aggregation-pipeline-stage/)

