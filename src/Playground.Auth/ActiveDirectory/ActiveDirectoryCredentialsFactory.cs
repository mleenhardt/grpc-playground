using System;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace Playground.Auth.ActiveDirectory
{
    public static class ActiveDirectoryCredentialsFactory
    {
        public const string AuthorizationHeader = "authorization";

        private static AsyncAuthInterceptor CreateAsyncAuthInterceptor(string userName, string password, string domain)
        {
            string credentials = String.Concat(userName, ":", password, ":", domain);
            string headerValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
            return (context, metadata) =>
            {
                metadata.Add(new Metadata.Entry(AuthorizationHeader, headerValue));
                return Task.FromResult(0);
            };
        }

        public static CallCredentials CreatetCallCredentials(string userName, string password, string domain)
        {
            AsyncAuthInterceptor asyncAuthInterceptor = CreateAsyncAuthInterceptor(userName, password, domain);
            return CallCredentials.FromInterceptor(asyncAuthInterceptor);
        }

        public static ChannelCredentials CreateChannelCredentials(string userName, string password, string domain, SslCredentials sslCredentials)
        {
            AsyncAuthInterceptor asyncAuthInterceptor = CreateAsyncAuthInterceptor(userName, password, domain);
            return ChannelCredentials.Create(sslCredentials, CallCredentials.FromInterceptor(asyncAuthInterceptor));
        }
    }
}