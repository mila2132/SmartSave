using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSave.ViewModel
{
	public partial class ThermostatViewModel : BaseViewModel
	{
		public ThermostatViewModel()
		{
			Title = "Termostato";
		}

		[ObservableProperty]
		[NotifyPropertyChangedFor(nameof(IsAutomatic))]
		bool isManual;

		public bool IsAutomatic => !IsManual;

	}

}
