using Godot;
using System;

namespace DrawingSystem;

public partial class StartMenu : Control
{
    private static string ScenePath => "res://DrawingSystem/start_menu.tscn";
    public static StartMenu GenerateInstance()
    {
        var instance = GD.Load<PackedScene>(ScenePath).Instantiate<StartMenu>();
        return instance;
    }

    private VBoxContainer Contents => GetNode<VBoxContainer>("CenterContainer/Panel/Contents");
    private Button NewDrawingButton => Contents.GetNode<Button>("New Drawing");
    private Button LoadPictureButton => Contents.GetNode<Button>("Load Picture");
    private Button ExitButton => Contents.GetNode<Button>("Exit");

    public override void _Ready()
    {
        ConnectSignals();
    }

    private void ConnectSignals()
    {
        NewDrawingButton.Pressed += OnNewDrawingButtonPressed;
        LoadPictureButton.Pressed += OnLoadPicturePressed;
        ExitButton.Pressed += OnExitButtonPressed;
    }


    private void OnNewDrawingButtonPressed()
    {
        var popup = NewDrawingPopup.GenerateInstance();
        AddChild(popup);
        popup.Popup();
    }

    private void OnLoadPicturePressed()
    {
        var dialog = new FileDialog()
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Access = FileDialog.AccessEnum.Filesystem,
            Filters = ["*.png", "*.jpg", "*.jpeg"],
            Title = "Load Picture",
            InitialPosition = Window.WindowInitialPosition.CenterMainWindowScreen,
            Size = new Vector2I(500, 400),
        };
        dialog.FileSelected += OnPictureSelected;
        AddChild(dialog);
        dialog.Popup();
    }

    private void OnPictureSelected(string path)
    {
        var image = new Image();
        var err = image.Load(path);
        if (err == Error.Ok)
        {
            var picture = ImageTexture.CreateFromImage(image);
            var popup = NewDrawingPopup.GenerateInstance(picture);
            AddChild(popup);
            popup.Popup();
        }
        
    }

    private void OnExitButtonPressed() => GetTree().Quit();
}
