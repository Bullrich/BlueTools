using Blue.Utility;
using UnityEngine;
using Blue.Utility;

// by @Bullrich

namespace game
{
    public class FlowField : MonoBehaviour
    {
        private Flow[,] field;
        [Range(0.5f, 6)] public float nodeRadius = 1;
        public Vector2 fieldSize;
        public bool drawArrows = false;

        private Vector2 _gridSize;
        private float _nodeDiameter;

        private void Start()
        {
            _nodeDiameter = nodeRadius * 2;
            _gridSize = new Vector2(Mathf.RoundToInt(fieldSize.x / _nodeDiameter),
                Mathf.RoundToInt(fieldSize.y / _nodeDiameter));
            field = GenerateField();
        }

        private Flow[,] GenerateField()
        {
            Flow[,] tempField = new Flow[(int) _gridSize.x, (int) _gridSize.y];
            Vector3 worldBottomLeft =
                transform.position - Vector3.right * fieldSize.x / 2 - Vector3.forward * fieldSize.y / 2;
            for (int x = 0; x < _gridSize.x; x++)
            {
                for (int y = 0; y < _gridSize.x; y++)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * _nodeDiameter + nodeRadius) +
                                         Vector3.forward * (y * _nodeDiameter + nodeRadius);
                    Flow flow = new Flow();
                    flow.position = worldPoint;
                    flow.direction = randomVector(transform.position.y);
                    flow.direction = (transform.position - worldPoint).normalized;
                    tempField[x, y] = flow;
                }
            }
            return tempField;
        }

        private Vector3 randomVector(float yPosition)
        {
            float x = Random.Range(-1f, 1f);
            return new Vector3(x, yPosition, (1 * (Mathf.Sign(x) - x)) * Mathf.Sign(Random.Range(-1, 1)));
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(fieldSize.x, 1, fieldSize.y));

            if (!Application.isPlaying || !drawArrows) return;
            for (int x = 0; x < _gridSize.x; x++)
            {
                for (int y = 0; y < _gridSize.x; y++)
                {
                    Flow flow = field[x, y];
                    DrawArrow.ForGizmo(flow.position, flow.direction, Color.green, 0.2f);
                    //Gizmos.DrawLine(flow.position, flow.direction);
                }
            }
        }

        public Vector3 getDirection(Vector3 position)
        {
            return NodeFromWorldPoint(position).direction;
        }

        private Flow NodeFromWorldPoint(Vector3 worldPosition)
        {
            float percentX = (transform.position.x + worldPosition.x + fieldSize.x / 2) / fieldSize.x;
            float percentY = (transform.position.z + worldPosition.z + fieldSize.y / 2) / fieldSize.y;
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.RoundToInt((_gridSize.x - 1) * percentX);
            int y = Mathf.RoundToInt((_gridSize.y - 1) * percentY);
            return field[x, y];
        }

        private class Flow
        {
            public Vector3 position;
            public Vector3 direction;
        }
    }
}