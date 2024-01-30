using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class ShipManager : MonoBehaviour
{
    public Node node;
    public Alien alien;
    public Captain captainRef;
    public Bot botRef;
    public Bot bot;
    public Captain captain;
    public int dim;
    public int k;

    public Dictionary<Vector2, Node> nodes;
    public List<Alien> aliens;

    public void Init(Bot botRef, int dim = -1, int k = -1, int sims = 1) {
        if (dim != -1) {
            this.dim = dim;
        }  
        if (k != -1) {
            this.k = k;
        }
        if(k > dim * dim) {
            Debug.LogError("k must be less than or equal to dim * dim");
            return;
        }
        if(botRef == null) {
            Debug.LogError("Bot cannot be null");
            return;
        }

        this.botRef = botRef;
        CreateGrid();
        GenerateShip();
    }

    public void Ready() {
        PlaceCaptain();
        BoardAliens();
        PlaceBot();
    }

    public void Reset() {
        Debug.Log("Resetting");
    }

    void CreateGrid() {
        nodes = new Dictionary<Vector2, Node>();

        // generate the grid of nodes
        for (int x = 0; x < dim; x++) {
            for (int y = 0; y < dim; y++) {
                Node spawnedNode = Instantiate(node, new Vector3(x, y, 0), Quaternion.identity);
                spawnedNode.pos = new Vector2(x, y);
                spawnedNode.transform.parent = this.transform;
                spawnedNode.name = $"Node {x} {y}";
                spawnedNode.Close();

                nodes[spawnedNode.pos] = spawnedNode;
            }
        }

        // draw column lines
        for (int x = 0; x < dim + 1; x++) {
            GameObject lineObj = new GameObject($"Column Line {x}");
            lineObj.transform.parent = this.transform;

            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = Color.black;
            line.endColor = Color.black;
            line.startWidth = 0.05f;
            line.endWidth = 0.05f;
            line.useWorldSpace = true;
            line.SetPosition(0, new Vector3(x - 0.5f, -0.5f, 0));
            line.SetPosition(1, new Vector3(x - 0.5f, dim - 0.5f, 0));
        }
        
        // draw row lines
        for (int y = 0; y < dim + 1; y++) {
            GameObject lineObj = new GameObject($"Row Line {y}");
            lineObj.transform.parent = this.transform;

            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = Color.black;
            line.endColor = Color.black;
            line.startWidth = 0.05f;
            line.endWidth = 0.05f;
            line.useWorldSpace = true;
            line.SetPosition(0, new Vector3(-0.5f, y - 0.5f, 0));
            line.SetPosition(1, new Vector3(dim - 0.5f, y - 0.5f, 0));
        }
    }

    /**
    * Generates the structure of the ship by opening nodes as per the requirements
    * 1. Choose a square at random to open
    * 2. Iterate:
    *  a. Identify all currently blocked cells that have exactly one open neighbor
    *  b. Open one of these cells at random
    *  c. Repeat until no more cells can be opened
    * 3. Idenfity all cells that are 'dead ends' - open cells with one neighbor.
    * 4. For approximately half these cells, pick one of their closed neighbors at random and open it.
    */
    void GenerateShip() {
        // choose a random square to open
        Vector2 start = new Vector2(Mathf.RoundToInt(Random.Range(0, dim)), Mathf.RoundToInt(Random.Range(0, dim)));
        GetNode(start).Open();
        
        List<Vector2> openNodes = new List<Vector2>{ start };
        while (openNodes.Count > 0) {
            // find all blocked nodes with open neighbors
            HashSet<Vector2> candidates = new HashSet<Vector2>();
            foreach (Vector2 pos in openNodes) {
                foreach (Vector2 neighbor in GetNeighbors(pos)) {
                    Node neighborNode = GetNode(neighbor);
                    if (neighborNode != null && !neighborNode.open) {
                        candidates.Add(neighbor);
                    }
                }
            }

            // of those blocked nodes, find the ones with exactly one open neighbor
            List<Vector2> eligible = new List<Vector2>();
            foreach(Vector2 candidate in candidates) {
                int numOpenNeighbors = 0;
                foreach (Vector2 neighbor in GetNeighbors(candidate)) {
                    Node neighborNode = GetNode(neighbor);
                    if (neighborNode != null && neighborNode.open) {
                        numOpenNeighbors++;
                    }
                }

                if (numOpenNeighbors == 1) {
                    eligible.Add(candidate);
                } 
            }

            // if there are no eligible candidates, we're done
            if (eligible.Count == 0) {
                break;
            }

            // otherwise, pick a random candidate and open it
            Vector2 chosen = eligible[Mathf.RoundToInt(Random.Range(0, eligible.Count - 1))];
            GetNode(chosen).Open();
            openNodes.Add(chosen);
        }

        // find all dead ends
        List<Vector2> deadEnds = new List<Vector2>();
        foreach (Vector2 pos in openNodes) {
            int numOpenNeighbors = 0;
            foreach (Vector2 neighbor in GetNeighbors(pos)) {
                Node neighborNode = GetNode(neighbor);
                if (neighborNode != null && neighborNode.open) {
                    numOpenNeighbors++;
                }
            }

            if (numOpenNeighbors == 1) {
                deadEnds.Add(pos);
            }
        }

        // open half of the dead ends
        foreach(Vector2 pos in deadEnds) {
            if (Random.Range(0, 2) == 0) {
                continue;
            }

            List<Vector2> closedNeighbors = new List<Vector2>();
            foreach (Vector2 neighbor in GetNeighbors(pos)) {
                Node neighborNode = GetNode(neighbor);
                if (neighborNode != null && !neighborNode.open) {
                    closedNeighbors.Add(neighbor);
                }
            }
            if (closedNeighbors.Count == 0) {
                continue;
            }

            Vector2 chosen = closedNeighbors[Mathf.RoundToInt(Random.Range(0, closedNeighbors.Count - 1))];
            GetNode(chosen).Open();
            openNodes.Add(chosen);
        }
        
    }

    /**
    * Places the captain in a random open node
    */
    void PlaceCaptain() {
        List<Vector2> openNodes = new List<Vector2>();
        foreach (KeyValuePair<Vector2, Node> entry in nodes) {
            if (entry.Value.open) {
                openNodes.Add(entry.Key);
            }
        }

        Vector2 chosen = openNodes[Mathf.RoundToInt(Random.Range(0, openNodes.Count - 1))];
        captain = Instantiate(this.captainRef, new Vector3(chosen.x, chosen.y, 0), Quaternion.identity);
        captain.pos = chosen;
    }

    /**
    * Boards k aliens in random open nodes
    */
    void BoardAliens() {
        GameObject fleet = new GameObject("Fleet");
        aliens = new List<Alien>();

        List<Vector2> openNodes = new List<Vector2>();
        foreach (KeyValuePair<Vector2, Node> entry in nodes) {
            if (entry.Value.open) {
                openNodes.Add(entry.Key);
            }
        }

        for (int i = 0; i < k; i++) {
            Vector2 chosen = openNodes[Mathf.RoundToInt(Random.Range(0, openNodes.Count - 1))];
            Node chosenNode = GetNode(chosen);
            if(chosenNode.occupied) {
                i--;
                continue;
            }
            
            chosenNode.occupied = true;
            Alien alien = Instantiate(this.alien, new Vector3(chosen.x, chosen.y, 0), Quaternion.identity);
            alien.transform.parent = fleet.transform;
            alien.pos = chosen;
            aliens.Add(alien);
        }
    }

    void PlaceBot() {
        List<Vector2> openNodes = new List<Vector2>();
        foreach (KeyValuePair<Vector2, Node> entry in nodes) {
            if (entry.Value.open && !entry.Value.occupied) {
                openNodes.Add(entry.Key);
            }
        }

        Vector2 chosen = openNodes[Mathf.RoundToInt(Random.Range(0, openNodes.Count - 1))];
        GetNode(chosen).occupied = true;
        bot = Instantiate(botRef, new Vector3(chosen.x, chosen.y, 0), Quaternion.identity);
        botRef.pos = chosen;
    }

    /**
    * Given a position, returns a list of all adjacent positions
    */
    List<Vector2> GetNeighbors(Vector2 pos) {
        List<Vector2> neighbors = new List<Vector2>
        {
            new Vector2(pos.x + 1, pos.y),
            new Vector2(pos.x - 1, pos.y),
            new Vector2(pos.x, pos.y + 1),
            new Vector2(pos.x, pos.y - 1)
        };
        return neighbors;


    }

    /**
    * Given a position, returns the node at that position, or null if there is no node there
    */
    public Node GetNode(Vector2 pos) {
        if (nodes.ContainsKey(pos)) {
            return nodes[pos];
        }

        return null;
    }
}
