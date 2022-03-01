using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class NonRectangularButton : MonoBehaviour {

	public bool interactable = true;
	public Image targetImage;
	public float fadeSpeed = 10f;
	public Color normalColor = new Color(1f, 1f, 1f, 0.7f);
	public Color highlightedColor = new Color(1f, 1f, 1f, 1f);
	public Color selectedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
	public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
	public UnityEvent onClick;
	//public UnityEvent onMouseEnter;
	//public UnityEvent onMouseExit;
	//public UnityEvent whileMouseOver;
	//public UnityEvent whileMouseAway;

	private PolygonCollider2D polygonCollider;
	private ColliderDetectMouseover filter;
	private Color targetColor = Color.white;
	private bool mouseoverDone = false;

	// Use this for initialization
	void Start () {
		polygonCollider = GetComponent<PolygonCollider2D>();
		filter = GetComponent<ColliderDetectMouseover>();
		targetColor = normalColor;

		if (targetImage == null || polygonCollider == null || filter == null) {
			Debug.LogWarning ("A radial button must have an Image, PolygonCollider2D, and a Collider2DRaycastFilter to function properly.");
			this.enabled = false;
		}
	}
	
	// Update is called once per frame
	/*void Update () {
		// TODO: Controller support
		// Detect mouse over and mouse click, and invoke events based on this, along with color changes as necessary.
		if (interactable) {
			if (filter.isMouseOver ()) {
				whileMouseOver.Invoke ();
				if (Input.GetMouseButtonUp(0)) {
					onClick.Invoke ();
				}

				/*if (Input.GetMouseButton (0)) {
					targetColor = selectedColor;
				} else {
					targetColor = highlightedColor;
				}

				if (!mouseoverDone) {
					mouseoverDone = true;
					onMouseEnter.Invoke ();
				}

			} else {

				if (mouseoverDone) {
					mouseoverDone = false;
					onMouseExit.Invoke ();
				}

				whileMouseAway.Invoke ();
				targetColor = normalColor;
			}
		} else {
			targetColor = disabledColor;
		}

		//targetImage.color = Utils.ColorLerp (targetImage.color, targetColor, Time.deltaTime*fadeSpeed);
	}

	public void testPrint() {
		print("test");
	}*/
}