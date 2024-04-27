using Amazon.CognitoIdentity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartSave.Services;
using SmartSave.View;
using SmartSave.ViewModel;
using System.Reflection;

namespace SmartSave
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            var getAssembly = Assembly.GetExecutingAssembly();
            using var stream = getAssembly.GetManifestResourceStream("SmartSave.appsettings.json");

            var configuration = new ConfigurationBuilder()
				.AddJsonStream(stream)
				.Build();

			// añadir appsettings.json a la configuración en una aplicación de Maui
			builder.Configuration.AddConfiguration(configuration);

			builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<PvpcService>();
            
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<ThermostatViewModel>();

            builder.Services.AddSingleton<MainPage>();
			builder.Services.AddSingleton<ThermostatPage>();

			return builder.Build();
        }
    }
}
