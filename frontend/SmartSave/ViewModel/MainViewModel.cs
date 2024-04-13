using SmartSave.Model;
using SmartSave.Services;

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
        async Task GetDatapvpcsAsync()
        {
			if (IsBusy)
				return;
            try
            {
				IsBusy = true;

				var data = await PvpcService.GetDatapvpcs();
				if (DatapvpcAM.Count != 0 && DatapvpcPM.Count != 0)
				{
					DatapvpcAM.Clear();
					DatapvpcPM.Clear();
				}

				data.TryGetValue("AM", out List<Datapvpc> dataAM);
				foreach (var datapvpcam in dataAM)
				{
					DatapvpcAM.Add(datapvpcam);
				}

				data.TryGetValue("PM", out List<Datapvpc> dataPM);
				foreach (var time in dataPM)
				{
					DatapvpcPM.Add(time);
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
			/*
			var now = DateTime.Now;
			var midnightTonight = now.Date.AddDays(1);
			var targetTimeTonight = midnightTonight.AddMinutes(-1);  //23:59
			var dueTime = targetTimeTonight - now;
			timer = new Timer(ExecutePeriodicTask, null, dueTime, TimeSpan.FromDays(1));
			*/

			// crear un timer que se ejecute cada 5 minutos
			timer = new Timer(ExecutePeriodicTask, null, TimeSpan.Zero, TimeSpan.FromMinutes(2));
		}

		private void ExecutePeriodicTask(object state)
		{
			GetDatapvpcsAsync();
		}

	}
}
