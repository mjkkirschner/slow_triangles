
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Linq;
using renderer.utilities;
using System;
using renderer_core.dataStructures;
using renderer.materials;
using System.Collections;

namespace renderer.dataStructures
{


    public struct TriangleFace
    {
        public int[] vertIndexList;
        public int[] UVIndexList;
        public int[] NormalIndexList;


        public TriangleFace(int[] inds, int[] uvInds = null, int[] normInds = null)
        {
            this.vertIndexList = inds;
            this.UVIndexList = uvInds;
            this.NormalIndexList = normInds;
        }
    }

    public class Matrix3x3
    {

        //
        // Summary:
        //     The first element of the first row.
        public float M11;

        //
        // Summary:
        //     The second element of the first row.
        public float M12;

        //
        // Summary:
        //     The third element of the first row.
        public float M13;


        //
        // Summary:
        //     The first element of the  second row.
        public float M21;

        //
        // Summary:
        //     The second element of the  second row.
        public float M22;

        //
        // Summary:
        //     The third element of the  second row.
        public float M23;


        //
        // Summary:
        //     The first element of the third row.
        public float M31;

        //
        // Summary:
        //     The second element of the third row.
        public float M32;

        //
        // Summary:
        //     The third element of the third row.
        public float M33;


        public Matrix3x3(Matrix4x4 mat4)
        {
            this.M11 = mat4.M11;
            this.M12 = mat4.M12;
            this.M13 = mat4.M13;

            this.M21 = mat4.M21;
            this.M22 = mat4.M22;
            this.M23 = mat4.M23;

            this.M31 = mat4.M31;
            this.M32 = mat4.M32;
            this.M33 = mat4.M33;

        }



    }


    public class Texture2d
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        //span?
        public IList<Color> ColorData { get; set; }

        public Color GetColorAtUV(Vector2 UV)
        {
            //clamp values at 0 and 1.
            UV = Vector2.Clamp(UV, Vector2.Zero, Vector2.One);

            var x = UV.X * Width;
            var y = (1.0f - UV.Y) * Height;
            var index = MathExtensions.Clamp<int>(Width * (int)y + (int)x, 0, (Width * Height) - 1);
            return ColorData[index];
        }

        public Texture2d(int width, int height, IEnumerable<Color> colorData)
        {
            this.Width = width;
            this.Height = height;
            this.ColorData = colorData.ToArray();
        }

        public void Flip()
        {
            ListExtensions.Flip(this.ColorData as IList, Height, Width);
        }
    }

    /// <summary>
    /// Shader which performs perspective projection in the vertex shader
    /// </summary>
    public class Base3dShader : Shader
    {
        protected Matrix4x4 ViewModelMatrix;
        protected Matrix4x4 ProjectionMatrix;
        protected Matrix4x4 ViewportMatrix;
        public Base3dShader(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 viewPort)
        {
            this.ViewModelMatrix = viewMatrix;
            this.ProjectionMatrix = projectionMatrix;
            this.ViewportMatrix = viewPort;
        }

        public void SetProjectionMatrices(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 viewPort)
        {
            this.ViewModelMatrix = viewMatrix;
            this.ProjectionMatrix = projectionMatrix;
            this.ViewportMatrix = viewPort;
        }

        public override Vector3 VertexToFragment(Mesh mesh, int triangleIndex, int vertIndex)
        {
            var vert = base.VertexToFragment(mesh, triangleIndex, vertIndex);
            var mvp = Matrix4x4.Transpose(Matrix4x4.Multiply(ViewModelMatrix, ProjectionMatrix));
            var final = Matrix4x4.Multiply(ViewportMatrix, mvp);
            return vert.ApplyMatrix(final);
        }

        public float removeMe_testing_W(Vector3 vect)
        {
            var mvp = Matrix4x4.Transpose(Matrix4x4.Multiply(ViewModelMatrix, ProjectionMatrix));
            var final = Matrix4x4.Multiply(ViewportMatrix, mvp);
            return vect.GetW(final);
        }

        //TODO consider moving to shader utilities.
        protected Color calcSingleDirLight_noSpec(IMaterial mat, Vector2 interpolatedUV, Color diffColor, double intensity, DirectionalLight light, float uniform_ambient)
        {
            var diffTerm = ((diffColor.ToVector3() * light.Color.ToVector3() / 255.0f) * (float)(light.Intensity * intensity)).ToColor();
            var clampedDiffTerm = Vector3.Clamp(diffTerm.ToVector3(), Vector3.Zero, new Vector3(255, 255, 255));

            var colIntermediate = clampedDiffTerm.ToColor();

            var diffColorOriginal = (mat as DiffuseMaterial).DiffuseTexture.GetColorAtUV(interpolatedUV);
            Vector3 ambientTerm = uniform_ambient * diffColorOriginal.ToVector3();
            return (colIntermediate.ToVector3() + ambientTerm).ToColor();
        }

        protected static Vector3 reflect(Vector3 vector, Vector3 normal)
        {
            return Vector3.Normalize(vector - normal * (normal * vector * 2.0f));
        }

        protected static (double, double) ConvertCartesianToSphericalCoords(Vector3 cartesianCoords)
        {
            var xysq = (cartesianCoords.X * cartesianCoords.X) +
            (cartesianCoords.Y * cartesianCoords.Y);
            var theta = System.Math.Atan2(cartesianCoords.Z, Math.Sqrt(xysq)); //elevation
            var denom = System.Math.Sqrt(
               xysq +
            (cartesianCoords.Z * cartesianCoords.Z));
            var phi = System.Math.Atan2(cartesianCoords.Y, cartesianCoords.X); //azimuth
            //lat,long
            return (theta, phi);
        }

        protected static (double, double) ConvertCartesianToSphericalCoords2(Vector3 cartesianCoords)
        {
            var theta = cartesianCoords.Z;
            var phi = Math.Atan2(cartesianCoords.Y, cartesianCoords.X);
            return (theta, phi);
        }

        protected static (double, double) ConvertCartesianToSphericalCoords3(Vector3 cartesianCoords)
        {
            var theta = cartesianCoords.X * Math.PI;
            var phi = Math.Asin(cartesianCoords.Y);
            return (theta, phi);
        }

        protected static (double, double) ConvertCartesianToSphericalCoords4(Vector3 cartesianCoords)
        {
            var theta = Math.Acos(-cartesianCoords.Y / cartesianCoords.Length());
            var phi = Math.Atan2(cartesianCoords.X, -cartesianCoords.Z);
            return (theta, phi);
        }

        //https://stackoverflow.com/questions/47819607/convert-cubemap-coordinates-to-equivalents-in-equirectangular
        protected static Vector2 convertVector3ToEquirectangularUVCoords(Vector3 vector, Texture2d tex)
        {
            var (theta, phi) = ConvertCartesianToSphericalCoords4(vector);
            var U = .5f + phi / (Math.PI * 2f);
            var V = theta / Math.PI;

            return new Vector2((float)U, (float)V);
        }

    }

    //TODO this type likely to be replaced after implementing materials/shaders / materialMeshes.
    //could also flip architecture to composable style.

    /// <summary>
    /// Base class of all shaders, performs no projection and does not shade :) - returns white for all pixels
    /// </summary>
    public class Shader
    {

        public virtual Vector3 VertexToFragment(Mesh mesh, int triangleIndex, int vertIndex)
        {
            var currentVert = mesh.VertexData[mesh.Triangles[triangleIndex].vertIndexList[vertIndex] - 1];
            //for reference
            var currentUV = mesh.VertexUVData[mesh.Triangles[triangleIndex].UVIndexList[vertIndex] - 1];
            var currentNormal = mesh.VertexNormalData[mesh.Triangles[triangleIndex].NormalIndexList[vertIndex] - 1];

            //we don't do any projection in this shader
            return new Vector3(currentVert.X, currentVert.Y, currentVert.Z);
        }


        public virtual bool FragmentToRaster(IMaterial mat, Vector3 baryCoords, ref Color color)
        {
            var intensity = 1.0;
            var channel = System.Math.Min(255, (int)(255 * intensity));
            color = Color.FromArgb(channel, channel, channel);
            return true;
        }
    }

    //TODO currently, has no use, will be used to set specific maps and parameters for a specified shader.
    //potentially can use reflection or description objects.
    public class Material : IMaterial
    {
        public Shader Shader { get; set; }
    }

    public interface IMaterial
    {
        Shader Shader { get; set; }
    }

    /// <summary>
    /// A bag for data and materials which specify how to render that data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Renderable<T> where T : Mesh
    {
        public T RenderableObject { get; set; }
        public IMaterial material { get; set; }
        public Renderable(IMaterial material, T data)
        {
            this.RenderableObject = data;
            this.material = material;
        }

        /* proposed interface
       public Color[] Render()
       {

       }

       public interpolateCoord Vector 3 (x,y)
       */
    }

    public class Mesh
    {
        public IList<TriangleFace> Triangles { get; set; }
        public IList<Vector4> VertexData { get; set; }
        public IList<Vector3> VertexNormalData { get; set; }
        public IList<Vector2> VertexUVData { get; set; }

        public IList<Vector3> BiNormals_akaBiTangents { get; set; }
        public IList<Vector3> Tangents { get; set; }

        public IList<Vector3> VertTangents { get; set; }
        public IList<Vector3> VertBiNormals { get; set; }


        public Mesh(IList<TriangleFace> tris, IList<Vector4> verts, IList<Vector3> normals, IList<Vector2> uvs, IList<Vector3> binormals = null, IList<Vector3> tangents = null)
        {
            this.Triangles = tris;
            this.VertexData = verts;
            this.VertexNormalData = normals;
            this.VertexUVData = uvs;
            this.BiNormals_akaBiTangents = binormals ?? new List<Vector3>();
            this.Tangents = tangents ?? new List<Vector3>();
            validateMesh();


        }

        public void computeAveragedNormals()
        {
            var averagedNormals = Enumerable.Repeat(new Vector3(0, 0, 0), this.VertexNormalData.Count).ToList();

            foreach (var tri in Triangles)
            {
                //we made sure that the normals have the same index as the verts so we can use either index.
                foreach (var vert in tri.vertIndexList)
                {
                    averagedNormals[vert - 1] = Vector3.Add(averagedNormals[vert - 1], this.VertexNormalData[vert - 1]);
                }
            }

            //one final pass to set all the normal data per vert.
            foreach (var tri in Triangles)
            {
                foreach (var vert in tri.vertIndexList)
                {
                    this.VertexNormalData[vert - 1] = Vector3.Normalize(averagedNormals[vert - 1]);
                }
            }


        }

        //TODO should be called as part of constructor or setter...
        public void computeAveragedTangents()
        {
            var averagedTangents = Enumerable.Repeat(new Vector3(0, 0, 0), this.VertexData.Count).ToList();
            var averagedBiNormals = Enumerable.Repeat(new Vector3(0, 0, 0), this.VertexData.Count).ToList();
            var triIndex = 0;

            foreach (var tri in Triangles)
            {
                foreach (var vert in tri.vertIndexList)
                {
                    averagedTangents[vert - 1] = Vector3.Add(averagedTangents[vert - 1], this.Tangents[triIndex]);
                    averagedBiNormals[vert - 1] = Vector3.Add(averagedBiNormals[vert - 1], this.BiNormals_akaBiTangents[triIndex]);
                }
                triIndex++;
            }

            this.VertBiNormals = new List<Vector3>();
            this.VertTangents = new List<Vector3>();
            for (var i = 0; i < averagedBiNormals.Count; i++)
            {
                var t = Vector3.Normalize(averagedTangents[i]);
                var b = Vector3.Normalize(averagedBiNormals[i]);
                var n = this.VertexNormalData[i];

                t = Vector3.Normalize(t - (n * Vector3.Dot(n, t)));

                if (Vector3.Dot(Vector3.Cross(n, t), b) < 0.0f)
                {
                    t = t * -1.0f;
                }
                //check for degenerate data...
                //TODO must be a better solution
                if (float.IsNaN(t.X) || float.IsNaN(t.Y) || float.IsNaN(t.Z)
                || float.IsNaN(b.X) || float.IsNaN(b.Y) || float.IsNaN(b.Z))
                {
                    //just default to two ws vector...
                    t = new Vector3(1, 0, 0);
                    b = new Vector3(0, 1, 0);
                    Console.WriteLine($"issue with vertex {i} when computing tangents");
                }

                this.VertTangents.Add(t);
                this.VertBiNormals.Add(b);
            }
        }

        private void validateMesh()
        {
            var counts = Enumerable.Repeat(0, this.VertexData.Count).ToList();
            foreach (var triFace in Triangles)
            {
                foreach (var vert in triFace.vertIndexList)
                {
                    counts[vert - 1] = counts[vert - 1] + 1;
                }
            }
            Console.WriteLine($"{counts.Where(x => x < 3).Count()} verts with no other triangles sharing this vert. ");
        }
    }



}