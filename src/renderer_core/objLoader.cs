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

                    //if there are more than 3 verts in this face we have potentially encountered a triangle strip
                    // like this F 0 1 2 3 - split into two tris - (012) - (023)
                    if (verts.Count() > 3)
                    {
                        output.Add(new TriangleFace(verts.Take(3).ToArray(), uvs.Take(3).ToArray(), normals.Take(3).ToArray()));
                        for (var i = 3; i < verts.Count(); ++i)
                            output.Add(new TriangleFace(
                                new int[] {
                                     verts[i - 3], verts[i - 1], verts[i] },
                            new int[] { uvs[i - 3], uvs[i - 1], uvs[i] },
                            new int[] { normals[i - 3], normals[i - 1], normals[i] }
                            ));
                    }
                    //simple triangle
                    else
                    {
                        output.Add(new TriangleFace(verts, uvs, normals));
                    }


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
                var split = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (split.Count() < 1)
                {
                    continue;
                }
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
            //we can't compute tangents (at least using uvs) if uvs don't exist.
            if (uvs.Count < 1)
            {
                return tempMesh;
            }

            if (normals.Count < 1)
            {
                //lets calculate some normals
                for (int triIndex =0; triIndex < triFaces.Count;triIndex++)
                {
                    var triface = triFaces[triIndex];
                    calculateAndSetNormalForTri(ref triface, tempMesh);
                    triFaces[triIndex] = triface;
                }
                //TODO get rid of this.
                //update this property manually with updated structs
                tempMesh.Triangles = triFaces;
                //all verts now have normal indices and some faceted normal data.
                tempMesh.computeAveragedNormals();
            }


            foreach (var triface in triFaces)
            {
                var (tan, binorm) = calculateTangetSpaceForTri(triface, tempMesh);
                tempMesh.BiNormals_akaBiTangents.Add(binorm);
                tempMesh.Tangents.Add(tan);
            }
            tempMesh.computeAveragedTangents();
            return tempMesh;
        }

        private static void calculateAndSetNormalForTri(ref TriangleFace triangleFace, Mesh mesh)
        {
            //if the normal list is empty for this mesh or null - set it to be the same size
            //as the list of verts. we'll generate on normal per vert.
            if (mesh.VertexNormalData == null || mesh.VertexNormalData.Count < 1)
            {
                mesh.VertexNormalData = Enumerable.Repeat(Vector3.Zero, mesh.VertexData.Count()).ToList();
            }

            var triVerts = triangleFace.vertIndexList.Select(ind => mesh.VertexData[ind - 1].ToVector3()).ToArray();
            var a = triVerts[0];
            var b = triVerts[1];
            var c = triVerts[2];

            var calculatedNormal = Vector3.Normalize(Vector3.Cross((b - a), (c - a)));
            //temporarily we will add these normals to the normals array - we'll average them later.
            var inds = new List<int>();
            foreach (var vertIndex in triangleFace.vertIndexList)
            {
                mesh.VertexNormalData[vertIndex - 1] = calculatedNormal;
            }
            //copy the array.
            //TODO because this is a struct, it does not end up modifying the tris we set in mesh...
            //bad design.
            triangleFace.NormalIndexList = triangleFace.vertIndexList.ToArray();
            

        }

        private static (Vector3, Vector3) calculateTangetSpaceForTri(TriangleFace triangleFace, Mesh mesh)
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

            //TODO are uv coords incorrect?
            var recip = 1.0 / ((temp1.X * temp2.Y) - (temp1.Y * temp2.X));
            if (double.IsInfinity(recip))
            {
                recip = 1.0;
            }

            //these seem exactly the same...

            var udir = Vector3.Multiply((float)recip, Vector3.Subtract(Vector3.Multiply(temp2.Y, q1), (Vector3.Multiply(temp1.Y, q2))));
            var vdir = Vector3.Multiply((float)recip, Vector3.Subtract(Vector3.Multiply(temp1.X, q2), (Vector3.Multiply(temp2.X, q1))));

            //var udir = new Vector3((t.Y * q1.X - t.X * q2.X) * recip, (t.Y * q1.Y - t.X * q2.Y) * recip, (t.Y * q1.Z - t.X * q2.Z) * recip);
            //var vdir = new Vector3((s.X * q2.X - s.Y * q1.X) * recip, (s.X * q2.Y - s.Y * q1.Y) * recip, (s.X * q2.Z - s.Y * q1.Z) * recip);

            if (float.IsInfinity(udir.X) || float.IsInfinity(udir.Y) || float.IsInfinity(udir.Z)
               || float.IsInfinity(vdir.X) || float.IsInfinity(vdir.Y) || float.IsInfinity(vdir.Z))
            {
                throw new ArgumentException();
            }

            return (udir, vdir);

        }



    }
}