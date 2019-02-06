using System.Collections.Generic;
using System.Linq;

namespace SRServer.Objects.Generation
{

    public static class LevelDecomposer
    {
        public static IEnumerable<IEnumerable<Node>> DecomposeLevel(bool[,] level, bool value = false)
        {
            var islands = new List<IEnumerable<Node>>();

            var found = new bool[level.GetLength(0), level.GetLength(1)];

            for (var y = 0; y < level.GetLength(0); y++)
            {
                for (var x = 0; x < level.GetLength(1); x++)
                {
                    // Skip if already in list or isn't wall
                    if (found[x, y] || level[x, y])
                        continue;

                    // Get contiguous shape
                    var island = BetterFloodFill(level, x, y, value);

                    if (island.Count() == 0)
                        continue;

                    // Mark all in contiguous island as found
                    foreach (var i in island)
                    {
                        found[i.x, i.y] = true;
                    }

                    // Add to list to decompose
                    islands.Add(island);
                }
            }

            return islands;
        }

        private static IEnumerable<Node> BetterFloodFill(bool[,] fill, int x, int y, bool value)
        {
            // Q allows us to non-recursively flood fill.
            var Q = new Queue<Node>();
            // List of nodes that we'll return, these will become 'ground' tiles
            var Nodes = new List<Node>();

            if (fill[x, y] != value) // Return because we can't do anything
                return Nodes;

            Q.Enqueue(new Node { x = x, y = y }); // Initial point

            while (Q.Count > 0) // While there are tiles to check
            {
                // Pull from queue
                Node N = Q.Dequeue();

                // Prevent crashes
                if (N.x < 0 || N.x > fill.GetLength(0)
                    || N.y < 0 || N.y > fill.GetLength(1)
                    || Nodes.Contains(N))
                    continue;

                // Check if value is what we want
                if (fill[N.x, N.y] == value)
                {
                    Nodes.Add(N); // Add to list to be returned
                    if (N.x - 1 > 0)
                        Q.Enqueue(new Node { x = N.x - 1, y = N.y }); // Add West node
                    if (N.y - 1 > 0)
                        Q.Enqueue(new Node { x = N.x, y = N.y - 1 }); // Add North node
                    if (N.x + 1 < fill.GetLength(0))
                        Q.Enqueue(new Node { x = N.x + 1, y = N.y }); // Add East node
                    if (N.y + 1 < fill.GetLength(1))
                        Q.Enqueue(new Node { x = N.x, y = N.y + 1 }); // Add South node
                }
            }

            return Nodes;
        }
    }

    public struct Node
    {
        public int x;
        public int y;
    }
}
