using System.Data.Common;

namespace GraphLib
{
    using EdgeMatrix = IDictionary<IVertex, IDictionary<IVertex, IEnumerable<IEdge>>>;

    public delegate bool EdgeFilter(IEdge edge);

    public interface IVertex
    {
        string ID { get; }
    }

    public record EdgeWeight(double Value);

    public interface IEdge
    {
        string ID { get; }
        IVertex FromVertex { get; }
        IVertex ToVertex { get; }
        EdgeWeight Weight { get; }
    }

    public interface IGraph
    {
        IEnumerable<IEdge> Edges { get; }
        EdgeFilter EdgeFilter { get; }
        EdgeMatrix Value { get; }
        bool IsDirected { get; }
    }

}