using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using CBB_CommonExtenderClass = CraftSynth.BuildingBlocks.Common.ExtenderClass;
using CBB_LoggingCustomTraceLog = CraftSynth.BuildingBlocks.Logging.CustomTraceLog;
using CBB_LoggingCustomTraceLogExtensions = CraftSynth.BuildingBlocks.Logging.CustomTraceLogExtensions;
using CBB_Encryptyion = CraftSynth.BuildingBlocks.Encryption;
using CBB_Error = CraftSynth.BuildingBlocks.Common.Error;
using CraftSynth.BuildingBlocks.Common;

namespace CraftSynth.BuildingBlocks.IO.Http
{
    [Serializable]
    public enum OAuth2GrantType
    {
        password
        //TODO: implement others
    }

    [Serializable]
    public enum OAuth2AccessTokenType
    {
        bearer
        //TODO: implement others
    }

    [Serializable]
    public class OAuth2AccessToken
    {
        public string Id { get; set; }
        public OAuth2AccessTokenType Type { get; set; }
        public string IssuedToUsername { get; set; }
        public DateTime IssuedAtAsUtc { get; set; }
        public DateTime? LastTimeUsedToValidateRequestAsUtc { get; set; }
        [XmlIgnore]
        public TimeSpan ExpiresIn { get; set; }
        //Needed for serialization
        public double ExpiresInAsMinutes
        {
            get { return ExpiresIn.TotalMinutes; }
            set { ExpiresIn = TimeSpan.FromMinutes(value); }
        }
        public string Value { get; set; }

        public double IssuedAtAsUtcAsUnixEpochTimeInMilliseconds
        {
            get
            {
                //source: https://www.codeproject.com/Questions/432371/Return-the-number-of-milliseconds-since-1970-01-01
                double r = new TimeSpan(this.IssuedAtAsUtc.Ticks - new DateTime(1970, 1, 1).Ticks).TotalSeconds;
                return r;
            }
        }

        public bool Expired
        {
            get
            {
                bool r;
                r = this.IssuedAtAsUtc.Ticks + this.ExpiresIn.Ticks <= DateTime.UtcNow.Ticks;
                return r;
            }
        }

        public bool CanBeDeletedFromStorage(OAuth2Client client)
        {
            bool r = IssuedAtAsUtc.Ticks + this.ExpiresIn.Ticks + client.AccessTokensAreDeletedFromStorageAfterThisTimeAfterExpiration.Ticks <= DateTime.UtcNow.Ticks;
            return r;
        }

        public override string ToString()
        {
            return $"Id={CBB_CommonExtenderClass.ToNonNullString(this.Id,"null")}|Type={this.Type.ToString()}|IssuedToUsername={CBB_CommonExtenderClass.ToNonNullString(this.IssuedToUsername, "null")}|IssuedAtAsUtc={this.IssuedAtAsUtc.ToString()}|ExpiresIn={this.ExpiresIn.Days+"days,"+this.ExpiresIn.Hours+"hours,"+this.ExpiresIn.Minutes+"mins,"+this.ExpiresIn.Seconds+"secs"}|Value={CBB_CommonExtenderClass.ToNonNullString(this.Value, "null")}|Expired={this.Expired.ToString()}";
        }
    }

    [Serializable]
    public class OAuth2User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string SecurityToken { get; set; }
    }

    [Serializable]
    public class OAuth2Client
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Secret { get; set; }
        public string InstanceUrl { get; set; }

        public List<OAuth2User> Users { get; set; }
        public List<OAuth2AccessToken> AccessTokens { get; set; }
        [XmlIgnore]
        public TimeSpan AccessTokensExpireIn { get; set; }
        //Needed for serialization
        public double AccessTokensExpireInAsMinutes
        {
            get { return AccessTokensExpireIn.TotalMinutes; }
            set { AccessTokensExpireIn = TimeSpan.FromMinutes(value); }
        }
        [XmlIgnore]
        public TimeSpan AccessTokensAreDeletedFromStorageAfterThisTimeAfterExpiration { get; set; }
        public double AccessTokensAreDeletedFromStorageAfterThisTimeAfterExpirationAsMinutes
        {
            get { return AccessTokensAreDeletedFromStorageAfterThisTimeAfterExpiration.TotalMinutes; }
            set { AccessTokensAreDeletedFromStorageAfterThisTimeAfterExpiration = TimeSpan.FromMinutes(value); }
        }
    }

    [Serializable]
    public class OAuth2Data
    {
        public List<OAuth2Client> Clients { get; set; }
    }

    internal class OAuthDataHandler
    {
        private string FilePathOrDbConnectionStringToOAuthData { get;set;}
        private OAuth2Data Data { get; set; }
        public OAuthDataHandler(string filePathOrDbConnectionStringToOAuthData)
        {
            //TODO: implement db source support
            this.FilePathOrDbConnectionStringToOAuthData = filePathOrDbConnectionStringToOAuthData;
            this.Load();
        }

        public OAuthDataHandler(string filePathOrDbConnectionStringToOAuthData, OAuth2Data initialDataToResetTo)
        {
            //TODO: implement db source support
            this.FilePathOrDbConnectionStringToOAuthData = filePathOrDbConnectionStringToOAuthData;
            this.Data = initialDataToResetTo;
        }

        private List<OAuth2Client> Load()
        {
            List<OAuth2Client> r = null;

            if (!File.Exists(this.FilePathOrDbConnectionStringToOAuthData))
            {
                throw new Exception("Can not load clients data. File not found:" + CBB_CommonExtenderClass.ToNonNullString(this.FilePathOrDbConnectionStringToOAuthData, "null"));
            }
            else
            {
                string xml = File.ReadAllText(this.FilePathOrDbConnectionStringToOAuthData);
                this.Data = OAuthDataHandler.Deserialize<OAuth2Data>(xml);
                ValidateOAuth2Data();
            }

            return r;
        }

        public OAuth2Client GetClientDataById(string clientId)
        {
            var r = Data.Clients.FirstOrDefault(c => c.Id == clientId);
            return r;
        }

        private void ValidateOAuth2Data()
        {
            if (this.Data == null
                                || this.Data.Clients == null
                                || this.Data.Clients.Any(c => CBB_CommonExtenderClass.IsNullOrWhiteSpace(c.Id))
                                || this.Data.Clients.Any(c => CBB_CommonExtenderClass.IsNullOrWhiteSpace(c.Secret))
                                || this.Data.Clients.Any(c => c.Users == null)
                                || this.Data.Clients.Any(c => c.Users.Any(u => CBB_CommonExtenderClass.IsNullOrWhiteSpace(u.Username)))
                                || this.Data.Clients.Any(c => c.Users.Any(u => CBB_CommonExtenderClass.IsNullOrWhiteSpace(u.Password)))
                                || this.Data.Clients.Any(c => c.Users.Any(u => CBB_CommonExtenderClass.IsNullOrWhiteSpace(u.SecurityToken)))
                               )
            {
                throw new Exception("oAuth2 data at server corrupted.");
            }
        }

        public static OAuth2Data BuildSampleData()
        {

            OAuth2Data r = new OAuth2Data()
            {
                Clients = new List<OAuth2Client>()
                {
                    new OAuth2Client()
                    {
                        Id = "095bfc13-c3e4-4a42-b401-7cbe2431bbfd",
                        Name = "Salesforce",
                        Secret = "f2dffaf0a761432797946d67f0160bd35354a3dc07cf452eac70668066896c35",
                        InstanceUrl = "",
                        Users = new List<OAuth2User>()
                        {
                            new OAuth2User()
                            {
                                Username = "admin",
                                Password = "1.@dm1n.1",
                                SecurityToken = "c8a1521c737944feaa0750a7"
                            }
                        },
                        AccessTokens = new List<OAuth2AccessToken>()
                        {
                            new OAuth2AccessToken()
                            {
                                Id = "Dummy token that should not be used because tokens are created automatically.",
                                ExpiresIn = new TimeSpan(1,0,0),
                                IssuedAtAsUtc = DateTime.UtcNow,
                                Type = OAuth2AccessTokenType.bearer,
                                Value = "2ac7ca0fcefc4b3e9ed0c233a0a64fc3fdae2e90da064d689c02b130e9447f73"
                            }
                        },
                        AccessTokensExpireIn = new TimeSpan(1,0,0),
                        AccessTokensAreDeletedFromStorageAfterThisTimeAfterExpiration = new TimeSpan(30,0,0,0)
                    }
                }               
            };
            return r;
        }

        public void Save()
        {
            //delete very old tokens so storage is not floded
            foreach(var client in this.Data.Clients)
            {
                for(int i=client.AccessTokens.Count-1;i>=0;i--)
                {
                    if (client.AccessTokens[i].CanBeDeletedFromStorage(client))
                    {
                        client.AccessTokens.RemoveAt(i);
                    }
                }
            }

            string xml = OAuthDataHandler.Serialize<OAuth2Data>(this.Data);
            File.WriteAllText(this.FilePathOrDbConnectionStringToOAuthData, xml);
        }

        public bool ValidateOAuth2GetTokenRequest(OAuth2GetTokenRequest oAuth2GetTokenRequest, CBB_LoggingCustomTraceLog log)
        {
            bool r = false;

            using (CBB_LoggingCustomTraceLogExtensions.LogScope(log, "OAuth2GetTokenRequestIsValid..."))
            {
                var client = this.Data.Clients.FirstOrDefault(c => c.Id == oAuth2GetTokenRequest.ClientId);
                if(client==null)
                {
                    throw new Error(HttpStatusCode.NotFound, "Can not find any client with client_id="+CBB_CommonExtenderClass.ToNonNullString(oAuth2GetTokenRequest.ClientId,"null"), false);
                }

                if (client.Secret != oAuth2GetTokenRequest.ClientSecret)
                {
                    throw new Error(HttpStatusCode.Unauthorized, "client_secret invalid", false);
                }

                var user = client.Users.FirstOrDefault(u => u.Username == oAuth2GetTokenRequest.Username);
                if(user==null)
                {
                    throw new Error(HttpStatusCode.NotFound, "Can not find any client with username=" + CBB_CommonExtenderClass.ToNonNullString(oAuth2GetTokenRequest.Username, "null"), false);
                }

                if (user.Password+user.SecurityToken != oAuth2GetTokenRequest.Password)
                {
                    throw new Error(HttpStatusCode.Unauthorized, "password invalid", false);
                }

                CBB_LoggingCustomTraceLogExtensions.AddLine(log, "Valid!");
                r = true;
            }

            return r;
        }

        public OAuth2AccessToken CreateAndAddTokenToClient(OAuth2GetTokenRequest oAuth2GetTokenRequest, CBB_LoggingCustomTraceLog log)
        {
            OAuth2AccessToken r = null;

            using (CBB_LoggingCustomTraceLogExtensions.LogScope(log, "CreateAndAddTokenToClient..."))
            {
                var client = this.Data.Clients.FirstOrDefault(c => c.Id == oAuth2GetTokenRequest.ClientId);

                OAuth2AccessTokenType? accessTokenType = null;
                switch (oAuth2GetTokenRequest.GrantTypeAsEnum)
                {
                    case OAuth2GrantType.password: accessTokenType = OAuth2AccessTokenType.bearer; break;
                    default: accessTokenType = null; break;
                }

                OAuth2AccessToken token = new OAuth2AccessToken()
                {
                    Type = accessTokenType.Value,
                    Id = Guid.NewGuid().ToString(),
                    IssuedToUsername = oAuth2GetTokenRequest.Username,
                    IssuedAtAsUtc = DateTime.UtcNow,
                    ExpiresIn = client.AccessTokensExpireIn,
                    Value = (Guid.NewGuid().ToString() + Guid.NewGuid().ToString()).Replace("-", "")
                };

                client.AccessTokens.Add(token);

                log._AddLine("Issuing token: "+token.ToString());
                r = token;
            }

            return r;
        }

        public bool ValidateOAuth2ValidateTokenRequest(ValidateTokenRequest validateOAuth2ValidateTokenRequest, CBB_LoggingCustomTraceLog log)
        {
            bool r = false;

            using (CBB_LoggingCustomTraceLogExtensions.LogScope(log, "ValidateOAuth2ValidateTokenRequest..."))
            {
                var client = this.Data.Clients.FirstOrDefault(c => c.AccessTokens!=null && c.AccessTokens.Any(t=>t.Value== validateOAuth2ValidateTokenRequest.TokenValue));
                if (client == null)
                {
                    CBB_LoggingCustomTraceLogExtensions.AddLine(log, $"Unauthorized access. Reason: NO CLIENT WITH SUCH TOKEN FOUND OR TOKEN EXPIRED LONG TIME AGO AND WAS DELETED FROM STORAGE.");

                    int logPointId = log._AddLogPointId();
                    throw new Error(HttpStatusCode.Unauthorized, "Unauthorized. LogId="+logPointId, false);
                }

                var token = client.AccessTokens.Single(t => t.Value == validateOAuth2ValidateTokenRequest.TokenValue);
                if (token.Expired)
                {
                    CBB_LoggingCustomTraceLogExtensions.AddLine(log, "Unauthorized access. Reason: TOKEN EXPIRED.");

                    int logPointId = log._AddLogPointId();
                    throw new Error(HttpStatusCode.Unauthorized, $"Token expired. Tokens expire after {client.AccessTokensExpireInAsMinutes} minutes. Please obtain new one. TokenId={token.Id}. LogId=" + logPointId, false);
                }

                token.LastTimeUsedToValidateRequestAsUtc = DateTime.UtcNow;
                this.Save();

                CBB_LoggingCustomTraceLogExtensions.AddLine(log, "Token valid!");
            }

            return r;
        }

        #region Helper methods
        private static string Serialize<T>(T entity)
        {
            string r;

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringWriter sw = new StringWriter())
            {
                serializer.Serialize(sw, entity);
                //TODO: serialize inner members... this.SerializeGenerics(serializedUser.ChildNodes[1], user);
                r = sw.ToString();
            }

            return r;
        }

        private static T Deserialize<T>(string xml)
        {
            T r = default(T);

            XmlDocument d = new XmlDocument();
            d.LoadXml(xml);
            StringReader reader = new StringReader(xml);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            r = (T)serializer.Deserialize(reader);

            return r;
        }
        #endregion

    }

    public class OAuth2GetTokenRequest
    {
        public string GrantType;
        public string ClientId;
        public string ClientSecret;
        public string Username;
        public string Password;
        public string Token;
        public OAuth2GrantType? GrantTypeAsEnum
        {
            get
            {
                OAuth2GrantType? r;
                if (this.GrantType == null)
                {
                    r = null;
                }
                else
                {
                    switch (this.GrantType)
                    {
                        case "password": r = OAuth2GrantType.password; break;
                        default: r = null; break;
                    }
                }
                return r;
            }
        }

        public OAuth2GetTokenRequest(List<KeyValuePair<string,string>> keyValuePairsFromRequest, CBB_LoggingCustomTraceLog log)
        {
            using (CBB_LoggingCustomTraceLogExtensions.LogScope(log, "Parsing OAuth2GetTokenRequest..."))
            {
                string message = "";
                this.GrantType = keyValuePairsFromRequest.FirstOrDefault(p=>p.Key=="grant_type").Value;
                CBB_LoggingCustomTraceLogExtensions.AddLine(log, "---grant_type=" + CBB_CommonExtenderClass.ToNonNullString(this.GrantType, "null"));
                if (CBB_CommonExtenderClass.IsNullOrWhiteSpace(this.GrantType))
                {
                    message += "Missing 'grant_type';";
                }

                this.ClientId = keyValuePairsFromRequest.FirstOrDefault(p => p.Key == "client_id").Value;
                CBB_LoggingCustomTraceLogExtensions.AddLine(log, "---client_id=" + CBB_CommonExtenderClass.ToNonNullString(this.ClientId, "null"));
                if (CBB_CommonExtenderClass.IsNullOrWhiteSpace(this.ClientId))
                {
                    message += "Missing 'client_id';";
                }

                this.ClientSecret = keyValuePairsFromRequest.FirstOrDefault(p => p.Key == "client_secret").Value;
                CBB_LoggingCustomTraceLogExtensions.AddLine(log, "---client_secret=" + CBB_CommonExtenderClass.ToNonNullString(this.ClientSecret, "null"));
                if (CBB_CommonExtenderClass.IsNullOrWhiteSpace(this.ClientSecret))
                {
                    message += "Missing 'client_secret';";
                }

                this.Username = keyValuePairsFromRequest.FirstOrDefault(p => p.Key == "username").Value;
                CBB_LoggingCustomTraceLogExtensions.AddLine(log, "---username=" + CBB_CommonExtenderClass.ToNonNullString(this.Username, "null"));
                if (CBB_CommonExtenderClass.IsNullOrWhiteSpace(this.Username))
                {
                    message += "Missing 'username';";
                }

                this.Password = keyValuePairsFromRequest.FirstOrDefault(p => p.Key == "password").Value;
                CBB_LoggingCustomTraceLogExtensions.AddLine(log, "---password=" + CBB_CommonExtenderClass.ToNonNullString(this.Password, "null"));
                if (CBB_CommonExtenderClass.IsNullOrWhiteSpace(this.Password))
                {
                    message += "Missing 'password';";
                }

                if (message.Length > 0)
                {
                    throw new CBB_Error(HttpStatusCode.BadRequest, message, false);
                }
            }
        }      
    }

    public class OAuth2GetTokenResponse
    {
        public string access_token { get;}
        public string instance_url { get;}
        public string id { get; }
        public string token_type { get; }
        public string issued_at { get; }
        public string expires_in { get; }
        public string signature { get; }

        public OAuth2GetTokenResponse(OAuth2Client client, OAuth2AccessToken token)
        {
            this.access_token = token.Value;
            this.instance_url = client.InstanceUrl;
            this.id = client.Id;
            this.token_type = token.Type.ToString();
            this.issued_at = token.IssuedAtAsUtcAsUnixEpochTimeInMilliseconds.ToString();
            this.expires_in = token.ExpiresIn.TotalMinutes.ToString();
            var json = this.ToJsonString(false);
            json = json + client.Secret;
            this.signature = CBB_Encryptyion.GetHashAsUnicodeStringUsingMD5Algorithm(json);
        }

        public string ToJsonString(bool includeSignature)
        {
            string r =

            $"{{" +
            $"  \"access_token\" : \"{CBB_CommonExtenderClass.ToNonNullNonEmptyString(this.access_token, "null")}\"," +
            $"  \"instance_url\" : \"{CBB_CommonExtenderClass.ToNonNullNonEmptyString(this.instance_url, "null")}\"," +
            $"  \"id\" : \"{CBB_CommonExtenderClass.ToNonNullNonEmptyString(this.id, "null")}\"," +
            $"  \"token_type\" : \"{CBB_CommonExtenderClass.ToNonNullNonEmptyString(this.token_type, "null")}\"," +
            $"  \"issued_at\" : \"{CBB_CommonExtenderClass.ToNonNullNonEmptyString(this.issued_at, "null")}\"," +
            (includeSignature?$"  \"signature\" : \"{CBB_CommonExtenderClass.ToNonNullNonEmptyString(this.signature, "null")}\"":"") +
            $"}}";

            return r;
        }
    }

    public class ValidateTokenRequest
    {
        public string ContentType { get; }
        public string Authorization { get; }
        public string TokenValue { get; }

        public ValidateTokenRequest(List<KeyValuePair<string, string>> keyValuePairsFromRequest, CBB_LoggingCustomTraceLog log)
        {
            using (CBB_LoggingCustomTraceLogExtensions.LogScope(log, "Parsing ValidateTokenRequest..."))
            {
                string message = "";

                this.ContentType = keyValuePairsFromRequest.FirstOrDefault(p => p.Key == "Content-Type").Value;
                CBB_LoggingCustomTraceLogExtensions.AddLine(log, "---Content-Type=" + CBB_CommonExtenderClass.ToNonNullString(this.ContentType, "null"));
                if (CBB_CommonExtenderClass.IsNullOrWhiteSpace(this.ContentType))
                {
                    message += "Missing 'Content-Type' header;";
                }
                else
                {
                    if (!this.ContentType.Contains("application/json"))
                    {
                        message += "Specified ContentType header value not recognized. Only application/json is supported.";
                    }
                }

                this.Authorization = keyValuePairsFromRequest.FirstOrDefault(p => p.Key == "Authorization").Value;
                CBB_LoggingCustomTraceLogExtensions.AddLine(log, "---Authorization=" + CBB_CommonExtenderClass.ToNonNullString(this.Authorization, "null"));
                if (CBB_CommonExtenderClass.IsNullOrWhiteSpace(this.Authorization))
                {
                    message += "Missing 'Authorization' header;";
                }
                else
                {
                    if (!this.Authorization.ToLower().StartsWith("bearer"))
                    {
                        message += "Specified Authorization header value not recognized. Only bearer is supported.";
                    }

                    try
                    {
                        this.TokenValue = this.Authorization.Split(' ')[1];
                        if (this.TokenValue.IsNullOrWhiteSpace())
                        {
                            throw new Exception("Can not be null or empty");
                        }
                    }
                    catch
                    {
                        message += "Token can not be parsed from Authorization header value.";
                    }
                }

                if (message.Length > 0)
                {
                    throw new CBB_Error(HttpStatusCode.BadRequest, message, false);
                }
            }
        }
    }

    public class OAuth2
	{
        private static object _lock = new object();

        //use this ONLY in development to generate xml file so you can then edit it
        public static void ResetClientDataToSampleData(string filePathToClientsDataOrDbConnectionString)
        {
            var sampleOAuth2Data = OAuthDataHandler.BuildSampleData();
            var oAuth2DataHandler = new OAuthDataHandler(filePathToClientsDataOrDbConnectionString, sampleOAuth2Data);
            lock (_lock)
            {
                oAuth2DataHandler.Save();
            }
        }

        public static OAuth2GetTokenResponse GetTokenIfCredentialsAreValid(List<KeyValuePair<string, string>> keyValuePairsFromRequest, string filePathToClientsDataOrDbConnectionString, CBB_LoggingCustomTraceLog log)
        {
            OAuth2GetTokenResponse r = null;

            log = log ?? new CBB_LoggingCustomTraceLog();
            try
            {
                using (CBB_LoggingCustomTraceLogExtensions.LogScope(log, "GetTokenIfCredentialsAreValid..."))
                {
                    OAuth2GetTokenRequest oAuth2GetTokenRequest = new OAuth2GetTokenRequest(keyValuePairsFromRequest, log);
                    lock (_lock)
                    {
                        var oAuth2DataHandler = new OAuthDataHandler(filePathToClientsDataOrDbConnectionString);
                        if (oAuth2DataHandler.ValidateOAuth2GetTokenRequest(oAuth2GetTokenRequest, log))
                        {
                            var client = oAuth2DataHandler.GetClientDataById(oAuth2GetTokenRequest.ClientId);
                            var token = oAuth2DataHandler.CreateAndAddTokenToClient(oAuth2GetTokenRequest, log);
                            oAuth2DataHandler.Save();
                            r = new OAuth2GetTokenResponse(client, token);
                            #region example response from salesforce
                            //{
                            //  "access_token" : "00D6g0000052C7S!AQ0AQJ98zeGD.i2Ud63zqoY84vDXgPWoFpVjj3dYh4m0mrI6EzdsWHucgtD8M3QlrFhIygeXu.GQMQQ7DQP2PfH.jQ1hLmev",
                            //  "instance_url" : "https://autovitals.my.salesforce.com",
                            //  "id" : "https://login.salesforce.com/id/00D6g0000052C7SEAU/0056g000004uvNlAAI",
                            //  "token_type" : "Bearer",
                            //  "issued_at" : "1586266445159",
                            //  "signature" : "KdEsUouIZtkpfF7d6XMeVK3AiW8Cw04hQm+5XvsB5tU="
                            //}

                            //JsonConvert.DeserializeObject<Dictionary<string, string>>
                            #endregion
                            CBB_LoggingCustomTraceLogExtensions.AddLine(log, "RESPONSE: " + r);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if(e is Error)
                {
                    throw;
                }
                else
                {
                    throw new Error($"Error occured while executing GetTokenIfCredentialsAreValid(keyValuePairsFromRequest,'{filePathToClientsDataOrDbConnectionString.ToNonNullString("null")}', log). See inner exception for more details.", e);
                }
            }

            return r;
        }

        public static void ValidateToken(List<KeyValuePair<string, string>> keyValuePairsFromRequest, string filePathToClientsDataOrDbConnectionString, CBB_LoggingCustomTraceLog log)
        {
            OAuth2GetTokenResponse r = null;

            log = log ?? new CBB_LoggingCustomTraceLog();
            try
            {
                using (CBB_LoggingCustomTraceLogExtensions.LogScope(log, "ValidateToken..."))
                {
                    ValidateTokenRequest validateTokenRequest = new ValidateTokenRequest(keyValuePairsFromRequest, log);
                    lock (_lock)
                    {
                        var oAuth2DataHandler = new OAuthDataHandler(filePathToClientsDataOrDbConnectionString);
                        oAuth2DataHandler.ValidateOAuth2ValidateTokenRequest(validateTokenRequest, log);
                        CBB_LoggingCustomTraceLogExtensions.AddLine(log, "Token valid! Token:"+validateTokenRequest.TokenValue);
                    }
                }
            }
            catch (Exception e)
            {
                if (e is Error)
                {
                    throw;
                }
                else
                {
                    throw new Error($"Error occured while executing ValidateToken(keyValuePairsFromRequest,'{filePathToClientsDataOrDbConnectionString.ToNonNullString("null")}', log). See inner exception for more details.", e);
                }
            }
        }
    }
}
