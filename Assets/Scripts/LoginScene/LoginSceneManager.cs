// Copyright 2013-2022 AFI, INC. All rights reserved.

using BackEnd;
using UnityEngine;
using UnityEngine.UI;

public class LoginSceneManager : MonoBehaviour {

    private static LoginSceneManager _instance;

    public static LoginSceneManager Instance {
        get {
            return _instance;
        }
    }
    
    [SerializeField] private Canvas _loginUICanvas;
    [SerializeField] private GameObject _loginButtonGroup;
    [SerializeField] private GameObject _touchStartButton;

    public Canvas GetLoginUICanvas() {
        return _loginUICanvas;
    }
    
    void Awake() {
        if (_instance == null) {
            _instance = this;
        }

        // StaticManager가 없을 경우 새로 생성
        if (FindObjectOfType(typeof(StaticManager)) == null) {
            var obj = Resources.Load<GameObject>("Prefabs/StaticManager");
            Instantiate(obj);
        }
    }

    void Start() {
        _loginButtonGroup.SetActive(false); // 로그인 버튼 비활성화
        SetTouchStartButton(); // 버튼 비활성화
    }
    
    // 터치하여 시작 버튼 활성화
    // 터치 시, 해당 UI는 사라지며 자동 로그인 함수를 호출한다.
    private void SetTouchStartButton() {
        _touchStartButton.GetComponent<Button>().onClick.AddListener(() => {
            Destroy(_touchStartButton);
            _touchStartButton = null;
            LoginWithBackendToken();
        });
    }

    // 로그인 버튼 씬에서 연결하기
    private void SetButton() {
        if (_loginButtonGroup.activeSelf) {
            return;
        }
        _loginButtonGroup.SetActive(true);

        Button[] buttons = _loginButtonGroup.GetComponentsInChildren<Button>();

        for (int i = 0; i < buttons.Length; i++) {
            buttons[i].onClick.RemoveAllListeners();
        }
        
        buttons[0].onClick.AddListener(FederationLogin);
        buttons[1].onClick.AddListener(CustomLogin);
        buttons[2].onClick.AddListener(GuestLogin);
        
        // 페데레이션 로그인 기능 미구현
        buttons[0].gameObject.SetActive(false);

    }

    // 자동로그인 함수 호출. 이후 처리는 AuthorizeProcess 참고
    private void LoginWithBackendToken() {
        SendQueue.Enqueue(Backend.BMember.LoginWithTheBackendToken, callback => {
            Debug.Log($"Backend.BMember.LoginWithTheBackendToken : {callback}");

            if (callback.IsSuccess()) {
                // 닉네임이 없을 경우
                if (string.IsNullOrEmpty(Backend.UserNickName)) {
                    StaticManager.UI.OpenUI<LoginUI_Nickname>("Prefabs/LoginScene/UI", GetLoginUICanvas().transform);
                }
                else {
                    GoNextScene();
                }
            }
            else {
                SetButton();
            }
        });
    }

    // 게스트로그인 함수 호출. 이후 처리는 AuthorizeProcess 참고
    private void GuestLogin() {
        SendQueue.Enqueue(Backend.BMember.GuestLogin, AuthorizeProcess);
    }

    // 로그인 UI 생성
    private void CustomLogin() {
        StaticManager.UI.OpenUI<LoginUI_CustomLogin>("Prefabs/LoginScene/UI", GetLoginUICanvas().transform);
    }
    private void FederationLogin() {
        
    }

    // 로그인 함수 후 처리 함수
    private void AuthorizeProcess(BackendReturnObject callback) {
        Debug.Log($"Backend.BMember.AuthorizeProcess : {callback}");

        // 에러가 발생할 경우 리턴
        // 로그인 버튼 활성화
        if (callback.IsSuccess() == false) {
            SetButton();
            return;
        }
        
        // 새로 가입인 경우에는 statusCode가 201, 기존 로그인일 경우에는 200이 리턴된다.
        if (callback.GetStatusCode() == "201") {
            GetPolicy();
        }
        else {
            GoNextScene();
        }
    }

    // 개인정보보호정책 UI 생성
    private void GetPolicy() {
        StaticManager.UI.OpenUI<LoginUI_Policy>("Prefabs/LoginScene/UI", GetLoginUICanvas().transform);
    }

    public void GoNextScene() {
        StaticManager.Instance.ChangeScene("LoadingScene");

    }
}