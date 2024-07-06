using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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