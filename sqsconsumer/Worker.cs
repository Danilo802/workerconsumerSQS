using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using sqsconsumer.Model;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace sqsconsumer
{
    public class Worker : BackgroundService
    {

        private string nomearquivo;
        private int tamanhoarquivo;
        private DateTime dataatualizacaoarquivo; 



        private readonly ILogger<Worker> _logger;
        private readonly IAmazonSQS _sqs;
        private readonly string queryurl = "https://sqs.us-east-1.amazonaws.com/conta/bemobiqueue";


        public Worker(ILogger<Worker> logger, IAmazonSQS sqs)
        {
            _logger = logger;
            _sqs = sqs;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var request = new ReceiveMessageRequest
                    {
                        QueueUrl = queryurl,
                        WaitTimeSeconds = 5

                    };

                    var result = await _sqs.ReceiveMessageAsync(request);
                    if (result.Messages.Any())
                    {
                        foreach (var message in result.Messages)
                        {
                            _logger.LogInformation("Processando mensagem: {message} | {time}", message.Body, DateTimeOffset.Now);

                            DeserializerJson(message.Body);
                            await _sqs.DeleteMessageAsync(request.QueueUrl, message.ReceiptHandle);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e.InnerException.ToString());
                }

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
        }

        protected void DeserializerJson(string mensagen)
        {
            try
            {
                var fileproperties = mensagen.ToString().Replace("object", "objeto");
                var filepropertiesdes = JsonConvert.DeserializeObject<dynamic>(fileproperties);
                var filename = Convert.ToString(filepropertiesdes.Records[0].s3.objeto.key);
                var filesize = Convert.ToInt16(filepropertiesdes.Records[0].s3.objeto.size);
                var last_modified = Convert.ToDateTime(filepropertiesdes.Records[0].eventTime);



                ValidarArquivo(filename, filesize, last_modified);


            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException.ToString());
            }
        }

        private void ValidarArquivo(string filename, int size, DateTime eventTime)
        {
            try
            {  
                var read = ConsultarArquivoExistente(filename, eventTime);

                if (read == 0)
                {                    
                    GravarArquivo(filename, size, eventTime);
                }

                else
                {
                    if (eventTime > dataatualizacaoarquivo)
                    {
                        AtualizarArquivo(filename, size, eventTime);
                    }

                    else
                    {
                        _logger.LogInformation("Dados não atualizados, o arquivo anterior é mais atual do que o enviado", DateTimeOffset.Now);
                    } 


                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException.ToString());
            }



        }


        private int ConsultarArquivoExistente(string filename, DateTime eventTime)
        {

            try
            {

                var dataevento = eventTime.ToString("yyyy-MM-dd HH:mm:ss");
                var connection = ConnectionString.GetRDSConnectionString();
                MySqlConnection MyCon = new(connection.ToString());

                MyCon.Open();
                var coman = MyCon.CreateCommand();

                //Verifica se o arquivo existe
                coman.CommandText = "SELECT filename, filesize, last_modified FROM files WHERE  filename = @filename";
               coman.Parameters.AddWithValue("?filename", filename);
               var reader = coman.ExecuteReader();        
                var count = 0;
                while (reader.Read())
                {
                    nomearquivo = reader.GetString(0).ToString();
                    tamanhoarquivo = reader.GetInt32(1);
                    dataatualizacaoarquivo = reader.GetDateTime(2);
                    count = 1;
                }
                
                reader.Close();
                MyCon.Close();

                return count;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException.ToString());
                throw;
            }
        }


        private void GravarArquivo(string filename, int size, DateTime eventTime)
        {

            try
            {

                var dataevento = eventTime.ToString("yyyy-MM-dd HH:mm:ss");
                var connection = ConnectionString.GetRDSConnectionString();
                MySqlConnection MyCon = new(connection.ToString());       
             
                MyCon.Open();
                var coman = MyCon.CreateCommand();
                    //Insere o novo arquivo
                var comn = MyCon.CreateCommand();
                comn.CommandText = "INSERT INTO files(filename,filesize,last_modified) VALUES(@filename, @filesize, @last_modified)";
                comn.Parameters.AddWithValue("?filename", filename);
                comn.Parameters.AddWithValue("?filesize", size);
                comn.Parameters.AddWithValue("?last_modified", dataevento);
                comn.ExecuteNonQuery();        

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException.ToString());
                throw;
            }
        }

        private void AtualizarArquivo(string filename, int size, DateTime eventTime)
        {

            try
            {

                var dataevento = eventTime.ToString("yyyy-MM-dd HH:mm:ss");
                var connection = ConnectionString.GetRDSConnectionString();
                MySqlConnection MyCon = new(connection.ToString());
             
                MyCon.Open();
                var coman = MyCon.CreateCommand();
                //Insere o novo arquivo
                var comn = MyCon.CreateCommand();
                comn.CommandText = "UPDATE files SET  filesize = @filesize,  last_modified = @last_modified WHERE filename = @filename";
                comn.Parameters.AddWithValue("?filename", filename);
                comn.Parameters.AddWithValue("?filesize", size);
                comn.Parameters.AddWithValue("?last_modified", dataevento);
                comn.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.InnerException.ToString());
                throw;
            }
        }

    }
}
