using CommunityToolkit.Maui.Views;
using SmartSave.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSave.ViewModel.PopUps
{
	public partial class AutomaticModeViewModel: BaseViewModel
	{
		[ObservableProperty]
		string temperature;

		[ObservableProperty]
		bool isHeatPressed;

		private GoogleNestThermostatService _thermostatService;

		public AutomaticModeViewModel(GoogleNestThermostatService thermostatService)
		{
			_thermostatService = thermostatService;
		}

		[RelayCommand]
		private void ToggleHeat()
		{
			IsHeatPressed = true;
		}

		[RelayCommand]
		private void ToggleCool()
		{
			IsHeatPressed = false;
		}

		[RelayCommand]
		private async void Send(Popup popup)
		{
			if (Temperature == null || Temperature == string.Empty || !int.TryParse(Temperature, out _))
			{
				await Shell.Current.DisplayAlert("Termostato", "Ingrese una temperatura válida", "OK");
				popup.Close(false);
				return;
			}
			bool isSuccessful = await _thermostatService.ActiveAutomaticMode(Temperature, IsHeatPressed ? "Heat" : "Cool");
			if (isSuccessful)
			{
				await Shell.Current.DisplayAlert("Termostato", "Modo automático activado", "OK");
			}
			else
			{
				await Shell.Current.DisplayAlert("Termostato", "Error al activar el modo automático", "OK");
			}
			popup.Close(isSuccessful);
		}

	}
}
