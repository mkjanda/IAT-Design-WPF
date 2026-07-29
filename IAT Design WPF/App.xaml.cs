using FluentValidation;
using IAT.Core.Domain;
using IAT.Core.Handlers;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Services;
using IAT.Core.Services.Export;
using IAT.Core.Services.Network;
using IAT.Core.Validation;
using IAT.ViewModels;
using IAT.ViewModels.Controls;
using IAT.Views;
using IAT_Design_WPF.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;
using Velopack;

namespace IAT_Design_WPF
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            /*        // 1. VelopackApp MUST run FIRST — this sets up the locator and is required
                    VelopackApp.Build()
                        .Run();

                    // 2. Velopack update check (the correct modern API)
                    var updateManager = new UpdateManager("https://github.com/mkjanda/IAT-Design-WPF/releases/latest");

                    var updateInfo = await updateManager.CheckForUpdatesAsync();
                    if (updateInfo != null)
                    {
                        // Optional: show a small "Updating..." window here later
                        await updateManager.DownloadUpdatesAsync(updateInfo);

                        // This applies the update and restarts the app cleanly
                        updateManager.ApplyUpdatesAndRestart(updateInfo);
                        return;   // important — stop startup if we're restarting
                    }
            */
            // 3. Dependency Injection setup (the professional way)
            var services = new ServiceCollection();

            services.AddSingleton<TransactionState>();

            services.AddLogging(builder =>
            {
                builder.AddDebug();               // shows up in VS Output → Debug
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<TransactionSuccessHandler>());

            // Register your services here as we build them
            services.AddSingleton<ILocalStorageService, LocalStorageService>();
            services.AddSingleton<IXmlDeserializationService, XmlDeserializationService>();
            services.AddSingleton<IStringResourceService, StringResourceService>();
            services.AddSingleton<IUserNotificationService, UserNotificationService>();
            services.AddSingleton<IWebSocketService, WebSocketService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<ILayoutCalculatorService, LayoutCalculatorService>();
            services.AddSingleton<IImagePackageService, ImagePackageService>();
            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IEmailVerificationService, EmailVerificationService>();
            services.AddSingleton<IGetItemSlidesService, GetItemSlidesService>();
            services.AddSingleton<IResendEmailVerificationService, ResendEmailVerificationService>();
            services.AddSingleton<IResultRetrievalService, ResultRetrievalService>();
            services.AddSingleton<ITestDeploymentService, TestDeploymentService>();
            services.AddSingleton<IBlockExportProcessor, BlockExportProcessor>();
            services.AddSingleton<IStimulusExportProcessor, StimulusExportProcessor>();
            services.AddSingleton<ITestExportService, TestExportService>();
            services.AddSingleton<ITextExportProcessor, TextExportProcessor>();
            services.AddSingleton<IProjectPackageService, ProjectPackageService>();
            services.AddSingleton<IFileManifestBuilder, FileManifestBuilder>();
            services.AddSingleton<IImageGenerationService, ImageGenerationService>();
            services.AddSingleton<IKeyService, KeyService>();
            services.AddSingleton<IValidator<IatTest>, IatTestValidator>();
            services.AddSingleton<IValidator<Block>, BlockValidator>();
            services.AddSingleton<IValidator<Stimulus>, StimulusValidator>();
            services.AddSingleton<IValidator<InstructionScreen>, InstructionScreenValidator>();
            services.AddSingleton<IValidator<Trial>, TrialValidator>();

            // Domain model first — shared singleton used by all designer tabs
            services.AddSingleton<IatTest>();

            services.AddSingleton<LayoutViewModel>();
            services.AddSingleton<BlockEditViewModel>();
            services.AddSingleton<StimuliManagerViewModel>();
            services.AddSingleton<TrialsManagerViewModel>();
            services.AddSingleton<SurveyManagerViewModel>();
            services.AddSingleton<InstructionManagerViewModel>();
            services.AddSingleton<DeployManagerViewModel>();
            services.AddSingleton<TestDesignerViewModel>();

            Services = services.BuildServiceProvider();

            base.OnStartup(e);

            // 4. Launch the main window with MVVM
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
