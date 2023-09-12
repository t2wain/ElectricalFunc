namespace RacewayLib
{
    public static class RacewayAction
    {
        #region Branch

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

        #endregion

        #region Raceway

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
        /// Create a raceway from this node to the next connected node.
        /// </summary>
        static Raceway ToRaceway(this Node node) =>
            new() { ID = node.ID, FromNode = node, ToNode = node.NextNode, BranchID = node.BranchID, Length = node.Length };

        /// <summary>
        /// Validate that these raceways is a complete branch
        /// </summary>
        public static (bool Success, Branch Branch) IsBranch(IEnumerable<Raceway> raceways)
        {
            // get a list of unique nodes
            var nodes = raceways.SelectMany(r => new[] { r.FromNode, r.ToNode }).Distinct().ToList();

            // check for only one start node, end node, and branch ID in a branch
            var lsn = nodes.Where(n => n.NodeType == NodeTypeEnum.Start).ToList();
            var len = nodes.Where(n => n.NodeType == NodeTypeEnum.End).ToList();
            var lbid = nodes.Select(n => n.BranchID).ToList();
            var isErr = lsn.Count != 1 || len.Count != 1 || lbid.Count != 1;
            isErr = lbid.Count != lbid.Distinct().Count();
            if ((isErr)) return (false, new());

            // check if raceway data is valid
            foreach(var r in raceways)
            {
                isErr = r.FromNode.NextNode.ID != r.ToNode.ID;
                isErr = r.FromNode.Length != r.Length;
                if ((isErr)) return (false, new());
            }

            // check if all nodes are connected with one another
            var dn = nodes.ToDictionary(n => n.ID);
            var cn = lsn.First().ID;
            while (dn.TryGetValue(cn, out var node))
            {
                dn.Remove(cn);
                cn = node.NextNode.ID;
            }
            isErr = dn.Count > 0;

            return (!isErr, isErr ? new() :
                new()
                {
                    ID = lbid.First(),
                    StartNode = lsn.First(),
                    EndNode = len.First()
                });
        }

        public static (bool Success, Branch Branch) ToBranch(IEnumerable<Raceway> raceways, string branchId)
        {
            var drw = raceways
                .Select(r => (NodeID: r.FromNode.ID, Raceway: r))
                .Concat(raceways.Select(r => (NodeID: r.ToNode.ID, Raceway: r)))
                .GroupBy(t => t.NodeID)
                .Select(g => (NodeID: g.Key, Raceways: g.Select(t => t.Raceway).ToList()))
                .ToDictionary(t => t.NodeID);

            // expect each node only has 1 or 2 connected raceways
            if (drw.Any(kv => kv.Value.Raceways.Count > 2))
                return (false, new());

            // end nodes of branch with only one associated raceway
            var ends = drw.Where(kv => kv.Value.Raceways.Count == 1)
                .Select(kv => kv.Key)
                .ToList();

            // expect only two end nodes for a branch
            if (ends.Count != 2)
                return (false, new());

            // pick one as start node
            // and the other as end node
            var sn = ends[0];
            var en = ends[1];

            // get rw associate with end node
            var cr = drw[en].Raceways.First();
            drw.Remove(en);

            // create the end node of branch
            Node endNode = new() { ID = en, Length = 0, NodeType = NodeTypeEnum.End, BranchID = branchId };
            Node cn = endNode;

            // find opposite node of raceway
            var pn = cr.FromNode.ID == en ? cr.ToNode.ID : cr.FromNode.ID;
            while (drw.ContainsKey(pn))
            {
                // get rw leading to node
                cr = drw[pn].Raceways.Where(r => r.ID != cr.ID).First();
                drw.Remove(pn);

                // create the from node
                cn = new() { ID = pn, Length = cr.Length, 
                    NodeType = pn == sn ? NodeTypeEnum.Start : NodeTypeEnum.Point, 
                    BranchID = branchId, NextNode = cn };
                pn = cr.FromNode.ID == pn ? cr.ToNode.ID : cr.FromNode.ID;
            }

            if (cn.NodeType != NodeTypeEnum.Start || drw.Count > 0)
                return (false, new());
            else return (true, new()
            {
                ID = branchId,
                StartNode = cn,
                EndNode = endNode
            });
        }

        #endregion

        #region Node Coord

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

        #endregion
    }
}
