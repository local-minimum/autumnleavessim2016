using UnityEngine;
using UnityEngine.UI;

public class UIPointLock : MonoBehaviour {

	[SerializeField]
	Image centerCursor;

	[SerializeField]
	KeyCode deactivateKey;

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
	}
}