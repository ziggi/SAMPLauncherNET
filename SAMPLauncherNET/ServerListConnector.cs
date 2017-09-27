﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using WinFormsTranslator;

/// <summary>
/// SA:MP launcher .NET namespace
/// </summary>
namespace SAMPLauncherNET
{
    /// <summary>
    /// Server list connector
    /// </summary>
    public class ServerListConnector : ITranslatable
    {
        /// <summary>
        /// Server list JSON serializer
        /// </summary>
        private static DataContractJsonSerializer serverListJSONSerializer = new DataContractJsonSerializer(typeof(ServerDataContract[]));

        /// <summary>
        /// Favourites list JSON serializer
        /// </summary>
        private static DataContractJsonSerializer favouriteListJSONSerializer = new DataContractJsonSerializer(typeof(FavouriteDataContract[]));

        /// <summary>
        /// API HTTP content type
        /// </summary>
        public const string APIHTTPContentType = "text/html";

        /// <summary>
        /// API HTTP accept
        /// </summary>
        public const string APIHTTPAccept = "text/html, */*";

        /// <summary>
        /// API HTTP user agent
        /// </summary>
        public const string APIHTTPUserAgent = "Mozilla/3.0 (compatible; SA:MP v0.3.7)";

        /// <summary>
        /// Name
        /// </summary>
        private string name = "";

        /// <summary>
        /// Server list type
        /// </summary>
        private EServerListType serverListType;

        /// <summary>
        /// Endpoint
        /// </summary>
        private string endpoint;

        /// <summary>
        /// Max servers
        /// </summary>
        private uint maxServers = 0U;

        /// <summary>
        /// Server count
        /// </summary>
        private uint serverCount = 0U;

        /// <summary>
        /// Not loaded
        /// </summary>
        private bool notLoaded = true;

        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// Server list type
        /// </summary>
        public EServerListType ServerListType
        {
            get
            {
                return serverListType;
            }
        }

        /// <summary>
        /// Endpoint
        /// </summary>
        public string Endpoint
        {
            get
            {
                return endpoint;
            }
        }

        /// <summary>
        /// Maximal servers
        /// </summary>
        public uint MaxServers
        {
            get
            {
                return maxServers;
            }
        }

        /// <summary>
        /// Server count
        /// </summary>
        public uint ServerCount
        {
            get
            {
                return serverCount;
            }
            set
            {
                serverCount = value;
            }
        }

        /// <summary>
        /// Not loaded
        /// </summary>
        public bool NotLoaded
        {
            get
            {
                return notLoaded;
            }
            set
            {
                if (notLoaded)
                    serverCount = 0;
                notLoaded = value;
            }
        }

        /// <summary>
        /// Server list I/O
        /// </summary>
        public Dictionary<string, Server> ServerListIO
        {
            get
            {
                Dictionary<string, Server> ret = new Dictionary<string, Server>();
                try
                {
                    switch (serverListType)
                    {
                        case EServerListType.Favourites:
                            using (FileStream stream = File.Open(endpoint, FileMode.Open))
                            {
                                FavouriteDataContract[] favourites = (FavouriteDataContract[])(favouriteListJSONSerializer.ReadObject(stream));
                                foreach (FavouriteDataContract fdc in favourites)
                                    ret.Add(fdc.host, new FavouriteServer(fdc));
                            }
                            break;
                        case EServerListType.BackendRESTful:
                            using (WebClientEx wc = new WebClientEx(5000))
                            {
                                wc.Headers.Set(HttpRequestHeader.ContentType, APIHTTPContentType);
                                wc.Headers.Set(HttpRequestHeader.Accept, APIHTTPAccept);
                                wc.Headers.Set(HttpRequestHeader.UserAgent, APIHTTPUserAgent);
                                using (MemoryStream stream = new MemoryStream(wc.DownloadData(endpoint)))
                                {
                                    ServerDataContract[] servers = serverListJSONSerializer.ReadObject(stream) as ServerDataContract[];
                                    if (servers != null)
                                    {
                                        foreach (ServerDataContract sdc in servers)
                                        {
                                            BackendRESTfulServer server = new BackendRESTfulServer(sdc);
                                            if (server.IsValid)
                                                ret.Add(server.IPPortString, server);
                                        }
                                    }
                                }
                            }
                            break;
                        case EServerListType.LegacyFavourites:
                            using (FileStream stream = File.Open(endpoint, FileMode.Open))
                            {
                                using (BinaryReader reader = new BinaryReader(stream))
                                {
                                    if (stream.Length >= 16)
                                    {
                                        string samp = new string(reader.ReadChars(4));
                                        if (samp == "SAMP")
                                        {
                                            if (reader.ReadUInt32() == 1U)
                                            {
                                                int sc = reader.ReadInt32();
                                                for (int i = 0; i < sc; i++)
                                                {
                                                    string ip = Encoding.Default.GetString(reader.ReadBytes(reader.ReadInt32()));
                                                    ushort port = (ushort)(reader.ReadUInt32());
                                                    string cn = Encoding.Default.GetString(reader.ReadBytes(reader.ReadInt32()));
                                                    string sp = Encoding.Default.GetString(reader.ReadBytes(reader.ReadInt32()));
                                                    string rp = Encoding.Default.GetString(reader.ReadBytes(reader.ReadInt32()));
                                                    ip = ip + ":" + port;
                                                    FavouriteServer s = new FavouriteServer(ip, cn, "", sp, rp);
                                                    if (s.IsValid)
                                                        ret.Add(s.IPPortString, s);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case EServerListType.LegacySAMP:
                            using (WebClientEx wc = new WebClientEx(5000))
                            {
                                wc.Headers.Set(HttpRequestHeader.ContentType, APIHTTPContentType);
                                wc.Headers.Set(HttpRequestHeader.Accept, APIHTTPAccept);
                                wc.Headers.Set(HttpRequestHeader.UserAgent, APIHTTPUserAgent);
                                string[] ips = wc.DownloadString(endpoint).Split(new char[] { '\n' });
                                foreach (string ip in ips)
                                {
                                    Server server = new Server(ip);
                                    if (server.IsValid)
                                        ret.Add(ip.Trim(), server);
                                }
                            }
                            break;
                    }
                }
                catch
                {
                    //
                }
                maxServers = (uint)(ret.Count);
                return ret;
            }
            set
            {
                if (value != null)
                {
                    switch (serverListType)
                    {
                        case EServerListType.Favourites:
                            try
                            {
                                using (FileStream stream = File.Open(endpoint, FileMode.Create))
                                {
                                    List<FavouriteDataContract> api = new List<FavouriteDataContract>();
                                    foreach (Server server in value.Values)
                                        api.Add(server.FavouriteDataContract);
                                    favouriteListJSONSerializer.WriteObject(stream, api.ToArray());
                                }
                            }
                            catch
                            {
                                //
                            }
                            maxServers = (uint)(value.Count);
                            break;

                        case EServerListType.LegacyFavourites:
                            try
                            {
                                using (FileStream fs = File.Open(endpoint, FileMode.Create))
                                {
                                    using (BinaryWriter writer = new BinaryWriter(fs))
                                    {
                                        writer.Write("SAMP".ToCharArray());
                                        writer.Write(1);
                                        writer.Write(value.Count);
                                        foreach (KeyValuePair<string, Server> kv in value)
                                        {
                                            byte[] data = Encoding.Default.GetBytes(kv.Value.IPv4AddressString);
                                            writer.Write(data.Length);
                                            writer.Write(data);
                                            writer.Write((uint)(kv.Value.Port));
                                            data = Encoding.Default.GetBytes(kv.Value.Hostname);
                                            writer.Write(data.Length);
                                            writer.Write(data);
                                            if (kv.Value is FavouriteServer)
                                                data = Encoding.Default.GetBytes(((FavouriteServer)(kv.Value)).ServerPassword);
                                            else
                                                data = new byte[0];
                                            writer.Write(data.Length);
                                            writer.Write(data);
                                            if (kv.Value is FavouriteServer)
                                                data = Encoding.Default.GetBytes(((FavouriteServer)(kv.Value)).RCONPassword);
                                            else
                                                data = new byte[0];
                                            writer.Write(data.Length);
                                            writer.Write(data);
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                //
                            }
                            maxServers = (uint)(value.Count);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// API data contract
        /// </summary>
        public APIDataContract APIDataContract
        {
            get
            {
                APIDataContract ret = new APIDataContract();
                ret.name = name;
                ret.type = serverListType.ToString();
                ret.endpoint = endpoint;
                return ret;
            }
        }

        /// <summary>
        /// Translatable text
        /// </summary>
        public string TranslatableText
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apidc">API data contract</param>
        public ServerListConnector(APIDataContract apidc)
        {
            name = apidc.name;
            if (!(Enum.TryParse(apidc.type, out serverListType)))
                serverListType = EServerListType.Error;
            endpoint = apidc.endpoint;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="serverListType">Server list type</param>
        /// <param name="endpoint">Endpoint</param>
        public ServerListConnector(string name, EServerListType serverListType, string endpoint)
        {
            this.name = name;
            this.serverListType = serverListType;
            this.endpoint = endpoint;
        }

        /// <summary>
        /// To string
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return name;
        }
    }
}
