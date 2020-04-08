using UnityEngine;

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

                Mesh mesh = new Mesh();
                var size = 0.25f / 2.0f;

                var length = Vector3.Distance(StartingCube.transform.position, gridPoint);
                var orientation = Quaternion.LookRotation(hit.point - StartingCube.transform.position);


                Vector3[] vertices = new Vector3[]
                {
                    new Vector3(size, size * 2, 0),
                    new Vector3(size * 2, size, 0),
                    new Vector3(size * 2, -size, 0),
                    new Vector3(size, -size * 2, 0),
                    new Vector3(-size, -size * 2, 0),
                    new Vector3(-size * 2, -size, 0),
                    new Vector3(-size * 2, size, 0),
                    new Vector3(-size, size * 2, 0),

                    new Vector3(size, size * 2, length),
                    new Vector3(size * 2, size, length),
                    new Vector3(size * 2, -size, length),
                    new Vector3(size, -size * 2, length),
                    new Vector3(-size, -size * 2, length),
                    new Vector3(-size * 2, -size, length),
                    new Vector3(-size * 2, size, length),
                    new Vector3(-size, size * 2, length),
                };
                mesh.vertices = vertices;

                int[] tris = new int[]
                {
                    //1
                    8, 1, 0,
                    1, 8, 9,
                    //2
                    9, 2, 1,
                    2, 9, 10,
                    //3
                    10, 3, 2,
                    3, 10, 11,
                    //4
                    11, 4, 3,
                    4, 11, 12,
                    //5
                    12, 5, 4,
                    5, 12, 13,
                    //6
                    13, 6, 5,
                    6, 13, 14,
                    //7
                    14, 7, 6,
                    7, 14, 15,
                    //8
                    15, 0, 7,
                    0, 15, 8

                };
                mesh.triangles = tris;

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

                mesh.RecalculateNormals();
                meshFilter.mesh = mesh;
                dynamicMesh.transform.SetParent(StartingCube.transform);
                dynamicMesh.transform.localPosition = Vector3.zero;
                dynamicMesh.transform.rotation = orientation;


                //StartingCube = newCube;
                StartingCube = null;
            }
            else
            {
                StartingCube = Instantiate(StartingCubePrefab, gridPoint, Quaternion.identity);
            }

        }
    }

    Vector3 MapToGrid(Vector3 point)
    {
        return new Vector3(
            MapToAxis(point.x),
            0.25f,//MapToAxis(point.y),
            MapToAxis(point.z));
    }

    float MapToAxis(float value) {
        if (value > 0) 
        {
            return (int)(value / GridSize) * GridSize + (GridSize / 2);
        }
        else
        {
            return (int)(value / GridSize) * GridSize - (GridSize / 2); 
        }
    }

}
