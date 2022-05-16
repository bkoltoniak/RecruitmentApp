namespace RecruitmentApp.Core.Options
{
    public class CacheOptions
    {
        public const string Section = "Cache";

        public int SlidingExprationSeconds { get; set; }

        public int AbsoluteExprationSeconds { get; set; }
    }
}
