using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace JailBreak.Helpers;

public static class PluginHelper
{
    public const string ColorTitle = "gold";
    public const string ColorTime = "red";
    public const string ColorSuccess = "green";
    public const string ColorInstruction = "#C0C0C0";
    public const string ColorSystem = "cyan";

    public static string FormatHud(string title, string content, string instruction = "")
    {
        string hud = $"<font color='{ColorTitle}' size='20'><b>--- {title.ToUpper()} ---</b></font><br>{content}";
        if (!string.IsNullOrEmpty(instruction))
        {
            hud += $"<br><font color='{ColorInstruction}' size='14'>{instruction}</font>";
        }
        return hud;
    }

    public static string ReplaceColors(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        return input.Replace("{Default}", $"{ChatColors.Default}")
                    .Replace("{White}", $"{ChatColors.White}")
                    .Replace("{DarkRed}", $"{ChatColors.DarkRed}")
                    .Replace("{Purple}", $"{ChatColors.Purple}")
                    .Replace("{Green}", $"{ChatColors.Green}")
                    .Replace("{LightYellow}", $"{ChatColors.LightYellow}")
                    .Replace("{LightBlue}", $"{ChatColors.LightBlue}")
                    .Replace("{Olive}", $"{ChatColors.Olive}")
                    .Replace("{Lime}", $"{ChatColors.Lime}")
                    .Replace("{Red}", $"{ChatColors.Red}")
                    .Replace("{LightPurple}", $"{ChatColors.LightPurple}")
                    .Replace("{Gray}", $"{ChatColors.Grey}")
                    .Replace("{Grey}", $"{ChatColors.Grey}")
                    .Replace("{Yellow}", $"{ChatColors.Yellow}")
                    .Replace("{Gold}", $"{ChatColors.Gold}")
                    .Replace("{Silver}", $"{ChatColors.Silver}")
                    .Replace("{Blue}", $"{ChatColors.Blue}")
                    .Replace("{DarkBlue}", $"{ChatColors.DarkBlue}")
                    .Replace("{BlueGrey}", $"{ChatColors.BlueGrey}")
                    .Replace("{Magenta}", $"{ChatColors.Magenta}")
                    .Replace("{LightRed}", $"{ChatColors.LightRed}")
                    .Replace("{Orange}", $"{ChatColors.Orange}");
    }

    public static string FormatChat(string prefix, string messageTemplate, params object[] args)
    {
        string formattedTemplate = ReplaceColors(messageTemplate);
        string message = args.Length > 0 ? string.Format(formattedTemplate, args) : formattedTemplate;
        return $" {ReplaceColors(prefix)} {message}";
    }

    /// <summary>
    /// Sends a response message to the command caller (player or console).
    /// </summary>
    /// <param name="player">The player initiating the command, or null if console.</param>
    /// <param name="prefix">The plugin prefix.</param>
    /// <param name="message">The message to send.</param>
    public static void ReplyToCommand(CCSPlayerController? player, string prefix, string messageTemplate, params object[] args)
    {
        if (player != null && player.IsValid)
        {
            player.PrintToChat(FormatChat(prefix, messageTemplate, args));
        }
        else
        {
            string message = args.Length > 0 ? string.Format(messageTemplate, args) : messageTemplate;
            Server.PrintToConsole($"[JailBreak] {message.Replace("{Default}", "").Replace("{Green}", "").Replace("{Red}", "")}");
        }
    }
}
