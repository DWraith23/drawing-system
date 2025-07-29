using Godot;
using System;
using System.Linq;

namespace DrawingSystem;

public partial class NewDrawingPopup : PopupPanel
{
    private static string ScenePath => "res://DrawingSystem/new_drawing_popup.tscn";
    public static NewDrawingPopup GenerateInstance(Texture2D? image = null)
    {
        var instance = GD.Load<PackedScene>(ScenePath).Instantiate<NewDrawingPopup>();
        instance.Image = image;
        return instance;
    }

    private VBoxContainer Contents => GetNode<VBoxContainer>("Contents");
    private SpinBox CanvasWidth => Contents.GetNode<SpinBox>("Width/SpinBox");
    private SpinBox CanvasHeight => Contents.GetNode<SpinBox>("Height/SpinBox");
    private ColorPickerButton BackgroundColor => Contents.GetNode<ColorPickerButton>("Background Color/ColorPickerButton");
    private Button ConfirmButton => Contents.GetNode<Button>("Buttons/Confirm");
    private Button CancelButton => Contents.GetNode<Button>("Buttons/Cancel");

    private Texture2D? Image { get; set; }

    private int Width => (int)Math.Floor(CanvasWidth.Value);
    private int Height => (int)Math.Floor(CanvasHeight.Value);

    public override void _Ready()
    {
        if (Image != null)
        {
            CanvasWidth.Value = Image.GetWidth();
            CanvasHeight.Value = Image.GetHeight();
            CanvasWidth.MinValue = Image.GetWidth();
            CanvasHeight.MinValue = Image.GetHeight();
        }

        ConnectSignals();
    }

    private void ConnectSignals()
    {
        ConfirmButton.Pressed += OnConfirmButtonPressed;
        CancelButton.Pressed += OnCancelButtonPressed;
    }

    private void OnConfirmButtonPressed()
    {
        var root = GetTree().Root;
        root.GetChildren().ToList().ForEach(child => child.QueueFree());

        root.AddChild(Canvas.GenerateInstance(Width, Height, BackgroundColor.Color, Image));
    }

    private void OnCancelButtonPressed() => QueueFree();

}
