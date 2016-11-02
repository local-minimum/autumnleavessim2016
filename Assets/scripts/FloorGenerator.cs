using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FloorGenerator : MonoBehaviour {

	struct Room {

		public Vector3 roomScale;
		public bool active;

		public List<int> edges;
		public List<bool> freeEdges;

		private float area;

		public float Area {
			get {
				return area;
			}
		}

		public void RecalculateArea(List<Vector3> verts) {
			Vector3 c = GetCenter(verts);

			//Unsorted area?

		}

		Vector3 GetCenter(List<Vector3> verts)
		{
			Vector3 c = Vector3.zero;
			for (int i = 0, l = edges.Count; i < l; i++) {
				c += edges [i];
			}
			return c;			
		}

		public void AddNewEdgeToControl(int n) {
			edges.Add (n);
			freeEdges.Add (true);
		}

		public Room(int n) {
			active = true;

			int corners = 4;

			edges = new List<int>(corners);
			freeEdges = new List<bool>(corners);

			for (int i=0;i<4;i++) {
				edges[i] = n + i;
				freeEdges[i] = true;
			}

			roomScale = new Vector3(1f, 0f, Random.Range(0.5f, 2));
			roomScale /= roomScale.magnitude;

			area = 0;
		}
	}

	[SerializeField]
	float stopAtArea = 5;

	[SerializeField]
	FloorGenerator parent;

	[SerializeField]
	MeshFilter mFilt;

	Mesh mesh;

	void Start () {
		mFilt = GetComponent<MeshFilter> ();
		mesh = new Mesh ();
		mesh.name = "ProcGen Floor";
		mFilt.mesh = mesh;

		if (parent == null) {
			StartCoroutine (_Build ());
		}
	}


	IEnumerator<WaitForSeconds> _Build() {
		float area = 0;

		List<Vector3> verts = new List<Vector3>();
		List<int> tris = new List<int> ();
		List<Vector2> UVs = new List<Vector2> ();
		mesh.MarkDynamic ();
		mesh.Clear ();

		List<Room> shapes = new List<Room> ();

		while (area < stopAtArea) {

			if (!shapes.Any (e => e.active)) {

				int n = verts.Count;

				shapes.Add (new Room (n));

				Vector3 scale = shapes [shapes.Count].roomScale;

				verts.Add (new Vector3 (-scale.x, 0, -scale.z));
				verts.Add (new Vector3 (-scale.x, 0, scale.z));
				verts.Add (new Vector3 (scale.x, 0, scale.z));
				verts.Add (new Vector3 (scale.x, 0, -scale.z));

				UVs.Add (new Vector2 (-1f, -1f));
				UVs.Add (new Vector2 (-1f, 1f));
				UVs.Add (new Vector2 (1f, 1f));
				UVs.Add (new Vector2 (1f, -1f));

				tris.Add (n);
				tris.Add (n + 1);
				tris.Add (n + 3);

				tris.Add (n + 1);
				tris.Add (n + 2);
				tris.Add (n + 3);

			}

			for (int s = 0; s < shapes.Count; s++) {

				Room r = shapes [s];
				if (r.active) {


					Vector3 center = Vector3.zero;

					for (int i=0; i<r.edges.Count; i++) {
						center += verts [r.edges[i]];
					}

					center /= 4f;

					for (int i=0, l=r.edges.Count; i<l; i++) {

						if (!r.freeEdges [i]) {
							continue;
						}

						Vector3 v = verts [r.edges[i]] - center;
						verts[r.edges[i]] = new Vector3(v.x * r.roomScale.x, 0, v.z * r.roomScale.z) * 1.1f;

					}

					area = 0;

					for (int i=0; i<r.edges.Count; i++) {
						area += verts [r.edges[i]].x * verts [r.edges[i]].z;
					}

				}

			}

			mesh.SetVertices (verts);
			mesh.SetTriangles (tris, 0);
			mesh.SetUVs (0, UVs); 
			mesh.RecalculateBounds ();
			mesh.RecalculateNormals ();

			yield return new WaitForSeconds (0.1f);

		}
		//mesh.UploadMeshData(false);

	}
}
