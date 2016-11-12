﻿using UnityEngine;
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

        bool free;
        public Vector3 roomScale;
        public bool active;

        List<bool> freeSides;
        public List<int> edges;
        public List<bool> freeEdges;
        int coreEdges;

        public bool Attatched
        {
            get
            {
                return !free;
            }
        }

        private float area;

        public float Area {
            get {
                return area;
            }
        }

        public string Status
        {
            get
            {
                return string.Format("{0} floor part ({1} m2), {2} scale",
                    free ? "Base" : "Attached", area, roomScale);
            }
        }
        public void RecalculateArea(List<Vector3> verts) {
            Vector3 c = GetCenter(verts);

            Vector3[] pts = new Vector3[edges.Count];
            float[] a = new float[pts.Length];
            for (int i = 0; i < pts.Length; i++) {
                pts[i] = verts[edges[i]] - c;
                a[i] = Mathf.Atan2(pts[i].y, pts[i].x);
            }

            area = pts
                .Select((p, i) => new { p = p, angle = a[i] })
                .OrderBy(x => x.angle)
                .Select(x => x.p)
                .Pairwise((v1, v2) => Vector3.Cross(v1, v2))
                .Sum(v => v.magnitude / 2f);
        }

        public Vector3 GetCenter(List<Vector3> verts)
        {
            Vector3 c = Vector3.zero;
            for (int i = 0, l = edges.Count; i < l; i++) {
                //Debug.Log(string.Format("{0}, {1}, {2}", i, edges[i], verts.Count));
                c += verts[edges[i]];
            }
            return c / edges.Count;
        }

        public void NewEdgeToControl(int n) {
            edges.Add(n);
            freeEdges.Add(true);
        }

        public Vector3[] GetAttachmentPoints(int n, out int[] points, List<Vector3> verts) {

            int s = Random.Range(1, freeSides.Sum(e => e ? 1 : 0) + 1);

            List<int> sums = new List<int>(coreEdges);
            freeSides.Select(e => e ? 1 : 0).Aggregate(0, (sum, elem) => { sum += elem; sums.Add(sum); return sum; });
            int attachmentSide = sums.IndexOf(s);

            freeSides[attachmentSide] = false;

            bool attachToCorner = Random.value < 0.7f;
            Vector3 a = verts[edges[attachmentSide]];
            int nextSide = attachmentSide < freeSides.Count - 1 ? attachmentSide + 1 : 0;

            Vector3 b = verts[edges[nextSide]];

            if (attachToCorner) {

                bool first = Random.value < 0.5f;

                points = new int[2] {
                    first ? edges[nextSide] : n,
                    first ? n : edges [attachmentSide]
                };

                edges.Add(n);
                freeEdges.Add(true);

                return new Vector3[2] {
                    first ? b  : Vector3.Lerp(b, a, Random.Range(0.1f, 0.5f)),
                    first ? Vector3.Lerp(b, a, Random.Range(0.5f, 0.9f)) : a
                };

            } else {

                points = new int[2] {
                    n,
                    n + 1
                };

                edges.Add(n);
                freeEdges.Add(true);
                edges.Add(n + 1);
                freeEdges.Add(true);

                Vector3 a1 = Vector3.Lerp(a, b, Random.Range(0.1f, 0.4f));
                return new Vector3[2] {
                    Vector3.Lerp(a1, b, Random.Range(0.5f, 0.9f)),
                    a1,
                };
            }

        }

        public IEnumerable<Vector2> GetUVs(List<Vector2> UVs)
        {
            int l = UVs.Count;
            Vector2 uv = new Vector2(1f, 0f);
            for (int i = 0, n = edges.Count; i < n; i++)
            {
                if (edges[i] < l)
                {
                    uv = UVs[edges[i]];
                } else if (uv.x == 1 && uv.y == 0)
                {
                    uv = new Vector2(0, 0);
                    yield return uv;
                } else if (uv.x == 0 && uv.y == 0)
                {
                    uv = new Vector2(0, 1);
                    yield return uv;
                }
                else if (uv.x == 0 && uv.y == 1)
                {
                    uv = new Vector2(1, 1);
                    yield return uv;
                }
                else if (uv.x == 1 && uv.y == 1)
                {
                    uv = new Vector2(1, 0);
                    yield return uv;
                }
            }

        }

        public IEnumerable<Vector3> GetVerts(List<Vector3> verts)
        {
            //Expand to support non rect forms      
            if (free)
            {
                //Core rect
                yield return new Vector3(-roomScale.x, 0, -roomScale.z);
                yield return new Vector3(-roomScale.x, 0, roomScale.z);
                yield return new Vector3(roomScale.x, 0, roomScale.z);
                yield return new Vector3(roomScale.x, 0, -roomScale.z);
            } else
            {
                //Attached rects

                //Vector for first side;
                Vector3 v = verts[edges[1]] - verts[edges[0]];

                //CCW rotate pi/2
                float a = roomScale.x / roomScale.z;                
                Vector3 o = new Vector3(v.z / a, 0, -v.x * a);
                //Debug.Log(string.Format("a {0}, ({1}, {2}", a, o.x, o.z));
                yield return verts[edges[1]] + o;
                yield return verts[edges[0]] + o;
            }
        }

        public IEnumerable<KeyValuePair<int, Vector3>> GetFreeVerts(List<Vector3> verts)
        {
            Vector3 v = verts[edges[1]] - verts[edges[0]];
            //CCW rotate pi/2
            float a = roomScale.x / roomScale.z;
            Vector3 o = new Vector3(v.z / a, 0, -v.x * a);

            for (int i = 0; i < 4; i++)
            {
                if (free)
                {
                    yield return new KeyValuePair<int, Vector3>(edges[i], verts[edges[i]]);
                }
                else
                {
                    if (freeEdges[i])
                    {
                        yield return new KeyValuePair<int, Vector3>(edges[i], verts[edges[(i + 1) % 2]] + o);
                    }
                }
            }
        }

		public IEnumerable<int> GetTris() {
            if (true)
            {
                yield return edges[0];
                yield return edges[1];
                yield return edges[3];
                yield return edges[1];
                yield return edges[2];
                yield return edges[3];
            } /*else
            {
                yield return edges[0];
                yield return edges[3];
                yield return edges[1];
                yield return edges[1];
                yield return edges[3];
                yield return edges[2];
            }*/

		}

		public Room(int n) {

            free = true;
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

			roomScale = new Vector3(1f, 0f, Random.Range(0.97f, 1.03f));
			roomScale /= Mathf.Min(1f, roomScale.z);

			area = 0;
		}

		public Room(int n, int[] oldCorners) {

            free = false;
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
					edges.Add(n);
					freeEdges.Add(true);
					freeSides.Add(true);
                    n++;
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

    List<Room> shapes = new List<Room>();

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

        shapes.Clear();		

        int nShapes = Random.Range (2, 5);

		Room baseRoom = new Room();

		while (area < stopAtArea) {

			int count = shapes.Count;

			if (count == 0 || count < nShapes && Random.value < area * count / stopAtArea * nShapes	) {

				int n = verts.Count;

				if (count == 0) {
					shapes.Add (new Room (n));
					baseRoom = shapes [0];
				} else {

					int[] attachIndices;
					Vector3[] newV = baseRoom.GetAttachmentPoints (n, out attachIndices, verts);
                    //int newVert = 0;
					for (int i = 0; i < attachIndices.Length; i++) {
						if (attachIndices [i] >= n) {
							verts.Add (newV [i]);
                            //newVert ++;
                            n++;
						}
					}
                    //Debug.Log(string.Format("{0} attached, {1} new", attachIndices.Length, newVert));

					shapes.Add (new Room (n, attachIndices));
				}

				Room newR = shapes [shapes.Count - 1];

				verts.AddRange(newR.GetVerts(verts));                
				UVs.AddRange(newR.GetUVs(UVs));

				tris.AddRange (newR.GetTris ());

			}

			area = 0;

            for (int s = 0; s < shapes.Count; s++) {

				Room r = shapes [s];
				if (r.active) {

                    if (r.Attatched)
                    {
                        foreach (KeyValuePair<int, Vector3> kvp in r.GetFreeVerts(verts))
                        {
                            verts[kvp.Key] = kvp.Value;
                        }
                    }
                    else {

                        Vector3 center = r.GetCenter(verts);

                        for (int i = 0, l = r.edges.Count; i < l; i++)
                        {

                            Vector3 v = verts[r.edges[i]] - center;
                            verts[r.edges[i]] = new Vector3(v.x * r.roomScale.x, 0, v.z * r.roomScale.z) * upscaleSpeed;

                        }
                    }
					r.RecalculateArea (verts);

				}

                //Debug.Log(r.Status);
				area += r.Area;

			}
            //Debug.Log(string.Format("{0} m2, {1} parts {2} verts {3} triangles", area, shapes.Count, verts.Count, tris.Count / 3));
			mesh.SetVertices (verts);
			mesh.SetTriangles (tris, 0);
			mesh.SetUVs (0, UVs); 
			mesh.RecalculateBounds ();
			mesh.RecalculateNormals ();

            //if (shapes.Count == 2)
            //    area = stopAtArea;

            yield return new WaitForSeconds (0.1f);

		}
		//mesh.UploadMeshData(false);
	}

    void OnDrawGizmosSelected()
    {
        float gScale = 0.5f;
        Color[] cornerColors = new Color[4] { Color.red, Color.green, Color.blue, Color.yellow };
        foreach (Room r in shapes)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3 pt = transform.TransformPoint(mesh.vertices[r.edges[i]]);
                if (r.freeEdges[i])
                {
                    Gizmos.color = cornerColors[i];
                    Gizmos.DrawWireCube(pt, Vector3.one * gScale);                    
                }
                else
                {
                    Gizmos.color = cornerColors[i];
                    Gizmos.DrawWireSphere(pt, gScale);
                }

            }
        }
    }

}
