﻿namespace CredentialHelper.Interface
{
    public interface ICredentialStorage
    {
        bool TryLoad(out UserInfo userInfo);
        void Save(UserInfo userInfo);
    }

    public readonly struct UserInfo
    {
        public readonly string PrivateLogin;
        public readonly string Password;

        public UserInfo(string privateLogin, string secureString)
        {
            PrivateLogin = privateLogin;
            Password = secureString;
        }
    }
}
