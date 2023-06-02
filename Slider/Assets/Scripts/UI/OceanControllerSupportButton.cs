using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OceanControllerSupportButton : MonoBehaviour
{
    [SerializeField] private OceanControllerSupportButtonsHolder holder;

    [SerializeField] private Image image;
    [SerializeField] private Button button;
    [SerializeField] private UIRotateParams UIRotateParams;
    private Color originalImageColor;

    [SerializeField] private bool currentlySelected = false;

    private void Start()
    {
        originalImageColor= image.color;
        ColorButtonBasedOnIfSelected();
    }

    public void OnClick()
    {
        Debug.Log("click " + gameObject.name);
        holder.lastControllerSupportButtonClicked = this;
        //StartCoroutine(DisappearThenReappearAfterTime(0.8f));
    }

    public IEnumerator DisappearThenReappearAfterTime(float time)
    {
        image.color = new Color(1,1,1,0); 
        //ColorButtonBasedOnIfSelected();
        yield return new WaitForSeconds(time);
        ColorButtonBasedOnIfSelected();
    }

    public void ColorButtonBasedOnIfSelected()
    {
        if (currentlySelected) { image.color = new Color(1, 1, 1, 1); }
        else { image.color = new Color(1, 1, 1, 0); UIRotateParams.OnHoverExit(); }
    }

    public void OnSelect() { currentlySelected = true; ColorButtonBasedOnIfSelected(); }
    public void OnDeselect() { currentlySelected = false; ColorButtonBasedOnIfSelected(); }
}
