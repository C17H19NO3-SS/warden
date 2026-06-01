namespace JailBreak.Services;

public interface ILangProvider
{
    string GetMessage(string key, params object[] args);
}
