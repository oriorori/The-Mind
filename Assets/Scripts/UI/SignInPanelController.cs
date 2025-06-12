using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignInPanelController : MonoBehaviour, IGameUI
{
    [Header("StartUpMenu")] 
    [SerializeField] private GameObject _startUpObject;
    [SerializeField] private Button _signInButton;
    [SerializeField] private Button _signUpButton;
    
    [Header("SignIn")]
    [SerializeField] private GameObject _signInObject;
    [SerializeField] private TextMeshProUGUI _inPlayerIdText;
    [SerializeField] private TextMeshProUGUI _inPasswordText;
    [SerializeField] private Button _inSignInButton;
    
    [Header("SignUp")]
    [SerializeField] private GameObject _signUpObject;
    [SerializeField] private TextMeshProUGUI _upPlayerIdText;
    [SerializeField] private TextMeshProUGUI _upNicknameText;
    [SerializeField] private TextMeshProUGUI _upPasswordText;
    [SerializeField] private TextMeshProUGUI _upPasswordConfirmationText;
    [SerializeField] private Button _upSignUpButton;

    void Awake()
    {
        _signInButton.onClick.AddListener(OnClickSignInButton);
        _signUpButton.onClick.AddListener(OnClickSignUpButton);
        _inSignInButton.onClick.AddListener(OnClickInSignInButton);
        _upSignUpButton.onClick.AddListener(OnClickUpSignUpButton);
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnClickSignInButton()
    {
        _startUpObject.SetActive(false);
        _signInObject.SetActive(true);
    }

    private void OnClickSignUpButton()
    {
        _startUpObject.SetActive(false);
        _signUpObject.SetActive(true);
    }

    private void OnClickInSignInButton()
    {
        
    }

    private void OnClickUpSignUpButton()
    {
        SignupData signupData = new SignupData()
        {
            userId = _upPlayerIdText.text,
            nickname = _upNicknameText.text,
            password = _upPasswordText.text,
            passwordConfirmation = _upPasswordConfirmationText.text,
        };
        
        StartCoroutine(NetworkManager.Instance.Signup(signupData, OnSucceedSignUp));
    }

    private void OnSucceedSignIn()
    {
        UIManager.Instance.GetUI<MainMenuPanelController>(UI_TYPE.MainMenu);
        UIManager.Instance.HideUI<SignInPanelController>(UI_TYPE.SignIn);
    }

    private void OnSucceedSignUp()
    {
        PopupUIController popupUIController = UIManager.Instance.GetUI<PopupUIController>(UI_TYPE.Popup);
        popupUIController.SetText("회원가입에 성공했습니다.");
    }

    public UniTask Show()
    {
        gameObject.SetActive(true);
        return UniTask.CompletedTask;
    }

    public UniTask Hide()
    {
        gameObject.SetActive(false);
        return UniTask.CompletedTask;
    }
}
