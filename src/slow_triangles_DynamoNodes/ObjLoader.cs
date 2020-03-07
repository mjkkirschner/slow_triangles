using Autodesk.DesignScript.Interfaces;
using renderer.core;
using renderer.dataStructures;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using System.IO;
using System.Numerics;

namespace slow_triangles_DynamoNodes
{
    /// <summary>
    /// Wrapper around slow triangles mesh that displays triangles in the dynamo preview.
    /// </summary>
    public class DynamoMesh : Mesh, IGraphicItem
    {
        [SupressImportIntoVM]
        public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        {
            for (int i = 0; i < Triangles.Count(); i++)
            {
                var currentTri = Triangles[i];
                var vertInds = currentTri.vertIndexList;
                var normInds = currentTri.NormalIndexList;

                var v1 = VertexData[vertInds[0]-1];
                var v2 = VertexData[vertInds[1] - 1];
                var v3 = VertexData[vertInds[2] - 1];

                var n1 = VertexNormalData[normInds[0] - 1];
                var n2 = VertexNormalData[normInds[1] - 1];
                var n3 = VertexNormalData[normInds[2] - 1];

                package.AddTriangleVertex(v1.X, v1.Y, v1.Z);
                package.AddTriangleVertex(v2.X, v2.Y, v2.Z);
                package.AddTriangleVertex(v3.X, v3.Y, v3.Z);

                package.AddTriangleVertexColor(255, 255, 255, 255);
                package.AddTriangleVertexColor(255, 255, 255, 255);
                package.AddTriangleVertexColor(255, 255, 255, 255);

                package.AddTriangleVertexNormal(n1.X, n1.Y, n1.Z);
                package.AddTriangleVertexNormal(n2.X, n2.Y, n2.Z);
                package.AddTriangleVertexNormal(n3.X, n3.Y, n3.Z);


                package.AddTriangleVertexUV(-1, -1);
                package.AddTriangleVertexUV(-1, -1);
                package.AddTriangleVertexUV(-1, -1);
            }

        }

        public DynamoMesh(Mesh internalMesh)
            : base(internalMesh.Triangles,
                 internalMesh.VertexData,
                 internalMesh.VertexNormalData,
                 internalMesh.VertexUVData,
                 internalMesh.VertBiNormals,
                 internalMesh.Tangents)
        {
            VertTangents = internalMesh.VertTangents;
            VertBiNormals = internalMesh.VertBiNormals;
    }

        public DynamoMesh(Mesh internalMesh, Vector3 offset, float scale = 1.0f)
            : base(internalMesh.Triangles,
                 internalMesh.VertexData.Select(vert=>Vector4.Multiply(scale,vert)).Select(x=>Vector4.Add(x,new Vector4(offset.X,offset.Y,offset.Z,1))).ToList(),
                 internalMesh.VertexNormalData,
                 internalMesh.VertexUVData,
                 internalMesh.VertBiNormals,
                 internalMesh.Tangents)
        
        {
            VertTangents = internalMesh.VertTangents;
            VertBiNormals = internalMesh.VertBiNormals;
        }


    }

    public static class ObjLoader
    {
        public static DynamoMesh LoadMeshFromPath(string path)
        {
           return new DynamoMesh( ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo(path)));
        }

        public static DynamoMesh LoadMeshFromOBJString(string objString,Vector3 offset, float scale)
        {
            //hack.
            var path = Path.GetTempFileName();
            File.WriteAllText(path, objString);
            return new DynamoMesh(ObjFileLoader.LoadMeshFromObjAtPath(new System.IO.FileInfo(path)),offset,scale);
        }
    }

}
