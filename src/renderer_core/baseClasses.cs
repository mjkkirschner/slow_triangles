
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Linq;
using renderer.utilities;

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


    public class Texture2d
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        //span?
        public IList<Color> ColorData { get; set; }

        public Color GetColorAtUV(Vector2 UV)
        {
            var x = UV.X * Width;
            var y = UV.Y * Height;

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

        public Mesh(IList<TriangleFace> tris, IList<Vector4> verts, IList<Vector3> normals, IList<Vector2> uvs)
        {
            this.Triangles = tris;
            this.VertexData = verts;
            this.VertexNormalData = normals;
            this.VertexUVData = uvs;
        }
    }



}