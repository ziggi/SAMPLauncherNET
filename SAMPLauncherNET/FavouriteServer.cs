﻿/// <summary>
/// SA:MP launcher .NET namespace
/// </summary>
namespace SAMPLauncherNET
{
    /// <summary>
    /// Favourite server
    /// </summary>
    public class FavouriteServer : Server
    {
        /// <summary>
        /// Server password
        /// </summary>
        private readonly string serverPassword;

        /// <summary>
        /// RCON password
        /// </summary>
        private readonly string rconPassword;

        /// <summary>
        /// Server password
        /// </summary>
        public string ServerPassword
        {
            get
            {
                return serverPassword;
            }
        }

        /// <summary>
        /// RCON password
        /// </summary>
        public string RCONPassword
        {
            get
            {
                return rconPassword;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ipAddressAndPortString">IP address and port string</param>
        /// <param name="cachedHostname">Cached hostname</param>
        /// <param name="cachedGamemode">Cached gamemode</param>
        /// <param name="serverPassword">Server password</param>
        /// <param name="rconPassword">RCON password</param>
        public FavouriteServer(string ipAddressAndPortString, string cachedHostname, string cachedGamemode, string serverPassword, string rconPassword) : base(ipAddressAndPortString, false)
        {
            requestsRequired[ERequestType.Information] = false;
            hostname = cachedHostname;
            gamemode = cachedGamemode;
            this.serverPassword = serverPassword;
            this.rconPassword = rconPassword;
            FetchDataAsync(ERequestType.Ping);
            FetchDataAsync(ERequestType.Information);
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fdc">Favorite data contract</param>
        public FavouriteServer(FavouriteDataContract fdc) : base(fdc.Host, false)
        {
            requestsRequired[ERequestType.Information] = false;
            hostname = fdc.Hostname;
            serverPassword = "";
            rconPassword = "";
            FetchDataAsync(ERequestType.Ping);
            FetchDataAsync(ERequestType.Information);
        }
    }
}
