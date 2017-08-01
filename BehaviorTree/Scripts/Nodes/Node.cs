using UnityEngine;
using System.Collections;

namespace Blue.Tree
{
    [System.Serializable]
    public abstract class Node
    {
        ///<summary>Node posible states</summary>
        public enum NodeStates
        {
            SUCCESS,
            FAILURE,
            RUNNING,
        }

        ///<summary>Delegate that returns the state of the node.</summary>
        public delegate NodeStates NodeReturn();

        ///<summary>The current state of the node</summary>
        protected NodeStates m_nodeState;

        public NodeStates nodeState
        {
            get { return m_nodeState; }
        }

        /* The constructor for the node */
        public Node() { }

        ///<summary>Implementing classes use this method to valuate the desired set of conditions</summary>
        public abstract NodeStates Evaluate();
    }
}