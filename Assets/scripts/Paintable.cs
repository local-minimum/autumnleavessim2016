using UnityEngine;
using System.Collections;

public class Paintable : MonoBehaviour {

	[SerializeField]
	bool hasPaint = false;

	[SerializeField]
	CamHelper cHelper;

	[SerializeField]
	ColorAutomata cAutomata;

	MeshRenderer mRend;

	[SerializeField]
	Material pMaterial;

	void Start () {
		
		cAutomata = GetComponent<ColorAutomata> ();
		cHelper = GetComponent<CamHelper> ();
		cHelper.Reshape (600, 400);
		cAutomata.SetupImage (cHelper);

		mRend = GetComponent<MeshRenderer> ();
		if (pMaterial == null) {
			if (mRend.materials.Length > 0) {
				pMaterial = mRend.materials [mRend.materials.Length - 1];
			}
		}

		if (pMaterial != null) {
			pMaterial.SetTexture ("_MainTex", cAutomata.Tex);
		}
	}

	void OnInteraction(InteractionMode mode) {
		if (!hasPaint && (mode == InteractionMode.Any	|| mode == InteractionMode.Any)) {
			cHelper.Snap (cAutomata);		
			hasPaint = true;
		}
	}
}
