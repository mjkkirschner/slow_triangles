
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Linq;
using renderer.utilities;
using System;

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
            var x = (1.0f - UV.X) * Width;
            var y = (1.0f - UV.Y) * Height;

            return ColorData[Width * (int)y + (int)x];
        }

        public Texture2d(int width, int height, IEnumerable<Color> colorData)
        {
            this.Width = width;
            this.Height = height;
            this.ColorData = colorData.ToArray();
        }
    }

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

        public override Vector3 VertexToFragment(Mesh mesh, int triangleIndex, int vertIndex)
        {
            var vert = base.VertexToFragment(mesh, triangleIndex, vertIndex);
            var mvp = Matrix4x4.Transpose(Matrix4x4.Multiply(ViewModelMatrix, ProjectionMatrix));
            var final = Matrix4x4.Multiply(ViewportMatrix, mvp);
            return vert.ApplyMatrix(final);
        }


    }

    //TODO this type likely to be replaced after implementing materials/shaders / materialMeshes.
    //could also flip architecture to composable style.

    public class Shader
    {
        public Vector3 LightDirection = new Vector3(0, -1, 0);
        protected float[] varying_intensity = new float[3];
        public virtual Vector3 VertexToFragment(Mesh mesh, int triangleIndex, int vertIndex)
        {
            var currentVert = mesh.VertexData[mesh.Triangles[triangleIndex].vertIndexList[vertIndex] - 1];
            var currentUV = mesh.VertexUVData[mesh.Triangles[triangleIndex].UVIndexList[vertIndex] - 1];
            var currentNormal = mesh.VertexNormalData[mesh.Triangles[triangleIndex].NormalIndexList[vertIndex] - 1];

            //dot normal*light = intensity for vert.
            varying_intensity[vertIndex] = System.Math.Max(0, Vector3.Dot(currentNormal, LightDirection));

            //we don't do any projection in this shader
            return new Vector3(currentVert.X, currentVert.Y, currentVert.Z);
        }


        public virtual bool FragmentToRaster(Material mat, Vector3 baryCoords, ref Color color)
        {
            var varying_vector = new Vector3(varying_intensity[0], varying_intensity[1], varying_intensity[2]);
            var intensity = Vector3.Dot(varying_vector, baryCoords);
            var channel = System.Math.Min(255, (int)(255 * intensity));
            color = Color.FromArgb(channel, channel, channel);
            return true;
        }
    }

    //TODO currently, has no use, will be used to set specific maps and parameters for a specified shader.
    //potentially can use reflection or description objects.
    public class Material
    {
        public Shader Shader { get; set; }
    }


    /// <summary>
    /// A bag for data and materials which specify how to render that data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Renderable<T> where T : Mesh
    {
        public T RenderableObject { get; set; }
        public Material material { get; set; }
        public Renderable(Material material, T data)
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
                    throw new ArgumentException();
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