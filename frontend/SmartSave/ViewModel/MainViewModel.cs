using SmartSave.Model;
using SmartSave.Services;
using SmartSave.View;
using SmartSave.View.PopUps;
using CommunityToolkit.Maui.Views;
using Newtonsoft.Json;

namespace SmartSave.ViewModel
{
    public partial class MainViewModel : BaseViewModel
    {
		private readonly IServiceProvider _serviceProvider;
		PvpcService _pvpcService;

		private Timer timer;
		private bool _isAuthenticated = false;

		public ObservableCollection<Datapvpc> DatapvpcAM { get; } = new();
		public ObservableCollection<Datapvpc> DatapvpcPM { get; } = new();

        public MainViewModel(PvpcService pvpcService, IServiceProvider serviceProvider)
        {
			Title = "Precio Luz hora";
			_serviceProvider = serviceProvider;
			_pvpcService = pvpcService;
			InitializeTimer();
			GetDatapvpcsAsync();
		}

		[RelayCommand]
		private async void OpenEmailAthenticate()
		{
			if (IsBusy)
				return;
			if (_isAuthenticated)
			{
				await Shell.Current.GoToAsync(nameof(ThermostatPage));
				return;
			}
			var popup = _serviceProvider.GetRequiredService<EmailAuthenticatePopup>();
			var result = await Application.Current.MainPage.ShowPopupAsync(popup);
			_isAuthenticated = (bool)result;
		}


        async Task GetDatapvpcsAsync()
        {
			if (IsBusy)
				return;
            try
            {
				IsBusy = true;

				var data = await _pvpcService.GetDatapvpcs();

				if (data.TryGetValue("AM", out List<Datapvpc> dataAM))
				{
					DatapvpcAM.Clear();
					foreach (var datapvpcam in dataAM)
					{
						DatapvpcAM.Add(datapvpcam);
					}
				}

				if (data.TryGetValue("PM", out List<Datapvpc> dataPM))
				{
					DatapvpcPM.Clear();
					foreach (var datapvpcpm in dataPM)
					{
						DatapvpcPM.Add(datapvpcpm);
					}
				}

			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				await Shell.Current.DisplayAlert("Error", $"Error al obtener los datos: {ex.Message}", "OK");
			} 
			finally
			{
				IsBusy = false;
			}
        }

		private void InitializeTimer()
		{
			var now = DateTime.Now;
			var midnightTonight = now.Date.AddDays(1);  
			var initialInterval = midnightTonight - now; 

			var dueTime = Convert.ToInt32(initialInterval.TotalMilliseconds);
			var period = 86400000; 

			timer = new Timer(ExecutePeriodicTask, null, dueTime, period);

		}

		private void ExecutePeriodicTask(object state)
		{
			Task.Run(async () => await GetDatapvpcsAsync()).ConfigureAwait(false);
		}

	}
}
