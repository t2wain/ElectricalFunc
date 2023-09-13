namespace RacewayLib
{
    public static class CoordAction
    {
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
