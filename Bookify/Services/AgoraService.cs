
using AgoraIO;
using AgoraIO.Rtc;

namespace Bookify.Services;
    public class AgoraService
    {
        private readonly string _appId;
        private readonly string _appCertificate;

        // The IConfiguration service will be injected by .NET's dependency injection system
        public AgoraService(IConfiguration config)
        {
            // Get the credentials from appsettings.json
            _appId = config["Agora:AppId"] ?? throw new ArgumentNullException("Agora AppId not found in configuration.");
            _appCertificate = config["Agora:AppCertificate"] ?? throw new ArgumentNullException("Agora AppCertificate not found in configuration.");
        }

        public string GenerateRtcToken(string channelName, uint uid)
        {
            // Tokens are valid for 1 hour by default
            const uint privilegeExpiredTs = 3600;

            var tokenBuilder = new RtcTokenBuilder();
            // The 5th argument is the user's role. RolePublisher can both send and receive audio.
            return tokenBuilder.BuildToken(_appId, _appCertificate, channelName,  true, privilegeExpiredTs);
        }
    }
