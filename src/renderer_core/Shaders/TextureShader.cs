using System;
using System.Drawing;
using System.Numerics;
using renderer.dataStructures;
using renderer.materials;
using renderer.utilities;
using renderer_core.dataStructures;
using System.Linq;

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


            //TODO comment out just for build.
            //var cameraSpaceLightDir = Vector3.Transform(this.uniform_light_array.Direction, matrix);
            //varying_lightDir_tangentSpace[vertIndex] = Vector3.Normalize(Vector3.Transform(cameraSpaceLightDir, TBN));

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
        //holds all lights that light this geometry.
        public ILight[] uniform_light_array;
        public Vector3 uniform_cam_world_pos;

        //normals for each tri vert.
        protected Vector3[] varying_normal;
        //the projected  tri verts.
        protected Vector3[] varying_vertex_world;

        public override Vector3 VertexToFragment(Mesh mesh, int triangleIndex, int vertIndex)
        {
            var currentNormal = mesh.VertexNormalData[mesh.Triangles[triangleIndex].NormalIndexList[vertIndex] - 1];
            var currentVert = mesh.VertexData[mesh.Triangles[triangleIndex].vertIndexList[vertIndex] - 1];
            var currentUV = mesh.VertexUVData[mesh.Triangles[triangleIndex].UVIndexList[vertIndex] - 1];
            varying_UVCoord[vertIndex] = currentUV;
            varying_normal[vertIndex] = currentNormal;

            var resultVert = base.VertexToFragment(mesh, triangleIndex, vertIndex);
            varying_vertex_world[vertIndex] = currentVert.ToVector3();

            return resultVert;
        }

        public override bool FragmentToRaster(Material mat, Vector3 baryCoords, ref Color color)
        {

            void calcPhongLighting()
            {

            }

            Vector3 reflect(Vector3 vector, Vector3 normal)
            {
                return Vector3.Normalize(vector - normal * (normal * vector * 2.0f));
            }

            var U = varying_UVCoord[0].X * baryCoords.X + varying_UVCoord[1].X * baryCoords.Y + varying_UVCoord[2].X * baryCoords.Z;
            var V = varying_UVCoord[0].Y * baryCoords.X + varying_UVCoord[1].Y * baryCoords.Y + varying_UVCoord[2].Y * baryCoords.Z;

            var interpolatedUV = new Vector2(U, V);

            var NormX = varying_normal[0].X * baryCoords.X + varying_normal[1].X * baryCoords.Y + varying_normal[2].X * baryCoords.Z;
            var NormY = varying_normal[0].Y * baryCoords.X + varying_normal[1].Y * baryCoords.Y + varying_normal[2].Y * baryCoords.Z;
            var NormZ = varying_normal[0].Z * baryCoords.X + varying_normal[1].Z * baryCoords.Y + varying_normal[2].Z * baryCoords.Z;

            var VX = varying_vertex_world[0].X * baryCoords.X + varying_vertex_world[1].X * baryCoords.Y + varying_vertex_world[2].X * baryCoords.Z;
            var VY = varying_vertex_world[0].Y * baryCoords.X + varying_vertex_world[1].Y * baryCoords.Y + varying_vertex_world[2].Y * baryCoords.Z;
            var VZ = varying_vertex_world[0].Z * baryCoords.X + varying_vertex_world[1].Z * baryCoords.Y + varying_vertex_world[2].Z * baryCoords.Z;

            //calculate the normal using the barycentric coordinates of the current pixel and the three vertex normals.
            var interpolatedNormal = Vector3.Normalize(new Vector3(NormX, NormY, NormZ));
            var interpolatedV = new Vector3(VX, VY, VZ);

            //TODO for now only handle directional lights.
            foreach (var light in uniform_light_array.OfType<DirectionalLight>())
            {

                var L = Vector3.Normalize(light.Direction); // directional light vector is just the light direction... position is irrelevant.
                var Eye = (Vector3.Normalize(uniform_cam_world_pos - interpolatedV));  // we are in Eye Coordinates, so EyePos is (0,0,0).
                var Reflect = Vector3.Normalize(reflect(-L, interpolatedNormal));

                //make up some terms
                var lightIntensity = 1;

                //diffuse term
                //var diffColor = (mat as DiffuseMaterial).DiffuseTexture.GetColorAtUV(interpolatedUV);
                var diffColor = Color.FromArgb(204, 102, 0);
                Color diffTerm = (diffColor.ToVector3() * MathF.Max(Vector3.Dot(interpolatedNormal, L), 0.0f)).ToColor();
                var clampedDiffTerm = Vector3.Clamp(diffTerm.ToVector3(), Vector3.Zero, new Vector3(255, 255, 255));

                Vector3 ambientTerm = (new Vector3(uniform_ambient, uniform_ambient, uniform_ambient)) * diffColor.ToVector3();

                //spec
                var matShiny = 500f;
                var lightSpecColor = Color.White;
                var materiLSpecColor = Color.White;
                var KS = 1.0f;

                var clampedSpecTerm = Vector3.Zero;
                if (clampedDiffTerm.Length() > 0)
                {
                var specFactor = MathF.Pow(MathF.Max(Vector3.Dot(Eye, Reflect), 0.0f), matShiny);
                var specTerm = light.Color.ToVector3() * specFactor;
                Console.WriteLine(specTerm);
                //clampedSpecTerm = reflectCol2.ToVector3();
                //clampedSpecTerm = Vector3.Clamp(specTerm, Vector3.Zero, new Vector3(255, 255, 255));
                clampedSpecTerm = Vector3.Clamp(specTerm, Vector3.Zero, new Vector3(255, 255, 255));
                }

                color = (color.ToVector3() + ambientTerm + clampedDiffTerm + clampedSpecTerm).ToColor();
                //color = (color.ToVector3() + clampedSpecTerm).ToColor();

            }


            return true;
        }

        public Lit_TextureShader(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 viewPort)
           : base(viewMatrix, projectionMatrix, viewPort)
        {
            varying_normal = new Vector3[3];
            varying_vertex_world = new Vector3[3];
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
