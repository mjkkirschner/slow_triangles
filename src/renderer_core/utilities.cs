using System.Numerics;
using renderer.interfaces;

namespace renderer.utilities
{

    public static class Vector3Extensions
    {
        public static Vector3 ToVector3(this Vector2 vec)
        {
            return new Vector3(vec.X, vec.Y, 1.0f);
        }

        public static Vector2 ToVector2(this Vector3 vec)
        {
            return new Vector2(vec.X, vec.Y);
        }
    }

    public static class TriangleExtensions
    {
        public static Vector3 BaryCoordinates(int x, int y, Vector2 triPt1, Vector2 triPt2, Vector2 triPt3)
        {

            var p = new Vector2(x, y);
            var a = triPt1;
            var b = triPt2;
            var c = triPt3;

            var ab = Vector2.Subtract(a, b).ToVector3();
            var bc = Vector2.Subtract(b, c).ToVector3();
            var bp = Vector2.Subtract(b, p).ToVector3();
            var cp = Vector2.Subtract(c, p).ToVector3();
            var ac = Vector2.Subtract(a, c).ToVector3();


            var areaABC = Vector3.Cross(ab, bc).Length() * .5f;
            var areaABP = Vector3.Cross(ab, bp).Length() * .5f;
            var areaACP = Vector3.Cross(ac, cp).Length() * .5f;
            var areaBCP = Vector3.Cross(bc, cp).Length() * .5f;

            var bary = new Vector3();
            bary.X = areaBCP / areaABC; // alpha
            bary.Y = areaACP / areaABC; // beta
            bary.Z = 1.0f - bary.X - bary.Y; // gamma

            return bary;
        }

        public static Vector3 BaryCoordinates(int x, int y, TriangleFace triangle, Vector2[] vectors)
        {
            var pt1 = vectors[triangle.indexList[0] - 1];
            var pt2 = vectors[triangle.indexList[1] - 1];
            var pt3 = vectors[triangle.indexList[2] - 1];
            return BaryCoordinates(x, y, pt1, pt2, pt3);
        }
    }
}