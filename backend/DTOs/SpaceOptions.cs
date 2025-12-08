namespace SpaceApp.Options;

public class SpaceOptions
{
    public string ApodUrl { get; set; } = null!;
    public string NeoUrl { get; set; } = null!;
    public string DonkiFLR { get; set; } = null!;
    public string DonkiCME { get; set; } = null!;
    public string SpaceXUrl { get; set; } = null!;
    public string OsdrDatasetUrl { get; set; } = null!;
    public string IssUrl { get; set; } = null!;

    public string NasaApiKey { get; set; } = null!;

    public int ApodInterval { get; set; }
    public int NeoInterval { get; set; }
    public int DonkiInterval { get; set; }
    public int SpaceXInterval { get; set; }
    public int OsdrInterval { get; set; }
    public int IssInterval { get; set; }
}
