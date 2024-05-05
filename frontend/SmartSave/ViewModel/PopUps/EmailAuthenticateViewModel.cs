using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using SmartSave.Services;
using SmartSave.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSave.ViewModel.PopUps
{
	public partial class EmailAuthenticateViewModel: BaseViewModel
	{
		[ObservableProperty]
		private string email;

		GoogleNestThermostatService _thermostatService;

		public EmailAuthenticateViewModel(GoogleNestThermostatService thermostatService)
		{
			_thermostatService = thermostatService;
		}

		[RelayCommand]
		private async void Authenticate(Popup popup)
		{
			bool isAuthenticated = await _thermostatService.AuthenticateEmail(Email);
			if (isAuthenticated)
			{
				await Shell.Current.DisplayAlert("Autenticación", "Autenticación exitosa", "OK");
				popup.Close(true);
				await Shell.Current.GoToAsync(nameof(ThermostatPage));
			}
			else
			{
				await Shell.Current.DisplayAlert("Autenticación", "Autenticación fallida", "Intenta de nuevo");
				popup.Close(false);
			}
		}
	}
}
