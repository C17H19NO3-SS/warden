using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Menu;
using System.Drawing;
using System;
using JailBreak.Helpers;
using JailBreak.Models;

namespace JailBreak.Services;

/// <summary>
/// Manages the placement and customization of visual markers on the map for the Warden.
/// </summary>
public class MarkerService : IMarkerService
{
    private readonly JailBreakPlugin _plugin;
    private readonly IWardenService _wardenService;

    private MarkerConfig _currentMarkerConfig = new MarkerConfig();
    private readonly List<uint> _activeBeams = new();
    private float _hue = 0;
    private CounterStrikeSharp.API.Modules.Timers.Timer? _rgbTimer = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkerService"/> class.
    /// </summary>
    /// <param name="plugin">The main plugin instance.</param>
    /// <param name="wardenService">The warden service instance.</param>
    public MarkerService(JailBreakPlugin plugin, IWardenService wardenService)
    {
        _plugin = plugin;
        _wardenService = wardenService;
    }

    /// <summary>
    /// Clears any existing markers at the start of a new round.
    /// </summary>
    public void OnRoundStart()
    {
        ClearMarker();
    }

    public void RegisterCommands()
    {
        _plugin.RegisterCommand("css_marker", "İşaretleyici menüsünü açar", OpenMarkerMenu);
    }

    /// <summary>
    /// Opens the marker configuration menu for the Warden.
    /// </summary>
    /// <param name="player">The player opening the menu.</param>
    /// <param name="info">Command information.</param>
    public void OpenMarkerMenu(CCSPlayerController? player, CommandInfo? info = null)
    {
        if (player == null || !player.IsValid) return;
        if (!_wardenService.HasPermission(player, "@css/chat"))
        {
            player.PrintToChat("Yetkiniz yok.");
            return;
        }

        var menu = new ChatMenu("İşaretleyici Ayarları");

        menu.AddMenuOption("Şekil Ayarla", (p, option) => OpenShapeMenu(p));
        menu.AddMenuOption("Renk Ayarla", (p, option) => OpenColorMenu(p));
        menu.AddMenuOption("Boyut Ayarla", (p, option) => OpenSizeMenu(p));
        menu.AddMenuOption("Efekt Ayarla", (p, option) => OpenEffectMenu(p));
        menu.AddMenuOption("⬅ Geri", (p, option) => _plugin.WardenService.OpenMainMenu(p));

        MenuManager.OpenChatMenu(player, menu);
    }

    private void OpenShapeMenu(CCSPlayerController player)
    {
        var menu = new ChatMenu("Şekil Seç");
        foreach (MarkerShape shape in Enum.GetValues(typeof(MarkerShape)))
        {
            menu.AddMenuOption(shape.ToString(), (p, option) =>
            {
                _currentMarkerConfig.Shape = shape;
                p.PrintToChat($"İşaretleyici şekli: {shape}");
            });
        }
        menu.AddMenuOption("⬅ Geri", (p, option) => OpenMarkerMenu(p, null));
        MenuManager.OpenChatMenu(player, menu);
    }

    private void OpenColorMenu(CCSPlayerController player)
    {
        var menu = new ChatMenu("Renk Seç");
        menu.AddMenuOption("Kırmızı", (p, option) => { _currentMarkerConfig.Color = Color.Red; p.PrintToChat("Renk: Kırmızı"); });
        menu.AddMenuOption("Mavi", (p, option) => { _currentMarkerConfig.Color = Color.Blue; p.PrintToChat("Renk: Mavi"); });
        menu.AddMenuOption("Yeşil", (p, option) => { _currentMarkerConfig.Color = Color.Green; p.PrintToChat("Renk: Yeşil"); });
        menu.AddMenuOption("⬅ Geri", (p, option) => OpenMarkerMenu(p, null));
        MenuManager.OpenChatMenu(player, menu);
    }

    private void OpenSizeMenu(CCSPlayerController player)
    {
        var menu = new ChatMenu("Boyut Seç");
        menu.AddMenuOption("Küçük (50)", (p, option) => { _currentMarkerConfig.Size = 50.0f; p.PrintToChat("Boyut: Küçük"); });
        menu.AddMenuOption("Orta (100)", (p, option) => { _currentMarkerConfig.Size = 100.0f; p.PrintToChat("Boyut: Orta"); });
        menu.AddMenuOption("Büyük (150)", (p, option) => { _currentMarkerConfig.Size = 150.0f; p.PrintToChat("Boyut: Büyük"); });
        menu.AddMenuOption("⬅ Geri", (p, option) => OpenMarkerMenu(p, null));
        MenuManager.OpenChatMenu(player, menu);
    }

    private void OpenEffectMenu(CCSPlayerController player)
    {
        var menu = new ChatMenu("Efekt Seç");
        foreach (MarkerEffect effect in Enum.GetValues(typeof(MarkerEffect)))
        {
            menu.AddMenuOption(effect.ToString(), (p, option) =>
            {
                _currentMarkerConfig.Effect = effect;
                p.PrintToChat($"Efekt: {effect}");
            });
        }
        menu.AddMenuOption("⬅ Geri", (p, option) => OpenMarkerMenu(p, null));
        MenuManager.OpenChatMenu(player, menu);
    }

    /// <summary>
    /// Renders a marker based on the provided configuration.
    /// </summary>
    /// <param name="config">The configuration defining the marker's shape, size, color, and position.</param>
    public void DrawMarker(MarkerConfig config)
    {
        ClearMarker();
        _currentMarkerConfig = config;

        switch (config.Shape)
        {
            case MarkerShape.Circle:
                DrawCircleMarker(config);
                break;
            case MarkerShape.Cube:
                DrawCubeMarker(config);
                break;
            case MarkerShape.Triangle:
                DrawTriangleMarker(config);
                break;
        }
    }

    private void DrawCircleMarker(MarkerConfig config)
    {
        int segments = 36;
        float step = (float)(2.0f * Math.PI / segments);
        float markerSize = config.Size;

        for (int i = 0; i < segments; i++)
        {
            float angleOld = i * step;
            float angleCur = (i + 1) * step;

            Vector start = new Vector(
                (float)(config.Position.X + (markerSize * Math.Cos(angleOld))),
                (float)(config.Position.Y + (markerSize * Math.Sin(angleOld))),
                config.Position.Z + 5.0f
            );

            Vector end = new Vector(
                (float)(config.Position.X + (markerSize * Math.Cos(angleCur))),
                (float)(config.Position.Y + (markerSize * Math.Sin(angleCur))),
                config.Position.Z + 5.0f
            );

            CEnvBeam? beam = CreateBeam(start, end, config.Color, config.Size / 30);
            if (beam != null) _activeBeams.Add(beam.Index);
        }

        if (config.Effect == MarkerEffect.RGB)
        {
            StartRgbEffect();
        }
    }

    private void DrawCubeMarker(MarkerConfig config)
    {
        Vector pos = config.Position;
        float s = config.Size / 2;

        Vector[] vertices = new Vector[]
        {
            new Vector(pos.X - s, pos.Y - s, pos.Z + 5.0f),
            new Vector(pos.X + s, pos.Y - s, pos.Z + 5.0f),
            new Vector(pos.X + s, pos.Y + s, pos.Z + 5.0f),
            new Vector(pos.X - s, pos.Y + s, pos.Z + 5.0f),
            new Vector(pos.X - s, pos.Y - s, pos.Z + 5.0f + config.Size),
            new Vector(pos.X + s, pos.Y - s, pos.Z + 5.0f + config.Size),
            new Vector(pos.X + s, pos.Y + s, pos.Z + 5.0f + config.Size),
            new Vector(pos.X - s, pos.Y + s, pos.Z + 5.0f + config.Size)
        };

        (int, int)[] edges = new (int, int)[]
        {
            (0, 1), (1, 2), (2, 3), (3, 0),
            (4, 5), (5, 6), (6, 7), (7, 4),
            (0, 4), (1, 5), (2, 6), (3, 7)
        };

        foreach (var edge in edges)
        {
            CEnvBeam? beam = CreateBeam(vertices[edge.Item1], vertices[edge.Item2], config.Color, config.Size / 30);
            if (beam != null) _activeBeams.Add(beam.Index);
        }

        if (config.Effect == MarkerEffect.RGB)
        {
            StartRgbEffect();
        }
    }

    private void DrawTriangleMarker(MarkerConfig config)
    {
        float size = config.Size;
        Vector pos = config.Position;
        Vector top = new Vector(pos.X, pos.Y, pos.Z + 5.0f + size);

        Vector p1 = new Vector(pos.X, pos.Y + size * (float)Math.Sqrt(3) / 3, pos.Z + 5.0f);
        Vector p2 = new Vector(pos.X - size / 2, pos.Y - size * (float)Math.Sqrt(3) / 6, pos.Z + 5.0f);
        Vector p3 = new Vector(pos.X + size / 2, pos.Y - size * (float)Math.Sqrt(3) / 6, pos.Z + 5.0f);

        CEnvBeam? beam1 = CreateBeam(p1, p2, config.Color, config.Size / 30);
        CEnvBeam? beam2 = CreateBeam(p2, p3, config.Color, config.Size / 30);
        CEnvBeam? beam3 = CreateBeam(p3, p1, config.Color, config.Size / 30);
        CEnvBeam? beam4 = CreateBeam(p1, top, config.Color, config.Size / 30);
        CEnvBeam? beam5 = CreateBeam(p2, top, config.Color, config.Size / 30);
        CEnvBeam? beam6 = CreateBeam(p3, top, config.Color, config.Size / 30);

        if (beam1 != null) _activeBeams.Add(beam1.Index);
        if (beam2 != null) _activeBeams.Add(beam2.Index);
        if (beam3 != null) _activeBeams.Add(beam3.Index);
        if (beam4 != null) _activeBeams.Add(beam4.Index);
        if (beam5 != null) _activeBeams.Add(beam5.Index);
        if (beam6 != null) _activeBeams.Add(beam6.Index);

        if (config.Effect == MarkerEffect.RGB)
        {
            StartRgbEffect();
        }
    }

    private CEnvBeam? CreateBeam(Vector start, Vector end, Color color, float width)
    {
        CEnvBeam? beam = Utilities.CreateEntityByName<CEnvBeam>("env_beam");
        if (beam != null)
        {
            beam.Render = color;
            beam.Width = width;
            beam.Teleport(start, new QAngle(0, 0, 0), new Vector(0, 0, 0));
            beam.EndPos.X = end.X;
            beam.EndPos.Y = end.Y;
            beam.EndPos.Z = end.Z;
            Utilities.SetStateChanged(beam, "CBeam", "m_vecEndPos");
            beam.DispatchSpawn();
        }
        return beam;
    }

    /// <summary>
    /// Handles player pings to set the marker position.
    /// </summary>
    /// <param name="event">The ping event data.</param>
    /// <param name="player">The player who pinged.</param>
    public void OnPlayerPing(EventPlayerPing @event, CCSPlayerController player)
    {
        if (!_wardenService.HasPermission(player, "@css/chat"))
            return;

        Vector position = new Vector(@event.X, @event.Y, @event.Z);
        _currentMarkerConfig.Position = position;
        DrawMarker(_currentMarkerConfig);
    }

    private void StartRgbEffect()
    {
        if (_rgbTimer == null)
        {
            _hue = 0;
            _rgbTimer = _plugin.AddTimer(0.1f, UpdateRgbEffect, CounterStrikeSharp.API.Modules.Timers.TimerFlags.REPEAT);
        }
    }

    private void UpdateRgbEffect()
    {
        if (_activeBeams.Count == 0)
        {
            _rgbTimer?.Kill();
            _rgbTimer = null;
            return;
        }

        _hue += 10.0f;
        if (_hue >= 360.0f) _hue -= 360.0f;

        Color currentColor = ColorFromHSV(_hue, 1.0f, 1.0f);

        for (int i = _activeBeams.Count - 1; i >= 0; i--)
        {
            uint index = _activeBeams[i];
            CEnvBeam? beam = Utilities.GetEntityFromIndex<CEnvBeam>((int)index);

            if (beam != null && beam.IsValid && beam.DesignerName == "env_beam")
            {
                beam.Render = currentColor;
                Utilities.SetStateChanged(beam, "CBaseModelEntity", "m_clrRender");
            }
            else
            {
                _activeBeams.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Removes all active marker entities from the map.
    /// </summary>
    public void ClearMarker()
    {
        foreach (uint index in _activeBeams)
        {
            CBaseEntity? ent = Utilities.GetEntityFromIndex<CBaseEntity>((int)index);
            if (ent != null && ent.IsValid && ent.DesignerName == "env_beam")
            {
                ent.Remove();
            }
        }
        _activeBeams.Clear();

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
