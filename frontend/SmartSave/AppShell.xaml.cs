using SmartSave.View;

namespace SmartSave
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(ThermostatPage), typeof(ThermostatPage));
        }
    }
}
