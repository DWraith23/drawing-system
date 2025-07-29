using DrawingSystem.Scripts;
using Godot;
using System;
using System.Collections.Generic;

namespace DrawingSystem;

[Tool]
public partial class RolloutPanel : Button
{

    private RolloutMenu.RolloutDirection _direction = RolloutMenu.RolloutDirection.Down;
    [Export]
    public RolloutMenu.RolloutDirection Direction
    {
        get => _direction;
        set
        {
            if (_direction == value) return;
            _direction = value;
            Update();
        }
    }

    private int _panelSpacing = 4;
    [Export]
    public int PanelSpacing
    {
        get => _panelSpacing;
        set
        {
            if (_panelSpacing == value) return;
            _panelSpacing = value;
            Update();
        }
    }

    private Node2D Positioner => GetNode<Node2D>("Child Positioner");
    public PanelContainer Panel => Positioner.GetNode<PanelContainer>("Panel");

    public override void _Ready()
    {
        Update();
        ConnectSignals();
        // SetEditableInstance(this, true);
    }

    private void Update()
    {
        if (!HasNode("Child Positioner")) return;

        // Set position of Positioner
        var x = Direction == RolloutMenu.RolloutDirection.Right ? Size.X : 0;
        var y = Direction == RolloutMenu.RolloutDirection.Down ? Size.Y : 0;
        Positioner.Position = new Vector2(x, y);

        // Setup Panel
        Panel.Size = Vector2.Zero;
        Panel.Theme = Theme;

        // Move nodes
        foreach (var child in GetChildren())
        {
            if (child == Positioner) continue;
            child.Reparent(Panel);
            if (child is CanvasItem item) item.Show();
        }

        // Set panel position
        Panel.SetAnchorsAndOffsetsPreset(RolloutMenu.RolloutAnchors[Direction]);
        var panelX = Panel.Position.X + (Direction == RolloutMenu.RolloutDirection.Right
            ? PanelSpacing
            : Direction == RolloutMenu.RolloutDirection.Left
                ? -PanelSpacing
                : 0);
        var panelY = Panel.Position.Y + (Direction == RolloutMenu.RolloutDirection.Down
            ? PanelSpacing
            : Direction == RolloutMenu.RolloutDirection.Up
                ? -PanelSpacing
                : 0);
        Panel.Position = new Vector2(panelX, panelY);
    }

    private void ConnectSignals()
    {
        Toggled += OnToggled;
    }

    private void OnToggled(bool toggledOn)
    {
        Panel.Visible = toggledOn;
    }
}
