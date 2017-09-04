using System.Collections;
using UnityEngine;

namespace Blue.Pathfinding
{
    public interface IPathAlgorithm
    {
        Vector3[] FindPath(Node startNode, Node targetNode);

        bool IsNodeAccesible(Node startNode, Node targetNode);
    }
}