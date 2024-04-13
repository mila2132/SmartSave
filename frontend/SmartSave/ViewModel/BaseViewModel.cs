using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSave.ViewModel
{
	public partial class BaseViewModel : ObservableObject
	{
		public BaseViewModel() 
		{
					
		}

		[ObservableProperty]
		[NotifyPropertyChangedFor(nameof(IsNotBusy))]
		bool isBusy;

		[ObservableProperty]
		string title = string.Empty;

		public bool IsNotBusy => !IsBusy;
	}
}
