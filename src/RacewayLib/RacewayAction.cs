namespace RacewayLib
{
    public static class RacewayAction
    {
        public static Branch CreateBranch(string ID, string fromNodeID, string endNodeID, double Length)
        {
            var sn = new Node()
            {
                ID = fromNodeID,
                NodeType = NodeTypeEnum.Start,
                BranchID = ID,
                NextNode = new() { ID = endNodeID, NodeType = NodeTypeEnum.End,  BranchID = ID },
                Length = Length
            };
            return new() { ID = ID, StartNode = sn, EndNode = sn.NextNode };
        }

        /// <summary>
        /// Create a set of raceways from a set of branches.
        /// </summary>
        public static IEnumerable<Raceway> CalcRaceway(IEnumerable<Branch> branches) =>
            branches.SelectMany(b => b.CalcRaceway()).ToList();

        /// <summary>
        /// Create a set of raceways from a branch.
        /// </summary>
        public static IEnumerable<Raceway> CalcRaceway(this Branch branch) =>
            branch.GetNodeList().Where(n => n.NextNode != null).Select(n => n.ToRaceway()).ToList();

        /// <summary>
        /// Get a ordered sequence of connected nodes of a branch
        /// from start to end excluding "Point" node type.
        /// </summary>
        /// <param name="branch"></param>
        /// <returns></returns>
        public static IEnumerable<Node> GetNodeListMerge(this Branch branch)
        {
            var lstNode = new List<Node>();

            Node cn = null; double cl = 0;
            foreach (var n in branch.GetNodeList())
            {
                switch (n)
                {
                    case { NodeType: NodeTypeEnum.Point }:
                        cl += n.Length;
                        break;
                    default:
                        if (cn != null)
                            lstNode.Add(cn with { NextNode = n, Length = cl });
                        cn = n; cl = n.Length;
                        break;
                }
            }

            return lstNode;
        }

        /// <summary>
        /// Get a ordered sequence of connected nodes of a branch
        /// from start to end.
        /// </summary>
        public static IEnumerable<Node> GetNodeList(this Branch branch) =>
            branch.StartNode.GetLinkNodes().Concat(new[] { branch.EndNode }).ToList();

        /// <summary>
        /// Sum up the length between the branch nodes.
        /// </summary>
        public static double GetLength(this Branch branch) =>
            branch.GetNodeList().Sum(n => n.Length);

        static IEnumerable<Node> GetLinkNodes(this Node node) =>
            node switch
            {
                var n when n.NextNode is null => new[] { node },
                _ => new[] { node }.Concat(node.NextNode.GetLinkNodes())
            };

        /// <summary>
        /// Create a raceway from this node to the next connected node.
        /// </summary>
        static Raceway ToRaceway(this Node node) =>
            new() { ID = node.ID, FromNode = node, ToNode = node.NextNode, BranchID = node.BranchID, Length = node.Length };

        /// <summary>
        /// Insert a node into the branch after the from node.
        /// </summary>
        /// <param name="fromNode">Add new node after this node.</param>
        /// <param name="nodeID">ID of new node.</param>
        /// <param name="length">Distance to the new node</param>
        /// <returns></returns>
        public static (bool Success, IEnumerable<Node> Result) AddNode(this Node fromNode, string nodeID, double length)
        {
            var l = fromNode.NextNode switch { null => fromNode.Length, _ => fromNode.Length - length };
            if (l < 0 || fromNode.NextNode == null) 
                return (false, Array.Empty<Node>());

            var tn1 = new Node() { ID = nodeID, BranchID = fromNode.BranchID, NextNode = fromNode.NextNode, Length = length };
            var fn1 = fromNode with { NextNode = tn1, Length = l };
            return (true, new[] { fn1, tn1 });
        }

        public static IDictionary<string, IEnumerable<Node>> GroupNode(IEnumerable<Node> nodes, Func<Node, string> hashfunc) =>
            nodes
                .GroupBy(n => hashfunc(n))
                .Select(g => (g.Key, Value: g.Select(n => n)))
                .Where(t => t.Value.Count() > 1)
                .Aggregate(new Dictionary<string, IEnumerable<Node>>(), (agg, v) => 
                {
                    agg.Add(v.Key, v.Value);
                    return agg;
                });

        /// <summary>
        /// Calculate neighbor nodes by grouping nodes into a common 3D volume of certain dimension. 
        /// </summary>
        public static IDictionary<string, IEnumerable<Node>> GroupNodeByCoord(IEnumerable<Node> nodes, 
            Func<Node, (double X, double Y, double Z)> getCoord)
        {
            string hashFunc(Node n)
            {
                var (X, Y, Z) = getCoord(n);
                return string.Format("{0}-{1}-{2}", 
                    Convert.ToInt32(X), Convert.ToInt32(Y), Convert.ToInt32(Z));
            };
            return GroupNode(nodes, hashFunc);
        }

        /// <summary>
        /// Scale node coord into a common 3D volume of certain dimension
        /// </summary>
        /// <returns>Scaled node coord</returns>
        public static (double X, double Y, double Z) ScaleNodeCoordExample(NodeCoord node, double scaleFactor) =>
            (Math.Floor(node.X * scaleFactor), Math.Floor(node.Y * scaleFactor), Math.Floor(node.Z * scaleFactor));
    }
}
