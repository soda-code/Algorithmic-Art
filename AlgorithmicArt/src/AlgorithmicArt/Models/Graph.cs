namespace AlgorithmicArt.Models;

/// <summary>图节点。</summary>
public class GraphNode
{
    public int Id { get; set; }
    public PointF Position { get; set; }

    public GraphNode(int id, PointF position)
    {
        Id = id;
        Position = position;
    }
}

/// <summary>图边（无向/有向由算法语义决定）。</summary>
public class GraphEdge
{
    public int From { get; set; }
    public int To { get; set; }
    public double Weight { get; set; }

    public GraphEdge(int from, int to, double weight = 1.0)
    {
        From = from;
        To = to;
        Weight = weight;
    }
}

/// <summary>图。</summary>
public class Graph
{
    public List<GraphNode> Nodes { get; } = new();
    public List<GraphEdge> Edges { get; } = new();

    public GraphNode AddNode(PointF position)
    {
        var node = new GraphNode(Nodes.Count, position);
        Nodes.Add(node);
        return node;
    }

    public void AddEdge(int from, int to, double weight = 1.0) => Edges.Add(new GraphEdge(from, to, weight));
}
