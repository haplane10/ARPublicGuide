    using System.Collections;
using TMPro;
using UnityEngine;

public class LocationManager : MonoBehaviour
{
    public TextMeshProUGUI GPS;
    private void Start()
    {
        StartCoroutine(GetLocation());
    }

    private IEnumerator GetLocation()
    {
        // 위치 서비스 활성화 여부 확인
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("Location service is disabled.");
            yield break;
        }

        // 위치 서비스 시작 (정확도, 거리)
        Input.location.Start(5f, 10f);

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // 초기화 실패
        if (maxWait <= 0)
        {
            Debug.Log("Timed out");
            yield break;
        }

        // 실패 처리
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            yield break;
        }

        // 성공
        LocationInfo location = Input.location.lastData;

        Debug.Log($"Latitude: {location.latitude}");
        Debug.Log($"Longitude: {location.longitude}");
        Debug.Log($"Altitude: {location.altitude}");
        Debug.Log($"Accuracy: {location.horizontalAccuracy}");
        Debug.Log($"Timestamp: {location.timestamp}");
        GPS.text = $"Latitude: {location.latitude}\nLongitude: {location.longitude}";

        // 필요 없으면 종료
        Input.location.Stop();
    }
}
