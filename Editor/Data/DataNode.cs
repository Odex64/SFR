using System.Collections.Generic;

namespace Editor.Data;

public class DataNode
{
    public readonly string Property;
    public readonly string Value;
    private string _childNodeData;
    public List<DataNode> ChildNodes;

    public DataNode()
    {
        Property = string.Empty;
        Value = string.Empty;
        ChildNodes = new List<DataNode>();
        _childNodeData = string.Empty;
    }

    public DataNode(string property)
    {
        Property = property;
        Value = string.Empty;
        ChildNodes = new List<DataNode>();
        _childNodeData = string.Empty;
    }

    public DataNode(string property, string value)
    {
        Property = property;
        Value = value;
        ChildNodes = new List<DataNode>();
        _childNodeData = string.Empty;
    }

    internal DataNode(string property, string value, string childNodeData)
    {
        Property = property;
        Value = value;
        ChildNodes = new List<DataNode>();
        _childNodeData = childNodeData;
    }
}