using System;
using System.Drawing;
using System.Numerics;
using renderer.dataStructures;
using renderer.materials;
using renderer.utilities;

namespace renderer.shaders
{


    public class SkyBoxShader : ImageBasedLightingShader
    {

        public override bool FragmentToRaster(IMaterial mat, Vector3 baryCoords, ref Color color)
        {

            var VX = varying_vertex_world[0].X * baryCoords.X + varying_vertex_world[1].X * baryCoords.Y + varying_vertex_world[2].X * baryCoords.Z;
            var VY = varying_vertex_world[0].Y * baryCoords.X + varying_vertex_world[1].Y * baryCoords.Y + varying_vertex_world[2].Y * baryCoords.Z;
            var VZ = varying_vertex_world[0].Z * baryCoords.X + varying_vertex_world[1].Z * baryCoords.Y + varying_vertex_world[2].Z * baryCoords.Z;

            var VCX = varying_vertex_clip[0].X * baryCoords.X + varying_vertex_clip[1].X * baryCoords.Y + varying_vertex_clip[2].X * baryCoords.Z;
            var VCY = varying_vertex_clip[0].Y * baryCoords.X + varying_vertex_clip[1].Y * baryCoords.Y + varying_vertex_clip[2].Y * baryCoords.Z;
            var VCZ = varying_vertex_clip[0].Z * baryCoords.X + varying_vertex_clip[1].Z * baryCoords.Y + varying_vertex_clip[2].Z * baryCoords.Z;

            var U = varying_UVCoord[0].X * baryCoords.X + varying_UVCoord[1].X * baryCoords.Y + varying_UVCoord[2].X * baryCoords.Z;
            var V = varying_UVCoord[0].Y * baryCoords.X + varying_UVCoord[1].Y * baryCoords.Y + varying_UVCoord[2].Y * baryCoords.Z;

            var interpolatedV = new Vector3(VX, VY, VZ);
            var interpolatedCV = new Vector3(VCX / 512, VCY / 1024, VCZ / 255);
            var interpolatedUV = new Vector2(U, V);

            var Eye = Vector3.Normalize(interpolatedV - uniform_cam_world_pos);


            var rectTexCoords = convertVector3ToEquirectangularUVCoords(Eye, (mat as ImageBasedLightMaterial).LightTexture);
            var indirectDiffuse = (mat as ImageBasedLightMaterial).LightTexture.GetColorAtUV(rectTexCoords).ToVector3();
            //(((Eye +Vector3.One)/2f) * 255).ToColor();
            color = Vector3.Clamp(indirectDiffuse, Vector3.Zero, new Vector3(255, 255, 255)).ToColor();
            return true;
        }

        public SkyBoxShader(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 viewPort)
          : base(viewMatrix, projectionMatrix, viewPort)
        {

        }
    }

    /// <summary>
    /// Shader displays an unlit texture - only ambient light intensity can be modified to light the scene.
    /// </summary>
    public class ImageBasedLightingShader : Unlit_TextureShader
    {
        //TODO maybe move these into base3dshader?
        public Vector3 uniform_cam_world_pos;
        //the projected  tri verts.
        protected Vector3[] varying_vertex_world;
        protected Vector3[] varying_vertex_clip;

        public override Vector3 VertexToFragment(Mesh mesh, int triangleIndex, int vertIndex)
        {
            var currentNormal = mesh.VertexNormalData[mesh.Triangles[triangleIndex].NormalIndexList[vertIndex] - 1];
            var currentUV = mesh.VertexUVData[mesh.Triangles[triangleIndex].UVIndexList[vertIndex] - 1];
            var currentVert = mesh.VertexData[mesh.Triangles[triangleIndex].vertIndexList[vertIndex] - 1];
            varying_UVCoord[vertIndex] = currentUV;
            varying_normal[vertIndex] = currentNormal;
            varying_vertex_world[vertIndex] = currentVert.ToVector3();

            var clipvert = base.VertexToFragment(mesh, triangleIndex, vertIndex);
            varying_vertex_clip[vertIndex] = clipvert;
            return clipvert;
        }

        public override bool FragmentToRaster(IMaterial mat, Vector3 baryCoords, ref Color color)
        {
            var U = varying_UVCoord[0].X * baryCoords.X + varying_UVCoord[1].X * baryCoords.Y + varying_UVCoord[2].X * baryCoords.Z;
            var V = varying_UVCoord[0].Y * baryCoords.X + varying_UVCoord[1].Y * baryCoords.Y + varying_UVCoord[2].Y * baryCoords.Z;

            var NormX = varying_normal[0].X * baryCoords.X + varying_normal[1].X * baryCoords.Y + varying_normal[2].X * baryCoords.Z;
            var NormY = varying_normal[0].Y * baryCoords.X + varying_normal[1].Y * baryCoords.Y + varying_normal[2].Y * baryCoords.Z;
            var NormZ = varying_normal[0].Z * baryCoords.X + varying_normal[1].Z * baryCoords.Y + varying_normal[2].Z * baryCoords.Z;

            var VX = varying_vertex_world[0].X * baryCoords.X + varying_vertex_world[1].X * baryCoords.Y + varying_vertex_world[2].X * baryCoords.Z;
            var VY = varying_vertex_world[0].Y * baryCoords.X + varying_vertex_world[1].Y * baryCoords.Y + varying_vertex_world[2].Y * baryCoords.Z;
            var VZ = varying_vertex_world[0].Z * baryCoords.X + varying_vertex_world[1].Z * baryCoords.Y + varying_vertex_world[2].Z * baryCoords.Z;

            var interpolatedUV = new Vector2(U, V);
            var interpolatedNormal = Vector3.Normalize(new Vector3(NormX, NormY, NormZ));
            var interpolatedV = new Vector3(VX, VY, VZ);

            var Eye = (Vector3.Normalize(interpolatedV - uniform_cam_world_pos));
            var NDotEye = Vector3.Dot(Eye, interpolatedNormal);
            var R = reflect(Eye, interpolatedNormal);


            var textureColor = Color.FromArgb(128, 128, 128);//((mat as ImageBasedLightMaterial).DiffuseTexture.GetColorAtUV(interpolatedUV).ToVector3()).ToColor();
            var reflectivity = .8f;
            var kd = .05f;
            var oneMinusReflectivity = 1f - reflectivity;
            //this makes the diffuse light darker the shinier an object is.
            var albedoColor = Vector3Extensions.Lerp(Vector3.Zero, textureColor.ToVector3(), oneMinusReflectivity);

            //leave this out for now - this is just our normal NdotL term.
            //var directDiffuse = ()
            var rectTexCoords = convertVector3ToEquirectangularUVCoords(interpolatedNormal, (mat as ImageBasedLightMaterial).LightTexture);
            var indirectDiffuse = (mat as ImageBasedLightMaterial).LightTexture.GetColorAtUV(rectTexCoords).ToVector3() * kd;

            //we would add directDiffuse to indirectDiff.
            var diffuse = albedoColor * (indirectDiffuse);
            //TODO do we need to rescale R vector?
            var reflectrectTexCoords = convertVector3ToEquirectangularUVCoords(R, (mat as ImageBasedLightMaterial).LightTexture);
            var indirectSpecular = (mat as ImageBasedLightMaterial).LightTexture.GetColorAtUV(reflectrectTexCoords).ToVector3() * kd;

            color = Vector3.Clamp(diffuse + indirectSpecular, Vector3.Zero, new Vector3(255, 255, 255)).ToColor();
            return true;
        }
        public ImageBasedLightingShader(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 viewPort)
           : base(viewMatrix, projectionMatrix, viewPort)
        {
            varying_normal = new Vector3[3];
            varying_vertex_world = new Vector3[3];
            varying_vertex_clip = new Vector3[3];
        }



    }
}