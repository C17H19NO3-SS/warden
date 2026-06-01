using System.Drawing;
using CounterStrikeSharp.API.Modules.Utils;

namespace JailBreak.Models
{
    public enum MarkerShape
    {
        Circle,
        Cube,
        Triangle
    }

    public enum MarkerEffect
    {
        Solid,
        Tilted,
        RGB
    }

    public class MarkerConfig
    {
        public MarkerShape Shape { get; set; } = MarkerShape.Circle;
        public Color Color { get; set; } = Color.Red;
        public MarkerEffect Effect { get; set; } = MarkerEffect.Solid;
        public float Size { get; set; } = 80.0f;
        public Vector Position { get; set; } = new Vector(0, 0, 0);
    }
}
