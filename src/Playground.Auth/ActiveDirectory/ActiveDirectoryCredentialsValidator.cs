using System;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Text;
using Grpc.Core;

namespace Playground.Auth.ActiveDirectory
{
    // Support for server credentials will come later
    // https://groups.google.com/forum/#!searchin/grpc-io/authentication/grpc-io/iLHgWC8o8UM/Va7k0tPndFQJ
    // This has to be used explicitely in server service implementation for now

    public static class ActiveDirectoryCredentialsValidator
    {
        private const int ErrorLoginFailure = 0x31;

        public static bool Validate(ServerCallContext context)
        {
            Metadata.Entry metadataEntry = context.RequestHeaders.FirstOrDefault(m =>
                String.Equals(m.Key, ActiveDirectoryCredentialsFactory.AuthorizationHeader, StringComparison.Ordinal));
            
            if (metadataEntry.Equals(default(Metadata.Entry)) || metadataEntry.Value == null)
            {
                return false;
            }

            string authorizationHeaderValue = Encoding.UTF8.GetString(Convert.FromBase64String(metadataEntry.Value));
            if (String.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                return false;
            }

            var splitedString = authorizationHeaderValue.Split(':');
            if (splitedString.Length < 3)
            {
                return false;
            }

            string userName = splitedString[0];
            string password = splitedString[1];
            string domain = splitedString[2];

            return ValidateLdapCredentials(userName, password, domain);
        }

        private static bool ValidateLdapCredentials(string userName, string password, string domain)
        {
            LdapDirectoryIdentifier directoryIdentifier = new LdapDirectoryIdentifier(domain);
            var credentials = new NetworkCredential(userName, password, domain);
            using (var connection = new LdapConnection(directoryIdentifier, credentials, AuthType.Kerberos))
            {
                connection.SessionOptions.Sealing = true;
                connection.SessionOptions.Signing = true;

                try
                {
                    connection.Bind();
                }
                catch (LdapException ex)
                {
                    if (ex.ErrorCode == ErrorLoginFailure)
                    {
                        return false;
                    }
                    throw;
                }
            }
            return true;
        }
    }
}