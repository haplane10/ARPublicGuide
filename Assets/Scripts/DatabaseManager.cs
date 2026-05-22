using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    private ApiManager _api;

    void Start()
    {
        _api = GetComponent<ApiManager>();

        // 전체 유저 조회
        StartCoroutine(_api.GetAllUsers());

        // ID로 조회
        StartCoroutine(_api.GetUserById(1));

        // 유저 생성
        StartCoroutine(_api.CreateUser("홍길동", "1234", "hong@test.com"));
    }
}
