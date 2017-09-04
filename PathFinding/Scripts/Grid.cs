using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// @Bullrich. March 2016

namespace Blue.Pathfinding
{
    public class Grid : MonoBehaviour
    {

        public bool displayGridGizmos;
        public LayerMask unwalkableMask;
        public Vector2 gridWorldSize;
        [Range(0.5f, 6)]
        public float nodeRadius = 1;
        [Range(1, 90)]
        public int angleLimit = 1;
        public TerrainType[] walkableRegions;
        private LayerMask _walkableMask;
        private readonly Dictionary<int, int> _walkableRegionsDictionary = new Dictionary<int, int>();

        private Node[,] _grid;

        private float _nodeDiameter;
        private int _gridSizeX, _gridSizeY;
        private bool _hasCreatedGrid = false;
        private List<Transform> _obstacles;
        private Vector3[] _obstaclesPosition;

        private float _heightLimit;

        private void Awake()
        {
            _nodeDiameter = nodeRadius * 2;
            _gridSizeX = Mathf.RoundToInt(gridWorldSize.x / _nodeDiameter);
            _gridSizeY = Mathf.RoundToInt(gridWorldSize.y / _nodeDiameter);

            _heightLimit = ConvertAngleToUnityValue(_nodeDiameter + nodeRadius, angleLimit);

            foreach (TerrainType region in walkableRegions)
            {
                _walkableMask.value = _walkableMask | region.terrainMask.value;
                _walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2f), region.terrainPenalty);
            }
            GenerateGrid();
        }

        public void GenerateGrid()
        {
            _hasCreatedGrid = false;
            _grid = CreateGrid();
            ValidatePaths();
            _hasCreatedGrid = true;
        }

        public int MaxSize
        {
            get
            {
                return _gridSizeX * _gridSizeY;
            }
        }

        private static float ConvertAngleToUnityValue(float distance, int angle)
        {
            return Mathf.Tan(Mathf.Deg2Rad * angle) * distance;
        }

        public bool GridCreated()
        {
            return _hasCreatedGrid;
        }

        public Node[,] CreateGrid()
        {
            _obstacles = new List<Transform>();
            Node[,] tempGrid = new Node[_gridSizeX, _gridSizeY];
            Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = 0; y < _gridSizeY; y++)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * _nodeDiameter + nodeRadius) + Vector3.forward * (y * _nodeDiameter + nodeRadius);
                    const int rayLengthMeters = 45;
                    RaycastHit hitInfo;

                    if (Physics.Raycast(worldPoint, Vector3.down, out hitInfo, rayLengthMeters))
                    {
                        worldPoint = hitInfo.point;
                        bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                        worldPoint += new Vector3(0, 1, 0);

                        int movementPenalty = 0;

                        if (walkable)
                        {
                            Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                            RaycastHit hit;
                            if (Physics.Raycast(ray, out hit, 100, _walkableMask))
                            {
                                _walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                            }
                        }
                        else
                        {
                            if (!_obstacles.Contains(hitInfo.collider.transform))
                                _obstacles.Add(hitInfo.collider.transform);
                        }

                        tempGrid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);
                    }
                    else
                        throw new System.Exception(string.Format("Point {0}, {1} didn't hit anything when raycasting down", _gridSizeX, _gridSizeY));
                }
            }
            _obstaclesPosition = new Vector3[_obstacles.Count];
            for (int i = 0; i < _obstacles.Count; i++)
                _obstaclesPosition[i] = _obstacles[i].position;
            return tempGrid;
        }

        private bool GridHasChange()
        {
            return _obstacles.Where((t, i) => _obstaclesPosition[i] != t.position).Any();
        }

        public bool CheckGridStatus()
        {
            bool changeOcurred = GridHasChange();
            if (changeOcurred)
                _grid = CreateGrid();
            return changeOcurred;

        }

        public List<Node> GetNeighbours(Node node)
        {
            List<Node> neighbours = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;

                    if (checkX >= 0 && checkX < _gridSizeX && checkY >= 0 && checkY < _gridSizeY &&
                        Mathf.Abs(node.worldPosition.y - _grid[checkX, checkY].worldPosition.y) < _heightLimit)
                    {
                        neighbours.Add(_grid[checkX, checkY]);
                    }
                }
            }

            return neighbours;
        }

        public Node NodeFromWorldPoint(Vector3 worldPosition)
        {
            float percentX = (transform.position.x + worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
            float percentY = (transform.position.z + worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.RoundToInt((_gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((_gridSizeY - 1) * percentY);
            return _grid[x, y];
        }

        private void ValidatePaths()
        {
            Node startNode = _grid[Mathf.RoundToInt(_grid.GetLength(0) / 2), Mathf.RoundToInt(_grid.GetLength(1) / 2)];
            Pathfinding path = GetComponent<Pathfinding>();
            foreach (Node _n in _grid)
            {
                if (startNode != _n)
                    _n.walkable = path.FoundIfAccesible(startNode, _n);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
            if (_grid != null && displayGridGizmos)
            {
                foreach (Node n in _grid)
                {
                    if (!n.walkable) continue;
                    
                    Gizmos.color = (n.walkable) ? Color.white : Color.red;
                    Gizmos.DrawWireSphere(n.worldPosition, .25f);
                    foreach (Node neighbor in GetNeighbours(n))
                    {
                        if (!neighbor.walkable) continue;
                        
                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(n.worldPosition, neighbor.worldPosition);
                    }
                }
            }
            if (Application.isPlaying || !displayGridGizmos) return;

            float nodeDiameter = nodeRadius * 2;
            int gridX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
            int gridY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
            Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
            for (int x = 0; x < gridX; x++)
            {
                for (int y = 0; y < gridY; y++)
                {
                    Gizmos.color = Color.blue;
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                    Gizmos.DrawWireSphere(worldPoint, .25f);
                }
            }
        }

        public Node FindRandomWalkableNode()
        {
            while (true)
            {
                Node newNode = _grid[Random.Range(0, _gridSizeX), Random.Range(0, _gridSizeY)];
                if (newNode.walkable)
                    return newNode;
            }
        }

        public Node GetNodeFromCoordinates(int x, int y)
        {
            try
            {
                return _grid[x, y];
            }
            catch (System.Exception e)
            {
                Debug.Log("Couldn't find " + x + "|" + y);
                Debug.LogError(e);
                return null;
            }
        }

        [System.Serializable]
        public class TerrainType
        {
            public LayerMask terrainMask;
            public int terrainPenalty;
        }
    }
}