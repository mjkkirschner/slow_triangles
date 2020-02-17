using renderer.dataStructures;
using renderer.materials;
using renderer.utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace renderer_core.Shaders.ForwardRenderingShaders
{

    public class Base3dForwardRenderingShader : IShader
    {
        protected Matrix4x4 ViewModelMatrix;
        protected Matrix4x4 ProjectionMatrix;
        protected Matrix4x4 ViewportMatrix;
        public Base3dForwardRenderingShader(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 viewPort)
        {
            this.ViewModelMatrix = viewMatrix;
            this.ProjectionMatrix = projectionMatrix;
            this.ViewportMatrix = viewPort;
        }

        public virtual Vector3 VertexToFragment(Mesh mesh, int triangleIndex, int vertIndex)
        {
            var currentVert = mesh.VertexData[mesh.Triangles[triangleIndex].vertIndexList[vertIndex] - 1].ToVector3();
            var currentUV = mesh.VertexUVData[mesh.Triangles[triangleIndex].UVIndexList[vertIndex] - 1];
            var currentNormal = mesh.VertexNormalData[mesh.Triangles[triangleIndex].NormalIndexList[vertIndex] - 1];

            var mvp = Matrix4x4.Transpose(Matrix4x4.Multiply(ViewModelMatrix, ProjectionMatrix));
            var final = Matrix4x4.Multiply(ViewportMatrix, mvp);
            return currentVert.ApplyMatrix(final);
        }
        public virtual bool FragmentToRaster(Material mat, Vector3 baryCoords, ref Color color)
        {
            var channel = System.Math.Min(255, (int)(255 * 1.0));
            color = Color.FromArgb(channel, channel, channel);
            return true;
        }

    }


    /// <summary>
    /// shader contributes constant ambient intensity * color from a texture map.
    /// </summary>
    public class ForwardAmbientShader : Base3dForwardRenderingShader
    {
        //TODO lets move these to an vert_out block.
        protected Vector2[] varying_UVCoord = new Vector2[3];
        public float uniform_ambientCoef = 1.0f;
            
        public override Vector3 VertexToFragment(Mesh mesh, int triangleIndex, int vertIndex)
        {
            var currentUV = mesh.VertexUVData[mesh.Triangles[triangleIndex].UVIndexList[vertIndex] - 1];
            varying_UVCoord[vertIndex] = currentUV;
            return base.VertexToFragment(mesh, triangleIndex, vertIndex);
        }

        public override bool FragmentToRaster(Material mat, Vector3 baryCoords, ref Color color)
        {
            var intensity = Math.Min(1, Math.Max(0, uniform_ambientCoef));

            var U = varying_UVCoord[0].X * baryCoords.X + varying_UVCoord[1].X * baryCoords.Y + varying_UVCoord[2].X * baryCoords.Z;
            var V = varying_UVCoord[0].Y * baryCoords.X + varying_UVCoord[1].Y * baryCoords.Y + varying_UVCoord[2].Y * baryCoords.Z;

            var interpolatedUV = new Vector2(U, V);
            //TODO more extensible, don't fail on different materials.
            var diffColor = (mat as DiffuseMaterial).DiffuseTexture.GetColorAtUV(interpolatedUV);
            color = Color.FromArgb((int)(diffColor.R * intensity), (int)(diffColor.G * intensity), (int)(diffColor.B * intensity));
            return true;
        }

        public ForwardAmbientShader(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 viewPort)
           : base(viewMatrix, projectionMatrix, viewPort)
        {
   
        }

    }

    public class ForwardDirectionalLightShader : ForwardAmbientShader
    {
        public Vector3 uniform_directionalLightVector;
        public Color uniform_directionalLightColor;

        //these uniform fields are used for normal transformation
        protected Matrix4x4 uniform_modelViewProjection;
        protected Matrix4x4 uniform_mpvInvertTranspose;

        protected Matrix4x4 viewMatrixInverseTranspose;
        protected Vector3[] varying_lightDir_tangentSpace = new Vector3[3];

        public ForwardDirectionalLightShader(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 viewPort)
           : base(viewMatrix, projectionMatrix, viewPort)
        {
            Matrix4x4 invertMat;
            Matrix4x4.Invert(viewMatrix, out invertMat);
            this.viewMatrixInverseTranspose = Matrix4x4.Transpose(invertMat);

            uniform_modelViewProjection = Matrix4x4.Multiply(viewMatrix, projectionMatrix);
            Matrix4x4.Invert(uniform_modelViewProjection, out invertMat);
            uniform_mpvInvertTranspose = Matrix4x4.Transpose(invertMat);
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



            var cameraSpaceLightDir = Vector3.Transform(this.uniform_directionalLightVector, matrix);
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

            //intensity is based on normal map and light direction from directional light in tangent space.
            var intensity = Math.Min(1.0, Math.Max(0.0, Vector3.Dot(normalFromTex, interpolatedLightVector)));

            color = Color.FromArgb(
                 (int)Math.Min((diffColor.R * intensity * uniform_directionalLightColor.R), 255),
                 (int)Math.Min((diffColor.G * intensity * uniform_directionalLightColor.G), 255),
                 (int)Math.Min((diffColor.B * intensity * uniform_directionalLightColor.B), 255));

            //TODO add our specular component here.

            return true;
        }


    }


}
