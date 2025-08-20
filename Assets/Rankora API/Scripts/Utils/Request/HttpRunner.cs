using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Rankora_API.Scripts.Utils.Request
{
    public class HttpRunner : MonoBehaviour
    {
        public static HttpRunner Instance { get; private set; }
        void Awake()
        {
            Instance = this;
        }

        public static Coroutine Run(IEnumerator request)
        {
            if (Instance == null)
            {
                var gameObject = new GameObject("HttpRequestRunner");
                Instance = gameObject.AddComponent<HttpRunner>();
                DontDestroyOnLoad(gameObject);
            }
            return Instance.StartCoroutine(request);
        }
    }
}
