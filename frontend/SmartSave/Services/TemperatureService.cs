using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSave.Services
{
	public class TemperatureService
	{
		
		private readonly IConfiguration _configuration;
		private IMqttClient _client;
		private MqttClientOptions _options;


		public TemperatureService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task SubscribeToTemperature()
		{
			_client = new MqttFactory().CreateMqttClient();
			_options = new MqttClientOptionsBuilder()
				.WithTcpServer(_configuration["GoogleNest:broker"], 1883)
				.WithCleanSession()
				.Build();
			try
			{
				await _client.ConnectAsync(_options);
				Console.WriteLine("Connected to MQTT Broker");
				await _client.SubscribeAsync("nest/temperature");
				_client.ApplicationMessageReceivedAsync += HandleReceivedMessage;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to connect to MQTT Broker: {ex.Message}");
			}
			await _client.SubscribeAsync("nest/temperature");
		}

		private Task HandleReceivedMessage(MqttApplicationMessageReceivedEventArgs e)
		{
			string message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
			Console.WriteLine($"Received message: {message}");

			MessagingCenter.Send(this, "TemperatureUpdated", message);
			return Task.CompletedTask;
		}

	}
}
