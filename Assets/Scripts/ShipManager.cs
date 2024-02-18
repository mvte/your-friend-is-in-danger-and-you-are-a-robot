using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using Utils;


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

    private ConcurrentBag<bool[,]> pregeneratedShips;

    public void Init(Bot botRef, int dim = -1, int k = -1) {
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

    public void PregenerateShips(int dim, int numShips) {
        pregeneratedShips = ParallelShipGenerator.GenerateParallelShips(dim, numShips);
    }

    public void Ready() {
        PlaceCaptain();
        BoardAliens();
        PlaceBot();
    }

    public void Reset() {
        // destroy all children
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        Destroy(GameObject.Find("Fleet"));
        Destroy(GameObject.Find("Bot"));
        Destroy(GameObject.Find("Captain(Clone)"));
        nodes = null;
        aliens = null;
        captain = null;
        bot = null;
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
    * 4. For approximately half these cells, pick one of their closed neighbors at random and open it.\
    *
    * TODO: if we have no pregenerated ships, open all nodes
    */
    void GenerateShip() {
        if(pregeneratedShips != null && pregeneratedShips.TryTake(out bool[,] ship)) {
            for (int x = 0; x < dim; x++) {
                for (int y = 0; y < dim; y++) {
                    if (ship[x, y]) {
                        nodes[new Vector2(x, y)].Open();
                    }
                }
            }
        } else {
            for (int x = 0; x < dim; x++) {
                for (int y = 0; y < dim; y++) {
                    nodes[new Vector2(x, y)].Open();
                }
            }
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

        Vector2 chosen = openNodes[Mathf.RoundToInt(Random.Range(0, openNodes.Count))];
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
            if (entry.Value.open && entry.Key != captain.pos) {
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
            if (entry.Value.open && !entry.Value.occupied && captain.pos != entry.Key) {
                openNodes.Add(entry.Key);
            }
        }

        Vector2 chosen = openNodes[Mathf.RoundToInt(Random.Range(0, openNodes.Count))];
        bot = Instantiate(botRef, new Vector3(chosen.x, chosen.y, 0), Quaternion.identity);
        bot.pos = chosen;
        bot.name = "Bot";
    }

    /**
    * Given a position vector, returns a list of all adjacent position vector
    */
    List<Vector2> GetNeighbors(Vector2 pos) {
        List<Vector2> neighbors = new List<Vector2>
        {
            (pos + Vector2.right),
            (pos + Vector2.left),
            (pos + Vector2.up),
            (pos + Vector2.down)
        };
        return neighbors;
    }

    /**
    * Returns a list of existing nodes adjacent to a given position.
    */
    public List<Node> GetNeighborNodes(Vector2 pos) {
        List<Node> neighborNodes = new List<Node>();
        List<Vector2> neighbors = GetNeighbors(pos);
        
        foreach(Vector2 neighbor in neighbors) {
            Node neighborNode = GetNode(neighbor);
            if(neighborNode != null) {
                neighborNodes.Add(neighborNode);
            }
        }

        return neighborNodes;
    }

    /**
    * Returns a list of valid nodes adjacent to a given position. 
    * Valid means that the nodes are not occupied or closed.
    */
    public List<Node> GetValidNeighborNodes(Vector2 pos) {
        List<Node> valid = new List<Node>();
        List<Vector2> neighbors = GetNeighbors(pos);
        
        foreach(Vector2 neighbor in neighbors) {
            Node validNode = GetNode(neighbor);
            if(validNode != null && validNode.open && !validNode.occupied) {
                valid.Add(validNode);
            }
        }

        return valid;
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
