namespace Remp.API.Services.Media;

public class AzureBlobStorageSettings
{
    public string ServiceUri { get; set; } = "";
    public string ContainerName { get; set; } = "recam-media";
    public int ReadUrlLifetimeMinutes { get; set; } = 15;
}
