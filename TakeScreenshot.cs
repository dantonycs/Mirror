using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text.RegularExpressions;

public class ScreenshotUploader : MonoBehaviour
{
    private string screenshotFileName;

    public void CaptureScreenshotAndUpload()
    {
        StartCoroutine(CaptureScreenshotAndUploadCoroutine());
    }

    private IEnumerator CaptureScreenshotAndUploadCoroutine()
    {
        screenshotFileName = $"screenshot_{System.DateTime.Now:yyyyMMddHHmmss}.png";

        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        byte[] screenshotBytes = screenshot.EncodeToPNG();
        Destroy(screenshot);

        string apiUrl = "https://api.github.com/repos/dantonycs/MirrorQR/contents/last_screenshot/last.png";
        string apiUrl_all = "https://api.github.com/repos/dantonycs/MirrorQR/contents/all_screenshots/" + screenshotFileName;

        //Obtaining sha code first to keep updating the last screenshot
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to fetch file details. Error: {request.error}");
            yield break;
        }

        string jsonResponse = request.downloadHandler.text;
        string sha = ExtractSHAFromResponse(jsonResponse);
        
        //Proceed to replace the last screenshot
        string jsonPayload = $@"
        {{
            ""message"": ""Uploaded screenshot"",
            ""content"": ""{System.Convert.ToBase64String(screenshotBytes)}"",
            ""sha"": ""{sha}""
        }}";

        UnityWebRequest upload_request = new UnityWebRequest(apiUrl, "PUT");
        upload_request.SetRequestHeader("Authorization", "Bearer <token>");
        upload_request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPayload));
        upload_request.downloadHandler = new DownloadHandlerBuffer();
        upload_request.SetRequestHeader("Content-Type", "application/json");

        yield return upload_request.SendWebRequest();

        if (upload_request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Screenshot upload failed. Error: {upload_request.error}");
        }
        else
        {
            Debug.Log("Screenshot uploaded successfully.");
        }


        //Storing in all_screenshots folder
        UnityWebRequest upload_request_all = new UnityWebRequest(apiUrl_all, "PUT");
        upload_request_all.SetRequestHeader("Authorization", "Bearer <token>");
        upload_request_all.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPayload));
        //upload_request_all.downloadHandler = new DownloadHandlerBuffer();
        upload_request_all.SetRequestHeader("Content-Type", "application/json");

        yield return upload_request_all.SendWebRequest();

        if (upload_request_all.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Screenshot upload failed. Error: {upload_request_all.error}");
        }
        else
        {
            Debug.Log("Screenshot uploaded to all_screenshots folder successfully.");
        }

        //string filePath = $"Assets/{screenshotFileName}";
        //File.WriteAllBytes(filePath, screenshotBytes);

    }

    private string ExtractSHAFromResponse(string jsonResponse)
    {
        Match match = Regex.Match(jsonResponse, @"""sha"":\s*""([^""]+)""");
        if (match.Success && match.Groups.Count >= 2)
        {
            return match.Groups[1].Value;
        }
        return null;
    }

}
