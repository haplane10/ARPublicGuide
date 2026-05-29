using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI 이벤트 처리 및 ApiManager 연동
/// DatabaseManager 대체 - ApiManager와 같은 GameObject에 attach하거나
/// ApiManager 참조를 Inspector에서 연결
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("API")]
    [SerializeField] private ApiManager _api;

    [Header("전체 조회 영역")]
    [SerializeField] private Button _getAllButton;        // "전체 조회" 버튼
    [SerializeField] private Transform _scrollContent;   // ScrollView > Viewport > Content
    [SerializeField] private GameObject _userItemPrefab; // 유저 1행 프리팹 (Text 포함)

    [Header("유저 추가 영역")]
    [SerializeField] private InputField _nameInput;      // 이름 InputField
    [SerializeField] private InputField _passwordInput;  // 암호 InputField
    [SerializeField] private InputField _emailInput;     // EMail InputField
    [SerializeField] private Button _applyButton;        // "적용" 버튼

    void Start()
    {
        // ApiManager가 Inspector에서 연결 안 됐으면 같은 오브젝트에서 찾기
        if (_api == null)
            _api = GetComponent<ApiManager>();

        _getAllButton.onClick.AddListener(OnClickGetAll);
        _applyButton.onClick.AddListener(OnClickApply);
    }

    // ── 전체 조회 버튼 ──────────────────────────────────────
    void OnClickGetAll()
    {
        StartCoroutine(_api.GetAllUsers(
            onSuccess: users => PopulateList(users),
            onError: err => Debug.LogError("전체 조회 실패: " + err)
        ));
    }

    void PopulateList(List<User> users)
    {
        // 기존 항목 초기화
        foreach (Transform child in _scrollContent)
            Destroy(child.gameObject);

        foreach (var user in users)
        {
            var item = Instantiate(_userItemPrefab, _scrollContent);

            item.transform.Find("ID").GetComponent<Text>().text = user.userId.ToString();
            item.transform.Find("Name").GetComponent<Text>().text = user.username;
            item.transform.Find("Password").GetComponent<Text>().text = user.password;
            item.transform.Find("Email").GetComponent<Text>().text = user.email;
        }
    }

    // ── 적용(추가) 버튼 ─────────────────────────────────────
    void OnClickApply()
    {
        string name = _nameInput.text.Trim();
        string pass = _passwordInput.text.Trim();
        string email = _emailInput.text.Trim();

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(email))
        {
            Debug.LogWarning("이름, 암호, 이메일을 모두 입력하세요.");
            return;
        }

        StartCoroutine(_api.CreateUser(
            username: name,
            password: pass,
            email: email,
            onSuccess: () =>
            {
                Debug.Log("유저 생성 완료!");
                ClearInputFields();
                // 생성 후 목록 자동 갱신 (원하면 주석 해제)
                // OnClickGetAll();
            },
            onError: err => Debug.LogError("유저 생성 실패: " + err)
        ));
    }

    void ClearInputFields()
    {
        _nameInput.text = string.Empty;
        _passwordInput.text = string.Empty;
        _emailInput.text = string.Empty;
    }
}