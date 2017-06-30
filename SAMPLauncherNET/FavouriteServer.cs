﻿using System.Threading.Tasks;

namespace SAMPLauncherNET
{
    public class FavouriteServer : Server
    {

        private string serverPassword;

        private string rconPassword;

        /*public override string Hostname
        {
            get
            {
                if (requestsRequired[ERequestType.Information])
                {
                    Task<string> hn = HostnameAsync;
                    if (hn.Wait(100))
                        cachedHostName = hn.Result;
                }
                return cachedHostName;
            }
        }*/

        public string ServerPassword
        {
            get
            {
                return serverPassword;
            }
        }

        public string RCONPassword
        {
            get
            {
                return rconPassword;
            }
        }

        public FavouriteServer(string ipAddressAndPortString, string cachedHostname = "", string serverPassword = "", string rconPassword = "") : base(ipAddressAndPortString, false)
        {
            requestsRequired[ERequestType.Information] = false;
            hostname = cachedHostname;
            this.serverPassword = serverPassword;
            this.rconPassword = rconPassword;
        }
    }
}
