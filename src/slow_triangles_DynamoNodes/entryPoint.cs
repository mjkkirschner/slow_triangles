using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using renderer._3d;
using renderer.dataStructures;
using adsk = Autodesk.DesignScript.Geometry;
using System.Linq;
using Dynamo.Visualization;
using renderer.core;
using renderer.materials;
using renderer.shaders;
using renderer_core.dataStructures;
using renderer.utilities;

namespace slow_triangles.DynamoNodes
{
    public static class ColorUtilities
    {
        public static Color FromDynamoColor(DSCore.Color color)
        {
            return Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
        }

        public static DSCore.Color FromWindowsColor(Color color)
        {
            return DSCore.Color.ByARGB(color.A, color.R, color.G, color.B);
        }


    }

    public static class Slow_Triangles_Renderer
    {


        public static Renderer3dGeneric<Mesh> CreateMeshRenderer(IEnumerable<Renderable<Mesh>> sceneItems, int width, int height, Color backgroundColor)
        {
            //don't care about this outer layer of structure.
            var renderer = new Renderer3dGeneric<Mesh>(width, height, backgroundColor, new List<IEnumerable<Renderable<Mesh>>>() { sceneItems });
            return renderer;
        }


        public static IEnumerable<Renderable<Mesh>> CreateRenderableObjects(IEnumerable<adsk.Geometry> geometryObjects, IEnumerable<Materials.Material> materials)
        {

            //first convert the geometry objects into meshes... this can be done by calling tessellate.
            var convertedMeshes = geometryObjects.Select(x => ConvertDynamoGeometryToSlowTriMesh(x));
            //create our renderable objects to associate these meshes with a material/shader setup.
            var renderables = convertedMeshes.Select((mesh, index) =>
            {
                var material = materials.ElementAt(index);
                return new Renderable<Mesh>(material.InternalMaterial, mesh);
            });
            return renderables;
        }

        public static Renderable<Mesh> CreateRenderableObjects2(adsk.Sphere geometryObject, Materials.Material material)
        {

            //first convert the geometry objects into meshes... this can be done by calling tessellate.
            var convertedMeshes = ConvertDynamoGeometryToSlowTriMesh(geometryObject);
            //create our renderable objects to associate these meshes with a material/shader setup.
            return new Renderable<Mesh>(material.InternalMaterial, convertedMeshes);

        }


        public static Texture2d Render(Camera camera, IEnumerable<Light> lights, Renderer3dGeneric<Mesh> renderer)
        {

            renderer.Scene.ToList().ForEach(x => x.ToList().ForEach(mesh =>
            {

                var currentShader = mesh.material.Shader;
                SetShaderUniforms(currentShader, camera, renderer,lights);
            })
            );

            var data = renderer.Render();
            return new Texture2d(renderer.Width, renderer.Height, data);
        }

      


        #region private

        private static Mesh ConvertDynamoGeometryToSlowTriMesh(adsk.Geometry geo)
        {
            var rpfactory = new DefaultRenderPackageFactory();
            var rp = rpfactory.CreateRenderPackage();
            geo.Tessellate(rp, rpfactory.TessellationParameters);

            var triFaces = new List<TriangleFace>();
            var verts = meshHelpers.Split<float>(rp.MeshVertices.Select(x => (float)x).ToList(), 3).Select(subarr => new Vector4(subarr[0], subarr[1], subarr[2], 1)).ToList();
            var normals = meshHelpers.Split<float>(rp.MeshNormals.Select(x => (float)x).ToList(), 3).Select(subarr => new Vector3(subarr[0], subarr[1], subarr[2])).ToList();
            var uvs = meshHelpers.Split<float>(rp.MeshTextureCoordinates.Select(x => (float)x).ToList(), 2).Select(subarr => new Vector2(subarr[0], subarr[1])).ToList();

            //trifaces will be 123,456,789 etc.
            for (var i = 1; i < verts.Count() + 1; i = i + 3)
            {
                var vertIndices = new int[] { i, i + 1, i + 2 };
                var normalIndices = new int[] { i, i + 1, i + 2 };
                var uvIndicies = new int[] { i, i + 1, i + 2 };
                triFaces.Add(new TriangleFace(vertIndices, uvIndicies, normalIndices));
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
                for (int triIndex = 0; triIndex < triFaces.Count; triIndex++)
                {
                    var triface = triFaces[triIndex];
                    ObjFileLoader.calculateAndSetNormalForTri(ref triface, tempMesh);
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
                var (tan, binorm) = ObjFileLoader.calculateTangetSpaceForTri(triface, tempMesh);
                tempMesh.BiNormals_akaBiTangents.Add(binorm);
                tempMesh.Tangents.Add(tan);
            }
            tempMesh.computeAveragedTangents();
            return tempMesh;
        }

        private static void SetShaderUniforms(Shader shader, Camera camera, Renderer3dGeneric<Mesh> renderer, IEnumerable<ILight> lights)
        {
            var cameraPos = camera.Position.ToVector3();
            var target = camera.LookTarget.ToVector3();
            var width = renderer.Width;
            var height = renderer.Height;
            var view = Matrix4x4.CreateLookAt(cameraPos, target, camera.UpDirection.ToVector3());
            var proj = Matrix4x4.CreatePerspective(camera.Width, camera.Height, camera.NearDistance, camera.FarDistance);
            var viewport = MatrixExtensions.CreateViewPortMatrix(0, 0, 255, width, height);

            if(shader is Base3dShader)
            {
                (shader as Base3dShader).SetProjectionMatrices(view, proj, viewport);
            }

            //now set lights based on shader type
            if(shader is Single_DirLight_NormalShader )
            {
                (shader as Single_DirLight_NormalShader).uniform_dir_light = lights.FirstOrDefault() as DirectionalLight;
            }
            if (shader is Single_DirLight_TextureShader)
            {
                (shader as Single_DirLight_TextureShader).uniform_dir_light = lights.FirstOrDefault() as DirectionalLight;
            }

            if(shader is Lit_SpecularTextureShader)
            {
                (shader as Lit_SpecularTextureShader).uniform_cam_world_pos = cameraPos;
                (shader as Lit_SpecularTextureShader).uniform_light_array = lights.ToArray();
            }
        }

        #endregion

    }
}
