using TUnit.Aspire;

namespace IntegrationTests;

public class AppFixture : AspireFixture<Projects.AppHost>
{
    protected override void ConfigureBuilder(IDistributedApplicationTestingBuilder builder) =>
        builder.Services.ConfigureHttpClientDefaults(http =>
            http.AddStandardResilienceHandler());
}
