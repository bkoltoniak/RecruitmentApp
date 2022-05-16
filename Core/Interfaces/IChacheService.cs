namespace RecruitmentApp.Core.Interfaces
{
    public interface IChacheService
    {
        TEntry? GetValueOrDefault<TEntry>(object key);
        void Set(object key, object value);
    }
}