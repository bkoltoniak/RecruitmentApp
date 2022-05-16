namespace RecruitmentApp.Core.Options;

public class ApiKeyOptions
{
    public const string Section = "ApiKey";

    public string? Secret { get; set; }

    public int ExpireMinutes { get; set; }
}
