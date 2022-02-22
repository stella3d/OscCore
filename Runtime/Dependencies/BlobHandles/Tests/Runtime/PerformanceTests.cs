using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using BlobHandles.Tests;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BlobHandles.Tests
{
    public class PerformanceTests
    {
        static readonly Stopwatch k_Stopwatch = new Stopwatch();
        
        public int StringCount = 1000;
        
        public int MinLength = 20;
        public int MaxLength = 100;
        
        string[] m_Strings;

        const string newlineChar = "\n";
        readonly byte[] m_NewLineBytes = Encoding.UTF8.GetBytes(newlineChar);

        public string RuntimeLog { get; set; }

        FileStream m_File;

        public void BeforeAll()
        {
            // init state to make sure that we always get the same test results
            Random.InitState(303);
            m_Strings = TestData.RandomStringsWithPrefix("/composition", StringCount, MinLength, MaxLength);

            var mode = File.Exists(RuntimeLog) ? FileMode.Append : FileMode.Create;
            m_File = new FileStream(RuntimeLog, mode); 
            WriteEnvironmentInfo(mode);

            var countBytes = Encoding.ASCII.GetBytes($"repeat count: {StringCount.ToString()}, times in ticks");
            m_File.Write(countBytes, 0, countBytes.Length);
            m_File.Write(m_NewLineBytes, 0, m_NewLineBytes.Length);
        }

        // write out automatic compiler & environment info
        void WriteEnvironmentInfo(FileMode mode)
        {
            if(mode == FileMode.Append)
                m_File.Write(m_NewLineBytes, 0, m_NewLineBytes.Length);
            
            var versionBytes = Encoding.ASCII.GetBytes(Application.unityVersion);
            m_File.Write(versionBytes, 0, versionBytes.Length);
#if UNITY_EDITOR
            var editorBytes = Encoding.ASCII.GetBytes(" - Editor, ");
            m_File.Write(editorBytes, 0, editorBytes.Length);
#else
            var playerBytes = Encoding.ASCII.GetBytes(" - Player, ");
            m_File.Write(playerBytes, 0, playerBytes.Length);
#endif
#if ENABLE_IL2CPP
            var runtimeBytes = Encoding.ASCII.GetBytes("IL2CPP");
#else
            var runtimeBytes = Encoding.ASCII.GetBytes("Mono");
#endif
            m_File.Write(runtimeBytes, 0, runtimeBytes.Length);
            m_File.Write(m_NewLineBytes, 0, m_NewLineBytes.Length);
        }

        public void AfterAll()
        {
            m_File.Close();
        }
        
        void WriteLog(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            m_File.Write(bytes, 0, bytes.Length);
            m_File.Write(m_NewLineBytes, 0, m_NewLineBytes.Length);
        }

        public void BlobString_Equals()
        {
            var searchForIndex = StringCount / 4;
            var searchString = m_Strings[searchForIndex];
            var searchBlobString = new BlobString(searchString);
            
            var blobStrings = new BlobString[m_Strings.Length];
            for (int i = 0; i < m_Strings.Length; i++)
                blobStrings[i] = new BlobString(m_Strings[i]);
            
            bool eql;
            // force the jit to compile the equals methods
            foreach (var str in m_Strings)
                eql = searchString.Equals(str);
            foreach (var blobString in blobStrings)
                eql = searchBlobString.Equals(blobString);
            
            k_Stopwatch.Restart();
            foreach (var str in m_Strings)
            {
                eql = searchString.Equals(str);
            }
            k_Stopwatch.Stop();
            var strTicks = k_Stopwatch.ElapsedTicks;
            
            k_Stopwatch.Restart();
            foreach (var blobString in blobStrings)
            {
                eql = searchBlobString.Equals(blobString);
            }
            k_Stopwatch.Stop();
            var intStrTicks = k_Stopwatch.ElapsedTicks;

            WriteLog($"Equals(), str {strTicks}, blobString  {intStrTicks}");
            foreach (var t in blobStrings)
                t.Dispose();
        }
        
        public void ManagedBlobString_GetHashCode()
        {
            var blobStrings = new BlobString[m_Strings.Length];
            for (int i = 0; i < m_Strings.Length; i++)
                blobStrings[i] = new BlobString(m_Strings[i]);

            // force jit to compile hashcode method            
            foreach (var blobString in blobStrings)
            {
                var hc = blobString.GetHashCode();
            }

            
            int hashCode;
            k_Stopwatch.Restart();
            foreach (var str in m_Strings)
            {
                hashCode = str.GetHashCode();
            }
            k_Stopwatch.Stop();
            var strTicks = k_Stopwatch.ElapsedTicks;
            
            k_Stopwatch.Restart();
            foreach (var blobString in blobStrings)
            {
                hashCode = blobString.GetHashCode();
            }
            k_Stopwatch.Stop();
            var intStrTicks = k_Stopwatch.ElapsedTicks;

            WriteLog($"GetHashCode(), str {strTicks}, blobString {intStrTicks}");
            
            foreach (var t in blobStrings)
                t.Dispose();
        }
        
        public void DictionaryTryGetValue_BlobString()
        {
            var blobStrings = new BlobString[m_Strings.Length];
            for (int i = 0; i < m_Strings.Length; i++)
                blobStrings[i] = new BlobString(m_Strings[i]);

            var strDict = new Dictionary<string, int>(m_Strings.Length);
            for (var i = 0; i < m_Strings.Length; i++)
                strDict.Add(m_Strings[i], i);

            var intStrDict = new Dictionary<BlobString, int>(blobStrings.Length);
            for (var i = 0; i < blobStrings.Length; i++)
                intStrDict.Add(blobStrings[i], i);
            
            foreach (var blobString in blobStrings)
                intStrDict.TryGetValue(blobString, out var index);
            
            k_Stopwatch.Restart();
            foreach (var str in m_Strings)
            {
                strDict.TryGetValue(str, out var index);
            }
            k_Stopwatch.Stop();
            var strTicks = k_Stopwatch.ElapsedTicks;

            k_Stopwatch.Restart();
            foreach (var blobString in blobStrings)
            {
                intStrDict.TryGetValue(blobString, out var index);
            }
            k_Stopwatch.Stop();
            var intStrTicks = k_Stopwatch.ElapsedTicks;

            WriteLog($"Dictionary.TryGetValue, str {strTicks}, blobString {intStrTicks}");
            
            foreach (var t in blobStrings)
                t.Dispose();
        }
        
        public unsafe void DictionaryTryGetValue_BlobHandles()
        {
            var bHandles = new BlobHandle[m_Strings.Length];
            var bytes = new byte[m_Strings.Length][];
            for (int i = 0; i < m_Strings.Length; i++)
            {
                var b = Encoding.ASCII.GetBytes(m_Strings[i]);
                bytes[i] = b;

                fixed (byte* bPtr = b)
                {
                    bHandles[i] = new BlobHandle(bPtr, b.Length);
                }
            }

            var bDict = new Dictionary<BlobHandle, int>(m_Strings.Length);
            for (var i = 0; i < m_Strings.Length; i++)
                bDict.Add(bHandles[i], i);

            // force jit compilation
            bDict.TryGetValue(bHandles[0], out var bJitValue);
            
            k_Stopwatch.Restart();
            foreach (var bh in bHandles)
            {
                k_Stopwatch.Start();
                bDict.TryGetValue(bh, out var value);
                k_Stopwatch.Stop();
            }
            var bTicks = k_Stopwatch.ElapsedTicks;

            WriteLog($"Dictionary.TryGetValue() w/ BlobHandle key {bTicks}");
        }

        public unsafe void DictionaryExtension_TryGetValueFromBytes()
        {
            var bHandles = new BlobHandle[m_Strings.Length];
            var bytes = new byte[m_Strings.Length][];
            for (int i = 0; i < m_Strings.Length; i++)
            {
                var b = Encoding.ASCII.GetBytes(m_Strings[i]);
                bytes[i] = b;

                fixed (byte* bPtr = b)
                {
                    bHandles[i] = new BlobHandle(bPtr, b.Length);
                }
            }

            var bDict = new Dictionary<BlobHandle, int>(m_Strings.Length);
            for (var i = 0; i < m_Strings.Length; i++)
                bDict.Add(bHandles[i], i);

            // force jit compilation
            fixed (byte* bPtr = bytes[0])
            {
                bDict.TryGetValueFromBytes(bPtr, bytes[0].Length, out var bJitValue);
            }
            
            // copy bytes so we can make sure we're not comparing identical pointers
            var copiedBytes = new byte[m_Strings.Length][];
            for (var i = 0; i < bytes.Length; i++)
            {
                var src = bytes[i];
                var copied = new byte[src.Length];
                copiedBytes[i] = copied;
                Buffer.BlockCopy(src, 0, copied, 0, src.Length);
            }

            k_Stopwatch.Restart();
            foreach (var byteArray in copiedBytes)
            {
                fixed (byte* ptr = byteArray)
                {
                    k_Stopwatch.Start();
                    bDict.TryGetValueFromBytes(ptr, byteArray.Length, out var bValue);
                    k_Stopwatch.Stop();
                }
            }
            var bTicks = k_Stopwatch.ElapsedTicks;

            WriteLog($"Dictionary.TryGetValueFromBytes() w/ BlobHandle {bTicks}");
        }
        
        public unsafe void GetAsciiStringFromBytes()
        {
            var jitAsciiStr = Encoding.ASCII.GetString(new byte[0]);
            var jitUtf8Str = Encoding.UTF8.GetString(new byte[0]);
            var bytes = new byte[m_Strings.Length][];
            for (int i = 0; i < m_Strings.Length; i++)
            {
                bytes[i] = Encoding.ASCII.GetBytes(m_Strings[i]);
            }
            
            k_Stopwatch.Restart();
            for (int i = 0; i < m_Strings.Length; i++)
            {
                var b = bytes[i];
                k_Stopwatch.Start();
                var str = Encoding.ASCII.GetString(b);
                k_Stopwatch.Stop();
            }
            
            WriteLog($"Encoding.ASCII.GetString(bytes), {k_Stopwatch.ElapsedTicks}");
            GC.Collect();
            
            k_Stopwatch.Restart();
            for (int i = 0; i < m_Strings.Length; i++)
            {
                var b = bytes[i];
                k_Stopwatch.Start();
                var str = Encoding.UTF8.GetString(b);
                k_Stopwatch.Stop();
            }
            
            WriteLog($"Encoding.UTF8.GetString(bytes), {k_Stopwatch.ElapsedTicks}");
            GC.Collect();
        }
        
        public unsafe void BlobStringLookup_TryGetValueFromBytes()
        {
            var blobStrings = new BlobString[m_Strings.Length];
            var bytes = new byte[m_Strings.Length][];
            for (int i = 0; i < m_Strings.Length; i++)
            {
                var str = m_Strings[i];
                blobStrings[i] = new BlobString(str);
                var b = Encoding.ASCII.GetBytes(str);
                bytes[i] = b;
            }
            
            var lookup = new BlobStringDictionary<int>();
            for (int i = 0; i < blobStrings.Length; i++)
                lookup.Add(blobStrings[i], i);

            // force JIT compilation of the relevant method
            fixed (byte* dummyPtr = bytes[0])
            {
                lookup.TryGetValueFromBytes(dummyPtr, bytes[0].Length, out var value);
            }

            k_Stopwatch.Reset();
            foreach (var byteStr in bytes)
            {
                fixed (byte* byteStrPtr = byteStr)
                {
                    k_Stopwatch.Start();
                    lookup.TryGetValueFromBytes(byteStrPtr, byteStr.Length, out var value);
                    k_Stopwatch.Stop();
                }
            }

            WriteLog($"TryGetValueFromBytes(byte* ) ticks: {k_Stopwatch.ElapsedTicks}");
            
            foreach (var t in blobStrings)
                t.Dispose();
        }
    }
}
