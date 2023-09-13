using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

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

        static (bool Success, Node Node) FindFirstNode(this Node node, IEnumerable<Node> searchNodes) =>
            node switch
            {
                null => (false, new()),
                var n when searchNodes.Contains(node) => (true, n),
                _ => FindFirstNode(node.NextNode, searchNodes)
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
            branch.GetNodeList().Where(n => n.NextNode != null).Select(n => n.ToRaceway().Raceway).ToList();

        /// <summary>
        /// Create a raceway from this node to the next link node.
        /// </summary>
        static (bool Success, Raceway Raceway) ToRaceway(this Node fromNode) =>
            fromNode.NextNode switch
            {
                null => (false, new()),
                _ => (true, new()
                {
                    ID = fromNode.ID,
                    FromNode = fromNode,
                    ToNode = fromNode.NextNode,
                    BranchID = fromNode.BranchID == fromNode.NextNode.BranchID ? fromNode.BranchID : "",
                    Length = fromNode.Length
                })
            };

        /// <summary>
        /// A raceway could span multiple nodes of a branch. If so, calculate a
        /// set of raceways between consecutive link nodes of this branch.
        /// </summary>
        /// <param name="startSearchNode">Could be a start node of a branch 
        /// or a node of the branch to begin a search for the first match
        /// node of the raceway. It is expected that the start node is eventually linked to
        /// to the nodes of the raceway once the search is completed starting with the start node.</param>
        /// <param name="raceway">A raceway that potentially span multiple branch nodes</param>
        /// <returns>A list of inner raceways between the two nodes of the input raceway.</returns>
        public static (bool Success, IEnumerable<Raceway> Raceways) ToRaceway(this Node startSearchNode, Raceway raceway)
        {
            var lstRw = new List<Raceway>();
            var (cont, node) = startSearchNode.FindFirstNode(new[] { raceway.FromNode, raceway.ToNode });
            if (!cont || node.NextNode == null) return (false, lstRw);

            var en = raceway.FromNode.ID == node.ID ? raceway.ToNode : raceway.FromNode;

            bool createRW(Node node)
            {
                if (node.ToRaceway() is (true, var rw))
                {
                    lstRw.Add(rw);
                    if (rw.ToNode.ID == en.ID)
                        return true;
                    return createRW(rw.ToNode);
                }
                else return false;
            }

            return (createRW(node), lstRw);
        }

        public static (bool Success, IEnumerable<Raceway> Raceways) ToRaceway(this Branch branch, Raceway raceway) =>
            branch.StartNode.ToRaceway(raceway);

        /// <summary>
        /// Validate that these raceways is a complete branch with
        /// a common branch ID, a start node, and an end node.
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
                // the 2 end nodes are linked
                isErr = r.FromNode.NextNode.ID != r.ToNode.ID;
                // raceway length and length to next node are equal
                isErr = r.FromNode.Length != r.Length;
                if ((isErr)) return (false, new());
            }

            // check if all nodes are linked with one another
            var dn = nodes.ToDictionary(n => n.ID);
            var cn = lsn.First().ID;
            while (dn.TryGetValue(cn, out var node))
            {
                dn.Remove(cn);
                cn = node.NextNode.ID;
            }
            // there are nodes that are not linked
            isErr = dn.Count > 0;

            return (!isErr, isErr ? new() :
                new()
                {
                    ID = lbid.First(),
                    StartNode = lsn.First(),
                    EndNode = len.First()
                });
        }

        /// <summary>
        /// Convert the raceways possibly from various branches to a
        /// branch data structure. These raceways could be a route
        /// of a cable.
        /// </summary>
        /// <returns>A branch data structure linking all the nodes.</returns>
        public static (bool Success, Branch Branch) ToBranch(IEnumerable<Raceway> raceways, string branchId)
        {
            var drw = raceways
                .Select(r => (Node: r.FromNode, Raceway: r))
                .Concat(raceways.Select(r => (Node: r.ToNode, Raceway: r)))
                .GroupBy(t => t.Node.ID)
                .Select(g => (NodeID: g.Key, Raceways: g.Select(t => t).ToList()))
                .ToDictionary(t => t.NodeID);

            // expect each node only has 1 or 2 connected raceways
            if (drw.Any(kv => kv.Value.Raceways.Count > 2))
                return (false, new());

            // end nodes of branch with only one (1) associated raceway
            var ends = drw.Where(kv => kv.Value.Raceways.Count == 1)
                .Select(kv => kv.Value.Raceways.First().Node)
                .ToList();

            // expect only two end nodes for a branch
            if (ends.Count != 2)
                return (false, new());

            // pick one as start node
            // and the other as end node
            var sn = ends[0];
            var en = ends[1];

            // get rw associate with end node
            var cr = drw[en.ID].Raceways.First().Raceway;
            drw.Remove(en.ID);

            // create the end node of branch
            Node endNode = en with { Length = 0, NodeType = NodeTypeEnum.End };
            Node cn = endNode;

            // find opposite node of raceway
            var pn = cr.FromNode.ID == en.ID ? cr.ToNode : cr.FromNode;
            while (drw.ContainsKey(pn.ID))
            {
                // get rw leading to node
                cr = drw[pn.ID].Raceways.Where(t => t.Raceway.ID != cr.ID).First().Raceway;
                drw.Remove(pn.ID);

                // create the from node
                cn = pn with { Length = cr.Length, 
                    NodeType = pn.ID == sn.ID ? NodeTypeEnum.Start : NodeTypeEnum.Point, 
                    NextNode = cn };
                pn = cr.FromNode.ID == pn.ID ? cr.ToNode : cr.FromNode;
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

        #region Node

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

        #endregion
    }
}
