# Cable Routing Criteria

The primary objective of routing a cable is to find the shortest path between two equipment. However, there are also other critera that will influence the final route, such as:
- Segregation system - cables of different voltages and or services sometime cannot be mixed together in the same raceway for safety reason. Therefore, cables can only be routed through a raceway with matching segregation system designation.
- Blocked raceway - some raceway might be blocked temporarily or permanently while some cable may also want to avoid certain raceway.
- Prerred raceway - some cable may give preference through certain raceway if it is possible.
- Required raceway -  some cable may be required to be routed through certain raceway.
- Tray fill - cable may need to avoid raceway that are already full.

# Graph Shortest Path Algorithm

The typical algorithm used to calculate the shortest cable route would be the **Dijkstra's** graph algorithm. This algorithm is a fundamental algorithm which is well documented in textbooks. The library **GraphLib** has a *standard* implementation of this graph algorithm. 

To accomodate the additional routing criteria, this algorithm implementation accepts an input parameter of a custom *weight function*. This weight function will evaluate each edge (raceway) and only return edges (with an additional scaled weight) that have satisfied all critera.