using CommunityToolkit.Maui.Views;
using SmartSave.ViewModel.PopUps;

namespace SmartSave.View.PopUps;

public partial class AutomaticModePopup : Popup
{
	public AutomaticModePopup(AutomaticModeViewModel automaticModeViewModel)
	{
		InitializeComponent();
		BindingContext = automaticModeViewModel;
	}
}