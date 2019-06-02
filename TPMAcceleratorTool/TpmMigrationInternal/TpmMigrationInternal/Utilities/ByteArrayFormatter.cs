//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System.IO;
    using System.Runtime.Serialization;
    using System.Xml;

    public static class ByteArrayFormatter<T>
    {
        private static DataContractSerializer formatter;

        private static DataContractSerializer Formatter
        {
            get
            {
                if (ByteArrayFormatter<T>.formatter == null)
                {
                    ByteArrayFormatter<T>.formatter = new DataContractSerializer(typeof(T));
                }

                return ByteArrayFormatter<T>.formatter;
            }
        }

        public static byte[] Serialize(T objectGraph)
        {
            MemoryStream stream = new MemoryStream();
            try
            {
                XmlDictionaryWriter writer = XmlDictionaryWriter.CreateBinaryWriter(stream);
                Formatter.WriteObject(writer, objectGraph);
                writer.Flush();
                return stream.ToArray();
            }
            finally
            {
                stream.Dispose();
            }
        }

        public static T Deserialize(byte[] bytes)
        {
            return (T)ByteArrayFormatter<T>.Formatter.ReadObject(XmlDictionaryReader.CreateBinaryReader(bytes, XmlDictionaryReaderQuotas.Max));
        }
    }
}