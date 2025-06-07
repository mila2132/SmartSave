using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using SmartSave.Services;
using SmartSave.View.PopUps;
using Newtonsoft.Json;
using System.Collections.Generic;
using SmartSave.Model;

namespace SmartSave.ViewModel
{
	public partial class ThermostatViewModel : BaseViewModel
	{
		private readonly IServiceProvider _serviceProvider;
		GoogleNestThermostatService _thermostatService;
		TemperatureService _temperatureService;

		[ObservableProperty]
		[NotifyPropertyChangedFor(nameof(ModeText))]
		bool isHeatMode;

		[ObservableProperty]
		string temperature;

		[ObservableProperty]
		bool isOff;

		[ObservableProperty]
		bool isAutomaticMode;

		[ObservableProperty]
		string temperatureMqtt;

		public string ModeText => IsHeatMode ? "Mode: Heat" : "Mode: Cool";

		public ThermostatViewModel(IServiceProvider serviceProvider, GoogleNestThermostatService thermostatService, TemperatureService temperatureService)
		{
			Title = "Termostato";
			_serviceProvider = serviceProvider;
			_thermostatService = thermostatService;
			_temperatureService = temperatureService;
			IsOff = true;
			IsAutomaticMode = false;
			_temperatureService.SubscribeToTemperature();
			MessagingCenter.Subscribe<TemperatureService, string>(this, "TemperatureUpdated", (sender, temp) =>
			{
				TemperatureMqtt = temp + "ºC";  
			});
		}


		~ThermostatViewModel()
		{
			MessagingCenter.Unsubscribe<TemperatureService, string>(this, "TemperatureUpdated");
		}

		[RelayCommand]
		private async void OpenAutomaticMode()
		{
			var popup = _serviceProvider.GetRequiredService<AutomaticModePopup>();
			bool result = (bool)await Application.Current.MainPage.ShowPopupAsync(popup);
			if (result)
			{
				IsAutomaticMode = true;
			}
		}

		[RelayCommand]
		private async void SendData()
		{
			if (Temperature == null || Temperature == string.Empty || !int.TryParse(Temperature, out _))
			{
				await Shell.Current.DisplayAlert("Termostato", "Ingrese una temperatura válida", "OK");
				return;
			}
			var isSended = await _thermostatService.SendDataTemperature(Temperature, IsHeatMode);
			if (isSended == "ok")
				await Shell.Current.DisplayAlert("Termostato", $"Se ha enviado la temperatura {Temperature} en modo {(IsHeatMode ? "Heat" : "Cool")}", "OK");
			else if (isSended == "forbidden")
				await Shell.Current.DisplayAlert("Termostato", "Esta activado el modo automatico", "OK");
			else
				await Shell.Current.DisplayAlert("Termostato", "Error al enviar la temperatura", "OK");
			
		}

		[RelayCommand]
		private async void ActiveManualMode()
		{
			if (!IsAutomaticMode)
			{
				return;
			}
			var isActivated = await _thermostatService.ActiveManualMode();
			if (isActivated)
			{
				IsAutomaticMode = false;
				await Shell.Current.DisplayAlert("Termostato", "Modo manual activado", "OK");
			}
			else
				await Shell.Current.DisplayAlert("Termostato", "Error al activar modo manual", "OK");
		}

		[RelayCommand]
		private async void TurnOnOff()
		{
			if (IsOff)
			{
				IsOff = false;
				return;
			}
			var isTurnedOff = await _thermostatService.TurnOffThermostat();
			if (isTurnedOff == "ok")
				IsOff = true;
			else if (isTurnedOff == "forbidden")
				await Shell.Current.DisplayAlert("Termostato", "Esta activado el modo automatico", "OK");
			else
				await Shell.Current.DisplayAlert("Termostato", "Error al apagar termostato", "OK");
		}

	}

}
