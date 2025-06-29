using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform _rt;
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;

    private RectTransform _centerRect; // 화면 중앙 카드 놓는 곳
    
    [SerializeField] private TextMeshProUGUI _cardTMP;

    private int _number;

    private float _positionUpdateInterval = 0.01f;
    private float _elapsedTime = 0f;
    
    private bool IsDraggable { get; set; }

    [HideInInspector] public Vector2 movingStartPosition;
    
    private Vector2 _dragStartPosition;
    
    public void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetCard(int number, RectTransform centerRect)
    {
        _number = number;
        _cardTMP.text = number.ToString();
        _centerRect = centerRect;
    }

    public void SetNumber(int number)
    {
        _number = number;
        _cardTMP.text = number.ToString();
    }

    public int GetCardNumber()
    {
        return _number;
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
        
        _elapsedTime += Time.deltaTime;
        
        _rt.anchoredPosition += eventData.delta / _canvas.scaleFactor;

        // 일정 주기마다 드래그하고있는 카드 position 정보 서버에 전송
        if (_elapsedTime >= _positionUpdateInterval)
        {
            _elapsedTime = 0f;
            SendCardMovement();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!IsDraggable) return;
        
        _elapsedTime = 0f;
        _canvasGroup.blocksRaycasts = true;
        
        Rect rect = ToRect(_rt);
        Rect rectCenter = ToRect(_centerRect);
        
        // 카드를 중앙에 놓았을 때
        if (rect.Overlaps(rectCenter))
        {
            GameManager.Instance.multiplayController.SendPlayCard(_number);
            
            transform.SetParent(_centerRect);
            transform.rotation = Quaternion.identity;
            _rt.anchorMin = new Vector2(0.5f, 0.5f);
            _rt.anchorMax = new Vector2(0.5f, 0.5f);
            _rt.anchoredPosition = Vector2.zero;
            
            DeactivateDrag();
            return;
        }
        
        // 카드를 중앙에 놓지 않으면 원래자리로 복귀
        _rt.anchoredPosition = _dragStartPosition;
        GameManager.Instance.multiplayController.SendRollbackCardMovement();
    }

    private Rect ToRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        Vector3 bottomLeft = corners[0];
        Vector3 topRight = corners[2];
        return new Rect(bottomLeft, topRight - bottomLeft);
    }

    public void DeactivateDrag()
    {
        IsDraggable = false;
    }

    public void ActivateDrag()
    {
        IsDraggable = true;
    }

    private void SendCardMovement()
    {
        Vector2 cardContainerPosition = transform.parent.position;
        Vector2 cardPosition = _rt.position;
            
        float ratioToCenter = (cardPosition.y - cardContainerPosition.y) / (Screen.height);
        float rationToCenterVertical = (cardPosition.x - cardContainerPosition.x) / (Screen.width);

        GameManager.Instance.multiplayController.SendCardMove(ratioToCenter, rationToCenterVertical);
    }
}
