using System.Xml.Serialization;

namespace CredentialHelper
{
    public struct Credentials
    {
        [XmlAttribute]
        public string Login;
        [XmlAttribute]
        public string Password;
        [XmlAttribute]
        public string Sessid;
        [XmlAttribute]
        public string Uidh;

        public Credentials(string login, string password, string sessid, string uidh)
        {
            Login = login;
            Password = password;
            Sessid = sessid;
            Uidh = uidh;
        }
    }

    public interface ICredentialStorage
    {
        bool TryLoad(out Credentials userInfo);
        void Save(Credentials userInfo);
    }
}
