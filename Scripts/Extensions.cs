using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace DrawingSystem.Scripts;

public static class Extensions
{

    public static async Task Completed(this Tween tween) => await tween.ToSignal(tween, Tween.SignalName.Finished);

    public static void ClearChildren(this Node node) => node.GetChildren().ToList().ForEach(child => child.QueueFree());

}
