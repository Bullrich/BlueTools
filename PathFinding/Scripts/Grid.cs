using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// @Bullrich. March 2016

namespace Blue.Pathfinding
{
    public class Grid : MonoBehaviour
    {

        public bool displayGridGizmos;
        public LayerMask unwalkableMask;
        public Vector2 gridWorldSize;
        public float nodeRadius = 1;
        [Range(1, 90)]
        public int angleLimit = 1;
        public TerrainType[] walkableRegions;
        LayerMask walkableMask;
        Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();

        Node[,] grid;

        float nodeDiameter;
        int gridSizeX, gridSizeY;
        private bool hasCreatedGrid = false;
        private List<Transform> obstacles;
        private Vector3[] obstaclesPosition;

        private float heightLimit;

        void Awake()
        {
            nodeDiameter = nodeRadius * 2;
            gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
            gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

            heightLimit = convertAngleToUnityValue(nodeDiameter + nodeRadius, angleLimit);

            foreach (TerrainType region in walkableRegions)
            {
                walkableMask.value = walkableMask | region.terrainMask.value;
                walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2f), region.terrainPenalty);
            }

            grid = CreateGrid();
        }

        public void Start()
        {
            ValidatePaths();
            hasCreatedGrid = true;
        }

        public int MaxSize
        {
            get
            {
                return gridSizeX * gridSizeY;
            }
        }

        float convertAngleToUnityValue(float distance, int angle)
        {
            return Mathf.Tan(Mathf.Deg2Rad * angle) * distance;
        }

        public bool gridCreated()
        {
            return hasCreatedGrid;
        }

        public Node[,] CreateGrid()
        {
            obstacles = new List<Transform>();
            Node[,] tempGrid = new Node[gridSizeX, gridSizeY];
            Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                    int rayLengthMeters = 45;
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
                            if (Physics.Raycast(ray, out hit, 100, walkableMask))
                            {
                                walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                            }
                        }
                        else
                        {
                            if (!obstacles.Contains(hitInfo.collider.transform))
                                obstacles.Add(hitInfo.collider.transform);
                        }

                        tempGrid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);
                    }
                    else
                        throw new System.Exception(string.Format("Point {0}, {1} didn't hit anything when raycasting down", gridSizeX, gridSizeY));
                }
            }
            obstaclesPosition = new Vector3[obstacles.Count];
            for (int i = 0; i < obstacles.Count; i++)
                obstaclesPosition[i] = obstacles[i].position;
            return tempGrid;
        }

        private bool GridHasChange()
        {
            for (int i = 0; i < obstacles.Count; i++)
                if (obstaclesPosition[i] != obstacles[i].position)
                    return true;
            return false;
        }

        public bool CheckGridStatus()
        {
            bool changeOcurred = GridHasChange();
            if (changeOcurred)
                grid = CreateGrid();
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

                    if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY &&
                        Mathf.Abs(node.worldPosition.y - grid[checkX, checkY].worldPosition.y) < heightLimit)
                    {
                        neighbours.Add(grid[checkX, checkY]);
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

            int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
            return grid[x, y];
        }

        private void ValidatePaths()
        {
            Node startNode = grid[Mathf.RoundToInt(grid.GetLength(0) / 2), Mathf.RoundToInt(grid.GetLength(1) / 2)];
            Pathfinding path = GetComponent<Pathfinding>();
            foreach (Node _n in grid)
            {
                if (startNode != _n)
                    _n.walkable = path.FoundIfAccesible(startNode, _n);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
            if (grid != null && displayGridGizmos)
            {
                foreach (Node n in grid)
                {
                    if (n.walkable)
                    {

                        Gizmos.color = (n.walkable) ? Color.white : Color.red;
                        Gizmos.DrawWireSphere(n.worldPosition, .25f);
                        foreach (Node neighbor in GetNeighbours(n))
                        {
                            if (neighbor.walkable)
                            {
                                Gizmos.color = Color.blue;
                                Gizmos.DrawLine(n.worldPosition, neighbor.worldPosition);
                            }
                        }
                    }
                }
            }
            if (!Application.isPlaying && displayGridGizmos)
            {
                Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
                //print(worldBottomLeft);
                for (int x = 0; x < gridWorldSize.x; x++)
                {
                    for (int y = 0; y < gridWorldSize.y; y++)
                    {
                        Gizmos.color = Color.blue;
                        Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * (nodeRadius) + nodeRadius) + Vector3.forward * (y * (nodeRadius) + nodeRadius);
                        Gizmos.DrawWireSphere(worldPoint, .25f);
                    }
                }
            }
        }

        public Node FindRandomWalkableNode()
        {
            while (true)
            {
                Node newNode = grid[Random.Range(0, gridSizeX), Random.Range(0, gridSizeY)];
                if (newNode.walkable)
                    return newNode;
            }
        }

        public Node getNodeFromCoordinates(int x, int y)
        {
            try
            {
                return grid[x, y];
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