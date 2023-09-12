using System.Security.Cryptography.X509Certificates;

namespace RacewayLib
{
    public enum NodeTypeEnum
    {
        Point,
        Start,
        End,
        Part,
        Equip
    }


    public record Node { 
        public string ID { get; init; }
        /// <summary>
        /// Belong to a branch
        /// </summary>
        public string BranchID { get; init; }
        /// <summary>
        /// The next connected node in a path.
        /// This path is a straight line segement
        /// between these nodes.
        /// </summary>
        public Node NextNode { get; init; }
        /// <summary>
        /// The distance to the next connected node.
        /// </summary>
        public double Length { get; init; }
        public NodeTypeEnum NodeType { get; init; }
    }

    public record NodeCoord
    {
        public string NodeID { get; init; }
        public double X { get; init; } = 0;
        public double Y { get; init; } = 0;
        public double Z { get; init; } = 0;
    }
    
    /// <summary>
    /// A branch is a sequential set of connected nodes
    /// that forms a path with a cycle. A branch has
    /// start and end nodes and each node point to the
    /// next connected node in the path and the distance
    /// to the next node.
    /// </summary>
    public record Branch
    {
        public string ID { get; init; }
        public string Name { get; init; }
        public Node StartNode { get; init; }
        public Node EndNode { get; init; }
    }

    /// <summary>
    /// A Pipe has a set of connected branches 
    /// like a tree with a set of tree branches.
    /// </summary>
    public record Pipe
    {
        public string ID { get; init; }
        public IEnumerable<Branch> Branches { get; init; }
    }

    /// <summary>
    /// A raceway is a STRAIGHT line segment between 
    /// any two nodes. Two or more raceways are 
    /// connected when they share a common node.
    /// </summary>
    public record Raceway
    {
        public string ID { get; init; }
        public Node FromNode { get; init; }
        public Node ToNode { get; init; }
        public string BranchID { get; init; } = "";
        public double Length { get; init; } = 0.0;
    }

}