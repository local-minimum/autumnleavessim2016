using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIPhotoViewer : MonoBehaviour {

	Image img;
	Texture2D myTex;
	Sprite mySprite;

	void Start () {
		img = GetComponent<Image> ();
	}

	void OnEnable() {
		CamHelper.OnNewImageRecorded += HandleNewImage;
	}

	void OnDisable() {
		CamHelper.OnNewImageRecorded -= HandleNewImage;
	}

	void OnDestroy() {
		CamHelper.OnNewImageRecorded -= HandleNewImage;
	}

	void HandleNewImage(CamHelper camHelper) {
		if (myTex == null) {
			SetupImage (camHelper);
		}
		myTex.SetPixels (camHelper.tex.GetPixels ());
		myTex.Apply ();
	}

	void SetupImage(CamHelper camHelper) {
		myTex = new Texture2D (camHelper.Width, camHelper.Height);
		mySprite = Sprite.Create(
			myTex, 
			new Rect(Vector2.zero, new Vector2(camHelper.Width, camHelper.Height)), 
			Vector2.one * 0.5f);
		mySprite.name = "Photo";

		img.sprite = mySprite;
	}		

}
