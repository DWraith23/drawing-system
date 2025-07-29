using Godot;

namespace DrawingSystem;

public partial class LineData : DrawingData
{
    [Export] public Vector2 Start { get; set; }
    [Export] public Vector2 End { get; set; }
}