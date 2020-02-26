using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using renderer.dataStructures;
using renderer.interfaces;

namespace renderer.utilities
{
    public static class ListExtensions
    {
        public static List<List<T>> Split<T>(List<T> source, uint groupSize)
        {
            return source
                .Select((x, i) => (Index: i, Value: x))
                .GroupBy(x => x.Index / groupSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        /// <summary>
        /// flips an array vertically and modifies the input array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        public static void Flip(IList array, int height, int width)
        {
            for (int i = 0; i < height / 2; ++i)
            {
                int k = height - 1 - i;
                for (int j = 0; j < width; ++j)
                {
                    var temp = array[i * width + j];
                    array[i * width + j] = array[k * width + j];
                    array[k * width + j] = temp;
                }
            }
        }
    }

    public static class ColorExtensions
    {
        public static Vector3 ToVector3(this Color color)
        {
            return new Vector3(2f * (color.R / 255f) - 1f, 2f * (color.G / 255f) - 1f, 2 * (color.B / 255f) - 1f);
        }
    }

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

        public static Vector3 ApplyMatrix(this Vector3 self, Matrix4x4 matrix)
        {

            var w = matrix.M41 * self.X + matrix.M42 * self.Y + matrix.M43 * self.Z + matrix.M44;

            //matrix = Matrix4x4.Transpose(matrix);
            var outVector = new Vector3(
                matrix.M11 * self.X + matrix.M12 * self.Y + matrix.M13 * self.Z + matrix.M14,
                matrix.M21 * self.X + matrix.M22 * self.Y + matrix.M23 * self.Z + matrix.M24,
                matrix.M31 * self.X + matrix.M32 * self.Y + matrix.M33 * self.Z + matrix.M34
            );

            if (w != 1)
            {
                outVector.X /= w;
                outVector.Y /= w;
                outVector.Z /= w;
            }
            return outVector;
        }

        public static Vector3 ToVector3(this Vector4 vec)
        {
            return new Vector3(vec.X, vec.Y, vec.Z);
        }

        public static Vector3 Transform(this Vector3 vec, Matrix3x3 mat)
        {
            var x = mat.M11 * vec.X + mat.M12 * vec.Y + mat.M13 * vec.Z;
            var y = mat.M21 * vec.X + mat.M22 * vec.Y + mat.M23 * vec.Z;
            var z = mat.M31 * vec.X + mat.M32 * vec.Y + mat.M33 * vec.Z;
            return new Vector3(x, y, z);
        }


    }

    public static class MatrixExtensions
    {
        public static Matrix4x4 CreateViewPortMatrix(int x, int y, int depthMax, int w, int h)
        {
            Matrix4x4 m = Matrix4x4.Identity;
            m.M14 = x + w / 2f;
            m.M24 = y + h / 2f;
            m.M34 = depthMax / 2f;

            m.M11 = w / 2f;
            m.M22 = h / 2f;
            m.M33 = depthMax / 2f;
            return m;

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

        public static Vector3 BaryCoordinates2(int x, int y, Vector2 triPt1, Vector2 triPt2, Vector2 triPt3)
        {
            var p = new Vector2(x, y);
            var a = triPt1;
            var b = triPt2;
            var c = triPt3;

            //for normal calc
            var edge1 = Vector2.Subtract(b, a).ToVector3();
            var edge2 = Vector2.Subtract(c, a).ToVector3();

            //for bary centric calc of sub triangle area.
            var edge3 = Vector2.Subtract(a, c).ToVector3();
            var edge4 = Vector2.Subtract(c, b).ToVector3();
            var edge5 = Vector2.Subtract(b, a).ToVector3();


            var edgePB = Vector2.Subtract(p, b).ToVector3();
            var edgePC = Vector2.Subtract(p, c).ToVector3();
            var edgePA = Vector2.Subtract(p, a).ToVector3();

            var triNormal = Vector3.Cross(edge1, edge2);
            var area = triNormal.Z / 2f;

            var temp1 = Vector3.Cross(edge4, edgePB);
            var temp2 = Vector3.Cross(edge3, edgePC);
            var temp3 = Vector3.Cross(edge5, edgePA);

            var u = (temp1.Z / 2f) / area;
            var v = (temp2.Z / 2f) / area;
            var w = (temp3.Z / 2f) / area;//(1.0f-u-v);

            return new Vector3(u, v, w);

        }

        public static Vector3 BaryCoordinates(int x, int y, TriangleFace triangle, Vector2[] vectors)
        {
            var pt1 = vectors[triangle.vertIndexList[0] - 1];
            var pt2 = vectors[triangle.vertIndexList[1] - 1];
            var pt3 = vectors[triangle.vertIndexList[2] - 1];
            return BaryCoordinates(x, y, pt1, pt2, pt3);
        }

        public static Vector3 BaryCoordinates2(int x, int y, TriangleFace triangle, Vector2[] vectors)
        {
            var pt1 = vectors[triangle.vertIndexList[0] - 1];
            var pt2 = vectors[triangle.vertIndexList[1] - 1];
            var pt3 = vectors[triangle.vertIndexList[2] - 1];
            return BaryCoordinates2(x, y, pt1, pt2, pt3);
        }


        public static bool pixelIsInsideTriangle(int x, int y, TriangleFace triangle, Vector3[] vectors)
        {
            var pt1 = vectors[triangle.vertIndexList[0] - 1];
            var pt2 = vectors[triangle.vertIndexList[1] - 1];
            var pt3 = vectors[triangle.vertIndexList[2] - 1];

            var barycenter = TriangleExtensions.BaryCoordinates2(x, y, pt1.ToVector2(), pt2.ToVector2(), pt3.ToVector2());
            //only in the triangle if coefs are all positive.

            if (barycenter.X < 0 || barycenter.X >= 1f || barycenter.Y < 0 || barycenter.Y >= 1f || barycenter.Z < 0 || barycenter.Z >= 1f)
            {
                return false;
            }
            return true;
        }

        public static bool pixelIsInsideTriangle(int x, int y, Vector3[] triPoints)
        {

            var pt1 = triPoints[0];
            var pt2 = triPoints[1];
            var pt3 = triPoints[2];
            var barycenter = TriangleExtensions.BaryCoordinates2(x, y, pt1.ToVector2(), pt2.ToVector2(), pt3.ToVector2());
            //only in the triangle if coefs are all positive.

            if (barycenter.X < 0 || barycenter.X > 1f || barycenter.Y < 0 || barycenter.Y > 1f || barycenter.Z < 0 || barycenter.Z > 1f)
            {
                return false;
            }
            return true;
        }


        //TODO should probably be Vector4
        public static void drawTriangle(int triIndex, Vector3[] screenCords, Material material, double[] zbuffer,
        Color[] imageBuffer, int imageBufferWidth, byte[] previewBuffer = null)
        {
            var minx = screenCords.Select(x => x.X).Min();
            var miny = screenCords.Select(x => x.Y).Min();
            var maxx = screenCords.Select(x => x.X).Max();
            var maxy = screenCords.Select(x => x.Y).Max();

            var A = screenCords[0];
            var B = screenCords[1];
            var C = screenCords[2];

            Enumerable.Range((int)minx, (int)(maxx - minx) + 2).ToList().ForEach(x =>
                   {

                       Enumerable.Range((int)miny, (int)(maxy - miny) + 2).ToList().ForEach(y =>
                       {
                           var IsInsideTriangle = pixelIsInsideTriangle(x, y, screenCords);
                           var bary = TriangleExtensions.BaryCoordinates2(x, y,
                               A.ToVector2(), B.ToVector2(), C.ToVector2());
                           //compute the depth of current pixel.
                           var z = bary.X * A.Z + bary.Y * B.Z + bary.Z * C.Z;
                           if (IsInsideTriangle)
                           {
                               var flatIndex = imageBufferWidth * (int)y + (int)x;
                               //don't draw unless we are within bounds
                               //don't draw if something is already in the depth buffer for this pixel.
                               if (flatIndex < imageBuffer.Length && flatIndex > -1 /*&& z < depthBuffer[flatIndex]*/)
                               {
                                   //only draw if nothing else is closer in the depth buffer and the shader does not ignore this pixel.
                                   Color diffColor;
                                   if (z < zbuffer[flatIndex] && material.Shader.FragmentToRaster(material, bary, ref diffColor))
                                   {

                                       imageBuffer[flatIndex] = diffColor;
                                       zbuffer[flatIndex] = z;
                                       if (previewBuffer != null && false)
                                       {
                                           //var height = (int)(imageBuffer.Length/ imageBufferWidth);
                                           //var midheight = height/2;
                                           //var newY = y - midheight
                                           var flatByteOffset = (imageBufferWidth * 4) * (int)y + (int)x*4;
                                           previewBuffer[flatByteOffset] = diffColor.R;
                                           previewBuffer[flatByteOffset + 1] = diffColor.G;
                                           previewBuffer[flatByteOffset + 2] = diffColor.B;
                                           previewBuffer[flatByteOffset + 3] = diffColor.A;
                                       }
                                   }
                               }
                           }
                       });
                   });
        }

    }
}