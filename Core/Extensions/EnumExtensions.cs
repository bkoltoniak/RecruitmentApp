namespace RecruitmentApp.Core.Extensions;

public static class EnumExtensions
{
    public static TEnum? GetEnumValueOrDefault<TEnum>(this TEnum enumeration, string value) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return default(TEnum?);
        }

        var result = Enum.TryParse<TEnum>(value, true, out var enumerationValue);

        if (!result)
        {
            return default(TEnum?);

        }

        return (TEnum?)enumerationValue;
    }
}
