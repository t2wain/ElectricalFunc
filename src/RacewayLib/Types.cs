using System.Security.Cryptography.X509Certificates;

namespace RacewayLib
{
    public enum NodeTypeEnum
    {
        Default,
        Point,
        Start,
        End,
        Part,
        Equip,
    }


    /// <summary>
    /// A node is a component of a branch separated by
    /// a length. Node is directional in the direction
    /// from this node to the NextNode. The branch id
    /// and the node id identify the next segment 
    /// of the branch.
    /// </summary>
    public record Node {

        static Node EmptyNode = new();

        public string ID { get; init; } = "";
        /// <summary>
        /// Belong to a branch
        /// </summary>
        public string BranchID { get; init; } = "";

        /// <summary>
        /// The next connected node in a path.
        /// This path is a straight line segement
        /// between these nodes.
        /// </summary>
        public Node NextNodeSet { protected get; init; }

        public Node NextNode => NextNodeSet ?? EmptyNode;

        /// <summary>
        /// The distance to the next connected node.
        /// </summary>
        public double Length { get; init; } = 0;
        public NodeTypeEnum NodeType { get; init; }
    }

    public record NodeCoord
    {
        public string NodeID { get; init; } = "";
        public double X { get; init; } = 0;
        public double Y { get; init; } = 0;
        public double Z { get; init; } = 0;
    }

    /// <summary>
    /// A branch is a sequential set of connected nodes
    /// that forms a path with a loop. A branch has a
    /// start and an end nodes and each node point to the
    /// next connected node in the path and the distance
    /// to the next node.
    /// </summary>
    public record Branch
    {
        public string ID { get; init; } = "";
        public string Name { get; init; } = "";
        public Node StartNode { get; init; } = new();
        public Node EndNode { get; init; } = new();
    }

    public record RouteBranch : Branch { }

    public record RouteNode : Node { }

    /// <summary>
    /// A Pipe has a set of connected branches 
    /// like a tree with a set of tree branches.
    /// </summary>
    public record Pipe
    {
        public string ID { get; init; } = "";
        public IEnumerable<Branch> Branches { get; init; } = new List<Branch>();
    }

    /// <summary>
    /// A raceway is a DIRECTED, STRAIGHT line segment 
    /// between any two nodes. Note, there might be 
    /// other nodes in between these two nodes. Two or more 
    /// raceways are connected when they share a common node.
    /// </summary>
    public record Raceway
    {
        /// <summary>
        /// When the raceway is a branch raceway, it is
        /// expected that the ID will be of the anchor 
        /// branch node. The anchor node and its linked node
        /// are the two nodes of this raceway.
        /// </summary>
        public string ID { get; init; } = "";
        public Node FromNode { get; init; } = new();
        /// <summary>
        /// Expecting that the FromNode is linked
        /// to the ToNode. There might be other nodes
        /// of the same branch in between.
        /// </summary>
        public Node ToNode { get; init; } = new();
        /// <summary>
        /// It is empty when the end nodes are
        /// not from the same branch
        /// </summary>
        public string BranchID { get; init; } = "";
        public double Length { get; init; } = 0.0;
        /// <summary>
        /// Is the raceway part of a branch
        /// </summary>
        public bool IsBranch =>
            !string.IsNullOrEmpty(BranchID) ||
            FromNode.BranchID == ToNode.BranchID;
    }

    /// <summary>
    /// A physical connection between two nodes
    /// not from the same branch (like JUMP, DROP).
    /// </summary>
    public record RwConn : Raceway { }

}