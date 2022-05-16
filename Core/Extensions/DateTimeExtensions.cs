namespace RecruitmentApp.Core.Extensions;

public static class DateTimeExtensions
{
    public static int ToUnixTimestamp(this DateTime dateTime)
    {
        return (int)dateTime.Subtract(DateTime.UnixEpoch).TotalSeconds;
    }
}
