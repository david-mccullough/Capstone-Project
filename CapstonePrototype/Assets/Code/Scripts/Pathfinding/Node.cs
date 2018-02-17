using UnityEngine;
using System.Collections.Generic;

public class Node {
	public List<Node> neighbours;
	public Map.Coord pos;
	
	public Node() {
		neighbours = new List<Node>();
	}
	
	public float DistanceTo(Node n) {
		if(n == null) {
			Debug.LogError("null reference to node");
		}
		
		return Vector2.Distance(
			new Vector2(pos.x, pos.y),
			new Vector2(n.pos.x, n.pos.y)
			);
	}
	
}
