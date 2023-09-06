using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLib
{
    public static class BFSAlgo
    {
        public record BFSEdge(IVertex ParentVertex, IVertex ChildVertex);

        public static IEnumerable<BFSEdge> BFS(this IGraph g, IVertex source)
        {
            var neigbors = g.GetNeigborVertices(source).GetEnumerator();
            var visitedV = new HashSet<IVertex>(new IVertex[] { source });
            var queue = new Queue<(IVertex Vertex, IEnumerator<IVertex> Neighbors)>();
            queue.Enqueue((source, neigbors));
            while (queue.Count > 0)
            {
                var i = queue.Peek();
                var parentV = i.Vertex;
                var children = i.Neighbors;

                while (children.MoveNext())
                {
                    var childV = children.Current;
                    if (!visitedV.Contains(childV))
                    {
                        yield return new BFSEdge(parentV, childV);
                        visitedV.Add(childV);
                        neigbors = g.GetNeigborVertices(childV).GetEnumerator();
                        queue.Enqueue((childV, neigbors));
                    }
                };
                queue.Dequeue().Neighbors.Dispose();
            }
        }

    }
}
