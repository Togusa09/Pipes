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


            int rotationDir = 0;
            int startingAngle = 0;

            var diff = gridPoint - StartingCube.transform.position;
            var angle =  Vector3.SignedAngle(diff, Vector3.forward, Vector3.up);
            Debug.Log(angle);

            // TODO -> Figure out rotation needed for angle, then int div 90
            if (angle < -90)
            {
                startingAngle = 180;
            }
            else if (angle < 0)
            {
                startingAngle = 0;
            } 
            else if (angle < 90)
            {
                startingAngle = 0;
            }
            else
            {
                startingAngle = 180;
            }

            // Todo - figure out a better way to get off vector...

            // var signedAngle = Vector3.SignedAngle(gridPoint, StartingCube.transform.position, Vector3.up);
            //rotationDir = (int)(signedAngle / Mathf.Abs(signedAngle));
            if (gridPoint.z < StartingCube.transform.position.z)
            {
                rotationDir = gridPoint.x < StartingCube.transform.position.x ? 1 : -1;
            }
            else
            {
                rotationDir = gridPoint.x < StartingCube.transform.position.x ? -1 : 1;
            }

            var startingRotation = Quaternion.Euler(0, startingAngle, 0);
            var incrementRotation = Quaternion.Euler(0, 45 * rotationDir, 0);
            var jointRotation = startingRotation * incrementRotation;
            var endingRotation = jointRotation * incrementRotation;

            var length = Vector3.Distance(StartingCube.transform.position, gridPoint);
            var orientation = Quaternion.LookRotation(gridPoint - StartingCube.transform.position);

            var pipeVertices = new List<Vector3>();
            // Pipe start
            pipeVertices.AddRange(GetPipeOuter(size).Select(x => startingRotation * x));
            // Joint
            pipeVertices.AddRange(GetPipeOuter(size).Select(x => (startingRotation * incrementRotation * x * 1.5f) + new Vector3(0, 0, diff.z))); // Need to increase width as looks narrower at 45 deg
            // Pipe end
            pipeVertices.AddRange(GetPipeOuter(size).Select(x => (startingRotation * incrementRotation * incrementRotation * x) + new Vector3(diff.x, 0, diff.z)));

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