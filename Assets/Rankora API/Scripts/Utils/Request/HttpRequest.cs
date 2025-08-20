using Rankora_API.Scripts.Rankora.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Rankora_API.Scripts.Utils.Request
{
    
    public class HttpRequest 
    {
        private IJsonParser jsonParser = new UnityJsonParser(); // Default to Unity JsonUtility parser

        public HttpRequest SetParser(IJsonParser jsonParser)
        {
            if(jsonParser == null)
            {
                Debug.LogError("[Rankora API] Json parser cannot be null.");
                return this;
            }
            this.jsonParser = jsonParser;
            return this;
        }
        public static HttpRequest New(RequestType type, string url)
        {
            return new HttpRequest(type, url);
        }

        private RequestType method;
        private string url;
        private Dictionary<string, string> headers = new Dictionary<string, string>();
        private Dictionary<string, string> query = new Dictionary<string, string>();
        private object body = null;

        private Action<string> onError;
        private Delegate onSuccess;
        private Type responseType;

        private HttpRequest(RequestType type, string baseUrl)
        {
            method = type;
            url = baseUrl;
        }

        public HttpRequest SetHeaders(Dictionary<string, string> customHeaders)
        {
            foreach (var pair in customHeaders)
                headers[pair.Key] = pair.Value;

            return this;
        }

        public HttpRequest SetHeader(string key, string value)
        {
            headers[key] = value;
            return this;
        }

        public HttpRequest SetQuery(Dictionary<string, string> queryParams)
        {
            foreach (var pair in queryParams)
                query[pair.Key] = pair.Value;

            return this;
        }
        public HttpRequest AddQuery(string key, string value)
        {
            query[key] = value;
            return this;
        }

        public HttpRequest SetBody(object payload)
        {
            body = payload;
            return this;
        }

        public HttpRequest SetOnSuccess<T>(Action<T> callback)
        {
            responseType = typeof(T);
            if (callback != null && typeof(BasicResponse).IsAssignableFrom(typeof(T)))
            {
                onSuccess = new Action<T>((response) =>
                {
                    if (response is BasicResponse rankoraResponse)
                    {
                        if (rankoraResponse.success)
                        {
                            // If the response is successful, invoke the callback with the parsed response
                            callback((T)response);
                        }
                        else if (!string.IsNullOrEmpty(rankoraResponse.error))
                        {
                            onError?.Invoke($"[Rankora Api Error] {rankoraResponse.error}");
                        }
                    }
                    else
                    {
                        // If the response is not a BasicResponse, just invoke the callback with the parsed response
                        Debug.LogWarning($"Response is not a BasicResponse: {response.GetType()}");
                        callback((T)response);
                    }
                });
            }
            else
            {
                onSuccess = callback;
            }
            return this;
        }

        public HttpRequest SetOnError(Action<string> callback)
        {
            onError = callback;
            return this;
        }

        public void Send()
        {
            HttpRunner.Run(SendCoroutine());
        }
        //public async Task SendAsync()
        //{
        //    // Build query string
        //    string finalUrl = url;
        //    if (query.Count > 0)
        //    {
        //        var sb = new StringBuilder(finalUrl);
        //        sb.Append('?');
        //        foreach (var kv in query)
        //            sb.Append($"{UnityWebRequest.EscapeURL(kv.Key)}={UnityWebRequest.EscapeURL(kv.Value)}&");
        //        sb.Length--; // Remove trailing '&'
        //        finalUrl = sb.ToString();
        //    }

        //    var request = new UnityWebRequest(finalUrl, method.ToString());
        //    request.downloadHandler = new DownloadHandlerBuffer();

        //    if (body != null)
        //    {
        //        string json = EnhancedJSON.Json.Serialize(body);
        //        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        //        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        //        if (!headers.ContainsKey("Content-Type"))
        //            headers["Content-Type"] = "application/json";
        //    }

        //    foreach (var kvp in headers)
        //        request.SetRequestHeader(kvp.Key, kvp.Value);

        //    var operation = request.SendWebRequest();

        //    while (!operation.isDone)
        //        await Task.Yield(); // Allow async continuation

        //    if (request.result == UnityWebRequest.Result.Success)
        //    {
        //        if (onSuccess != null && responseType != null)
        //        {
        //            try
        //            {
        //                var parsed = EnhancedJSON.Json.Deserialize(request.downloadHandler.text, responseType);
        //                onSuccess.DynamicInvoke(parsed);
        //            }
        //            catch (Exception e)
        //            {
        //                Debug.LogWarning($"Parse error: {e.Message}");
        //                onError?.Invoke("Failed to parse response.");
        //            }
        //        }
        //    }
        //    else
        //    {
        //        onError?.Invoke($"{request.responseCode} - {request.error}");
        //    }
        //}
#if UNITY_ANDROID
private class ForceAcceptAll : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData) => true;
}
#endif

        private IEnumerator SendCoroutine()
        {
            if (responseType == null)
            {
                onError?.Invoke("Missing response type.");
                yield break;
            }
            // Construct URL with query parameters if present
            string finalUrl = url;
            if (query.Count > 0)
            {
                var sb = new StringBuilder(finalUrl);
                sb.Append('?');
                foreach (var kv in query)
                {
                    sb.Append($"{UnityWebRequest.EscapeURL(kv.Key)}={UnityWebRequest.EscapeURL(kv.Value)}&");
                }
                sb.Length--; // Remove trailing '&'
                finalUrl = sb.ToString();
            }

            Debug.Log($"Sending {method} request to: {finalUrl}");

            string responseText;
            using (var request = new UnityWebRequest(finalUrl, method.ToString()))
            {
#if UNITY_ANDROID
        request.certificateHandler = new ForceAcceptAll();
#endif
                request.downloadHandler = new DownloadHandlerBuffer();

                // Handle body and content type
                if (body != null)
                {
                    string json = jsonParser.FromObjectToJson(body);
                    Debug.Log($"Sending JSON: {json}");
                    request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));

                    if (!headers.ContainsKey("Content-Type"))
                        headers["Content-Type"] = "application/json";
                }

                // Set all headers
                foreach (var kvp in headers)
                {
                    request.SetRequestHeader(kvp.Key, kvp.Value);
                }

                // Send the request and wait for response
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var errorParsed = jsonParser.FromJsonToObject(request.downloadHandler.text, typeof(BasicResponse)) as BasicResponse;
                        if (!string.IsNullOrEmpty(errorParsed?.error))
                        {
                            onError?.Invoke(errorParsed.error);
                        }
                        else
                        {
                            onError?.Invoke($"{request.responseCode} - {request.error}");
                        }
                    }
                    catch (Exception)
                    {
                        onError?.Invoke($"{request.responseCode} - {request.error} - {request.downloadHandler.text}");
                    }

                    yield break;
                }

                responseText = request.downloadHandler.text;
            }

            Debug.Log($"Received Text: {responseText}");

            object parsed = null;

            try
            {
                parsed = jsonParser.FromJsonToObject(responseText, responseType);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSON parse error: {ex.Message}");
                onError?.Invoke("Failed to parse response.");
                yield break;
            }

            if (parsed == null)
            {
                onError?.Invoke("Parsed response is null.");
                yield break;
            }

            try
            {
                onSuccess?.DynamicInvoke(parsed);
            }
            catch (Exception ex)
            {
                Debug.LogError($"onSuccess invocation error: {ex.Message}");
                onError?.Invoke("An error occurred while processing the response.");
            }

        }

    }

    public enum RequestType
    {
        GET,
        POST,
        PUT,
        DELETE
    }

}
