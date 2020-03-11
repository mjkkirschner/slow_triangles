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
        public double Intensity { get; set; }

        public Matrix4x4 ShadowProjectionMatrix { get; set; }

        public Light(bool castShadow, Vector3 pos, Color color, Double intensity = 1.0)
        {
            CastShadow = castShadow;
            Position = pos;
            Color = color;
            Intensity = intensity;
            ShadowProjectionMatrix = Matrix4x4.Identity;
        }
    }

    public class PointLight : Light
    {
        public PointLight(bool castShadow, Vector3 pos, Color color, double intensity = 1.0) : base(castShadow, pos, color, intensity)
        {

        }
    }

    public class DirectionalLight : Light
    {
        public Vector3 Direction { get; set; }
        public DirectionalLight(Vector3 direction, bool castShadow, Color color, Double intensity = 1.0) : base(castShadow, Vector3.Zero, color, intensity)
        {
            this.Direction = direction;
            //TODO this is just random.
            ShadowProjectionMatrix = Matrix4x4.CreateOrthographic(10, 10, 1, 5);
           this.Position =  Vector3.Multiply(this.Direction,2) + this.Position;  
        }
    }

    public interface ILight
    {
        bool CastShadow { get; set; }
        Vector3 Position { get; set; }
        Color Color { get; set; }
        Double Intensity { get; set; }
        
        Matrix4x4 ShadowProjectionMatrix { get; }
    }

}