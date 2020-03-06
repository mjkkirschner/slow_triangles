using Autodesk.DesignScript.Runtime;
using renderer_core.dataStructures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace slow_triangles.DynamoNodes.Lights
{
    [IsVisibleInDynamoLibrary(false)]
    public class DynamoLight : Light
    {
        public DynamoLight(bool castShadow, Vector3 pos, Color color, Double intensity = 1.0) : base(castShadow, pos, color, intensity)
        {
          
        }
    }
    [IsVisibleInDynamoLibrary(false)]
    public class DynamoDirLight : renderer_core.dataStructures.DirectionalLight
    {
        public DynamoDirLight(Vector3 dir, bool castShadow, Color color, Double intensity = 1.0) : base(dir, castShadow, color, intensity)
        {

        }
    }

    public static class DirectionalLight
    {
        public static renderer_core.dataStructures.DirectionalLight ByDirectionColorIntensity(Autodesk.DesignScript.Geometry.Vector direction, DSCore.Color color, float intensity = 1.0f)
        {
            return new renderer_core.dataStructures.DirectionalLight(direction.ToVector3(), false, Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue), intensity);
        }

    }
}
