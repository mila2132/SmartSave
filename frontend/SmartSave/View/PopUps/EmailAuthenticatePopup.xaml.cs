using CommunityToolkit.Maui.Views;
using SmartSave.ViewModel.PopUps;

namespace SmartSave.View.PopUps;

public partial class EmailAuthenticatePopup : Popup
{
	public EmailAuthenticatePopup(EmailAuthenticateViewModel emailAuthenticateViewModel)
	{
		InitializeComponent();
		BindingContext = emailAuthenticateViewModel;
	}
}