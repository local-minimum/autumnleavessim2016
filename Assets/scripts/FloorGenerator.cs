using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class EnumerableExtensions
{
	public static IEnumerable<T> Pairwise<T>(
		this IEnumerable<T> source, System.Func<T, T, T> selector)
	{
		if (source == null) throw new System.ArgumentNullException("source");
		if (selector == null) throw new System.ArgumentNullException("selector");

		using (var e = source.GetEnumerator())
		{
			if (!e.MoveNext()) throw new System.InvalidOperationException("Sequence cannot be empty.");

			T prev = e.Current;

			if (!e.MoveNext()) throw new System.InvalidOperationException("Sequence must contain at least two elements.");

			do
			{
				yield return selector(prev, e.Current);
				prev = e.Current;
			} while (e.MoveNext());
		}
	}
}

public class FloorGenerator : MonoBehaviour {

	struct Room {

		public Vector3 roomScale;
		public bool active;

		List<bool> freeSides;
		public List<int> edges;
		public List<bool> freeEdges;
		int coreEdges;

		private float area;

		public float Area {
			get {
				return area;
			}
		}

		public void RecalculateArea(List<Vector3> verts) {
			Vector3 c = GetCenter(verts);

			Vector3[] pts = new Vector3[edges.Count];
			float[] a = new float[pts.Length];
			for (int i=0; i<pts.Length; i++) {
				pts [i] = verts [edges [i]] - c;
				a [i] = Mathf.Atan2 (pts [i].y, pts [i].x);
			}

			area = pts
				.Select ((p, i) => new {p = p, angle = a [i]})
				.OrderBy (x => x.angle)
				.Select (x => x.p)
				.Pairwise ((v1, v2) => Vector3.Cross(v1, v2))
				.Sum (v => v.magnitude / 2f);
		}

		public Vector3 GetCenter(List<Vector3> verts)
		{
			Vector3 c = Vector3.zero;
			for (int i = 0, l = edges.Count; i < l; i++) {
				c += verts[edges [i]];
			}
			return c / edges.Count;			
		}

		public void NewEdgeToControl(int n) {
			edges.Add (n);
			freeEdges.Add (true);
		}

		public Vector3[] GetAttachmentPoints(int n, out int[] points, List<Vector3> verts) {

			int s = Random.Range(0, freeSides.Sum (e => e ? 1 : 0));
			List<int> sums = new List<int> (coreEdges);
			freeSides.Select (e => e ? 1 : 0).Aggregate(0, (sum, elem) => {sum += elem; sums.Add(sum); return sum;} );
			int attachmentSide = sums.IndexOf (s);

			freeSides [attachmentSide] = false;

			bool attachToCorner = Random.value < 0.7f;
			Vector3 a = verts [edges [attachmentSide]];
			Vector3 b = verts [edges [attachmentSide + 1]];

			if (attachToCorner) {
				
				bool first = Random.value < 0.5f;

				points = new int[2] {
					first ? edges[attachmentSide + 1] : n,
					first ? n : edges [attachmentSide]
				};

				return new Vector3[2] {
					first ? b  : Vector3.Lerp(a, b, Random.Range(0.1f, 0.5f)),
					first ? Vector3.Lerp(a, b, Random.Range(0.5f, 0.9f)) : a
				};

			} else {
				
				points = new int[2] {
					n,
					n + 1
				};

				Vector3 a1 = Vector3.Lerp (a, b, Random.Range (0.1f, 0.4f));
				return new Vector3[2] { 
					Vector3.Lerp(a1, b, Random.Range(0.5f, 0.9f)),
					a1
				};
			}
				
		}

		public Room(int n) {
			
			active = true;
			edges = new List<int>();
			freeEdges = new List<bool>();
			freeSides = new List<bool>();

			coreEdges = 4;
			for (int i=0;i<coreEdges;i++) {
				edges.Add(n + i);
				freeEdges.Add(true);
				freeSides.Add(true);
			}

			roomScale = new Vector3(1f, 0f, Random.Range(0.95f, 1.05f));
			roomScale /= Mathf.Min(1f, roomScale.z);

			area = 0;
		}

		public Room(int n, int[] oldCorners) {

			active = true;
			edges = new List<int>();
			freeEdges = new List<bool>();
			freeSides = new List<bool>();

			coreEdges = 4;
			for (int i=0;i<coreEdges;i++) {

				if (i < oldCorners.Length) {
					edges.Add(oldCorners[i]);
					freeEdges.Add(false);
					freeSides.Add(i == 0);
				} else {
					edges.Add(n + i);
					freeEdges.Add(true);
					freeSides.Add(true);
				}
			}

			roomScale = new Vector3(1f, 0f, Random.Range(0.95f, 1.05f));
			roomScale /= Mathf.Min(1f, roomScale.z);

			area = 0;
		}

	}

	[SerializeField]
	float stopAtArea = 5;

	[SerializeField]
	FloorGenerator parent;

	[SerializeField]
	MeshFilter mFilt;

	[SerializeField, Range(1f, 2f)]
	float upscaleSpeed = 1.7f;

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

	public void ReBuild() {
		StartCoroutine (_Build ());
	}

	IEnumerator<WaitForSeconds> _Build() {
		float area = 0;

		List<Vector3> verts = new List<Vector3>();
		List<int> tris = new List<int> ();
		List<Vector2> UVs = new List<Vector2> ();
		mesh.MarkDynamic ();
		mesh.Clear ();

		List<Room> shapes = new List<Room> ();

		int nShapes = Random.Range (1, 4);

		Room baseRoom = new Room();

		while (area < stopAtArea) {

			int count = shapes.Count;

			if (count == 0 || count < nShapes && Random.value < area * count / stopAtArea * nShapes	) {

				int n = verts.Count;

				if (count == 0) {
					shapes.Add (new Room (n));
					baseRoom = shapes [0];
				} else {

					int[] attachNs;
					Vector3[] newV = baseRoom.GetAttachmentPoints (n, out attachNs, verts);
					for (int i = 0; i < attachNs.Length; i++) {
						if (attachNs [i] >= n) {
							verts.Add (newV [i]);
						}
					}

					//TODO: Make thees
					shapes.Add (new Room (n, attachNs));
				}

				Room newR = shapes [shapes.Count - 1];
				Vector3 scale = newR.roomScale;

				//TODO: Room should have these enumerate
				// verts.AddRange(newR.GetVerts(verts));

				verts.Add (new Vector3 (-scale.x, 0, -scale.z));
				verts.Add (new Vector3 (-scale.x, 0, scale.z));
				verts.Add (new Vector3 (scale.x, 0, scale.z));
				verts.Add (new Vector3 (scale.x, 0, -scale.z));

				// UVs.AddRange(newR.GetUVs());
				UVs.Add (new Vector2 (-1f, -1f));
				UVs.Add (new Vector2 (-1f, 1f));
				UVs.Add (new Vector2 (1f, 1f));
				UVs.Add (new Vector2 (1f, -1f));

				//tris.AddRange(newR.GetTris())
				tris.Add (n);
				tris.Add (n + 1);
				tris.Add (n + 3);

				tris.Add (n + 1);
				tris.Add (n + 2);
				tris.Add (n + 3);

			}

			area = 0;

			for (int s = 0; s < shapes.Count; s++) {

				Room r = shapes [s];
				if (r.active) {

					Vector3 center = r.GetCenter (verts);

					for (int i=0, l=r.edges.Count; i<l; i++) {

						if (!r.freeEdges [i]) {
							continue;
						}

						Vector3 v = verts [r.edges[i]] - center;
						verts[r.edges[i]] = new Vector3(v.x * r.roomScale.x, 0, v.z * r.roomScale.z) * upscaleSpeed;

					}

					r.RecalculateArea (verts);

				}

				area += r.Area;

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
