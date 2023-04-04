using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using CampaignEditor.StartupHelpers;
using Database.Data;
using System.Globalization;
using System.Threading;
using CampaignEditor.UserControls;

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
                    services.AddFormFactory<NewCampaign>();
                    services.AddFormFactory<NewTarget>();
                    services.AddFormFactory<AssignTargets>();
                    services.AddFormFactory<PriceList>();
                    services.AddFormFactory<Sectable>();
                    services.AddFormFactory<Seasonality>();
                    services.AddFormFactory<Channels>();
                    services.AddFormFactory<Spots>();
                    services.AddFormFactory<Goals>();
                    services.AddFormFactory<CmpInfo>();
                    services.AddFormFactory<Rename>();
                    services.AddFormFactory<GroupChannels>();
                    services.AddFormFactory<Campaign>();
                    services.AddFormFactory<CampaignOverview>();
                    services.AddFormFactory<CampaignForecast>();
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
                    Database.Extensions.PricelistExtensions.AddPricelistExtensions(services);
                    Database.Extensions.PricelistChannelsExtensions.AddPricelistChannelsExtensions(services);
                    Database.Extensions.SectableExtensions.AddSectableExtensions(services);
                    Database.Extensions.SectablesExtensions.AddSectablesExtensions(services);
                    Database.Extensions.SectableChannelsExtensions.AddSectableChannelsExtensions(services);
                    Database.Extensions.SeasonalityExtensions.AddSeasonalityExtensions(services);
                    Database.Extensions.SeasonalitiesExtensions.AddSeasonalitiesExtensions(services);
                    Database.Extensions.SeasonalityChannelsExtensions.AddSeasonalityChannelsExtensions(services);
                    Database.Extensions.ActivityExtensions.AddActivityExtensions(services);
                    Database.Extensions.PricesExtensions.AddPricesExtensions(services);
                    Database.Extensions.SpotExtensions.AddSpotExtensions(services);
                    Database.Extensions.TargetCmpExtensions.AddTargetCmpExtensions(services);
                    Database.Extensions.GoalsExtensions.AddGoalsExtensions(services);
                    Database.Extensions.ChannelCmpExtensions.AddChannelCmpExtensions(services);
                    Database.Extensions.ChannelGroupExtensions.AddChannelGroupExtensions(services);
                    Database.Extensions.ChannelGroupsExtensions.AddChannelGroupsExtensions(services);
                    Database.Extensions.SchemaExtensions.AddSchemaExtensions(services);
                    Database.Extensions.MediaPlanExtensions.AddMediaPlanExtensions(services);
                    Database.Extensions.MediaPlanHistExtensions.AddMediaPlanHistExtensions(services);

                }).Build();

            // For displaying time format
            CultureInfo ci = CultureInfo.CreateSpecificCulture(CultureInfo.CurrentCulture.Name);
            ci.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
            Thread.CurrentThread.CurrentCulture = ci;
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
