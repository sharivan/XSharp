using System.Configuration;
using System.Windows.Forms;

using static XSharp.Engine.Consts;

namespace XSharp.Engine;

public sealed class ProgramConfiguratinSection : ConfigurationSection
{
    public ProgramConfiguratinSection()
    {
    }

    [ConfigurationProperty("left",
        DefaultValue = -1,
        IsRequired = false
        )]
    public int Left
    {
        get => (int) this["left"];
        set => this["left"] = value;
    }

    [ConfigurationProperty("top",
        DefaultValue = -1,
        IsRequired = false
        )]
    public int Top
    {
        get => (int) this["top"];

        set => this["top"] = value;
    }

    [ConfigurationProperty("width",
        DefaultValue = -1,
        IsRequired = false
        )]
    public int Width
    {
        get => (int) this["width"];
        set => this["width"] = value;
    }

    [ConfigurationProperty("height",
        DefaultValue = -1,
        IsRequired = false
        )]
    public int Height
    {
        get => (int) this["height"];
        set => this["height"] = value;
    }

    [ConfigurationProperty("drawCollisionBox",
        DefaultValue = DEBUG_DRAW_HITBOX,
        IsRequired = false
        )]
    public bool DrawCollisionBox
    {
        get => (bool) this["drawCollisionBox"];
        set => this["drawCollisionBox"] = value;
    }

    [ConfigurationProperty("showColliders",
        DefaultValue = DEBUG_SHOW_COLLIDERS,
        IsRequired = false
        )]
    public bool ShowColliders
    {
        get => (bool) this["showColliders"];
        set => this["showColliders"] = value;
    }

    [ConfigurationProperty("drawMapBounds",
        DefaultValue = DEBUG_DRAW_MAP_BOUNDS,
        IsRequired = false
        )]
    public bool DrawMapBounds
    {
        get => (bool) this["drawMapBounds"];
        set => this["drawMapBounds"] = value;
    }

    [ConfigurationProperty("drawTouchingMapBounds",
        DefaultValue = DEBUG_HIGHLIGHT_TOUCHING_MAPS,
        IsRequired = false
        )]
    public bool DrawTouchingMapBounds
    {
        get => (bool) this["drawTouchingMapBounds"];
        set => this["drawTouchingMapBounds"] = value;
    }

    [ConfigurationProperty("drawHighlightedPointingTiles",
        DefaultValue = DEBUG_HIGHLIGHT_POINTED_TILES,
        IsRequired = false
        )]
    public bool DrawHighlightedPointingTiles
    {
        get => (bool) this["drawHighlightedPointingTiles"];
        set => this["drawHighlightedPointingTiles"] = value;
    }

    [ConfigurationProperty("drawPlayerOriginAxis",
        DefaultValue = DEBUG_DRAW_PLAYER_ORIGIN_AXIS,
        IsRequired = false
        )]
    public bool DrawPlayerOriginAxis
    {
        get => (bool) this["drawPlayerOriginAxis"];
        set => this["drawPlayerOriginAxis"] = value;
    }

    [ConfigurationProperty("showInfoText",
        DefaultValue = DEBUG_SHOW_INFO_TEXT,
        IsRequired = false
        )]
    public bool ShowInfoText
    {
        get => (bool) this["showInfoText"];
        set => this["showInfoText"] = value;
    }

    [ConfigurationProperty("showCheckpointBounds",
        DefaultValue = DEBUG_DRAW_CHECKPOINT,
        IsRequired = false
        )]
    public bool ShowCheckpointBounds
    {
        get => (bool) this["showCheckpointBounds"];
        set => this["showCheckpointBounds"] = value;
    }

    [ConfigurationProperty("showTriggerBounds",
        DefaultValue = DEBUG_SHOW_TRIGGERS,
        IsRequired = false
        )]
    public bool ShowTriggerBounds
    {
        get => (bool) this["showTriggerBounds"];
        set => this["showTriggerBounds"] = value;
    }

    [ConfigurationProperty("showTriggerCameraLook",
        DefaultValue = DEBUG_SHOW_CAMERA_TRIGGER_EXTENSIONS,
        IsRequired = false
        )]
    public bool ShowTriggerCameraLook
    {
        get => (bool) this["showTriggerCameraLook"];
        set => this["showTriggerCameraLook"] = value;
    }

    [ConfigurationProperty("currentSaveSlot",
        DefaultValue = 0,
        IsRequired = false
        )]
    public int CurrentSaveSlot
    {
        get => (int) this["currentSaveSlot"];
        set => this["currentSaveSlot"] = value;
    }
}

public class GameEngine : BaseEngine
{
    new public static GameEngine Engine => (GameEngine) BaseEngine.Engine;

    protected GameEngine()
    {
    }

    public void LoadConfig()
    {
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        if (config.Sections["ProgramConfiguratinSection"] is not ProgramConfiguratinSection section)
        {
            section = new ProgramConfiguratinSection();
            config.Sections.Add("ProgramConfiguratinSection", section);
            config.Save();
        }

        if (Control is Form)
        {
            if (section.Left != -1)
                Control.Left = section.Left;

            if (section.Top != -1)
                Control.Top = section.Top;
        }

        drawHitbox = section.DrawCollisionBox;
        showColliders = section.ShowColliders;
        drawLevelBounds = section.DrawMapBounds;
        drawTouchingMapBounds = section.DrawTouchingMapBounds;
        drawHighlightedPointingTiles = section.DrawHighlightedPointingTiles;
        drawPlayerOriginAxis = section.DrawPlayerOriginAxis;
        showInfoText = section.ShowInfoText;
        showCheckpointBounds = section.ShowCheckpointBounds;
        showTriggerBounds = section.ShowTriggerBounds;
        showTriggerCameraLockDirection = section.ShowTriggerCameraLook;
        CurrentSaveSlot = section.CurrentSaveSlot;
    }

    public void SaveConfig()
    {
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        if (config.Sections["ProgramConfiguratinSection"] is not ProgramConfiguratinSection section)
        {
            section = new ProgramConfiguratinSection();
            config.Sections.Add("ProgramConfiguratinSection", section);
        }

        if (Control is Form)
        {
            section.Left = Control.Left;
            section.Top = Control.Top;
        }

        section.DrawCollisionBox = drawHitbox;
        section.ShowColliders = showColliders;
        section.DrawMapBounds = drawLevelBounds;
        section.DrawTouchingMapBounds = drawTouchingMapBounds;
        section.DrawHighlightedPointingTiles = drawHighlightedPointingTiles;
        section.DrawPlayerOriginAxis = drawPlayerOriginAxis;
        section.ShowInfoText = showInfoText;
        section.ShowCheckpointBounds = showCheckpointBounds;
        section.ShowTriggerBounds = showTriggerBounds;
        section.ShowTriggerCameraLook = showTriggerCameraLockDirection;
        section.CurrentSaveSlot = CurrentSaveSlot;

        config.Save();
    }
}