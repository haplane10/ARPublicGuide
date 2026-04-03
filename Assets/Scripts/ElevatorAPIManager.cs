using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ElevatorAPIManager : MonoBehaviour
{
    [Header("API 설정")]
    [SerializeField] private string serviceKey = "24b4cee4f5b1088676633c168cf07beb1e5799b644cfc7c47f3d6124387d37f1";
    private const string BASE_URL = "https://apis.data.go.kr/B553766/facility/getFcElvtr";

    [Header("검색 파라미터")]
    [SerializeField] private string dataType = "JSON";
    [SerializeField] private int pageNo = 1;
    [SerializeField] private int numOfRows = 10;
    [SerializeField] private string lineNm = "";   // 예: 7호선
    [SerializeField] private string stnCd = "";   // 예: 2738
    [SerializeField] private string stnNm = "";   // 예: 이수
    [SerializeField] private string vcntEntrcNo = "";   // 근접 출입구 번호
    [SerializeField] private string oprtngSitu = "";   // 가동현황

    private void Start()
    {
        FetchElevatorData();
    }

    public void FetchElevatorData()
    {
        StartCoroutine(GetElevatorData());
    }

    private IEnumerator GetElevatorData()
    {
        // URL 조립
        string url = $"{BASE_URL}" +
                     $"?serviceKey={serviceKey}" +
                     $"&dataType={dataType}" +
                     $"&pageNo={pageNo}" +
                     $"&numOfRows={numOfRows}";

        // 값이 있을 때만 파라미터 추가
        if (!string.IsNullOrEmpty(lineNm)) url += $"&lineNm={UnityWebRequest.EscapeURL(lineNm)}";
        if (!string.IsNullOrEmpty(stnCd)) url += $"&stnCd={stnCd}";
        if (!string.IsNullOrEmpty(stnNm)) url += $"&stnNm={UnityWebRequest.EscapeURL(stnNm)}";
        if (!string.IsNullOrEmpty(vcntEntrcNo)) url += $"&vcntEntrcNo={vcntEntrcNo}";
        if (!string.IsNullOrEmpty(oprtngSitu)) url += $"&oprtngSitu={oprtngSitu}";

        Debug.Log($"요청 URL: {url}");

        using var request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Accept", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"응답 수신:\n{request.downloadHandler.text}");
            ParseResponse(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError($"API 오류: {request.error}");
        }
    }

    private void ParseResponse(string json)
    {
        try
        {
            var response = JsonUtility.FromJson<ElevatorApiResponse>(json);

            if (response?.response?.body?.items?.item == null)
            {
                Debug.LogWarning("데이터가 없습니다.");
                return;
            }

            var items = response.response.body.items.item;
            Debug.Log($"총 {items.Length}개 엘리베이터 조회됨");

            foreach (var item in items)
            {
                Debug.Log($"[{item.stnNm}역 / {item.lineNm}] " +
                          $"출입구: {item.vcntEntrcNo}번 | " +
                          $"위치: {item.elvtrLocDesc} | " +
                          $"상태: {item.oprtngSitu}");
            }

            // 여기서 UI 업데이트 or Gemini/TTS 연동 가능
            // 예: ttsManager.Speak($"{items[0].stnNm}역 엘리베이터 상태: {items[0].oprtngSitu}");
        }
        catch (Exception e)
        {
            Debug.LogError($"JSON 파싱 오류: {e.Message}\n원문: {json}");
        }
    }
}

// ──────────────────────────────────────
//  JSON 역직렬화 데이터 클래스
// ──────────────────────────────────────
[Serializable]
public class ElevatorApiResponse
{
    public ElevatorResponseBody response;
}

[Serializable]
public class ElevatorResponseBody
{
    public ElevatorHeader header;
    public ElevatorBody body;
}

[Serializable]
public class ElevatorHeader
{
    public string resultCode; // 00 = 정상
    public string resultMsg;
}

[Serializable]
public class ElevatorBody
{
    public ElevatorItems items;
    public int numOfRows;
    public int pageNo;
    public int totalCount;
}

[Serializable]
public class ElevatorItems
{
    public ElevatorItem[] item;
}

[Serializable]
public class ElevatorItem
{
    public string lineNm;        // 호선명
    public string stnCd;         // 역코드
    public string stnNm;         // 역명
    public string vcntEntrcNo;   // 근접 출입구 번호
    public string elvtrLocDesc;  // 엘리베이터 위치 설명
    public string oprtngSitu;    // 가동현황
    public string bgngFlr;       // 시작 층
    public string endFlr;        // 종료 층
    public string endFlrGrndUdgdSe; // 지상/지하 구분
    public string pspcpNope;     // 장애인 이용 여부
}