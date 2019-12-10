using System.Drawing;
using System.Numerics;

namespace renderer.tests
{
    public static class Utilities
    {
        public static double ComputeSimpleColorDistance(Color col1, Color col2)
        {
            var vec1 = new Vector3(col1.R, col1.G, col1.B);
            var vec2 = new Vector3(col2.R, col2.G, col2.B);
            return Vector3.Distance(vec1, vec2);


        }
    }
}