using RecruitmentApp.Core.Extensions;

namespace RecruitmentApp.WebApi.ApiModels;

public class GetRatesRequestModel : IEquatable<GetRatesRequestModel>
{
    public Dictionary<string, string> currencyCodes { get; set; }
    public DateTime startDate { get; set; }
    public DateTime endDate { get; set; }

    public GetRatesRequestModel()
    {
        currencyCodes = new Dictionary<string, string>();
    }

    public bool Equals(GetRatesRequestModel? other)
    {
        if(other is null)
        {
            return false;
        }

        if (startDate != other.startDate || endDate != other.endDate)
        {
            return false;
        }

        bool equal = false;
        if (currencyCodes.Count == other.currencyCodes.Count)
        {
            equal = true;
            foreach (var pair in currencyCodes)
            {
                if (other.currencyCodes.TryGetValue(pair.Key, out var value))
                {
                    if (value != pair.Value)
                    {
                        equal = false;
                        break;
                    }
                }
                else
                {
                    equal = false;
                    break;
                }
            }
        }

        return equal;
    }

    public override bool Equals(object? obj)
    {
        var other = obj as GetRatesRequestModel;

        if(other is null)
        {
            return false;
        }

        return Equals(other);
    }

    public override int GetHashCode()
    {
        return startDate.ToUnixTimestamp() ^ endDate.ToUnixTimestamp();
    }
}


