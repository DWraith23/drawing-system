using Godot;

namespace DrawingSystem;

public partial class CircleData : DrawingData
{
    [Export] public Vector2 Start { get; set; }
    [Export] public float Radius { get; set; }
}
