using Godot;

namespace DrawingSystem;

 public partial class DrawingData : Resource
{
    private Color _color;
    [Export] 
    public Color Color
    {
        get => _color;
        set
        {
            if (value == _color) return;
            _color = value;
            EmitChanged();
        }
    }
    [Export] public int Thickness { get; set; }
}
