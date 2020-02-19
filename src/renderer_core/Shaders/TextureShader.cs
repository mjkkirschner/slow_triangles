using System;
using System.Drawing;
using System.Numerics;
using renderer.dataStructures;
using renderer.materials;
using renderer.utilities;
using renderer_core.dataStructures;

namespace renderer.shaders
{

    /// <summary>
    /// Normal shader which supports diffuse and normal maps, 
    /// </summary>
    public class Lit_NormalShader : Lit_TextureShader
    {
        //these uniform fields are used for normal transformation
        protected Matrix4x4 modelViewProjection;
        protected Matrix4x4 mpvInvertTranspose;

        protected Matrix4x4 viewMatrixInverseTranspose;
        protected Vector3[] varying_lightDir_tangentSpace = new Vector3[3];

        public Lit_NormalShader(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 viewPort)
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



            var cameraSpaceLightDir = Vector3.Transform(this.uniform_dirLight.Direction, matrix);
            varying_lightDir_tangentSpace[vertIndex] = Vector3.Normalize(Vector3.Transform(cameraSpaceLightDir, TBN));

            return base.VertexToFragment(mesh, triangleIndex, vertIndex);
        }

        public override bool FragmentToRaster(Material mat, Vector3 baryCoords, ref Color color)
        {
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

            //TODO - this ambient should probably be multipled with intensity...
            color = Color.FromArgb(
                 (int)Math.Min(uniform_ambient + (diffColor.R * intensity), 255),
                 (int)Math.Min(uniform_ambient + (diffColor.G * intensity), 255),
                 (int)Math.Min(uniform_ambient + (diffColor.B * intensity), 255));
            //color = Color.FromArgb((int)(interpolatedLightVector.X * 255f), (int)(interpolatedLightVector.Y * 255f), (int)(interpolatedLightVector.Z * 255f));

            return true;
        }


    }

    public class Lit_TextureShader : Unlit_TextureShader
    {
        //TODO make array
        public DirectionalLight uniform_dirLight;
        protected Vector3[] varying_normal;

        public override Vector3 VertexToFragment(Mesh mesh, int triangleIndex, int vertIndex)
        {
            var currentNormal = mesh.VertexNormalData[mesh.Triangles[triangleIndex].NormalIndexList[vertIndex] - 1];
            var currentUV = mesh.VertexUVData[mesh.Triangles[triangleIndex].UVIndexList[vertIndex] - 1];
            varying_UVCoord[vertIndex] = currentUV;
            varying_normal[vertIndex] = currentNormal;

            return base.VertexToFragment(mesh, triangleIndex, vertIndex);
        }

        public override bool FragmentToRaster(Material mat, Vector3 baryCoords, ref Color color)
        {

            var U = varying_UVCoord[0].X * baryCoords.X + varying_UVCoord[1].X * baryCoords.Y + varying_UVCoord[2].X * baryCoords.Z;
            var V = varying_UVCoord[0].Y * baryCoords.X + varying_UVCoord[1].Y * baryCoords.Y + varying_UVCoord[2].Y * baryCoords.Z;

            var interpolatedUV = new Vector2(U, V);

            var NormX = varying_normal[0].X * baryCoords.X + varying_normal[1].X * baryCoords.Y + varying_normal[2].X * baryCoords.Z;
            var NormY = varying_normal[0].Y * baryCoords.X + varying_normal[1].Y * baryCoords.Y + varying_normal[2].Y * baryCoords.Z;
            var NormZ = varying_normal[0].Z * baryCoords.X + varying_normal[1].Z * baryCoords.Y + varying_normal[2].Z * baryCoords.Z;

            var interpolatedNormal = new Vector3(NormX, NormY, NormZ);

            //calculate directional light intensity using normal
            var normaldotLight = Math.Clamp(Vector3.Dot(Vector3.Normalize(interpolatedNormal), Vector3.Normalize(uniform_dirLight.Direction)), 0, 1);
            var lightColorContrib = (Vector3.Multiply(uniform_dirLight.Color.ToVector3(), .5f/*intensity*/) * (normaldotLight)).ToColor();
            var texColor = (mat as DiffuseMaterial).DiffuseTexture.GetColorAtUV(interpolatedUV);
            var result = (lightColorContrib.ToVector3() + (new Vector3(uniform_ambient, uniform_ambient, uniform_ambient)) * texColor.ToVector3()).ToColor();
            color = result;
            return true;
        }

        public Lit_TextureShader(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 viewPort)
           : base(viewMatrix, projectionMatrix, viewPort)
        {
            varying_normal = new Vector3[3];
        }
    }


    /// <summary>
    /// Shader displays an unlit texture - only ambient light intensity can be modified to light the scene.
    /// </summary>
    public class Unlit_TextureShader : Base3dShader
    {
        public float uniform_ambient = 1.0f;
        protected Vector2[] varying_UVCoord = new Vector2[3];


        public override Vector3 VertexToFragment(Mesh mesh, int triangleIndex, int vertIndex)
        {
            var currentNormal = mesh.VertexNormalData[mesh.Triangles[triangleIndex].NormalIndexList[vertIndex] - 1];
            var currentUV = mesh.VertexUVData[mesh.Triangles[triangleIndex].UVIndexList[vertIndex] - 1];
            varying_UVCoord[vertIndex] = currentUV;

            return base.VertexToFragment(mesh, triangleIndex, vertIndex);
        }

        public override bool FragmentToRaster(Material mat, Vector3 baryCoords, ref Color color)
        {

            var U = varying_UVCoord[0].X * baryCoords.X + varying_UVCoord[1].X * baryCoords.Y + varying_UVCoord[2].X * baryCoords.Z;
            var V = varying_UVCoord[0].Y * baryCoords.X + varying_UVCoord[1].Y * baryCoords.Y + varying_UVCoord[2].Y * baryCoords.Z;

            var interpolatedUV = new Vector2(U, V);

            var diffColor = (mat as DiffuseMaterial).DiffuseTexture.GetColorAtUV(interpolatedUV);
            color = Color.FromArgb((int)(uniform_ambient + diffColor.R), (int)(uniform_ambient + diffColor.G), (int)(uniform_ambient + diffColor.B));
            return true;
        }

        public Unlit_TextureShader(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 viewPort)
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
