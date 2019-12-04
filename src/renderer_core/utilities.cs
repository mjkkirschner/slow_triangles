using System;
using System.Numerics;
using renderer.dataStructures;
using renderer.interfaces;

namespace renderer.utilities
{

    public static class Vector3Extensions
    {
        /// <summary>Converts to a vector3 struct - Z is set to 0 by default
        /// 
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static Vector3 ToVector3(this Vector2 vec, float Z = 0f)
        {
            return new Vector3(vec.X, vec.Y, Z);
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

            var ca = Vector2.Subtract(c, a).ToVector3();
            var ba = Vector2.Subtract(b, a).ToVector3();
            var ap = Vector2.Subtract(a, p).ToVector3();

            var u = Vector3.Cross(new Vector3(ca.X, ba.X, ap.X), new Vector3(ca.Y, ba.Y, ap.Y));
            if (System.Math.Abs(u.Z) < .0001f)
            {
                return new Vector3(-1, 1, 1);
            }

            var bary = new Vector3();
            bary.X = (1.0f - (u.X + u.Y) / u.Z);
            bary.Y = u.Y / u.Z;
            bary.Z = u.X / u.Z;

            return bary;
        }

        public static Vector3 BaryCoordinates(int x, int y, TriangleFace triangle, Vector2[] vectors)
        {
            var pt1 = vectors[triangle.vertIndexList[0] - 1];
            var pt2 = vectors[triangle.vertIndexList[1] - 1];
            var pt3 = vectors[triangle.vertIndexList[2] - 1];
            return BaryCoordinates(x, y, pt1, pt2, pt3);
        }
    }
}