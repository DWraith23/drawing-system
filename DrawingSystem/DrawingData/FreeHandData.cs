using Godot;

namespace DrawingSystem;

public partial class FreeHandData : DrawingData
{
    [Export] public Vector2[]? Points { get; set; }
}
