using System;
using ExtendedXmlSerializer.ContentModel.Content;

namespace GitBack.Credential.Manager
{
    public interface ICredentialRecordCommon<in T>
    {
        string Protocol { get; set; }
        string Host { get; set; }
        string Path { get; set; }
        string Username { get; set; }

        [Verbatim]
        string Password { get; set; }

        bool IsEmpty();

        bool IsMatch(T record);

        bool Equals(T other);
    }
}