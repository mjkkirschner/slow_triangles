using System.Drawing;
using System.Numerics;
using renderer.dataStructures;
using renderer.materials;

namespace renderer.shaders
{
    public class TextureShader : Shader
    {
        float[] varying_intensity = new float[3];
        Vector2[] varying_UVCoord = new Vector2[3];
        //TODO maybe these methods should be generic to the type of material.

        public override Vector3 VertexToFragment(Mesh mesh, int triangleIndex, int vertIndex)
        {
            var currentNormal = mesh.VertexNormalData[mesh.Triangles[triangleIndex].NormalIndexList[vertIndex] - 1];
            var currentUV = mesh.VertexUVData[mesh.Triangles[triangleIndex].UVIndexList[vertIndex] - 1];
            varying_UVCoord[vertIndex] = currentUV;


            //dot normal*light = intensity for vert.
            varying_intensity[vertIndex] = System.Math.Max(0, Vector3.Dot(currentNormal, LightDirection));

            //we don't do any projection in this shader
            return base.VertexToFragment(mesh, triangleIndex, vertIndex);
        }

        public override bool FragmentToRaster(Material mat, Vector3 baryCoords, ref Color color)
        {
            var varying_vector_int = new Vector3(varying_intensity[0], varying_intensity[1], varying_intensity[2]);
            var intensity = Vector3.Dot(varying_vector_int, baryCoords);

            var U = varying_UVCoord[0].X * baryCoords.X + varying_UVCoord[1].X * baryCoords.Y + varying_UVCoord[2].X * baryCoords.Z;
            var V = varying_UVCoord[0].Y * baryCoords.X + varying_UVCoord[1].Y * baryCoords.Y + varying_UVCoord[2].Y * baryCoords.Z;

            var interpolatedUV = new Vector2(U, V);
            var diffColor = (mat as DiffuseMaterial).DiffuseTexture.GetColorAtUV(interpolatedUV);
            color = Color.FromArgb((int)(diffColor.R * intensity), (int)(diffColor.G * intensity), (int)(diffColor.B * intensity));
            return true;
        }
    }
}

namespace renderer.materials
{
    public class DiffuseMaterial : Material
    {
        public Texture2d DiffuseTexture;
    }
}
