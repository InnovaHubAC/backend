using Innova.Application.Validations.Idea;

namespace Innova.Application.MappingConfig;

public class MappingConfig
{
    public static void ConfigureMappings()
    {
        // Base configuration settings
        TypeAdapterConfig.GlobalSettings.Default.NameMatchingStrategy(NameMatchingStrategy.Flexible);
        TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);

        // Register all mapping configurations in this assembly
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

        TypeAdapterConfig.GlobalSettings
            .ForType<UpdateIdeaDto, Idea>()
            .Ignore(dest => dest.Attachments!);
    }
}
