using SmartSave.Model;
using SmartSave.Services;
using SmartSave.View;

namespace SmartSave.ViewModel
{
    public partial class MainViewModel : BaseViewModel
    {
		PvpcService PvpcService;

		private Timer timer;

		public ObservableCollection<Datapvpc> DatapvpcAM { get; } = new();
		public ObservableCollection<Datapvpc> DatapvpcPM { get; } = new();

        public MainViewModel(PvpcService pvpcService)
        {
			Title = "Precio Luz hora";
			this.PvpcService = pvpcService;
			InitializeTimer();
			GetDatapvpcsAsync();
		}

		[RelayCommand]
		async Task GoToThermostatPage()
		{
			try
			{
				await Shell.Current.GoToAsync($"{nameof(ThermostatPage)}");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

		}


        async Task GetDatapvpcsAsync()
        {
			if (IsBusy)
				return;
            try
            {
				IsBusy = true;

				var data = await PvpcService.GetDatapvpcs();

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
			Task.Run(async () => await GetDatapvpcsAsync()).Wait();
		}


	}
}
