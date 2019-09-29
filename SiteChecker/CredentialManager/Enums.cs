namespace CredentialManager
{
	/// <summary>
	/// Credential type class
	/// </summary>
	public enum CredentialType : uint
	{
		/// <summary>
		/// No credentials.
		/// </summary>
		None = 0,

		/// <summary>
		/// Generic credential.
		/// The credential is a generic credential. The credential will not be used by any particular authentication package. The credential will be stored securely but has no other significant characteristics. 
		/// </summary>
		Generic,

		/// <summary>
		/// Domain password.
		/// The credential is a password credential and is specific to Microsoft's authentication packages. The NTLM, Kerberos, and Negotiate authentication packages will automatically use this credential when connecting to the named target.
		/// </summary>
		DomainPassword,

		/// <summary>
		/// Domain certificate.
		/// The credential is a certificate credential and is specific to Microsoft's authentication packages. The Kerberos, Negotiate, and Schannel authentication packages automatically use this credential when connecting to the named target.
		/// </summary>
		DomainCertificate,

		/// <summary>
		/// Domain visibility.
		/// This value is no longer supported.
		/// </summary>
		//DomainVisiblePassword
	}

	/// <summary>
	/// PersistanceType enum
	/// </summary>
	public enum PersistanceType : uint
	{
		/// <summary>
		/// Session
		/// </summary>
		Session = 1,

		/// <summary>
		/// Local
		/// </summary>
		LocalComputer = 2,

		/// <summary>
		/// Enterprise
		/// </summary>
		Enterprise = 3
	}
}
