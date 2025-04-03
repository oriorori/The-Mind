using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform _rt;
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;

    [SerializeField] private RectTransform _centerRect;
    
    public bool IsDraggable { get; set; }
    
    private Vector2 _dragStartPosition;
    
    public void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
        IsDraggable = true;
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsDraggable) return;
        
        _dragStartPosition = _rt.anchoredPosition;
        _canvasGroup.blocksRaycasts = false; // 드래그 중 다른 UI 이벤트 통과 가능하게
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsDraggable) return;
        
        _rt.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!IsDraggable) return;
        
        _canvasGroup.blocksRaycasts = true;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, _rt.position);

        Rect rect = ToRect(_rt);
        Rect rectCenter = ToRect(_centerRect);
        if (rect.Overlaps(rectCenter))
        {
            transform.SetParent(_centerRect);
            _rt.anchoredPosition = Vector2.zero;

            return;
        }
        _rt.anchoredPosition = _dragStartPosition;
    }

    private Rect ToRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        Vector3 bottomLeft = corners[0];
        Vector3 topRight = corners[2];
        return new Rect(bottomLeft, topRight - bottomLeft);
    }

    public void DisableDrag()
    {
        IsDraggable = false;
    }

    public void ActivateDrag()
    {
        IsDraggable = true;
    }
}
