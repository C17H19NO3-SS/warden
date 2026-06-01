namespace JailBreak.Services;

public interface IHudManager
{
    void SetHudText(string text, float duration);
    void ClearHud();
    void Update();
}
