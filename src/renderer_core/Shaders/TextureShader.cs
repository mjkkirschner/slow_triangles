using System;
using System.Drawing;
using System.Numerics;
using renderer.dataStructures;
using renderer.materials;
using renderer.utilities;
using renderer_core.dataStructures;
using System.Linq;
using System.Diagnostics;

namespace renderer.shaders
{

    /// <summary>
    /// Normal shader which supports diffuse and normal maps, but only a single directional light.
    /// </summary>
    public class Single_DirLight_NormalShader : Unlit_TextureShader
    {
        public DirectionalLight uniform_dir_light;
        public bool uniform_debug_normal;

        //these uniform fields are used for normal transformation
        protected Matrix4x4 modelViewProjection;
        protected Matrix4x4 mpvInvertTranspose;

        protected Matrix4x4 viewMatrixInverseTranspose;
        protected Vector3[] varying_lightDir_tangentSpace = new Vector3[3];

        public Single_DirLight_NormalShader(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 viewPort)
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


            var cameraSpaceLightDir = Vector3.Transform(this.uniform_dir_light.Direction, matrix);
            varying_lightDir_tangentSpace[vertIndex] = Vector3.Normalize(Vector3.Transform(cameraSpaceLightDir, TBN));

            return base.VertexToFragment(mesh, triangleIndex, vertIndex);
        }

        public override bool FragmentToRaster(Material mat, Vector3 baryCoords, ref Color color)
        {
            var U = varying_UVCoord[0].X * baryCoords.X + varying_UVCoord[1].X * baryCoords.Y + varying_UVCoord[2].X * baryCoords.Z;
            var V = varying_UVCoord[0].Y * baryCoords.X + varying_UVCoord[1].Y * baryCoords.Y + varying_UVCoord[2].Y * baryCoords.Z;

            var interpolatedUV = new Vector2(U, V);
            var diffColor = (mat as NormalMaterial).DiffuseTexture.GetColorAtUV(interpolatedUV);
            //TODO add a base color or fallback color somehow if texture is not set...?
            //var diffColor = Color.LightGray;
            var normalFromTex = Vector3.Normalize((mat as NormalMaterial).NormalMap.GetColorAtUV(interpolatedUV).ToVector3());

            //TODO convert this into matrix multiplication.
            //interpolate the tangentspace light vectors which were computed for each vertex.
            var interpx = varying_lightDir_tangentSpace[0].X * baryCoords.X + varying_lightDir_tangentSpace[1].X * baryCoords.Y + varying_lightDir_tangentSpace[2].X * baryCoords.Z;
            var interpy = varying_lightDir_tangentSpace[0].Y * baryCoords.X + varying_lightDir_tangentSpace[1].Y * baryCoords.Y + varying_lightDir_tangentSpace[2].Y * baryCoords.Z;
            var interpz = varying_lightDir_tangentSpace[0].Z * baryCoords.X + varying_lightDir_tangentSpace[1].Z * baryCoords.Y + varying_lightDir_tangentSpace[2].Z * baryCoords.Z;

            var interpolatedLightVector = new Vector3(interpx, interpy, interpz);

            var intensity = Math.Min(1.0, Math.Max(0.0, Vector3.Dot(normalFromTex, interpolatedLightVector)));

            //ambient acts as minimum brightness.
            color = Color.FromArgb(
                 Math.Clamp((int)Math.Min(diffColor.R * intensity * uniform_ambient, 255), 0, 255),
                 Math.Clamp((int)Math.Min(diffColor.G * intensity * uniform_ambient, 255), 0, 255),
                 Math.Clamp((int)Math.Min(diffColor.B * intensity * uniform_ambient, 255), 0, 255));

            //debugging...
            if (uniform_debug_normal)
            {
                color = Color.FromArgb(
                       Math.Clamp((int)(normalFromTex.X * 255f), 0, 255),
                  Math.Clamp((int)(normalFromTex.Y * 255f), 0, 255),
                  Math.Clamp((int)(normalFromTex.Z * 255f), 0, 255));

            }

            return true;
        }


    }

    public class Lit_SpecularTextureShader : Unlit_TextureShader
    {
        //holds all lights that light this geometry.
        public ILight[] uniform_light_array;
        public Vector3 uniform_cam_world_pos;
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
            var specmat = mat as SpecularMaterial;
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
                var Reflect = (reflect(-L, interpolatedNormal));

                //diffuse term
                var diffColor = (mat as DiffuseMaterial).DiffuseTexture.GetColorAtUV(interpolatedUV);
                Color diffTerm = (specmat.Kd * (diffColor.ToVector3() * specmat.Kd * light.Color.ToVector3()) * (float)light.Intensity * MathF.Max(Vector3.Dot(interpolatedNormal, L), 0.0f)).ToColor();
                var clampedDiffTerm = Vector3.Clamp(diffTerm.ToVector3(), Vector3.Zero, new Vector3(255, 255, 255));



                var clampedSpecTerm = Vector3.Zero;
                if (clampedDiffTerm.Length() > 0)
                {
                    var specFactor = MathF.Pow(MathF.Max(Vector3.Dot(Eye, Reflect), 0.0f), specmat.Shininess);
                    var specTerm = light.Color.ToVector3() * (float)light.Intensity * specFactor * specmat.Ks;
                    clampedSpecTerm = Vector3.Clamp(specTerm, Vector3.Zero, new Vector3(255, 255, 255));

                }
                color = (color.ToVector3() + clampedDiffTerm + clampedSpecTerm).ToColor();

            }
            var diffColorOriginal = (mat as DiffuseMaterial).DiffuseTexture.GetColorAtUV(interpolatedUV);
            Vector3 ambientTerm = uniform_ambient * diffColorOriginal.ToVector3();
            color = (color.ToVector3() + ambientTerm).ToColor();

            return true;
        }

        public Lit_SpecularTextureShader(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 viewPort)
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

        /// <summary>
        /// This is set here for other derived classes which use normals, but this shader does not access this in the fragment shader.
        /// </summary>
        protected Vector3[] varying_normal = new Vector3[3];

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

            var diffColor = (mat as DiffuseMaterial).DiffuseTexture.GetColorAtUV(interpolatedUV);
            var colorvec = new Vector3((int)(uniform_ambient * diffColor.R), (int)(uniform_ambient * diffColor.G), (int)(uniform_ambient * diffColor.B));
            color = Vector3.Clamp(colorvec, Vector3.Zero, new Vector3(255, 255, 255)).ToColor();
            return true;
        }
        public Unlit_TextureShader(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 viewPort)
           : base(viewMatrix, projectionMatrix, viewPort)
        {

        }

    }

    public class Single_DirLight_TextureShader : Unlit_TextureShader
    {
        public DirectionalLight uniform_dir_light;

        public Single_DirLight_TextureShader(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 viewPort)
          : base(viewMatrix, projectionMatrix, viewPort)
        {

        }

        public override bool FragmentToRaster(Material mat, Vector3 baryCoords, ref Color color)
        {
            var U = varying_UVCoord[0].X * baryCoords.X + varying_UVCoord[1].X * baryCoords.Y + varying_UVCoord[2].X * baryCoords.Z;
            var V = varying_UVCoord[0].Y * baryCoords.X + varying_UVCoord[1].Y * baryCoords.Y + varying_UVCoord[2].Y * baryCoords.Z;

            var NormX = varying_normal[0].X * baryCoords.X + varying_normal[1].X * baryCoords.Y + varying_normal[2].X * baryCoords.Z;
            var NormY = varying_normal[0].Y * baryCoords.X + varying_normal[1].Y * baryCoords.Y + varying_normal[2].Y * baryCoords.Z;
            var NormZ = varying_normal[0].Z * baryCoords.X + varying_normal[1].Z * baryCoords.Y + varying_normal[2].Z * baryCoords.Z;

            var interpolatedUV = new Vector2(U, V);
            var interpolatedNormal = Vector3.Normalize(new Vector3(NormX, NormY, NormZ));

            var intensity = Math.Min(1.0, Math.Max(0.0, Vector3.Dot(interpolatedNormal, this.uniform_dir_light.Direction)));


            var diffColor = (mat as DiffuseMaterial).DiffuseTexture.GetColorAtUV(interpolatedUV);
            var colorvec = new Vector3((int)(uniform_ambient * diffColor.R * intensity), (int)(uniform_ambient * diffColor.G * intensity), (int)(uniform_ambient * diffColor.B * intensity));
            color = Vector3.Clamp(colorvec, Vector3.Zero, new Vector3(255, 255, 255)).ToColor();
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
    public class SpecularMaterial : DiffuseMaterial
    {
        /// <summary>
        /// power to raise specular light to. (higher is smaller highlight)
        /// </summary>
        public float Shininess;
        /// <summary>
        /// ratio of specular light to other light. lower is less bright specular light. 0 -1.0 is usual.
        /// </summary>
        public float Ks;
        /// <summary>
        /// ratio of diffuse light.
        /// </summary>
        public float Kd;
    }
    public class NormalMaterial : DiffuseMaterial
    {
        public Texture2d NormalMap;
    }
}
