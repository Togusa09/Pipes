using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Experiments.RightAngle
{
    public class CubeClick : MonoBehaviour
    {
        public GameObject StartingCubePrefab;
        public GameObject EndingCubePrefab;
        public Material PipeMeterial;
        public float GridSize = 1f;

        private GameObject StartingCube;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Clicked();
            }
        }

        void Clicked()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit = new RaycastHit();

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log(hit.collider.gameObject.name);

                var gridPoint = MapToGrid(hit.point);


                if (StartingCube)
                {
                    var newCube = Instantiate(EndingCubePrefab, gridPoint, Quaternion.identity);

                    var dynamicMesh = new GameObject();

                    var meshRenderer = dynamicMesh.AddComponent<MeshRenderer>();
                    meshRenderer.sharedMaterial = PipeMeterial;
                    MeshFilter meshFilter = dynamicMesh.AddComponent<MeshFilter>();
                    Mesh mesh = GeneratePipe(gridPoint);

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
                    dynamicMesh.transform.SetParent(StartingCube.transform);
                    dynamicMesh.transform.localPosition = Vector3.zero;
                    //dynamicMesh.transform.rotation = orientation;
                    dynamicMesh.transform.rotation = Quaternion.identity;


                    //StartingCube = newCube;
                    StartingCube = null;
                }
                else
                {
                    StartingCube = Instantiate(StartingCubePrefab, gridPoint, Quaternion.identity);
                }

            }
        }

        private Mesh GeneratePipe(Vector3 gridPoint)
        {
            Mesh mesh = new Mesh();
            var size = 0.25f / 2.0f;

            Quaternion q45;
            Quaternion q90;
            Quaternion q0;

            if (gridPoint.x < StartingCube.transform.position.x)
            {
                q90 = Quaternion.Euler(0, 270, 0);
            }
            else
            {
                q90 = Quaternion.Euler(0, 90, 0);
            }

            if (gridPoint.z < StartingCube.transform.position.z)
            {
                q0 = Quaternion.Euler(0, 180, 0);

                if (gridPoint.x < StartingCube.transform.position.x)
                {
                    q45 = Quaternion.Euler(0, 225, 0);
                }
                else
                {
                    q45 = Quaternion.Euler(0, 135, 0);
                }
            }
            else
            {
                q0 = Quaternion.Euler(0, 0, 0);

                if (gridPoint.x < StartingCube.transform.position.x)
                {
                    q45 = Quaternion.Euler(0, -45, 0);
                }
                else
                {
                    q45 = Quaternion.Euler(0, 45, 0);
                }
            }

            var xDist = gridPoint.x - StartingCube.transform.position.x;
            var zDist = gridPoint.z - StartingCube.transform.position.z;
            var length = Vector3.Distance(StartingCube.transform.position, gridPoint);
            var orientation = Quaternion.LookRotation(gridPoint - StartingCube.transform.position);

            var pipeVertices = new List<Vector3>();
            // Pipe start
            pipeVertices.AddRange(GetPipeOuter(size).Select(x => q0 * x));
            // Joint
            pipeVertices.AddRange(GetPipeOuter(size).Select(x => (q45 * x * 1.55f) + new Vector3(0, 0, zDist)));
            // Pipe end
            pipeVertices.AddRange(GetPipeOuter(size).Select(x => (q90 * x) + new Vector3(xDist, 0, zDist)));

            mesh.vertices = pipeVertices.ToArray();

            var tris = new List<int>();
            // First pipe section
            tris.AddRange(GetPipeOuterIndices(0));
            // Second pipe section
            tris.AddRange(GetPipeOuterIndices(1));

            mesh.triangles = tris.ToArray();
            mesh.RecalculateNormals();
            return mesh;
        }

        Vector3 MapToGrid(Vector3 point)
        {
            return new Vector3(
                MapToAxis(point.x),
                0.25f,//MapToAxis(point.y),
                MapToAxis(point.z));
        }

        float MapToAxis(float value)
        {
            if (value > 0)
            {
                return (int)(value / GridSize) * GridSize + (GridSize / 2);
            }
            else
            {
                return (int)(value / GridSize) * GridSize - (GridSize / 2);
            }
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