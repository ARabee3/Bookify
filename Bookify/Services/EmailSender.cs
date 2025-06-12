using Bookify.Interfaces;
using MailKit.Net.Smtp; // <<< جديد
using MailKit.Security;  // <<< جديد
using MimeKit;           // <<< جديد
using MimeKit.Text;      // <<< جديد
using Microsoft.Extensions.Configuration; // <<< جديد (لقراءة الإعدادات)
using System.Threading.Tasks;
// using System; // مش محتاجينها هنا بنفس الشكل

namespace Bookify.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration; // <<< جديد

        // Constructor لحقن IConfiguration
        public EmailSender(IConfiguration configuration) // <<< جديد
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage) // غيرنا اسم الباراميتر لـ htmlMessage
        {
            try
            {
                // قراءة إعدادات الإيميل من appsettings.json
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = emailSettings["SmtpServer"];
                var port = int.Parse(emailSettings["Port"]); // لازم نحولها لـ int
                var senderName = emailSettings["SenderName"];
                var senderEmail = emailSettings["SenderEmail"];
                var password = emailSettings["Password"]; // ده هيكون باسورد الجيميل أو الـ App Password

                if (string.IsNullOrEmpty(smtpServer) || port == 0 || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(password))
                {
                    Console.WriteLine("Email settings are not configured properly in appsettings.json");
                    // ممكن ترمي Exception هنا أو تعمل Log للخطأ
                    return;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName ?? senderEmail, senderEmail)); // اسم المرسل وإيميله
                message.To.Add(MailboxAddress.Parse(email)); // إيميل المستلم
                message.Subject = subject;

                // محتوى الإيميل (يفضل يكون HTML عشان اللينكات تبقى clickable)
                message.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };

                using (var client = new SmtpClient())
                {
                    // الاتصال بسيرفر SMTP بتاع Gmail
                    await client.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls); // نستخدم StartTls

                    // المصادقة (Login) على سيرفر Gmail
                    await client.AuthenticateAsync(senderEmail, password);

                    // إرسال الإيميل
                    await client.SendAsync(message);

                    // قطع الاتصال
                    await client.DisconnectAsync(true);
                }
                Console.WriteLine($"Email sent to {email} with subject: {subject}"); // ممكن نعمل Log للنجاح
            }
            catch (Exception ex)
            {
                // مهم جداً تعمل Log للـ Exception ده عشان تعرف لو فيه مشكلة في إرسال الإيميل
                Console.WriteLine($"Failed to send email to {email}. Error: {ex.Message}");
                // لا ترمي Exception هنا عشان متوقفش باقي العملية (زي تسجيل المستخدم)
                // إلا لو إرسال الإيميل حرج جداً
            }
        }
    }
}