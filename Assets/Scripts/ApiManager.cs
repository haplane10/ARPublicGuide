using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class User
{
    public int userId;
    public string username;
    public string password;
    public string email;
    public string signupDate;
}


[System.Serializable]
public class UserCreateDto
{
    public string username;
    public string password;
    public string email;
}

[System.Serializable]
public class UserList
{
    public List<User> users;
}

public class ApiManager : MonoBehaviour
{
    [SerializeField] string BaseUrl = "https://localhost:7248/api";

    // 전체 유저 조회
    public IEnumerator GetAllUsers()
    {
        using var request = UnityWebRequest.Get($"{BaseUrl}/user");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log("유저 목록: " + json);

            // JSON 파싱
            var users = JsonUtility.FromJson<User[]>(json);
        }
        else
        {
            Debug.LogError("오류: " + request.error);
        }
    }

    // ID로 유저 조회
    public IEnumerator GetUserById(int id)
    {
        using var request = UnityWebRequest.Get($"{BaseUrl}/user/{id}");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            var user = JsonUtility.FromJson<User>(json);
            Debug.Log($"유저 이름: {user.username}");
        }
        else
        {
            Debug.LogError("오류: " + request.error);
        }
    }

    // 유저 생성
    public IEnumerator CreateUser(string username, string password, string email)
    {
        var dto = new UserCreateDto
        {
            username = username,
            password = password,
            email = email
        };

        string json = JsonUtility.ToJson(dto);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using var request = new UnityWebRequest($"{BaseUrl}/user", "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("생성 완료: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("오류: " + request.error);
        }
    }
}