using UnityEngine;

public class CardArrangeTest : MonoBehaviour
{
    public GameObject cardPrefab;
    
    private RectTransform _rectTransform;

    private float _curveAngle = 5f;

    private float _downAmount = 80f;

    private bool _updated = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_updated) return;
        
        var cards = _rectTransform.GetComponentsInChildren<Card>();
        int length = cards.Length;
        float centerIndex = (length - 1) / 2f;
        for (int i = 0; i < length; i++)
        {
            RectTransform card = cards[i].GetComponent<RectTransform>();
    
            float y = (i - centerIndex) * 50f;
            float x = Mathf.Pow(i - centerIndex, 2) * 1.2f - _downAmount;
            card.anchoredPosition = new Vector2(x, y);
    
            float angle = (i - centerIndex) * -_curveAngle + 90f;
            card.transform.localRotation = Quaternion.Euler(0, 0, angle);
        }
        _updated = true;
    }

    public void OnClickButton()
    {
        _updated = false;
        Quaternion rotation = Quaternion.Euler(0, 0, 90f);
        
        GameObject obj = Instantiate(cardPrefab, _rectTransform);
        obj.transform.localRotation = rotation;
    }
}
