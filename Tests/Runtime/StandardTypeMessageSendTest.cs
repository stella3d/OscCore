using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OscCore.Tests
{
    public class StandardTypeMessageSendTest : MonoBehaviour
    {
        static readonly StringBuilder k_Builder = new StringBuilder();

        [Range(4, 32)]
        [SerializeField]
#pragma warning disable 649
        int m_RandomAddressCount = 16;
#pragma warning restore 649

        [SerializeField]
        int m_Port = 7000;
        
        [Range(4, 64)]
        [SerializeField]
        int m_RandomCharCount = 16;

        string[] m_Addresses;
        string[] m_StringElements;
        
        byte[][] m_Blobs = new byte[4][];
        
        OscClient m_Client;
        
        void Awake()
        {
            m_Client = new OscClient("127.0.0.1", m_Port);
            m_Addresses = new string[m_RandomAddressCount];
            m_StringElements = new string[16];
            MakeRandomStrings();
            MakeRandomBlobs();
        }

        void Update()
        {
            if(Random.Range(0f, 1f) > 0.9f)
                RandomFloatMessage();
            
            if(Random.Range(0f, 1f) > 0.9f)
                RandomIntMessage();
            
            if(Random.Range(0f, 1f) > 0.95f)
                RandomBlobMessage();
            
            if(Random.Range(0f, 1f) > 0.92f)
                RandomStringMessage();
        }

        string RandomAddress()
        {
            return m_Addresses[Random.Range(0, m_Addresses.Length - 1)];
        }

        void RandomStringMessage()
        {
            var str = m_StringElements[Random.Range(0, m_StringElements.Length - 1)];
            m_Client.Send(RandomAddress(), str);
        }

        void RandomFloatMessage()
        {
            m_Client.Send(RandomAddress(), Random.Range(0f, 1f));
        }
        
        void RandomIntMessage()
        {
            m_Client.Send(RandomAddress(), Random.Range(0, 50));
        }

        void RandomBlobMessage()
        {
            var blob = m_Blobs[Random.Range(0, m_Blobs.GetLength(0) - 1)];
            m_Client.Send(RandomAddress(), blob, blob.Length);
        }

        void MakeRandomBlobs()
        {
            for (int i = 0; i < m_Blobs.GetLength(0); i++)
            {
                var length = 64 + i * 64;
                var blob = new byte[length];
                for (int j = 0; j < blob.Length; j++)
                {
                    blob[j] = (byte) Random.Range(0, 255);
                }

                m_Blobs[i] = blob;
            }
        }

        void MakeRandomStrings()
        {
            for (int i = 0; i < m_Addresses.Length; i++)
            {
                k_Builder.Clear();
                var prefix = Random.Range(0f, 1f) > 0.75f ? "/layer/" : "/composition/";
                
                k_Builder.Append(prefix);
                for (int j = 0; j < m_RandomCharCount; j++)
                {
                    char randChar;
                    do
                    {
                        randChar = (char) Random.Range(32, 255);
                    } while (!OscParser.CharacterIsValidInAddress(randChar));
                    k_Builder.Append((byte) randChar);
                }

                k_Builder.Append((byte) ' ');
                m_Addresses[i] = k_Builder.ToString();
            }

            for (int i = 0; i < m_StringElements.Length; i++)
            {
                k_Builder.Clear();
                for (int j = 0; j < m_RandomCharCount; j++)
                {
                    char randChar;
                    do
                    {
                        randChar = (char) Random.Range(32, 255);
                    } while (!OscParser.CharacterIsValidInAddress(randChar));
                    k_Builder.Append((byte) randChar);
                }

                k_Builder.Append((byte) ' ');
                m_StringElements[i] = k_Builder.ToString();
            }
        }
    }
}

