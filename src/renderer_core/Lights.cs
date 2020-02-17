using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace renderer_core.dataStructures
{

    public abstract class Light : ILight
    {
        public bool CastShadow { get; set; }
        public Vector3 Position { get; set; }
        public Color Color { get; set; }

        public Light(bool castShadow, Vector3 pos, Color color)
        {
            CastShadow = castShadow;
            Position = pos;
            Color = color;
        }
    }

    public class PointLight : Light
    {
        public PointLight(bool castShadow, Vector3 pos, Color color) : base(castShadow, pos, color)
        {

        }
    }

    public class DirectionalLight : Light
    {
        public Vector3 Direction { get; set; }
        public DirectionalLight(Vector3 direction, bool castShadow, Color color) : base(castShadow, Vector3.Zero, color)
        {
            this.Direction = direction;
        }
    }

    public interface ILight
    {
        bool CastShadow { get; set; }
        Vector3 Position { get; set; }
        Color Color { get; set; }
    }

}