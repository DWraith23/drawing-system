using DrawingSystem.Scripts;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrawingSystem;

public partial class DrawingSystem : Control
{

    #region Enums
    private enum Mode
    {
        Freehand,
        Line,
        Circle
    }

    private enum ActionType
    {
        Add,
        Clear
    }

    public enum OptionsBarMode
    {
        Horizontal,
        Vertical
    }

    #endregion

    [Signal] public delegate void ExportRequestedEventHandler(string path);
    [Signal] public delegate void SaveCanvasRequestedEventHandler(string path);

    public List<DrawingData> Drawings { get; set; } = [];

    private Dictionary<Mode, Action> DrawingStarts => new()
    {
        { Mode.Freehand, StartDrawingFreehand },
        { Mode.Line, StartDrawingLine },
        { Mode.Circle, StartDrawingCircle },
    };

    private Dictionary<Mode, Action> DrawingActions => new()
    {
        { Mode.Freehand, DrawingFreehand },
        { Mode.Line, DrawingLine },
        { Mode.Circle, DrawingCircle },
    };

    private Dictionary<Mode, Action> DrawingEnds => new()
    {
        { Mode.Freehand, EndDrawingFreehand },
        { Mode.Line, EndDrawingLine },
        { Mode.Circle, EndDrawingCircle },
    };

    private Mode CurrentMode { get; set; } = Mode.Freehand;
    private bool Drawing { get; set; } = false;

    private Color SelectedColor { get; set; } = Colors.Black;
    private int SelectedThickness { get; set; } = 10;

    private Vector2 CurrentLineStart { get; set; }
    private List<Vector2>? CurrentFreeHandPoints { get; set; }

    private List<ActionType> ActionsList { get; set; } = [];
    private List<DrawingData>? LastDrawingDataList { get; set; }
    private List<DrawingData> RedoDrawings { get; set; } = [];
    private bool LastUndoWasClear { get; set; } = false;

    public Rect2 CanvasRect { get; set; }
    private Vector2 LastValidPosition { get; set; } = Vector2.Zero;

    #region Overrides

    public override void _Ready()
    {
        ConnectSignals();
        QueueRedraw();
    }

    private void ConnectSignals()
    {

    }

    public override void _Process(double delta)
    {
        if (Drawing)
        {
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        if (Drawing)
        {
            DrawingActions[CurrentMode]();
        }

        Drawings
            .OfType<LineData>().ToList()
            .ForEach(line => DrawLine(line.Start, line.End, line.Color, line.Thickness));
        Drawings
            .OfType<FreeHandData>().ToList()
            .ForEach(freeHand => DrawPolyline(freeHand.Points, freeHand.Color, freeHand.Thickness));
        Drawings
           .OfType<CircleData>().ToList()
           .ForEach(circle => DrawCircle(circle.Start, circle.Radius, circle.Color, false, circle.Thickness));
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouse) return;

        if (mouse.ButtonIndex == MouseButton.Left)
            {
                Drawing = mouse.Pressed;
                if (Drawing)
                {
                    var mousePosition = GetGlobalMousePosition();
                    if (!CanvasRect.HasPoint(mousePosition)) return;
                    DrawingStarts[CurrentMode]();
                }
                else
                {
                    DrawingEnds[CurrentMode]();
                }
                QueueRedraw();
            }
            else if (mouse.ButtonIndex == MouseButton.Right && !mouse.Pressed)
            {
                Drawing = false;
                CurrentFreeHandPoints = null;
                CurrentLineStart = Vector2.Zero;
                QueueRedraw();
            }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey key) return;
        if (Input.IsActionPressed("key_ctrl") && !key.Pressed)
        {
            if (key.Keycode == Key.Z)
            {
                OnUndoRequested();
            }
            else if (key.Keycode == Key.Y)
            {
                OnRedoRequested();
            }
        }
    }


    #endregion


    #region Drawing

    private void StartDrawingFreehand()
    {
        CurrentFreeHandPoints ??= [];
        CurrentFreeHandPoints.Add(GetGlobalMousePosition());
    }

    private void DrawingFreehand()
    {
        if (CurrentFreeHandPoints == null) return;

        var pos = GetGlobalMousePosition();
        if (!CanvasRect.HasPoint(pos)) pos = LastValidPosition;
        else LastValidPosition = pos;
        var last = CurrentFreeHandPoints.Last();
        if (pos.DistanceTo(last) > 1f) CurrentFreeHandPoints.Add(pos);
        if (CurrentFreeHandPoints.Count <= 1) return;

        DrawPolyline([.. CurrentFreeHandPoints], SelectedColor, SelectedThickness);
    }

    private void EndDrawingFreehand()
    {
        if (CurrentFreeHandPoints == null) return;
        if (CurrentFreeHandPoints.Count <= 1)
        {
            var point = new CircleData()
            {
                Start = CurrentFreeHandPoints.First(),
                Radius = SelectedThickness / 4f,
                Color = SelectedColor,
                Thickness = SelectedThickness,
            };
            AddDrawing(point);
            return;
        }

        var data = new FreeHandData()
        {
            Points = [.. CurrentFreeHandPoints],
            Color = SelectedColor,
            Thickness = SelectedThickness
        };
        AddDrawing(data);
    }

    private void StartDrawingLine()
    {
        CurrentLineStart = GetGlobalMousePosition();
    }

    private void DrawingLine()
    {
        var pos = GetGlobalMousePosition();
        if (!CanvasRect.HasPoint(pos)) pos = LastValidPosition;
        else LastValidPosition = pos;
        DrawLine(CurrentLineStart, pos, SelectedColor, SelectedThickness);
    }

    private void EndDrawingLine()
    {
        if (CurrentLineStart.DistanceTo(GetGlobalMousePosition()) <= 1f) return;
        var data = new LineData()
        {
            Start = CurrentLineStart,
            End = GetGlobalMousePosition(),
            Color = SelectedColor,
            Thickness = SelectedThickness
        };
        AddDrawing(data);
    }

    private void StartDrawingCircle()
    {
        CurrentLineStart = GetGlobalMousePosition();
    }

    private void DrawingCircle()
    {
        var pos = GetGlobalMousePosition();
        if (!CanvasRect.HasPoint(pos)) pos = LastValidPosition;
        else LastValidPosition = pos;
        var radius = pos.DistanceTo(CurrentLineStart) / 2f;
        var start = CurrentLineStart.MoveToward(pos, radius);
        DrawCircle(start, radius, SelectedColor, false, SelectedThickness);
    }

    private void EndDrawingCircle()
    {
        var pos = GetGlobalMousePosition();
        var radius = pos.DistanceTo(CurrentLineStart) / 2f;
        var start = CurrentLineStart.MoveToward(pos, radius);
        var data = new CircleData()
        {
            Start = start,
            Radius = radius,
            Color = SelectedColor,
            Thickness = SelectedThickness,
        };
        AddDrawing(data);
    }

    private void AddDrawing(DrawingData data)
    {
        Drawings.Add(data);
        if (Input.IsActionPressed("key_shift"))
        {
            SetDrawingToFade(data);
        }
        else
        {
            ActionsList.Add(ActionType.Add);
        }

        RedoDrawings.Clear();
        CurrentLineStart = Vector2.Zero;
        CurrentFreeHandPoints = null;
    }

    private async void SetDrawingToFade(DrawingData data)
    {
        var timer = GetTree().CreateTimer(2d);
        await timer.ToSignal(timer, Timer.SignalName.Timeout);

        data.Changed += QueueRedraw;
        var tween = CreateTween().SetEase(Tween.EaseType.Out);
        tween.TweenProperty(data, "Color", Colors.Transparent, 1d);
        await tween.ToSignal(tween, Tween.SignalName.Finished);

        Drawings.Remove(data);
        QueueRedraw();
    }

    #endregion



    #region Events
    private void OnLineStylesButtonPressed(int index)
    {
        switch (index)
        {
            case 0: OnFreeHandButtonPressed(); break;
            case 1: OnLineButtonPressed(); break;
            case 2: OnCircleButtonPressed(); break;
        }
    }

    private void OnFreeHandButtonPressed() => CurrentMode = Mode.Freehand;
    private void OnLineButtonPressed() => CurrentMode = Mode.Line;
    private void OnCircleButtonPressed() => CurrentMode = Mode.Circle;

    private void OnColorSelected(Color color) => SelectedColor = color;
    private void OnThicknessChanged(int thickness) => SelectedThickness = thickness;


    private void OnClearButtonPressed()
    {
        LastDrawingDataList = [.. Drawings];
        ActionsList.Add(ActionType.Clear);
        Drawings.Clear();
        Drawing = false;
        QueueRedraw();
    }

    private void OnUndoRequested()
    {
        if (ActionsList.Count == 0) return;
        var last = ActionsList.Last();
        if (last == ActionType.Add)
        {
            var drawing = Drawings.Last();
            Drawings.Remove(drawing);
            RedoDrawings.Add(drawing);
            LastUndoWasClear = false;
        }
        else if (last == ActionType.Clear)
        {
            if (LastDrawingDataList == null || LastDrawingDataList.Count == 0)
            {
                ActionsList.RemoveAt(ActionsList.Count - 1);
                return;
            }
            Drawings = LastDrawingDataList;
            LastUndoWasClear = true;
        }
        ActionsList.RemoveAt(ActionsList.Count - 1);
        QueueRedraw();
    }

    private void OnRedoRequested()
    {
        if (LastUndoWasClear)
        {
            OnClearButtonPressed();
            LastUndoWasClear = ActionsList.Last() == ActionType.Clear;
            ActionsList.Add(ActionType.Clear);
        }
        else
        {
            if (RedoDrawings.Count == 0) return;
            var drawing = RedoDrawings.Last();
            Drawings.Add(drawing);
            RedoDrawings.Remove(drawing);
            ActionsList.Add(ActionType.Add);
        }
        QueueRedraw();
    }

    private void OnMenuButtonPressed(int index)
    {
        switch (index)
        {
            case 0: OnNewCanvasButtonPressed(); break;
            case 1: OnExportButtonPressed(); break;
            case 2: OnSaveButtonPressed(); break;
            case 3: OnLoadButtonPressed(); break;
            case 4: OnExitPressed(); break;
            default: break;
        }
    }

    private void OnNewCanvasButtonPressed()
    {
         var root = GetTree().Root;
        root.GetChildren().ToList().ForEach(child => child.QueueFree());

        root.AddChild(StartMenu.GenerateInstance());
    }

    private void OnExportButtonPressed()
    {
        var dialog = new FileDialog()
        {
            FileMode = FileDialog.FileModeEnum.SaveFile,
            Access = FileDialog.AccessEnum.Filesystem,
            Filters = ["*.png"],
            Title = "Export Picture",
            InitialPosition = Window.WindowInitialPosition.CenterMainWindowScreen,
            Size = new Vector2I(500, 400),
        };
        dialog.FileSelected += OnExportFileSelected;
        AddChild(dialog);
        dialog.Popup();
    }

    private async void OnExportFileSelected(string path)
    {
        await Task.Delay(100);
        EmitSignal(SignalName.ExportRequested, path);
    }

    private void OnSaveButtonPressed()
    {
        var dialog = new FileDialog()
        {
            FileMode = FileDialog.FileModeEnum.SaveFile,
            Access = FileDialog.AccessEnum.Filesystem,
            Filters = ["*.res"],
            Title = "Save Canvas",
            InitialPosition = Window.WindowInitialPosition.CenterMainWindowScreen,
            Size = new Vector2I(500, 400),
        };
        dialog.FileSelected += (path) => EmitSignal(SignalName.SaveCanvasRequested, path);
        AddChild(dialog);
        dialog.Popup();
    }

    private void OnLoadButtonPressed()
    {
        var dialog = new FileDialog()
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Access = FileDialog.AccessEnum.Filesystem,
            Filters = ["*.res"],
            Title = "Load Canvas",
            InitialPosition = Window.WindowInitialPosition.CenterMainWindowScreen,
            Size = new Vector2I(500, 400),
        };
        dialog.FileSelected += OnCanvasSelectedForLoad;
        AddChild(dialog);
        dialog.Popup();
    }

    private void OnCanvasSelectedForLoad(string path)
    {
        var resource = ResourceLoader.Load(path);
        if (resource is not CanvasDetails canvas) return;
        if (canvas.Canvas == null) return;

        var scene = canvas.Canvas.Instantiate<Canvas>();
        if (canvas.Drawings != null) scene.DrawingSystem.Drawings = [.. canvas.Drawings];
        var root = GetTree().Root;
        root.GetChildren().ToList().ForEach(child => child.QueueFree());

        root.AddChild(scene);
    }

    private void OnExitPressed() => GetTree().Quit();

    #endregion



}
