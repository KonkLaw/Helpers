using System.IO;
using System.Xml.Serialization;

namespace CredentialHelper
{
    class SerializationHelper<T>
    {
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(T));
        private static readonly XmlSerializerNamespaces Namespaces = new XmlSerializerNamespaces();

        static SerializationHelper()
        {
            Namespaces.Add(string.Empty, string.Empty);
        }

        public static string Serialize(T data)
        {
            
            using (var stream = new StringWriter())
            {
                Serializer.Serialize(stream, data, Namespaces);
                return stream.ToString();
            }
        }

        public static T Deserialize(string value)
        {
            using (var stream = new StringReader(value))
            {
                return (T)Serializer.Deserialize(stream);
            }
        }
    }
}