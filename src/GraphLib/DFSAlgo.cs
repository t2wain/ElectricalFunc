﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace GraphLib
{
    public static class DFSAlgo
    {
        // TODO
        public interface IDFSAction
        {
            void VisitEdge(IVertex FromVertex, IVertex ToVertex, IEdge Edge, int EdgeType, int Component);
            void PreVisitVertex(IVertex Vertex, int Component, int PreVisitIndex);
            void PostVisitVertex(IVertex Vertex, int PostVisitIndex);
        }

        public record DFSEdge(IVertex FromVertex, IVertex ToVertex, IEdge Edge, int EdgeType, int Component);

        public record DFSVertice(IVertex Vertex, int Component, int PreVisitIndex, int PostVisitIndex);

        record VisitedVertex(IVertex Vertex, IEnumerator<IEdge> OutEdges, int component, int PreVisitIndex);

        public static IEnumerable<DFSEdge> DFS(this IGraph g, IVertex? source = null, IDFSAction? action = null)
        {
            var vertices = g.GetVertices(); // all vertices in the graph
            if (source != null)
                vertices = vertices.Where(v => v == source);

            // DFS results
            // TODO: return both DFSEdge and DFSVertice results
            var lstDFSEdge = new List<DFSEdge>();
            var lstDFSVertice = new List<DFSVertice>();

            // edges of parent vertex that were visited 
            // to be explored in the order of deepest vertex first
            // or most recently visited
            var predecessorV = new Stack<VisitedVertex>();
            var visitedV = new HashSet<IVertex>(); // visited vertex
            var visitedE = new HashSet<IEdge>(); // visited edge
            int component = -1; // component labeling
            int visitIndex = -1; // vertex visited index
            foreach (var topV in vertices)
            {
                // select a vertex not visited to begin a new search
                if (visitedV.Contains(topV))
                    continue; // vertex visited, skip vertex

                component++; // the component of the new top vertex
                visitedV.Add(topV); // track visited vertex
                // save vertex and its edges as the most 
                //recently visited vertex to be explored next
                predecessorV.Push(new (topV, 
                    g.GetOutEdges(topV).GetEnumerator(), component, ++visitIndex));
                action?.PreVisitVertex(topV, component, visitIndex);
                while (predecessorV.Count > 0)
                {
                    // begin to explore the current vertex
                    // get to the most recent visited vertex
                    var parent = predecessorV.Peek();
                    var parentV = parent.Vertex;
                    // edge iterator to resume with next edge to visit
                    var outEdges = parent.OutEdges;

                    // explore the neighbor vertices
                    if (outEdges.MoveNext())
                    {
                        var outEdge = outEdges.Current; // unvisited edge to the next vertex
                        var childV = outEdge.GetOtherVertex(parentV); // visit the child/neighbor vertex
                        var inEdge = outEdge; // edge lead to child/neighbor vertex
                        var etype = 1; // loop back

                        // check if the child/neighbor vertex has been visited
                        if (!visitedV.Contains(childV))
                        {
                            // new vertex found
                            etype = 0; // forward
                            visitedV.Add(childV); // mark vertex as visited
                            // save vertex and its out edges as the most 
                            // recently visited vertex to be explored next
                            predecessorV.Push(new VisitedVertex(childV, 
                                g.GetOutEdges(childV).GetEnumerator(), component, ++visitIndex));
                            action?.PreVisitVertex(childV, component, visitIndex);
                        }

                        // check if the edge has been returned
                        // as the result of the search
                        if (!visitedE.Contains(inEdge))
                        {
                            // return the edge
                            var resEdge = new DFSEdge(parentV, childV, inEdge, etype, component);
                            lstDFSEdge.Add(resEdge);
                            action?.VisitEdge(parentV, childV, inEdge, etype, component);
                            yield return resEdge;
                            visitedE.Add(inEdge); // mark the edge as returned
                        }
                    }
                    else
                    {
                        // all out edges of the current vertex have been
                        // explored and returned. Remove the vertex from stack.
                        var postV = predecessorV.Pop();
                        lstDFSVertice.Add(new(postV.Vertex, 
                            postV.component, postV.PreVisitIndex, ++visitIndex));
                        action?.PostVisitVertex(postV.Vertex, visitIndex);
                        postV.OutEdges.Dispose();
                    }
                }
            }

            // TODO
            // return (Edges: lstDFEdge, Vertices: lstVertice); 
        }

    }
}
