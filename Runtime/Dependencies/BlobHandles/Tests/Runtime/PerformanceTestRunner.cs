using System.IO;
using BlobHandles.Tests;
using UnityEngine;

namespace BlobHandles
{
    public class PerformanceTestRunner : MonoBehaviour
    {
        const string logFile = "PerformanceTestLog.txt";

        PerformanceTests m_Tests;
        int m_StartFrame;

        public void Start()
        {
            m_Tests = new PerformanceTests { RuntimeLog = Path.Combine(Application.dataPath, logFile) };
            m_Tests.BeforeAll();
            m_StartFrame = Time.frameCount;
        }
        
        void Update()
        {
            var frame = Time.frameCount - m_StartFrame;
            switch (frame)
            {
                case 20:
                    m_Tests.BlobString_Equals();
                    break;
                case 40:
                    m_Tests.ManagedBlobString_GetHashCode();
                    break;
                case 60:
                    m_Tests.DictionaryTryGetValue_BlobString();
                    break;
                case 70:
                    m_Tests.DictionaryTryGetValue_BlobHandles();
                    break;
                case 80:
                    m_Tests.DictionaryExtension_TryGetValueFromBytes();
                    break;
                case 100:
                    m_Tests.BlobStringLookup_TryGetValueFromBytes();
                    break;
                case 120:
                    m_Tests.GetAsciiStringFromBytes();
                    break;
                case 125:
                    m_Tests.AfterAll();
                    enabled = false;
                    break;
            }
        }
    }
}
