﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using CampaignEditor.StartupHelpers;
using CampaignEditor.Repositories;
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
                    services.AddFormFactory<AddUser>();
                    services.AddFormFactory<AddClient>();
                    services.AddFormFactory<Clients>();
                    services.AddFormFactory<ClientsTreeView>();
                    services.AddFormFactory<AddCampaign>();
                    services.AddScoped<IDataContext, DataContext>();
                    // services for user
                    Extensions.UserExtensions.AddUserExtensions(services);
                    // services for Campaign
                    Extensions.CampaignExtensions.AddCampaignExtensions(services);
                    Database.Extensions.UserClientsExtensions.AddUserClientsExtensions(services);
                    Database.Extensions.ClientExtensions.AddClientExtensions(services);
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
