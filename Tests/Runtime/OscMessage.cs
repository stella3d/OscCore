using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OscCore.Tests
{
    [Serializable]
    public class OscMessage
    {
        public string Address;
        public string TypeTags;

        public byte[] Data;

        public OscMessage(string address, string tags, byte[] data)
        {
            if(!address.StartsWith("/"))
                throw new ArgumentException("address must start with '/'");
            if(!tags.StartsWith(","))
                throw new ArgumentException("type tags must start with ','");

            Address = address;
            TypeTags = tags;
            Data = data;
        }

        public override string ToString()
        {
            return $"{Address}  {TypeTags}  {Encoding.ASCII.GetString(Data)}";
        }
    }
}

