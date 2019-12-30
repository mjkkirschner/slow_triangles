//load our obj data for our test model
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using renderer.dataStructures;
using renderer.interfaces;
using renderer.utilities;

namespace renderer.core
{
    public static class ObjFileLoader
    {
        /// <summary>
        /// Loads verts from an wavefront obj file from a file path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<Vector4> LoadVertsFromObjAtPath(FileInfo path)
        {
            var text = File.ReadAllText(path.FullName);
            var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            var output = new List<Vector4>();
            foreach (var line in lines)
            {
                var split = line.Split(' ');
                if (split.First() == "v")
                {

                    var x = float.Parse(split[1]);
                    var y = float.Parse(split[2]);
                    var z = float.Parse(split[3]);

                    output.Add(new Vector4(x, y, z, 1));
                }

            }
            return output;
        }

        /// <summary>
        /// Loads triangle faces from wavefront obj at filepath
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<TriangleFace> LoadTrisFromObjAtPath(FileInfo path)
        {
            var text = File.ReadAllText(path.FullName);
            var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            var output = new List<TriangleFace>();
            foreach (var line in lines)
            {
                var split = line.Split(' ');

                if (split.First() == "f")
                {

                    var faceData = split.Skip(1).Select(x =>
                    {
                        //each entry in here is /vertI/uvI/normI
                        //ie f v1/vt1/vn1
                        var data = x.Split('/');
                        return data.Select(y => int.Parse(y));
                    });

                    var verts = faceData.Select(x => x.ElementAt(0)).ToArray();
                    var uvs = faceData.Select(x => x.ElementAtOrDefault(1)).ToArray();
                    var normals = faceData.Select(x => x.ElementAtOrDefault(2)).ToArray();
                    output.Add(new TriangleFace(verts, uvs, normals));
                }
            }
            return output;
        }

        public static Mesh LoadMeshFromObjAtPath(FileInfo path)
        {
            var triFaces = LoadTrisFromObjAtPath(path);

            var text = File.ReadAllText(path.FullName);
            var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            var verts = new List<Vector4>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();


            foreach (var line in lines)
            {
                var split = line.Split(' ');

                if (split.First() == "vt")
                {
                    var u = float.Parse(split[1]);
                    var v = float.Parse(split[2]);
                    uvs.Add(new Vector2(u, v));

                }

                else if (split.First() == "vn")
                {
                    var x = float.Parse(split[1]);
                    var y = float.Parse(split[2]);
                    var z = float.Parse(split[3]);
                    normals.Add(Vector3.Normalize(new Vector3(x, y, z)));
                }

                else if (split.First() == "v")
                {
                    var x = float.Parse(split[1]);
                    var y = float.Parse(split[2]);
                    var z = float.Parse(split[3]);
                    verts.Add(new Vector4(x, y, z, 1));
                }
            }


            var tempMesh = new Mesh(triFaces, verts, normals, uvs);
            //TODO this can't be calculated if UVs don't exist.
            foreach (var triface in triFaces)
            {
                //calculate tangents //TODO average these based on shared faces.
                var (tan, binorm) = caluculatetangentSpaceForTri(triface, tempMesh);
                tempMesh.BiNormals_akaBiTangents.Add(binorm);
                tempMesh.Tangents.Add(tan);
            }
            tempMesh.computeAveragedTangents();
            return tempMesh;
        }

        public static (Vector3, Vector3) caluculatetangentSpaceForTri(TriangleFace triangleFace, Mesh mesh)
        {
            var triVerts = triangleFace.vertIndexList.Select(ind => mesh.VertexData[ind - 1].ToVector3()).ToArray();
            var uvs = triangleFace.UVIndexList.Select(ind => mesh.VertexUVData[ind - 1]).ToArray();
            var normals = triangleFace.NormalIndexList.Select(ind => mesh.VertexNormalData[ind - 1]).ToArray();


            var p0 = triVerts[0];
            var p1 = triVerts[1];
            var p2 = triVerts[2];

            var tex0 = uvs[0];
            var tex1 = uvs[1];
            var tex2 = uvs[2];

            var norm0 = normals[0];
            var norm1 = normals[1];
            var norm2 = normals[2];

            var q1 = Vector3.Subtract(p1, p0);
            var q2 = Vector3.Subtract(p2, p0);

            var temp1 = Vector2.Subtract(tex1, tex0);
            var temp2 = Vector2.Subtract(tex2, tex0);

            var s = new Vector2(temp1.X, temp2.X);
            var t = new Vector2(temp1.Y, temp2.Y);

            var recip = 1f / ((temp1.X * temp2.Y) - (temp1.Y * temp2.X));

            //these seem exactly the same...

            var udir = Vector3.Multiply(recip, Vector3.Subtract(Vector3.Multiply(temp2.Y, q1), (Vector3.Multiply(temp1.Y, q2))));
            var vdir = Vector3.Multiply(recip, Vector3.Subtract(Vector3.Multiply(temp1.X, q2), (Vector3.Multiply(temp2.X, q1))));

            //var udir = new Vector3((t.Y * q1.X - t.X * q2.X) * recip, (t.Y * q1.Y - t.X * q2.Y) * recip, (t.Y * q1.Z - t.X * q2.Z) * recip);
            //var vdir = new Vector3((s.X * q2.X - s.Y * q1.X) * recip, (s.X * q2.Y - s.Y * q1.Y) * recip, (s.X * q2.Z - s.Y * q1.Z) * recip);

            return (udir, vdir);

        }



    }
}