using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class ColorAutomata : MonoBehaviour {

	class CrawlerState {

		public int x;
		public int y;
		int width;
		int height;
		Color color;
		float colorT;
		float inertia;
		int stopVotes = 0;
		int startPaintAt = 10;	
		public Vector2 velocity;

		public bool Painting {

			get {
				return startPaintAt < 1;
			}
		}

		public bool Alive {
			get {
				return x > 0 && y > 0 && x < width && y < height && (stopVotes < 10 || !traversed [x, y]);
			}
		}

		bool[,] traversed;

		public CrawlerState(Texture2D tex) {

			width = tex.width;
			height = tex.height;
			colorT = Random.Range(0, 1);
			x = Random.Range(0, tex.width);
			y = Random.Range(0, tex.height);
			inertia = Random.Range(0.25f, 1.5f); 
			color = tex.GetPixel(x, y);


			velocity = new Vector2(
				Random.Range(-1f, 1f),
				Random.Range(-1f, 1f));
			if (velocity.sqrMagnitude > 0) {
				velocity /= velocity.magnitude;
			}

			traversed = new bool[width, height];
		}

		public List<Vector2> CalculateForces(Texture2D tex) {
			
			List<Vector2> forces = new List<Vector2> ();

			for (int offX = -1; offX < 2; offX++) {
				for (int offY = -1; offY < 2; offY++) {
					if (offY == 0 && offX == 0) {
						continue;
					}

					int x = this.x + offX;
					int y = this.y + offY;

					Color? other = GetTexColor (tex, x, y);
					forces.Add(CalculateForce (other, x, y));
				}
			}

			return forces;
		}

		Vector2 CalculateForce(Color? other, int x, int y) {
			return new Vector2 (x - this.x, y - this.y) *
			1f / (
			    (other == null ? 1f : 1f + ColorDistance ((Color)other)) * 
					(1 + Mathf.Pow(Mathf.Pow (x - this.x, 2) + Mathf.Pow (y - this.y, 2), 0.25f)));
		}

		public void ApplyForces(List<Vector2> forces) {
			for (int i = 0, l = forces.Count; i < l; i++) {
				velocity += forces [i] / inertia;
			}
		}

		Color? GetTexColor(Texture2D tex, int x, int y) {
			if (x < 0 || y < 0 || x >= width || y >= height) {
				return null;
			} else {
				return tex.GetPixel(x, y);
			}

		}

		float ColorDistance(Color other) {
			float dist = Mathf.Pow((Mathf.Abs (other.r - color.r) + Mathf.Abs (other.g - color.g) + Mathf.Abs (other.b - color.b)) / 3f, 0.25f);
			return dist;
		}

		public void UpdateColor(Texture2D tex) {
			color = Color.Lerp (color, tex.GetPixel (x, y), colorT);
		}

		public void Move() {
			if (traversed [x, y]) {
				stopVotes++;
			} else {
				startPaintAt--;
			}

			traversed [x, y] = true;
			x += velocity.x > 0.5 ? 1 : (velocity.x < -.5 ? -1 : 0);
			y += velocity.y > 0.5 ? 1 : (velocity.y < -.5 ? -1 : 0);
		}

	}

	Image img;

	Texture2D myTex;

	public Texture2D Tex {
		get {
			return myTex;
		}
	}

	Sprite mySprite;

	[SerializeField, Range(1, 700)]
	int nCrawlers = 10;

	[SerializeField]
	float crawlSpeed = 0.01f; 

	[SerializeField]
	Color backgroundColor;

	[SerializeField]
	Color noVelociyColor;

	[SerializeField]
	Color fullVelocityColor;

	[SerializeField]
	float zeroVelocityMagnitude = 0.5f;

	[SerializeField]
	float fullVelocityMagnitude = 5f;

	[SerializeField]
	AnimationCurve velocityToColor;

	[SerializeField, Range(0, 700)]
	int fills = 42;

	[SerializeField, Range(0, 10)]
	float traceStartAhead = 5f;

	[SerializeField, Range(10, 240)]
	int fillDepth = 200;

	[SerializeField]
	Color fillColor;

	[SerializeField]
	bool autoPaint = true;

	void Start () {
		img = GetComponent<Image> ();
	}

	public void SetupImage(CamHelper camHelper) {
		myTex = new Texture2D (camHelper.Width, camHelper.Height);
		mySprite = Sprite.Create(
			myTex, 
			new Rect(Vector2.zero, new Vector2(camHelper.Width, camHelper.Height)), 
			Vector2.one * 0.5f);
		mySprite.name = "Color Dream";

		if (img) {
			img.sprite = mySprite;
		}
	}		

	void OnEnable() {
		if (autoPaint) {
			CamHelper.OnNewImageRecorded += HandleCamImageRecorded;
		}
	}

	void OnDisable() {
		CamHelper.OnNewImageRecorded -= HandleCamImageRecorded;
	}

	void OnDestroy() {
		CamHelper.OnNewImageRecorded -= HandleCamImageRecorded;
	}


	void OnSnap(CamHelper camHelper) {
		HandleCamImageRecorded (camHelper);
	}

	void HandleCamImageRecorded(CamHelper camHelper) {

		if (myTex == null) {
			SetupImage (camHelper);
		}

		Fill (backgroundColor);
		Texture2D tex = camHelper.tex;

		//TODO: Need to know when all crawlers are done
		for (int i = 0; i < nCrawlers; i++) {
			StartCoroutine (Crawl (tex));
		}
		StartCoroutine (TexApplier (tex));
	}

	void Fill(Color col) {
		for (int x = 0, X = myTex.width; x < X; x++) {
			for (int y = 0, Y = myTex.height; y < Y; y++) {
				myTex.SetPixel (x, y, col);
			}
		}
	}

	bool[,] visited;

	IEnumerator<WaitForSeconds> TexApplier(Texture2D tex) {
		float startTime = Time.timeSinceLevelLoad;
		while (Time.timeSinceLevelLoad - startTime < traceStartAhead) {
			myTex.Apply ();
			yield return new WaitForSeconds (0.01f);
		}

		int width = myTex.width;
		int height = myTex.height;
		visited = new bool[width, height];
		int free = 0;
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				visited [x, y] = myTex.GetPixel (x, y).a != 0;
				if (!visited [x, y]) {
					free++;
				}
			}
			myTex.Apply ();
		}

		Debug.Log ("Fooding " + free);

		for (int seed = 0; seed < fills; seed++) {
			int x = 0;
			int y = 0;
			for (int idx=0; idx<100; idx++) {
				x = Random.Range (0, width);
				y = Random.Range (0, height);
				if (!visited [x, y]) {
					break;
				}
			}
			Color fill = tex.GetPixel (x, y);
			fill.a = 1;
			int i = 0;
			//Debug.Log (string.Format ("start at {0} {1} => {2}", x, y, !visited[x, y]));
			IEnumerator<int> iter = FloodFill (x, y, width, height, fill, 0).GetEnumerator();
			while(iter.MoveNext()) {
				Debug.Log (iter.Current);
				yield return new WaitForSeconds (0.01f);
				if (i > 10) {
					i = 0;
					myTex.Apply ();
				} else {
					i++;
				}
			}
			//Debug.Log (".............. " + seed);
			myTex.Apply ();
			yield return new WaitForSeconds (0.01f);
		}

		myTex.Apply ();

		Debug.Log ("Blacking");
		for (int x = 0, X = myTex.width; x < X; x++) {
			for (int y = 0, Y = myTex.height; y < Y; y++) {
				if (myTex.GetPixel (x, y).a == 0) {
					myTex.SetPixel (x, y, fillColor);
				}
			}
			if (x % 5 == 0) {
				myTex.Apply ();
				yield return new WaitForSeconds (0.01f);
			}
		}

		Debug.Log ("Done");
		myTex.Apply ();
	}

	IEnumerator<WaitForSeconds> Crawl(Texture2D tex) {

		CrawlerState state = new CrawlerState (tex);
		int i = 0;
		int n = Mathf.RoundToInt (0.01f / crawlSpeed);

		while (true) {
			List<Vector2> f = state.CalculateForces (tex);
			state.ApplyForces (f);
			state.Move ();

			if (state.Alive) {
				state.UpdateColor (tex);
				if (state.Painting) {
					myTex.SetPixel (state.x, state.y, GetColor (state.velocity.magnitude));
				}

				i++;

				if (i > n) {
					i = 0;
					yield return new WaitForSeconds (crawlSpeed);
				}
			} else {
				break;
			}
		}
	}

	Color GetColor(float velocityMagnitude) {
		float magnitude = (Mathf.Clamp (velocityMagnitude, zeroVelocityMagnitude, fullVelocityMagnitude) - zeroVelocityMagnitude) / fullVelocityMagnitude;
		return Color.Lerp (noVelociyColor, fullVelocityColor, velocityToColor.Evaluate (magnitude));
	}

	IEnumerable<int> FloodFill(int x, int y, int width, int height, Color fill, int depth) {
		if (x < 0 || y < 0 || x >= width || y >= height || depth > fillDepth) {
			yield return depth;
			//yield break;
		} else if (!visited[x, y]) {
			visited [x, y] = true;
			myTex.SetPixel (x, y, fill);
			depth++;
			List<IEnumerator<int>> iters = new List<IEnumerator<int>> ();
			for (int xOff = -1; xOff < 2; xOff++) {
				for (int yOff = -1; yOff < 2; yOff++) {

					if (xOff == yOff && xOff == 0) {
						continue;
					} 

					iters.Add (FloodFill (x + xOff, y + yOff, width, height, fill, depth).GetEnumerator ());
				}
			}

			int i = 0;
			int n = iters.Count;
			while (n > 0) {
				IEnumerator<int> iter = iters [i];
				if (iter.MoveNext ()) {
					i++;
					i %= n;
				} else {
					iters.RemoveAt (i);
					n--;
					if (n > 0) {
						i %= n;
					}
				}
			}
		} else {
			yield return depth;
			//yield break;
		}

	}
}
