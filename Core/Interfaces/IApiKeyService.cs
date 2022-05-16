namespace RecruitmentApp.Core.Interfaces;

public interface IApiKeyService
{
    string GetKey();

    bool ValidateKey(string key);
}
