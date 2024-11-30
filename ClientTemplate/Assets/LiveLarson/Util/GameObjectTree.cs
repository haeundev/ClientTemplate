using System;
using System.Collections.Generic;
using UnityEngine;

namespace LiveLarson.Util
{
    public class GameObjectTree : ScriptableObject, ISerializationCallbackReceiver
    {
        public class Node
        {
            public GameObject target;
            public List<Node> children = new List<Node>();
            public Node parent;

            public Node AddChild()
            {
                var child = new Node {parent = this};
                children.Add(child);
                return child;
            }
        
            public Node AddChild(Node child)
            {
                child.parent = this;
                children.Add(child);
                return child;
            }

            public void Delete()
            {
                parent?.children.Remove(this);
            }
        }
    
        [Serializable]
        public struct SerializableNode
        {
            public GameObject target;
            public int childCount;
            public int indexOfFirstChild;
        }

        Node root = new Node();

        public List<SerializableNode> serializedNodes = new List<SerializableNode>();

        public void OnBeforeSerialize()
        {
            serializedNodes.Clear();
            AddNodeToSerializedNodes(root);
        }

        void AddNodeToSerializedNodes(Node n)
        {
            var serializedNode = new SerializableNode()
            {
                target = n.target,
                childCount = n.children.Count,
                indexOfFirstChild = serializedNodes.Count + 1
            };
            serializedNodes.Add(serializedNode);
            foreach (var child in n.children)
                AddNodeToSerializedNodes(child);
        }

        public void OnAfterDeserialize()
        {
            if (serializedNodes.Count > 0)
                root = ReadNodeFromSerializedNodes(0);
            else
                root = new Node();
        }

        Node ReadNodeFromSerializedNodes(int index)
        {
            var serializedNode = serializedNodes[index];
            var children = new List<Node>();
        
            var node = new Node()
            {
                target = serializedNode.target,
                children = children
            };
        
            for (int i = 0; i != serializedNode.childCount; i++)
            {
                node.AddChild(ReadNodeFromSerializedNodes(serializedNode.indexOfFirstChild + i));
            }

            return node;
        }

        public Node Root => root;
    }
}
