using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Experiments.Dynamic
{

    public class PipeOrigin : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        internal void CreatePipe(GameObject newCube, Material pipeMaterial, InterpolationMethod interpolationMethod)
        {
            var dynamicMesh = new GameObject();

            var meshRenderer = dynamicMesh.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = pipeMaterial;
            MeshFilter meshFilter = dynamicMesh.AddComponent<MeshFilter>();
            Mesh mesh = GeneratePipe(newCube.transform.position, interpolationMethod);

            //Vector3[] normals = new Vector3[4]
            //{
            //    -Vector3.forward,
            //    -Vector3.forward,
            //    -Vector3.forward,
            //    -Vector3.forward
            //};
            //mesh.normals = normals;

            //Vector2[] uv = new Vector2[4]
            //{
            //    new Vector2(0, 0),
            //    new Vector2(1, 0),
            //    new Vector2(0, 1),
            //    new Vector2(1, 1)
            //};
            //mesh.uv = uv;


            meshFilter.mesh = mesh;
            dynamicMesh.transform.SetParent(transform);
            dynamicMesh.transform.localPosition = Vector3.zero;
            dynamicMesh.transform.rotation = Quaternion.identity;

        }

        private Mesh GeneratePipe(Vector3 gridPoint, InterpolationMethod interpolationMethod)
        {
            Mesh mesh = new Mesh();
            var size = 0.25f / 2.0f;

            int startingAngle = 0;

            var diff = gridPoint - transform.position;

            // A pure math way for this would be nice, but not sure if is...
            if (diff.z > 0)
            {
                startingAngle = 0;
            }
            else
            {
                startingAngle = 180;
            }

            var rotationDir = Mathf.Clamp(diff.z, -1, 1) * Mathf.Clamp(diff.x, -1, 1);

            var startingRotation = Quaternion.Euler(0, startingAngle, 0);
            var endingRotation = startingRotation * Quaternion.Euler(0, 90 * rotationDir, 0);

            var pipeVertices = new List<Vector3>();
            var tris = new List<int>();


            var pipeSections = 4;
            for (var i = 0; i <= pipeSections; i++)
            {
                var frac = (1.0f / pipeSections) * i;
                var vec1 = Vector3.Project(diff, Vector3.left) * -1;
                var vec2 = Vector3.Project(diff, Vector3.forward);

                Quaternion jointRotation;
                Vector3 jointPosition;

                jointRotation = Quaternion.Lerp(startingRotation, endingRotation, frac);

                switch (interpolationMethod)
                {
                    case InterpolationMethod.Oval:
                        var angle = Mathf.LerpAngle(0, 90, frac);
                        jointPosition = new Vector3(-diff.x * Mathf.Cos(angle / Mathf.Rad2Deg), 0.0f, diff.z * Mathf.Sin(angle / Mathf.Rad2Deg)) + new Vector3(diff.x, 0, 0);
                        break;
                    case InterpolationMethod.Lerp:
                        jointPosition = Vector3.Lerp(vec1, vec2, frac) + new Vector3(diff.x, 0, 0);
                        break;
                    case InterpolationMethod.Slerp:
                    default:
                        jointPosition = Vector3.Slerp(vec1, vec2, frac) + new Vector3(diff.x, 0, 0);
                        break;
                }

                pipeVertices.AddRange(GetPipeOuter(size).Select(x => (jointRotation * x) + jointPosition)); // Need to increase width as looks narrower at 45 deg
                if (i < pipeSections)
                {
                    tris.AddRange(GetPipeOuterIndices(i));
                }
            }


            mesh.vertices = pipeVertices.ToArray();

            mesh.triangles = tris.ToArray();
            mesh.RecalculateNormals();
            return mesh;
        }

        Vector3[] GetPipeOuter(float size)
        {
            return new Vector3[]
            {
                new Vector3(size, size * 2, 0),
                new Vector3(size * 2, size, 0),
                new Vector3(size * 2, -size, 0),
                new Vector3(size, -size * 2, 0),
                new Vector3(-size, -size * 2, 0),
                new Vector3(-size * 2, -size, 0),
                new Vector3(-size * 2, size, 0),
                new Vector3(-size, size * 2, 0)
            };
        }

        int[] GetPipeOuterIndices(int sectionId)
        {
            var tris = new List<int>();

            for (var i = 0; i < 8; i++)
            {
                tris.AddRange(new[] { (i % 8) + 8, (i + 1) % 8, i % 8 });
                tris.AddRange(new[] { (i + 1) % 8, (i % 8) + 8, ((i + 1) % 8) + 8 });
            }

            return tris.Select(x => x + sectionId * 8).ToArray();
        }
    }
}