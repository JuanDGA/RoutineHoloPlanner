using MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;

public class MessageMenu : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private PressableButton _acknowledgeButton;
    
    // Start is called before the first frame update
    void Start()
    {
        _text = gameObject.GetComponent<TextMeshProUGUI>();
        _acknowledgeButton = gameObject.GetComponent<PressableButton>();

        if (_acknowledgeButton == null) return;
        
        _acknowledgeButton.OnClicked.AddListener(() =>
        {
            gameObject.SetActive(false);
        });
    }

    public void SetMessage(string message)
    {
        _text.text = message;
    }

    private void OnDestroy()
    {
        _acknowledgeButton.OnClicked.RemoveAllListeners();
    }
}
