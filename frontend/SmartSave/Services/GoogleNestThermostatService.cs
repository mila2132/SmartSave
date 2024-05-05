using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;


namespace SmartSave.Services
{
	public class GoogleNestThermostatService
	{
		private static readonly HttpClient client = new HttpClient();
		private readonly IConfiguration _configuration;
		private string _requestUri;

		public GoogleNestThermostatService(IConfiguration configuration)
		{
			_configuration = configuration;
			_requestUri = _configuration["API:GoogleNestThermostat"];
		}

		public async Task<bool> AuthenticateEmail(string email)
		{
			if (!IsValidEmail(email))
			{
				Console.WriteLine("Email is not valid.");
				return false;
			}
			var jsonData = JsonConvert.SerializeObject(new { email = email });
			var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
			try
			{
				var response = await client.PostAsync(_requestUri + "/auth", content);
				if (response.IsSuccessStatusCode)
				{
					Console.WriteLine("Authentication successful.");
					return true;
				}
				else
				{
					Console.WriteLine("Authentication failed.");
					return false;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				return false;
			}

		}

		public async Task<bool> ActiveManualMode()
		{
			try
			{
				var response = await client.GetAsync(_requestUri + "/modeManual");
				if (response.IsSuccessStatusCode)
				{
					Console.WriteLine("Manual mode activated.");
					return true;
				}
				else
				{
					Console.WriteLine("Manual mode activation failed.");
					return false;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				return false;
			}
		}

		public async Task<bool> ActiveAutomaticMode(string temperature, string thermostatMode)
		{
			if (!IsValidTwoDigitNumber(temperature))
			{
				Console.WriteLine("Temperature is not valid.");
				return false;
			}
			var jsonData = JsonConvert.SerializeObject(new { temperature = temperature, thermostatMode = thermostatMode });
			var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
			try
			{
				var response = await client.PostAsync(_requestUri + "/modeAutomatic", content);
				if (response.IsSuccessStatusCode)
				{
					Console.WriteLine("Automatic mode activated.");
					return true;
				}
				else
				{
					Console.WriteLine("Automatic mode activation failed.");
					return false;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				return false;
			}
		}

		public async Task<string> SendDataTemperature(string temperature, bool isHeatMode)
		{
			if (!IsValidTwoDigitNumber(temperature))
			{
				Console.WriteLine("Temperature is not valid.");
				return "error";
			}
			string thermostatMode = "Cool";
			if (isHeatMode)
			{
				thermostatMode = "Heat";
			}
			var jsonData = JsonConvert.SerializeObject(new { temperature = temperature, thermostatMode = thermostatMode });
			var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
			try
			{
				var response = await client.PostAsync(_requestUri + "/updateTemperature", content);
				if (response.IsSuccessStatusCode)
				{
					Console.WriteLine("Data sent successfully.");
					return "ok";
				}
				else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
				{
					return "forbidden";
				}
				else
				{
					Console.WriteLine("Data sending failed.");
					return "error";
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				return "error";
			}
		}

		public async Task<string> TurnOffThermostat()
		{
			try
			{
				var response = await client.GetAsync(_requestUri + "/turnOff");
				if (response.IsSuccessStatusCode)
				{
					Console.WriteLine("Thermostat turned off.");
					return "ok";
				}
				else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
				{
					return "forbidden";
				}
				else
				{
					Console.WriteLine("Thermostat turn off failed.");
					return "error";
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				return "error";
			}
		}

		private static bool IsValidEmail(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
				return false;

			try
			{
				return Regex.IsMatch(email,
					@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
					RegexOptions.IgnoreCase);
			}
			catch (FormatException)
			{
				return false;
			}
		}

		private bool IsValidTwoDigitNumber(string temperature)
		{
			return temperature.All(char.IsDigit) && temperature.Length == 2;
		}

	}
}
