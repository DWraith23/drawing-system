using Godot;
using System;
using System.Threading.Tasks;

namespace DrawingSystem;

public partial class Canvas : Node2D
{
    private static string ScenePath => "res://DrawingSystem/canvas.tscn";
    public static Canvas GenerateInstance(int width, int height, Color backgroundColor, Texture2D? image)
    {
        var instance = GD.Load<PackedScene>(ScenePath).Instantiate<Canvas>();
        instance.CanvasSize = new(width, height);
        instance.LoadedImage.Texture = image;
        instance.ChangeBackgroundColor(backgroundColor);
        return instance;
    }

    public PanelContainer Background => GetNode<PanelContainer>("Background");
    public TextureRect LoadedImage => GetNode<TextureRect>("Loaded Image");
    public Camera Camera => GetNode<Camera>("Camera");
    public DrawingSystem DrawingSystem => GetNode<DrawingSystem>("Drawing System");
    public HorizontalOptionsBar OptionsBar => GetNode<HorizontalOptionsBar>("CanvasLayer/Options");

    private Vector2 _canvasSize = new(500, 500);
    [Export]
    public Vector2 CanvasSize
    {
        get => _canvasSize;
        set
        {
            if (_canvasSize == value) return;
            _canvasSize = value;
            Update();
        }
    }

    public override void _Ready()
    {
        Update();
        DrawingSystem.ExportRequested += OnExportRequested;
        DrawingSystem.SaveCanvasRequested += OnSaveRequested;
    }

    private void Update()
    {
        if (!IsNodeReady()) return;
        Background.Size = CanvasSize;
        DrawingSystem.Size = CanvasSize;
        Camera.CanvasSize = CanvasSize;
        UpdateCamera();
        DrawingSystem.CanvasRect = Background.GetRect();
    }

    private void UpdateCamera()
    {
        Camera.LimitLeft = 0;
        Camera.LimitTop = 0;
        var right = Math.Max(GetViewportRect().Size.X, CanvasSize.X);
        var bottom = Math.Max(GetViewportRect().Size.Y, CanvasSize.Y);
        Camera.LimitRight = (int)Math.Floor(right);
        Camera.LimitBottom = (int)Math.Floor(bottom);
    }

    #region Load and Change
    public void ChangeBackgroundColor(Color color)
    {
        var stylebox = Background.GetThemeStylebox("panel").Duplicate(true) as StyleBoxFlat;
        stylebox!.BgColor = color;
        Background.AddThemeStyleboxOverride("panel", stylebox);
    }

    public void LoadImage(Texture2D image)
    {
        var x = Math.Max(CanvasSize.X, image.GetWidth());
        var y = Math.Max(CanvasSize.Y, image.GetHeight());

        CanvasSize = new Vector2(x, y);
        LoadedImage.Texture = image;
    }

    #endregion

    private void OnExportRequested(string path)
    {
        CreateExportViewport(path);
    }

    private async void CreateExportViewport(string path)
    {
        var drawings = DrawingSystem.Drawings;
        SubViewport view = new()
        {
            Size = new((int)CanvasSize.X, (int)CanvasSize.Y)
        };
        var background = Background.Duplicate() as PanelContainer;
        var image = LoadedImage.Duplicate() as TextureRect;
        var drawing = DrawingSystem.Duplicate() as DrawingSystem;
        view.AddChild(background);
        view.AddChild(image);
        view.AddChild(drawing);
        drawing!.Drawings = drawings;
        drawing.QueueRedraw();
        PopupPanel panel = new()
        {
            InitialPosition = Window.WindowInitialPosition.CenterPrimaryScreen,
            Size = new((int)CanvasSize.X, (int)CanvasSize.Y)
        };
        TextureRect rect = new();
        panel.AddChild(rect);
        panel.AddChild(view);
        AddChild(panel);
        panel.Popup();
        await Task.Delay(500);
        ViewportTexture texture = view.GetTexture();
        rect.Texture = texture;
        panel.PopupHide += () => { texture.GetImage().SavePng(path); };
        await Task.Delay(1000);
        panel.Hide();
    }

    private void OnSaveRequested(string path)
    {
        CanvasDetails.SaveCanvas(path, this);
    }

}

