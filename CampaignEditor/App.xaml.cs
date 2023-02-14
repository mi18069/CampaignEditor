using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using CampaignEditor.StartupHelpers;
using Database.Data;

namespace CampaignEditor
{
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }


        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<MainWindow>();
                    services.AddFormFactory<UsersAndClients>();
                    services.AddFormFactory<AssignUser>();
                    services.AddFormFactory<UsersOfClient>();
                    services.AddFormFactory<AddUser>();
                    services.AddFormFactory<AddClient>();
                    services.AddFormFactory<Clients>();
                    services.AddFormFactory<ClientsTreeView>();
                    services.AddFormFactory<AddCampaign>();
                    services.AddFormFactory<NewCampaign>();
                    services.AddFormFactory<NewTarget>();
                    services.AddFormFactory<AssignTargets>();
                    services.AddScoped<IDataContext, DataContext>();
                    // Add services
                    Extensions.UserExtensions.AddUserExtensions(services);
                    Extensions.CampaignExtensions.AddCampaignExtensions(services);
                    Database.Extensions.UserClientsExtensions.AddUserClientsExtensions(services);
                    Database.Extensions.ClientExtensions.AddClientExtensions(services);
                    Database.Extensions.TargetExtensions.AddTargetExtensions(services);
                    Database.Extensions.TargetClassExtensions.AddTargetClassExtensions(services);
                    Database.Extensions.TargetValueExtensions.AddTargetValueExtensions(services);
                    Database.Extensions.ChannelExtensions.AddChannelExtensions(services);

                }).Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            var startupForm = AppHost.Services.GetRequiredService<MainWindow>();
            startupForm.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();
            base.OnExit(e);
        }
    }
}
