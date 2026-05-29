using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class LocationManager : MonoBehaviour
{
    public TextMeshProUGUI GPS;
    public TextMeshProUGUI north;
    public TextMeshProUGUI one;
    public TextMeshProUGUI two;
    public Transform northVector;
    public TextMeshProUGUI distance;
    public Image arrowImage;
    bool isReady = false;
    int index = 0;

    [Space] // 예시: 이수 메가박스
    public float targetLatitude = 37.484684f; 
    public float targetLongitude = 126.981636f;

    private IEnumerator Start()
    {
        // 1. 권한 요청
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);

            // 사용자가 권한 부여할 때까지 대기
            float timeout = 10f;
            while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation) && timeout > 0)
            {
                yield return new WaitForSeconds(0.5f);
                timeout -= 0.5f;
            }
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Debug.LogError("위치 권한 거부됨");
            yield break;
        }

        // 2. 위치 서비스 켜져있는지 확인
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("기기 위치 서비스가 꺼져있음");
            one.text = "기기 GPS 설정을 켜주세요";
            yield break;
        }

        // 3. Location service 시작 (한 번만!)
        Input.location.Start(1f, 0.1f);

        // 4. 초기화 완료까지 대기
        int wait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && wait > 0)
        {
            yield return new WaitForSeconds(1);
            wait--;
        }

        if (Input.location.status != LocationServiceStatus.Running)
        {
            Debug.LogError($"Location 초기화 실패: {Input.location.status}");
            yield break;
        }

        // 5. Location 완전히 Running 된 후에 compass 켜기
        Input.compass.enabled = true;

        // 센서 안정화 대기
        yield return new WaitForSeconds(1f);

        isReady = true;

        // 6. 두 코루틴 실행
        StartCoroutine(GetLocation());
        StartCoroutine(GetCompass());
    }

    private IEnumerator GetCompass()
    {
        while (Input.location.status == LocationServiceStatus.Running)
        {
            one.text = $"Permission Granted: {Permission.HasUserAuthorizedPermission(Permission.FineLocation)}";
            two.text = $"Compass Enabled: {Input.compass.enabled}\n" +
                       $"True Heading: {Input.compass.trueHeading}\n" +
                       $"Raw Vector: {Input.compass.rawVector}\n" +
                       $"Timestamp: {Input.compass.timestamp}\n" +
                       $"Gyro Supported: {SystemInfo.supportsGyroscope}\n" +
                       $"Accelerometer: {SystemInfo.supportsAccelerometer}\n" +
                       $"Device: {SystemInfo.deviceModel}";

            if (isReady && Input.compass.enabled)
            {
                var heading = Input.compass.trueHeading;
                north.text = $"Heading: {heading:F2}°";

                Quaternion deviceRotation = Quaternion.Euler(0, -heading, 0);
                northVector.rotation = deviceRotation;
            }

            yield return new WaitForSeconds(0.1f); // compass는 짧게 갱신
        }
    }

    private IEnumerator GetLocation()
    {
        // Input.location.Start() 여기서 다시 호출하지 않음!
        while (Input.location.status == LocationServiceStatus.Running)
        {
            index++;
            LocationInfo location = Input.location.lastData;
            float distance = GetDistanceInMeters(location, targetLatitude, targetLongitude);
            float bearing = GetBearingInDegrees(location, targetLatitude, targetLongitude);
            float deviceHeading = Input.compass.trueHeading;

            // bearing - trueHeading = 화면 기준 목적지 방향
            float relativeAngle = (bearing - deviceHeading + 360f) % 360f;

            // UI 화살표 회전 (UI는 위가 0°, 시계방향 양수)
            arrowImage.rectTransform.rotation = Quaternion.Euler(0, 0, -relativeAngle);

            GPS.text = $"current Latitude: {location.latitude} Longitude: {location.longitude}";
            this.distance.text = $"target {distance:F2}m";

            yield return new WaitForSeconds(2);
        }
    }

    public static float GetDistanceInMeters(LocationInfo from, float toLat, float toLng)
    {
        const float R = 6371000f; // 지구 반지름 (m)

        float lat1 = from.latitude * Mathf.Deg2Rad;
        float lat2 = toLat * Mathf.Deg2Rad;
        float dLat = (toLat - from.latitude) * Mathf.Deg2Rad;
        float dLng = (toLng - from.longitude) * Mathf.Deg2Rad;

        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2)
                + Mathf.Cos(lat1) * Mathf.Cos(lat2)
                * Mathf.Sin(dLng / 2) * Mathf.Sin(dLng / 2);

        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));

        return R * c;
    }

    public static float GetBearingInDegrees(LocationInfo from, float toLat, float toLng)
    {
        float lat1 = from.latitude * Mathf.Deg2Rad;
        float lat2 = toLat * Mathf.Deg2Rad;
        float dLng = (toLng - from.longitude) * Mathf.Deg2Rad;

        float y = Mathf.Sin(dLng) * Mathf.Cos(lat2);
        float x = Mathf.Cos(lat1) * Mathf.Sin(lat2)
                 - Mathf.Sin(lat1) * Mathf.Cos(lat2) * Mathf.Cos(dLng);

        float bearing = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        return (bearing + 360f) % 360f; // 0~360 정규화
    }

    void OnDisable()
    {
        Input.location.Stop();
        Input.compass.enabled = false;
    }
}