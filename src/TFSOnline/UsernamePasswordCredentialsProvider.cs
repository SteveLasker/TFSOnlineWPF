using Microsoft.TeamFoundation.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TFSOnline {
    public class UsernamePasswordCredentialsProvider : ICredentialsProvider {
        public ICredentials GetCredentials(Uri uri, ICredentials iCredentials) {
            return new NetworkCredential("UserName", "Password", "Domain");
            //return new NetworkCredential("LandeskMSRef@outlook.com", "M!crosoft1");
        }

        public void NotifyCredentialsAuthenticated(Uri uri) {
            throw new ApplicationException("Unable to authenticate");
        }
    }
}