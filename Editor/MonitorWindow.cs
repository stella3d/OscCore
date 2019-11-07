using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlobHandles;
using UnityEditor;
using UnityEngine;

namespace OscCore
{
    public class MonitorWindow : EditorWindow
    {
        const int showLinesCount = 32;
        
        static readonly StringBuilder k_Builder = new StringBuilder();
        
        OscServer m_Server;
        
        readonly Queue<string> m_LogMessages = new Queue<string>(showLinesCount);
        readonly List<string> m_ToQueue = new List<string>(16);
        readonly List<string> m_ToQueueAlt = new List<string>(16);
        bool m_UseAlt;
        List<string> m_ActiveQueueBuffer;
        
        bool m_NeedsRepaint;
        
        void OnEnable()
        {
            m_ActiveQueueBuffer = m_ToQueue;
            m_Server = OscServer.PortToServer.First().Value;
            m_Server.AddMonitorCallback(Monitor);
        }

        void OnDisable()
        {
            m_Server.RemoveMonitorCallback(Monitor);
        }

        void Update()
        {
            // only run every 10 frames to reduce flickering
            if (Time.frameCount % 10 != 0) return;
            
            if(m_NeedsRepaint) Repaint();
        }

        public void OnGUI()
        {
            if (m_LogMessages.Count == 0 && m_ToQueue.Count == 0) 
                return;

            var useAlt = m_UseAlt;
            m_UseAlt = !m_UseAlt;
            if (useAlt)
            {
                try
                {
                    foreach (var msg in m_ToQueueAlt)
                    {
                        m_LogMessages.Enqueue(msg);
                    }
                }
                catch (InvalidOperationException) { }

                m_ToQueueAlt.Clear();
            }
            else
            {
                try
                {
                    foreach (var msg in m_ToQueue)
                    {
                        m_LogMessages.Enqueue(msg);
                    }
                    m_ToQueue.Clear();
                }
                catch (InvalidOperationException) { }
            }

            lock (m_LogMessages)
            {
                while (m_LogMessages.Count > showLinesCount)
                {
                    m_LogMessages.Dequeue();
                }
                foreach (var line in m_LogMessages)
                {
                    EditorGUILayout.LabelField(line);
                }
            }
        }

        void Monitor(BlobString address, OscMessageValues values)
        {
            if(m_UseAlt)
                m_ToQueueAlt.Add(MessageToString(address.ToString(), values));
            else
                m_ToQueue.Add(MessageToString(address.ToString(), values));
            
            m_NeedsRepaint = true;
        }
        
        static string MessageToString(string address, OscMessageValues values)
        {
            k_Builder.Clear();
            k_Builder.Append(address);
            const string divider = " ,";
            k_Builder.Append(divider);
            values.ForEachElement((i, type) => { k_Builder.Append((char)type); });
            k_Builder.Append("   ");

            var lastIndex = values.ElementCount - 1;
            values.ForEachElement((i, type) =>
            {
                var elementText = values.ReadStringElement(i);
                k_Builder.Append(elementText);
                if(i != lastIndex) k_Builder.Append(' ');
            });

            return k_Builder.ToString();
        }

        [MenuItem("Window/OscCore/Monitor")]
        static void InitWindow()
        {
            ((MonitorWindow) GetWindow(typeof(MonitorWindow)))?.Show();
        }
    }
}


