using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util.Store;
using Google.Apis.Auth.OAuth2.Responses;

namespace Assignment.Utilities
{
    public static class EmailSender
    {
        public static async Task<bool> SendMail(IConfiguration configuration, string to, string subject, string html, string fromEmail = null)
        {
            try
            {
                string clientId = configuration["GoogleMailer:ClientId"] ?? string.Empty;
                string clientSecret = configuration["GoogleMailer:ClientSecret"] ?? string.Empty;
                string refreshToken = configuration["GoogleMailer:RefreshToken"] ?? string.Empty;
                string primaryAddress = configuration["GoogleMailer:EmailPrimaryAddress"] ?? string.Empty;
                string sendAddress = configuration["GoogleMailer:EmailSendAddress"] ?? string.Empty;

                if (string.IsNullOrEmpty(fromEmail))
                {
                    fromEmail = sendAddress;
                }

                var credential = new UserCredential(new GoogleAuthorizationCodeFlow(
                    new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = new ClientSecrets
                        {
                            ClientId = clientId,
                            ClientSecret = clientSecret
                        },
                        Scopes = new[] { "https://mail.google.com/" },
                        DataStore = new FileDataStore("TokenStore")
                    }),
                    primaryAddress,
                    new TokenResponse { RefreshToken = refreshToken }
                );

                await credential.RefreshTokenAsync(CancellationToken.None);
                var accessToken = credential.Token.AccessToken;

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("", fromEmail));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;

                message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = html
                };

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect);

                    var oauth2 = new SaslMechanismOAuth2(primaryAddress, accessToken);
                    await client.AuthenticateAsync(oauth2);

                    await client.SendAsync(message);
                    client.Disconnect(true);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gửi email: {ex.Message}");
                return false;
            }
        }
    }
}