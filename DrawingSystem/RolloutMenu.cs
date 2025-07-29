using DrawingSystem.Scripts;
using Godot;
using System;
using System.Collections.Generic;

namespace DrawingSystem;

public partial class RolloutMenu : Button
{
    public enum RolloutDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public static Dictionary<RolloutDirection, LayoutPreset> RolloutAnchors => new()
    {
        { RolloutDirection.Up, LayoutPreset.BottomLeft },
        { RolloutDirection.Down, LayoutPreset.TopLeft },
        { RolloutDirection.Left, LayoutPreset.TopRight },
        { RolloutDirection.Right, LayoutPreset.TopLeft }
    };

    [Signal] public delegate void OnButtonPressedEventHandler(int index);
    [Signal] public delegate void OnButtonToggledEventHandler(int index, bool toggledOn);

    private string[]? _buttonNames;
    [Export]
    public string[]? ButtonNames
    {
        get => _buttonNames;
        set
        {
            if (_buttonNames == value) return;
            _buttonNames = value;
            Update();
        }
    }

    private RolloutDirection _direction = RolloutDirection.Down;
    [Export]
    public RolloutDirection Direction
    {
        get => _direction;
        set
        {
            if (_direction == value) return;
            _direction = value;
            Update();
        }
    }
    [Export] public bool CloseOnSelect { get; set; } = true;
    [Export] public bool ButtonsToggleable { get; set; } = false;
    private bool _buttonsGrouped = false;
    [Export]
    public bool ButtonsGrouped
    {
        get => _buttonsGrouped;
        set
        {
            if (_buttonsGrouped == value) return;
            _buttonsGrouped = value;
            if (value)
            {
                Group = new();
            }
            else
            {
                Group = null;
            }
            Update();
        }
    }
    [Export] public int ButtonSpacing { get; set; } = 4;
    [ExportGroup("Styles")]
    [Export] public StyleBox? NormalStyle { get; set; }
    [Export] public StyleBox? PressedStyle { get; set; }
    [Export] public StyleBox? HoveredStyle { get; set; }
    [Export] public StyleBox? DisabledStyle { get; set; }



    
    public BoxContainer? Container { get; set; }
    public List<Button> Buttons { get; set; } = [];

    private ButtonGroup? Group { get; set; }

    private Node2D Positioner => GetNode<Node2D>("Child Positioner");
    public PanelContainer Panel => Positioner.GetNode<PanelContainer>("Panel");

    public override void _Ready()
    {
        Update();
        ConnectSignals();
    }

    private void Update()
    {
        if (!HasNode("Child Positioner")) return;

        // Set position of Positioner
        var x = Direction == RolloutDirection.Right ? Size.X : 0;
        var y = Direction == RolloutDirection.Down ? Size.Y : 0;
        Positioner.Position = new Vector2(x, y);

        // Setup Panel
        Panel.Size = Vector2.Zero;
        Panel.Theme = Theme;

        // Setup container
        Container?.QueueFree();
        Container = (Direction == RolloutDirection.Up || Direction == RolloutDirection.Down) ? new VBoxContainer() : new HBoxContainer();
        Panel.AddChild(Container);
        Container.Alignment = (Direction == RolloutDirection.Up || Direction == RolloutDirection.Left)
            ? BoxContainer.AlignmentMode.End
            : BoxContainer.AlignmentMode.Begin;
        Container.AddThemeConstantOverride("separation", ButtonSpacing);

        // Setup buttons
        Buttons.Clear();
        if (ButtonNames == null || ButtonNames.Length == 0) return;
        foreach (var button in ButtonNames)
        {
            AddButton(button);
        }

        // Set panel position
        Panel.SetAnchorsAndOffsetsPreset(RolloutAnchors[Direction]);
        var panelX = Panel.Position.X + (Direction == RolloutDirection.Right
            ? ButtonSpacing
            : Direction == RolloutDirection.Left
                ? -ButtonSpacing
                : 0);
        var panelY = Panel.Position.Y + (Direction == RolloutDirection.Down
            ? ButtonSpacing
            : Direction == RolloutDirection.Up
                ? -ButtonSpacing
                : 0);
        Panel.Position = new Vector2(panelX, panelY);
    }

    private void ConnectSignals()
    {
        Toggled += OnToggled;
    }

    private void AddButton(string name)
    {
        if (Container == null) return;

        var button = new Button()
        {
            Text = name,
            ToggleMode = ButtonsToggleable,
            ButtonGroup = Group,
        };

        // Events
        var count = Container.GetChildCount();
        button.Pressed += () => OnGeneratedButtonPressed(count);
        button.Toggled += (toggledOn) => OnGeneratedButtonToggled(count, toggledOn);

        // Styles
        if (NormalStyle != null) button.AddThemeStyleboxOverride("normal", NormalStyle);
        if (PressedStyle != null)
        {
            button.AddThemeStyleboxOverride("pressed", PressedStyle);
            var hovered = PressedStyle.Duplicate(true) as StyleBoxFlat;
            hovered!.ShadowColor = Colors.Black;
            hovered!.ShadowSize = 4;
            hovered!.ShadowOffset = new Vector2(2, 2);
            button.AddThemeStyleboxOverride("hover_pressed", hovered);
        }
        if (HoveredStyle != null) button.AddThemeStyleboxOverride("hover", HoveredStyle);
        if (DisabledStyle != null) button.AddThemeStyleboxOverride("disabled", DisabledStyle);
        button.AddThemeStyleboxOverride("focus", new StyleBoxEmpty());

        // Add to container
        Container.AddChild(button);
        Buttons.Add(button);
    }

    #region Events

    private void OnToggled(bool toggledOn)
    {
        Panel.Visible = toggledOn;
    }

    private void OnGeneratedButtonPressed(int index)
    {
        EmitSignal(SignalName.OnButtonPressed, index);
        if (CloseOnSelect)
        {
            Panel.Hide();
            ButtonPressed = false;
        }
    }

    private void OnGeneratedButtonToggled(int index, bool toggledOn)
    {
        EmitSignal(SignalName.OnButtonToggled, index, toggledOn);
        if (CloseOnSelect)
        {
            Panel.Hide();
            ButtonPressed = false;
        }
    }

    #endregion
}
