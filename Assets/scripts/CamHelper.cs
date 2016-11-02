using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public delegate void NewImageRecorded(CamHelper camHelper);

public class CamHelper : MonoBehaviour {

	public static NewImageRecorded OnNewImageRecorded;

    Sprite _sprite;
	Texture2D _tex;

	[SerializeField, Range(8, 1024)]
	int height = 400;

	[SerializeField, Range(8, 1024)]
	int width = 600;

	[SerializeField]
	KeyCode debugKey = KeyCode.None;

	public int Height {
		get {
			return height;
		}

		set {
			height = value;
			ResizeImage ();
		}
	}

	public int Width {
		get {
			return width;
		}

		set {
			width = value;
			ResizeImage ();
		}
	}


	public void Reshape(int width, int height) {
		this.width = width;
		this.height = height;
		ResizeImage ();
	}

	void Awake() {
		ResizeImage ();
	}

    void ResizeImage() {
		//TODO: Limit size if needed!
		_tex = new Texture2D(width, height);
		_sprite = Sprite.Create(_tex, new Rect(Vector2.zero, new Vector2(width, height)), Vector2.one * 0.5f);
		_sprite.name = "WebCamImage";
	}

	public Sprite sprite {
		get {
			return _sprite;
		}
	}

	public Texture2D tex {
		get {
			return _tex;
		}
	}

	public void Snap(MonoBehaviour target) {
		StartCoroutine (SnapImage (target));
	}

	IEnumerator<WaitForEndOfFrame> SnapImage(MonoBehaviour target) {
		WebCamTexture cTex = new WebCamTexture ();
		cTex.Play ();
		bool captured = false;
		yield return new WaitForEndOfFrame ();

		while (!captured) {
			yield return new WaitForEndOfFrame ();
			if (cTex.didUpdateThisFrame) {

				Copy (cTex);
				cTex.Stop ();
				captured = true;
				if (target != null) {
					target.SendMessage ("OnSnap", this, SendMessageOptions.DontRequireReceiver);
				} else if (OnNewImageRecorded != null) {
					OnNewImageRecorded (this);
				}
				
			}
		}
	}

	void Copy(WebCamTexture source) {
		int width = Mathf.Min (source.width, this.width);
		int height = Mathf.Min (source.height, this.height);
		tex.SetPixels(0, 0, width, height, source.GetPixels (0, 0, width, height));
		tex.Apply ();
	}

	void Update() {

		if (debugKey != KeyCode.None && Input.GetKeyUp (debugKey)) {
			Snap (null);
		}
	}
}
