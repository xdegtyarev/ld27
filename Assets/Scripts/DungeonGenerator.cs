using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
	public static DungeonGenerator instance;
	public float density;
	public float maxConnectionLength = 15f;
	public int ConnectionWidth = 2;

	public Vector2 maxCellBounds;
	public Vector2 minCellBounds;
	public Vector2 dungeonBounds;
	public GameObject floorTile;
	public GameObject wallTile;
	public Texture2D[] floorTextures;
	public Texture2D[] wallTextures;
	public List<Rect> cells;
	public List<Rect> Vcorridor;
	public List<Rect> Hcorridor;
	public List<Rect> cellWithVcorridors;
	public List<GameObject> rooms;
	public Dictionary<Rect,List<Rect>> connections = new Dictionary<Rect, List<Rect>>();
	public System.Random random = new System.Random();

	void Awake()
	{
		instance = this;
		bool intersected;
		for (int i = 0; i<dungeonBounds.x; i++) {
			for (int j = 0; j<dungeonBounds.y; j++) {
				if (random.NextDouble() > (1 - density)) {
					Rect r = new Rect(i, j, random.Next((int)minCellBounds.x, (int)maxCellBounds.x), random.Next((int)minCellBounds.y, (int)maxCellBounds.y));
					intersected = false;
					for (int k = 0; k < cells.Count; k++) {
						if (Intersects(r, cells[k])) {
							intersected = true;
							break;
						}
					}
					if (intersected) {
						j--;
						continue;
					} else {
						cells.Add(r);
					}
				}
			}
		}
		Debug.Log("Created Cells");
		foreach (var o in cells) {
			foreach (var o2 in cells) {
				if (o.center != o2.center) {					
					CreateConnection(o, o2);
				}
			}
		}
		Debug.Log("Created Connections");
		foreach (var r in cells) {
			if (connections.ContainsKey(r)) {
				var go = CreateFloor(r);
				if (!cellWithVcorridors.Contains(r)) {
					var go2 = CreateWall(r);
					go2.transform.parent = go.transform;
				}
				rooms.Add(go);
			}
		}
		Debug.Log("Created Rooms");
		foreach (var r in Hcorridor) {
			CreateFloor(r);
			CreateWall(r);
		}

		foreach (var r in Vcorridor) {
			CreateFloor(r);
		}
		Debug.Log("Created Corridors");
		Debug.Log("Done");
	}

	GameObject CreateWall(Rect r)
	{
		GameObject go2 = Instantiate(wallTile, new Vector3(r.center.x, 0, r.yMax), Quaternion.identity) as GameObject;
		go2.transform.parent = transform;
		go2.transform.localScale = new Vector3(r.width, 1, 1);
		var goRenderer = go2.GetComponentInChildren<MeshRenderer>();
		Texture2D tex = wallTextures[Random.Range(0, wallTextures.Length)];
		goRenderer.material.SetTexture("_MainTex", tex);
		goRenderer.material.SetTextureScale("_MainTex", new Vector2(r.width / (tex.width / 128), 1));
		go2.name = "Wall";
		return go2;
	}

	GameObject CreateFloor(Rect r)
	{
		GameObject go = Instantiate(floorTile, new Vector3(r.center.x, 0, r.center.y), Quaternion.identity) as GameObject;
		go.transform.parent = transform;
		go.transform.localScale = new Vector3(r.width, 1, r.height);
		var goRenderer = go.GetComponentInChildren<MeshRenderer>();
		goRenderer.material.SetTexture("_MainTex", floorTextures[Random.Range(0, floorTextures.Length)]);
		goRenderer.material.SetTextureScale("_MainTex", new Vector2(r.width, r.height));
		return go;
	}

	void CreateConnection(Rect a, Rect b)
	{
		if (Vector2.Distance (a.center, b.center) > maxConnectionLength) {
			return;
		}

		if (connections.ContainsKey(a)) {
			if (connections[a].Contains(b)) {
				return;
			}
		}

		bool overlaps;
		float yMin = a.yMin < b.yMin ? a.yMin : b.yMin; 
		float yMax = a.yMax > b.yMax ? a.yMax : b.yMax; 
		float intXMin;
		float intXMax;
		if (a.xMax - b.xMin < 0) {
			intXMin = a.xMax;
			intXMax = b.xMin;
		} else {
			intXMin = b.xMax;
			intXMax = a.xMin;
		}

		for (int i = (int)yMin; i<yMax; i++) {
			if (a.Contains(new Vector2(a.center.x, i)) && b.Contains(new Vector2(b.center.x, i))&&
			    a.Contains(new Vector2(a.center.x, i+ConnectionWidth)) && b.Contains(new Vector2(b.center.x, i+ConnectionWidth))) {

				Rect rect = new Rect(intXMin, i, intXMax - intXMin, ConnectionWidth);
				overlaps = false;
				foreach (var o in cells) {
					if (o != a && o != b) {
						if (Intersects(rect, o)) {
							overlaps = true;
							break;
						}
					}
				}
				foreach (var o in Vcorridor) {
					if (Intersects(rect, o)) {
						overlaps = true;
						break;
					}
				}
				foreach (var o in Hcorridor) {
					if (Intersects(rect, o)) {
						overlaps = true;
						break;
					}
				}
				if (overlaps) {
					continue;
				}

				Hcorridor.Add(rect);
				if (connections.ContainsKey(a)) {
					connections[a].Add(b);
				} else {
					var r = new List<Rect>();
					r.Add(b);
					connections.Add(a, r);
				}
				if (connections.ContainsKey(b)) {
					connections[b].Add(a);
				} else {
					var r = new List<Rect>();
					r.Add(a);
					connections.Add(b, r);
				}
				return;
			}
		}

		float xMin = a.xMin < b.xMin ? a.xMin : b.xMin; 
		float xMax = a.xMax > b.xMax ? a.xMax : b.xMax; 
		float intYMin;
		float intYMax;
		bool aCutWall;
		if (a.yMax - b.yMin < 0) {
			aCutWall = true;
			intYMin = a.yMax;
			intYMax = b.yMin;
		} else {
			aCutWall = false;
			intYMin = b.yMax;
			intYMax = a.yMin;
		}

		for (int i = (int)xMin; i<xMax; i++) {
			if (a.Contains(new Vector2(i, a.center.y)) && b.Contains(new Vector2(i, b.center.y)) &&
				a.Contains(new Vector2(i+ConnectionWidth, a.center.y)) && b.Contains(new Vector2(i+ConnectionWidth, b.center.y))) {
				overlaps = false;
				Rect rect = new Rect(i, intYMin, ConnectionWidth, intYMax - intYMin);

				foreach (var o in cells) {
					if (o != a && o != b) {
						if (Intersects(rect, o)) {
							overlaps = true;
							break;
						}
					}
				}
				foreach (var o in Vcorridor) {
					if (Intersects(rect, o)) {
						overlaps = true;
						break;
					}
				}
				foreach (var o in Hcorridor) {
					if (Intersects(rect, o)) {
						overlaps = true;
						break;
					}
				}
				if (overlaps) {
					continue;
				}

				if (aCutWall) {
					if(cellWithVcorridors.Contains(a)){
						return;
					}
					Vcorridor.Add(rect);
					CreateWall(new Rect(a.x, a.yMax, (i - a.x), 0f));
					CreateWall(new Rect(i + ConnectionWidth, a.yMax, (a.xMax - i - ConnectionWidth), 0f));
					cellWithVcorridors.Add(a);
				} else {
					if(cellWithVcorridors.Contains(b)){
						return;
					}
					Vcorridor.Add(rect);
					CreateWall(new Rect(b.x, b.yMax, (i - b.x), 0f));
					CreateWall(new Rect(i + ConnectionWidth, b.yMax, (b.xMax - i -ConnectionWidth), 0f));
					cellWithVcorridors.Add(b);
				}
				if (connections.ContainsKey(a)) {
					connections[a].Add(b);
				} else {
					var r = new List<Rect>();
					r.Add(b);
					connections.Add(a, r);
				}
				if (connections.ContainsKey(b)) {
					connections[b].Add(a);
				} else {
					var r = new List<Rect>();
					r.Add(a);
					connections.Add(b, r);
				}
				return;
			}
		}
	}

	bool Intersects(Rect o, Rect o2)
	{
		return !(o.yMax < o2.yMin || o.yMin > o2.yMax || o.xMax < o2.xMin || o.xMin > o2.xMax);
	}
}
