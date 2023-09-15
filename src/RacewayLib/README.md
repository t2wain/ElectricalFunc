# Modeling Raceway Data

This project models raceway data structures needed for cable routing. Standard graph algorithm, like Dijkstra, is employed in the cable routing calculation. Graph algorithm works with edges and vertices and, therefore, raceway data should be modeled as edges (raceway) and vertices (node) in the routing application.

# Importing Raceway Data

On a project, raceway are typically modeled in a 3D application, like Aveva E3D or Intergraph Smart 3D, with significant effort. The routing application will need features to import such data from the 3D application. It also needs change management features, like Add, Update, and Delete, for subsequent imports. As an example, raceway data from Aveva E3D can be exported as pipe, branch, HEAD, TAIL, fitting, TUBI, and coordinate. The routing application needs to transform such data into edges (raceway) and vertices (node).

# Generated Raceway Data

Additionally, the routing application will generate other raceway data such as intermediate nodes, connections, and paths which reference the imported raceway data and so the change management features must also consider these data relationships on subsequent imports of raceway data. 

# Objective

This project implements two types of data structure, Branch and Raceway. Branch data structure is similar to that from Aveve E3D while Raceway data structue is similar to edges and vertices use graph algorithm. 
