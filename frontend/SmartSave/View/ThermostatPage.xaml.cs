using SmartSave.ViewModel;

namespace SmartSave.View;

public partial class ThermostatPage : ContentPage
{
	public ThermostatPage(ThermostatViewModel thermostatViewModel)
	{
		InitializeComponent();
		BindingContext = thermostatViewModel;
	}
}