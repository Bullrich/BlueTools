using UnityEngine;
using System.Collections;

namespace Blue.Tree
{
    public class Inverter : Node
    {
        ///<summary>Child node to evaluate</summary>
        private Node m_node;

        public Node node
        {
            get { return m_node; }
        }

        /* The constructor requires the child node that this inverter  decorator
         * wraps*/
        public Inverter(Node node)
        {
            m_node = node;
        }

        /* Reports a success if the child fails and
         * a failure if the child succeeeds. Running will report
         * as running */
        public override NodeStates Evaluate()
        {
            switch (m_node.Evaluate())
            {
                case NodeStates.FAILURE:
                    m_nodeState = NodeStates.SUCCESS;
                    return m_nodeState;
                case NodeStates.SUCCESS:
                    m_nodeState = NodeStates.FAILURE;
                    return m_nodeState;
                case NodeStates.RUNNING:
                    m_nodeState = NodeStates.RUNNING;
                    return m_nodeState;
            }
            m_nodeState = NodeStates.SUCCESS;
            return m_nodeState;
        }
    }
}