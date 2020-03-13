using renderer.dataStructures;
using renderer.utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace renderer.shaders
{

    /// <summary>
    /// Shader renders shadows to a shadow map texture
    /// </summary>
    public class ShadowMapGenShader : Base3dShader
    {
        protected Vector3[] varying_vertex;

        public override Vector3 VertexToFragment(Mesh mesh, int triangleIndex, int vertIndex)
        {

            var result= base.VertexToFragment(mesh, triangleIndex, vertIndex);
            varying_vertex[vertIndex] = result;
            return result;
        }

        public override bool FragmentToRaster(IMaterial mat, Vector3 baryCoords, ref Color color)
        {
            var VX = varying_vertex[0].X * baryCoords.X + varying_vertex[1].X * baryCoords.Y + varying_vertex[2].X * baryCoords.Z;
            var VY = varying_vertex[0].Y * baryCoords.X + varying_vertex[1].Y * baryCoords.Y + varying_vertex[2].Y * baryCoords.Z;
            var VZ = varying_vertex[0].Z * baryCoords.X + varying_vertex[1].Z * baryCoords.Y + varying_vertex[2].Z * baryCoords.Z;

            var interpolatedV = new Vector3(VX, VY, VZ);

            //we only care about depth
            //TODO what is the domain of z?
            var dist = MathExtensions.Clamp((int)(interpolatedV.Z),0,255);
            color = Color.FromArgb(255, dist, dist, dist);
            
            return true;
        }

        public ShadowMapGenShader(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 viewPort)
         : base(viewMatrix, projectionMatrix, viewPort)
        {
           varying_vertex = new Vector3[3];
        }
    }



}
