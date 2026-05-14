using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using System.Drawing;

namespace JailBreak.Services;

public class MarkerService
{
    private readonly JailBreakPlugin _plugin;
    private readonly WardenService _wardenService;

    private float _markerSize = 80.0f;
    private readonly List<uint> _beamIndices = new();
    private CounterStrikeSharp.API.Modules.Timers.Timer? _rgbTimer;
    private float _hue = 0;

    public MarkerService(JailBreakPlugin plugin, WardenService wardenService)
    {
        _plugin = plugin;
        _wardenService = wardenService;
    }

    public void OnRoundStart()
    {
        ClearMarker();
    }

    public void CommandMarker(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (!_plugin.IsJailbreakMap())
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgOnlyJailbreakMap)}");
            return;
        }

        if (!_wardenService.IsWarden(player) && !_wardenService.IsWardenAdmin(player))
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgOnlyWardenAdminOrRootCanUse)}");
            return;
        }

        string arg = info.GetArg(1);
        if (string.IsNullOrEmpty(arg))
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgMarkerUsage)}");
            return;
        }

        if (float.TryParse(arg, out float size))
        {
            if (size < 1.0f) size = 1.0f;
            if (size > 250.0f) size = 250.0f;

            _markerSize = size;
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(string.Format(_plugin.Config.MsgMarkerSizeSet, size))}");
        }
        else
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgInvalidNumber)}");
        }
    }

    public void OnPlayerPing(EventPlayerPing @event, CCSPlayerController player)
    {
        if (!_wardenService.IsWarden(player) && !_wardenService.IsWardenAdmin(player))
            return;

        // X, Y, Z from event
        Vector position = new Vector(@event.X, @event.Y, @event.Z);
        DrawMarker(position);
    }

    public void DrawMarker(Vector position)
    {
        ClearMarker();

        int segments = 36; // Number of beams to form the circle
        float step = (float)(2.0f * Math.PI / segments);

        for (int i = 0; i < segments; i++)
        {
            float angleOld = i * step;
            float angleCur = (i + 1) * step;

            Vector start = new Vector(
                (float)(position.X + (_markerSize * Math.Cos(angleOld))),
                (float)(position.Y + (_markerSize * Math.Sin(angleOld))),
                position.Z + 5.0f
            );

            Vector end = new Vector(
                (float)(position.X + (_markerSize * Math.Cos(angleCur))),
                (float)(position.Y + (_markerSize * Math.Sin(angleCur))),
                position.Z + 5.0f
            );

            CEnvBeam? laser = Utilities.CreateEntityByName<CEnvBeam>("env_beam");
            if (laser != null)
            {
                laser.Render = Color.Red;
                laser.Width = 2.5f;
                laser.Teleport(start, new QAngle(0, 0, 0), new Vector(0, 0, 0));
                laser.EndPos.X = end.X;
                laser.EndPos.Y = end.Y;
                laser.EndPos.Z = end.Z;
                Utilities.SetStateChanged(laser, "CBeam", "m_vecEndPos");

                laser.DispatchSpawn();
                _beamIndices.Add(laser.Index);
            }
        }

        if (_rgbTimer == null)
        {
            _hue = 0;
            _rgbTimer = _plugin.AddTimer(0.1f, UpdateRgbEffect, CounterStrikeSharp.API.Modules.Timers.TimerFlags.REPEAT);
        }
    }

    private void UpdateRgbEffect()
    {
        if (_beamIndices.Count == 0)
        {
            _rgbTimer?.Kill();
            _rgbTimer = null;
            return;
        }

        _hue += 10.0f;
        if (_hue >= 360.0f) _hue -= 360.0f;

        Color currentColor = ColorFromHSV(_hue, 1.0f, 1.0f);

        // Update all beams
        for (int i = _beamIndices.Count - 1; i >= 0; i--)
        {
            uint index = _beamIndices[i];

            // Try casting to int if needed by checking compatibility, Utilities.GetEntityFromIndex takes int or uint depending on version. 
            // In CS# net8.0 it usually takes int or uint. We'll cast to int to be safe if it's an older build, but since index is uint, we can try cast.
            CEnvBeam? laser = Utilities.GetEntityFromIndex<CEnvBeam>((int)index);

            if (laser != null && laser.IsValid && laser.DesignerName == "env_beam")
            {
                laser.Render = currentColor;
                Utilities.SetStateChanged(laser, "CBaseModelEntity", "m_clrRender");
            }
            else
            {
                _beamIndices.RemoveAt(i);
            }
        }
    }

    public void ClearMarker()
    {
        foreach (uint index in _beamIndices)
        {
            CBaseEntity? ent = Utilities.GetEntityFromIndex<CBaseEntity>((int)index);
            if (ent != null && ent.IsValid && ent.DesignerName == "env_beam")
            {
                ent.Remove();
            }
        }
        _beamIndices.Clear();

        _rgbTimer?.Kill();
        _rgbTimer = null;
    }

    private static Color ColorFromHSV(float hue, float saturation, float value)
    {
        int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        float f = hue / 60 - (float)Math.Floor(hue / 60);

        value = value * 255;
        int v = Convert.ToInt32(value);
        int p = Convert.ToInt32(value * (1 - saturation));
        int q = Convert.ToInt32(value * (1 - f * saturation));
        int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

        if (hi == 0)
            return Color.FromArgb(255, v, t, p);
        else if (hi == 1)
            return Color.FromArgb(255, q, v, p);
        else if (hi == 2)
            return Color.FromArgb(255, p, v, t);
        else if (hi == 3)
            return Color.FromArgb(255, p, q, v);
        else if (hi == 4)
            return Color.FromArgb(255, t, p, v);
        else
            return Color.FromArgb(255, v, p, q);
    }
}
