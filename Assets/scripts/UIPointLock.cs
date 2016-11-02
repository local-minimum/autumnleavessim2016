using UnityEngine;
using UnityEngine.UI;

public enum InteractionMode {Any, Build, Color, StateChange};

public class UIPointLock : MonoBehaviour {

	[SerializeField]
	Image centerCursor;

	[SerializeField]
	KeyCode deactivateKey;

	[SerializeField]
	float maxDist = 10;

	[SerializeField]
	LayerMask layers;

	[SerializeField]
	InteractionMode iMode = InteractionMode.Any;

	CursorLockMode releaseMode;
	bool releaseVisibility;

	void OnEnable() {
		releaseMode = Cursor.lockState;
		Cursor.lockState = CursorLockMode.Locked;
		releaseVisibility = Cursor.visible;
		Cursor.visible = false;
	}

	void OnDisable() {		
		Cursor.lockState = releaseMode;
		Cursor.visible = releaseVisibility;
	}

	void Update() {
		if (Input.GetKeyDown(deactivateKey)) { 	
			this.enabled = false;
		}
		if (Input.GetMouseButtonUp (0)) {
			MessageInteractionTarget ();
		}
	}

	void MessageInteractionTarget() {
		RaycastHit hit;
		Ray r = Camera.main.ScreenPointToRay (new Vector3 (Screen.width / 2, Screen.height / 2));

		if (Physics.Raycast (r, out hit, maxDist, layers)) {
			hit.transform.gameObject.SendMessage ("OnInteraction", iMode, SendMessageOptions.DontRequireReceiver);
		}
	}
}