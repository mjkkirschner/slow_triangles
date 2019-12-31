using System;
using System.Drawing;
using System.Numerics;
using renderer.dataStructures;
using renderer.materials;
using renderer.utilities;

namespace renderer.shaders
{

    //TODO move to new file.
    public class NormalShader : TextureShader
    {
        //these uniform fields are used for normal transformation
        protected Matrix4x4 modelViewProjection;
        protected Matrix4x4 mpvInvertTranspose;

        protected Matrix4x4 viewMatrixInverseTranspose;
        protected Vector3[] varying_lightDir_tangentSpace = new Vector3[3];

        public float ambientCoef = 5;

        public NormalShader(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 viewPort)
           : base(viewMatrix, projectionMatrix, viewPort)
        {
            Matrix4x4 invertMat;
            Matrix4x4.Invert(viewMatrix, out invertMat);
            this.viewMatrixInverseTranspose = Matrix4x4.Transpose(invertMat);

            modelViewProjection = Matrix4x4.Multiply(viewMatrix, projectionMatrix);
            Matrix4x4.Invert(modelViewProjection, out invertMat);
            mpvInvertTranspose = Matrix4x4.Transpose(invertMat);
        }

        public override Vector3 VertexToFragment(Mesh mesh, int triangleIndex, int vertIndex)
        {
            //successful pt and vector transfoms in this library seem to required transpose.
            var matrix = Matrix4x4.Transpose(ViewModelMatrix);
            var currentNormal = Vector3.Transform(mesh.VertexNormalData[mesh.Triangles[triangleIndex].NormalIndexList[vertIndex] - 1], matrix);

            var currentTan = Vector3.Transform(mesh.VertTangents[mesh.Triangles[triangleIndex].vertIndexList[vertIndex] - 1], matrix);
            var currentBiNorm = Vector3.Transform(mesh.VertBiNormals[mesh.Triangles[triangleIndex].vertIndexList[vertIndex] - 1], matrix);

            //we don't transpose this matrix because WE want the transpose...
            var TBN = (new Matrix4x4(currentTan.X, currentBiNorm.X, currentNormal.X, 0,
                                                                            currentTan.Y, currentBiNorm.Y, currentNormal.Y, 0,
                                                                            currentTan.Z, currentBiNorm.Z, currentNormal.Z, 0,
                                                                            0, 0, 0, 1));



            var cameraSpaceLightDir = Vector3.Transform(this.LightDirection, matrix);
            varying_lightDir_tangentSpace[vertIndex] = Vector3.Normalize(Vector3.Transform(cameraSpaceLightDir, TBN));

            return base.VertexToFragment(mesh, triangleIndex, vertIndex);
        }

        public override bool FragmentToRaster(Material mat, Vector3 baryCoords, ref Color color)
        {
            var varying_vector_int = new Vector3(varying_intensity[0], varying_intensity[1], varying_intensity[2]);

            var U = varying_UVCoord[0].X * baryCoords.X + varying_UVCoord[1].X * baryCoords.Y + varying_UVCoord[2].X * baryCoords.Z;
            var V = varying_UVCoord[0].Y * baryCoords.X + varying_UVCoord[1].Y * baryCoords.Y + varying_UVCoord[2].Y * baryCoords.Z;

            var interpolatedUV = new Vector2(U, V);
            var diffColor = (mat as NormalMaterial).DiffuseTexture.GetColorAtUV(interpolatedUV);

            var normalFromTex = Vector3.Normalize((mat as NormalMaterial).NormalMap.GetColorAtUV(interpolatedUV).ToVector3());

            //TODO convert this into matrix multiplication.
            //interpolate the tangentspace light vectors which were computed for each vertex.
            var interpx = varying_lightDir_tangentSpace[0].X * baryCoords.X + varying_lightDir_tangentSpace[1].X * baryCoords.Y + varying_lightDir_tangentSpace[2].X * baryCoords.Z;
            var interpy = varying_lightDir_tangentSpace[0].Y * baryCoords.X + varying_lightDir_tangentSpace[1].Y * baryCoords.Y + varying_lightDir_tangentSpace[2].Y * baryCoords.Z;
            var interpz = varying_lightDir_tangentSpace[0].Z * baryCoords.X + varying_lightDir_tangentSpace[1].Z * baryCoords.Y + varying_lightDir_tangentSpace[2].Z * baryCoords.Z;

            var interpolatedLightVector = new Vector3(interpx, interpy, interpz);

            var intensity = Math.Min(1.0, Math.Max(0.0, Vector3.Dot(normalFromTex, interpolatedLightVector)));

            color = Color.FromArgb(
                 (int)Math.Min(ambientCoef + (diffColor.R * intensity), 255),
                 (int)Math.Min(ambientCoef + (diffColor.G * intensity), 255),
                 (int)Math.Min(ambientCoef + (diffColor.B * intensity), 255));
            //color = Color.FromArgb((int)(interpolatedLightVector.X * 255f), (int)(interpolatedLightVector.Y * 255f), (int)(interpolatedLightVector.Z * 255f));

            return true;
        }


    }

    public class TextureShader : Base3dShader
    {
        protected Vector2[] varying_UVCoord = new Vector2[3];
        //TODO maybe these methods should be generic to the type of material.

        public override Vector3 VertexToFragment(Mesh mesh, int triangleIndex, int vertIndex)
        {
            var currentNormal = mesh.VertexNormalData[mesh.Triangles[triangleIndex].NormalIndexList[vertIndex] - 1];
            var currentUV = mesh.VertexUVData[mesh.Triangles[triangleIndex].UVIndexList[vertIndex] - 1];
            varying_UVCoord[vertIndex] = currentUV;


            //dot normal*light = intensity for vert.
            varying_intensity[vertIndex] = System.Math.Max(0, Vector3.Dot(currentNormal, LightDirection));

            return base.VertexToFragment(mesh, triangleIndex, vertIndex);
        }

        public override bool FragmentToRaster(Material mat, Vector3 baryCoords, ref Color color)
        {
            var varying_vector_int = new Vector3(varying_intensity[0], varying_intensity[1], varying_intensity[2]);
            var intensity = Math.Min(1, Math.Max(0, Vector3.Dot(varying_vector_int, baryCoords)));

            var U = varying_UVCoord[0].X * baryCoords.X + varying_UVCoord[1].X * baryCoords.Y + varying_UVCoord[2].X * baryCoords.Z;
            var V = varying_UVCoord[0].Y * baryCoords.X + varying_UVCoord[1].Y * baryCoords.Y + varying_UVCoord[2].Y * baryCoords.Z;

            var interpolatedUV = new Vector2(U, V);

            var diffColor = (mat as DiffuseMaterial).DiffuseTexture.GetColorAtUV(interpolatedUV);
            color = Color.FromArgb((int)(diffColor.R * intensity), (int)(diffColor.G * intensity), (int)(diffColor.B * intensity));
            return true;
        }

        public TextureShader(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 viewPort)
           : base(viewMatrix, projectionMatrix, viewPort)
        {

        }

    }
}

namespace renderer.materials
{
    public class DiffuseMaterial : Material
    {
        public Texture2d DiffuseTexture;
    }

    public class NormalMaterial : DiffuseMaterial
    {
        public Texture2d NormalMap;
    }
}
