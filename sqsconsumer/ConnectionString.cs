using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using MySqlConnector;


namespace sqsconsumer.Model
{
    class ConnectionString
    {
     
        public static string GetRDSConnectionString()

 
        {

            var appConfig = new ConfigurationBuilder()
                .AddJsonFile(@"Properties\appsettings.json", optional: false)
                .Build();
            MySqlConnectionStringBuilder conn_string = new();

            string dbname = appConfig.GetSection("Config:RDS_DB_NAME").Value;
            conn_string.Database = dbname;
            if (string.IsNullOrEmpty(dbname)) return null;
            conn_string.UserID = GetSecret(appConfig.GetSection("Config:SM_USER").Value);
            conn_string.Password = GetSecret(appConfig.GetSection("Config:SM_PASSWORD").Value); 
            conn_string.Server = GetSecret(appConfig.GetSection("Config:SM_HOST").Value);
            conn_string.Port = 3306;          
           


            return conn_string.ToString();


        }

        public static string GetSecret(string uri)
        {
            string secretName = uri;
            string region = "us-east-1";
            string secret = "";

           

            IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

            GetSecretValueRequest request = new();
            request.SecretId = secretName;
            request.VersionStage = "AWSCURRENT"; // VersionStage defaults to AWSCURRENT if unspecified.

            GetSecretValueResponse response;

            try
            {
                response = client.GetSecretValueAsync(request).Result;

            }
            catch (DecryptionFailureException e)
            {
         
                throw e;
            }
            catch (InternalServiceErrorException e)
            {
                throw e;
            }
            catch (InvalidParameterException e)            {

                throw e;
            }
            catch (InvalidRequestException e)
            {
                throw e;
            }
            catch (ResourceNotFoundException e)
            {
                throw e;
            }
            catch (System.AggregateException e)
            {
                throw e;
            }           

            return secret;
        }

       
    }
}
