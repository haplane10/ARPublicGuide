using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Android;

public class LocationManager : MonoBehaviour
{
    public TextMeshProUGUI GPS;
    public TextMeshProUGUI north;
    public TextMeshProUGUI one;
    public TextMeshProUGUI two;
    public Transform northVector;
    public Transform pinPoint;
    bool isReady = false;
    int index = 0;

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
                pinPoint.position = northVector.forward * 2f; 
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
            GPS.text = $"{index}. Latitude: {location.latitude} Longitude: {location.longitude}";
            yield return new WaitForSeconds(3);
        }
    }

    void OnDisable()
    {
        Input.location.Stop();
        Input.compass.enabled = false;
    }
}