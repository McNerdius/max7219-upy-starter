
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup( typeof( Karmatach.MaxPlay.Startup ) )]
namespace Karmatach.MaxPlay
{
    public class Startup : FunctionsStartup
    {
        public override void Configure( IFunctionsHostBuilder builder )
        {
            builder.Services.AddHttpClient();
        }

        public override void ConfigureAppConfiguration( IFunctionsConfigurationBuilder builder )
        {
            FunctionsHostBuilderContext context = builder.GetContext();

            builder.ConfigurationBuilder
                .AddEnvironmentVariables()
                .SetBasePath( context.ApplicationRootPath )
                .AddJsonFile( "local.settings.json", optional: true, reloadOnChange: true );
        }
    }
}