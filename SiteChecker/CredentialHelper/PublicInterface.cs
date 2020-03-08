using System.Xml.Serialization;

namespace CredentialHelper
{
    public struct Credentials
    {
        [XmlAttribute]
        public string Login;
        [XmlAttribute]
        public string Password;

        public Credentials(string login, string password)
        {
            Login = login;
            Password = password;
        }
    }

    public interface ICredentialStorage
    {
        bool TryLoad(out Credentials userInfo);
        void Save(Credentials userInfo);
    }
}
