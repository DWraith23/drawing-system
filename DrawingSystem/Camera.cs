using Godot;
using System;
using System.Threading.Tasks;

namespace DrawingSystem;

public partial class Camera : Camera2D
{
    private Vector2 MouseStart { get; set; } = Vector2.Zero;
    private Vector2 CameraStart { get; set; } = Vector2.Zero;
    private bool IsDragging { get; set; } = false;

    private bool DoubleClickAvailable { get; set; } = false;

    private Vector2 _canvasSize = Vector2.Zero;
    public Vector2 CanvasSize
    {
        get => _canvasSize;
        set
        {
            if (_canvasSize == value) return;
            _canvasSize = value;
            Zoom = GetDefaultZoom();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouse)
        {
            if (mouse.ButtonIndex == MouseButton.Middle)
            {
                if (mouse.Pressed)
                {
                    ListenForDoubleClick();
                    MouseStart = GetViewport().GetMousePosition();
                    CameraStart = Position;
                    IsDragging = true;
                }
                else
                {
                    IsDragging = false;
                }
            }
            else if (mouse.ButtonIndex == MouseButton.WheelUp)
            {
                var zoom = Math.Min(Zoom.X + 0.01f, 3f);
                Zoom = new(zoom, zoom);
            }
            else if (mouse.ButtonIndex == MouseButton.WheelDown)
            {
                var maxZoomOut = GetViewportRect().Size / CanvasSize - new Vector2(0.01f, 0.01f);
                var zoomX = Zoom.X - 0.01f;
                var zoomY = Zoom.Y - 0.01f;
                var maxX = Math.Max(zoomX, maxZoomOut.X);
                var maxY = Math.Max(zoomY, maxZoomOut.Y);
                Zoom = new(maxX, maxY);
            }
        }
        else if (@event is InputEventMouseMotion && IsDragging)
        {
            var currentMousePosition = GetViewport().GetMousePosition();
            var dragOffset = currentMousePosition - MouseStart;
            Position = CameraStart - dragOffset / Zoom;
        }

    }

    private async void ListenForDoubleClick()
    {
        if (DoubleClickAvailable)
        {
            Zoom = GetDefaultZoom();
            return;
        }
        DoubleClickAvailable = true;
        await Task.Delay(500);
        DoubleClickAvailable = false;
    }

    private Vector2 GetDefaultZoom()
    {
        var fullZoom = GetViewportRect().Size / CanvasSize;
        var normalZoom = Vector2.One;
        var x = Math.Min(fullZoom.X, normalZoom.X);
        var y = Math.Min(fullZoom.Y, normalZoom.Y);
        return new Vector2(x, y);
    }

}
