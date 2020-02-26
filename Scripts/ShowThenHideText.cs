using UnityEngine;

public class ShowThenHideText : MonoBehaviour
{
    public void ShowThenHide()
    {
        gameObject.SetActive(true);
        Invoke("Hide", 3.0f);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
