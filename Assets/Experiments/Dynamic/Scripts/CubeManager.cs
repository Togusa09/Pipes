using UnityEngine;


namespace Experiments.Dynamic
{
    public class CubeManager : MonoBehaviour
    {
        public GameObject StartingCubePrefab;
        public GameObject EndingCubePrefab;
        public Material PipeMeterial;
        public float GridSize = 1f;

        private PipeOrigin StartingCube;
        public InterpolationMethod InterpolationMethod = InterpolationMethod.Slerp;

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
                    StartingCube.CreatePipe(newCube, PipeMeterial, InterpolationMethod);
                }
                else
                {
                    var origin = Instantiate(StartingCubePrefab, gridPoint, Quaternion.identity);
                    StartingCube = origin.AddComponent<PipeOrigin>();
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
    }
    }
