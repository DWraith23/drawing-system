using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace DrawingSystem;

public partial class CanvasDetails : Resource
{
    [Export] public PackedScene? Canvas { get; set; }
    [Export] public DrawingData[]? Drawings { get; set; }

    public static void SaveCanvas(string path, Canvas canvas)
    {
        var details = new CanvasDetails();
        var scene = new PackedScene();
        scene.Pack(canvas);
        details.Canvas = scene;
        details.Drawings = [.. canvas.DrawingSystem.Drawings];
        ResourceSaver.Save(details, path);
    }
}
