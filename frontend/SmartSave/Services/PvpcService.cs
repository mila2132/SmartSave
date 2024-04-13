using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;
using SmartSave.Model;
using Newtonsoft.Json.Converters;
using Amazon.CognitoIdentity;
using Amazon;
using Microsoft.Extensions.Configuration;

namespace SmartSave.Services
{
    public class PvpcService
    {

		IAmazonS3 amazonS3Client { get; set; }
		CognitoAWSCredentials credentials { get; set;}
		private readonly IConfiguration _configuration;

		private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
		{
			DateFormatString = "dd/MM/yyyy",
			Converters = new List<JsonConverter> { new StringEnumConverter() }
		};

		public PvpcService(IConfiguration configuration)
		{
			_configuration = configuration;

			var awsCognitoPoolId = _configuration["AWS:CognitoPoolId"]; 
			var awsRegion = RegionEndpoint.GetBySystemName(_configuration["AWS:Region"]);

			this.credentials = new CognitoAWSCredentials(awsCognitoPoolId,
										awsRegion);
		}


		public async Task<Dictionary<string,List<Datapvpc>>> GetDatapvpcs()
		{
			amazonS3Client = new AmazonS3Client(credentials, RegionEndpoint.USEast1);
			try
			{
				var request = new GetObjectRequest
				{
					BucketName = _configuration["AWS:BucketS3:0:BucketName"],
					Key = _configuration["AWS:BucketS3:0:Key"]
				};

				using GetObjectResponse response = await amazonS3Client.GetObjectAsync(request);

				if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
				{
					using Stream responseStream = response.ResponseStream;
					using StreamReader reader = new StreamReader(responseStream);
					string content = await reader.ReadToEndAsync();

					List<Datapvpc> entradas = JsonConvert.DeserializeObject<List<Datapvpc>>(content, JsonSettings);

					if (entradas.Count >= 24)
					{
						var diccionario = new Dictionary<string, List<Datapvpc>>
						{
							["AM"] = entradas.GetRange(0, 12),
							["PM"] = entradas.GetRange(12, 12)
						};

						return diccionario;
					}

					throw new Exception("La lista de entradas no tiene suficientes elementos.");
				}
			}
			catch (AmazonS3Exception e)
			{
				Console.WriteLine("Error encountered on server. Message:'{0}' when reading object", e.Message);
				throw;
			}
			catch (Exception e)
			{
				Console.WriteLine("Unknown error encountered on server. Message:'{0}' when reading object", e.Message);
				throw;
			}
			finally
			{
				amazonS3Client.Dispose();
			}
			return null;
		}



	}
}
