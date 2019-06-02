using System;
using System.Collections.Generic;
namespace SchemaMigration
{
    /// <summary>
    /// Depth First Directed Acyclic Forest Traversal And Backtracking Algorithm to identify the order in which the schemas are to be uploaded to the IA
    /// </summary>
    internal class DependencyGraph
    {
        Dictionary<SchemaDetails, bool> visited { get; set; }
        Dictionary<SchemaDetails, List<SchemaDetails>> adjacencyList { get; set; }

        private List<SchemaDetails> dependencyList; 
        internal DependencyGraph()
        {
            adjacencyList = new Dictionary<SchemaDetails, List<SchemaDetails>>();
            dependencyList = new List<SchemaDetails>();
        }

        internal void createAdjacencyList(Dictionary<SchemaDetails, List<SchemaDetails>> adjacencyList)
        {
            this.adjacencyList = adjacencyList;
        }
        internal List<SchemaDetails> DFSCaller()
        {
            visited = new Dictionary<SchemaDetails, bool>();
            foreach (var x in adjacencyList.Keys)
                visited.Add(x, false);
            
            foreach (var elem in adjacencyList.Keys)
                DFS(elem);
            return this.dependencyList;
        }
        internal void DFS(SchemaDetails schema)
        {
            if (visited[schema] == false)
            {
                visited[schema] = true;
                foreach (var i in adjacencyList[schema])
                    DFS(i);
                this.dependencyList.Add(schema);

            }
        }

    }
}
