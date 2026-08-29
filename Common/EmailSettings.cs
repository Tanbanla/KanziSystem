using Dapper;
using OfficeOpenXml.Packaging.Ionic.Zlib;
using PRJ_WAREHOUSE_BIVN.DTO;
using PRJ_WAREHOUSE_BIVN.Models;
using PRJ_WAREHOUSE_BIVN.Models_Agent;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace PRJ_WAREHOUSE_BIVN.Common
{
    public static class EmailSender
    {
        private static EmailSettings _emailSettings;
        private static string _agentContext;
        private const string DecryptionKey = "BIVNIT2071";

        public static void Initialize(EmailSettings emailSettings, string agentConnection)
        {
            _emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
            _agentContext = agentConnection ?? throw new ArgumentNullException(nameof(agentConnection));
        }

        private static void EnsureInitialized()
        {
            if (_emailSettings == null) throw new InvalidOperationException("EmailSender is not initialized. Call EmailSender.Initialize(...) at startup.");
            if (string.IsNullOrWhiteSpace(_agentContext)) throw new InvalidOperationException("Agent connection string is not initialized.");
        }

        private static void AddAddresses(MailAddressCollection collection, string addresses)
        {
            if (string.IsNullOrWhiteSpace(addresses)) return;
            var list = addresses.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x?.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x));

            foreach (var addr in list)
            {
                collection.Add(new MailAddress(addr));
            }
        }

        public static async Task<bool> sendEmailNotifyAsync(string title, string mail_from, string mail_to, string mail_cc, string mail_bcc, string body, int priority)
        {
            EnsureInitialized();
            bool blresult = true;

            try
            {
                //mail_to = "lethiphuong.lan@brother-bivn.com.vn;dinhthithu.huyen@brother-bivn.com.vn;huongoh@brothergroup.net;huyente@brothergroup.net;";
                //mail_cc = "lethiphuong.lan@brother-bivn.com.vn;dinhthithu.huyen@brother-bivn.com.vn;huongoh@brothergroup.net;huyente@brothergroup.net;";

                using var msg = new MailMessage();

                var fromAddress = _emailSettings.SenderEmail+ "@brother-bivn.com.vn";//!string.IsNullOrWhiteSpace(mail_from) ? mail_from : _emailSettings.SenderEmail;
                if (string.IsNullOrWhiteSpace(fromAddress)) throw new ArgumentException("Sender address is not specified.", nameof(mail_from));
                msg.From = new MailAddress(fromAddress);

                AddAddresses(msg.To, mail_to);
                AddAddresses(msg.CC, mail_cc);
                AddAddresses(msg.Bcc, mail_bcc);

                if (priority == 1)
                {
                    msg.Priority = MailPriority.High;
                }

                msg.Subject = title ?? string.Empty;
                msg.Body = body ?? string.Empty;
                msg.IsBodyHtml = true;

                var smtpUser = !string.IsNullOrWhiteSpace(_emailSettings.SenderEmail)
                    ? _emailSettings.SenderEmail
                    : _emailSettings.SenderName;

                if (string.IsNullOrWhiteSpace(smtpUser)) throw new InvalidOperationException("SMTP user is not configured.");

                using var emailClient = new System.Net.Mail.SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
                try
                {
                    var passWord = await DecryptEmailInfoAsync(smtpUser);
                    emailClient.Credentials = new System.Net.NetworkCredential(smtpUser, passWord);
                    emailClient.EnableSsl = true;

                    await emailClient.SendMailAsync(msg);
                }
                catch (SmtpException ex) when (ex.Message != null && ex.Message.Contains("does not support secure connections", StringComparison.OrdinalIgnoreCase))
                {
                    using var emailClientFallback = new System.Net.Mail.SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                    {
                        Credentials = new System.Net.NetworkCredential(smtpUser, await DecryptEmailInfoAsync(smtpUser)),
                        EnableSsl = false
                    };
                    await emailClientFallback.SendMailAsync(msg);
                }
            }
            catch (Exception)
            {
                blresult = false;
            }

            return blresult;
        }

        // send mail gửi file 
        public static async Task<GenericResponse<bool>> SendEmailNotifyCustomSendMultiAttachFileAsync(EmailFormNetMailCustomSendMultiAttachFile emailForm)
        {
            EnsureInitialized();
            var result = new GenericResponse<bool>();
            if (emailForm == null)
            {
                result.Success = false;
                result.Message = "Không nhận được dữ liệu input!";
                return result;
            }
            try
            {
                using var msg = new MailMessage();
                var fromAddress = _emailSettings.SenderEmail + "@brother-bivn.com.vn";//!string.IsNullOrWhiteSpace(mail_from) ? mail_from : _emailSettings.SenderEmail;
                if (string.IsNullOrWhiteSpace(fromAddress)) throw new ArgumentException("Sender address is not specified.", nameof(emailForm.mail_from));
                msg.From = new MailAddress(fromAddress);

                AddAddresses(msg.To, emailForm.mail_to);
                AddAddresses(msg.CC, emailForm.mail_cc);
                AddAddresses(msg.Bcc, emailForm.mail_bcc);

                if (emailForm.attachmentPaths != null && emailForm.attachmentPaths.Any())
                {
                    foreach (var path in emailForm.attachmentPaths)
                    {
                        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                        {
                            msg.Attachments.Add(new Attachment(path));
                        }
                    }
                }

                msg.Subject = emailForm.title ?? string.Empty;
                msg.Body = emailForm.body ?? string.Empty;
                msg.IsBodyHtml = true;

                var smtpUser = !string.IsNullOrWhiteSpace(_emailSettings.SenderEmail)
                    ? _emailSettings.SenderEmail
                    : _emailSettings.SenderName;
                if (string.IsNullOrWhiteSpace(smtpUser)) throw new InvalidOperationException("SMTP user is not configured.");

                using var emailClient = new System.Net.Mail.SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
                try
                {
                    var pass = await DecryptEmailInfoAsync(smtpUser);
                    emailClient.Credentials = new System.Net.NetworkCredential(smtpUser, pass);
                    emailClient.EnableSsl = true;
                    await emailClient.SendMailAsync(msg);
                }
                catch (SmtpException ex) when (ex.Message != null && ex.Message.Contains("does not support secure connections", StringComparison.OrdinalIgnoreCase))
                {
                    using var fallback = new System.Net.Mail.SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                    {
                        Credentials = new System.Net.NetworkCredential(smtpUser, await DecryptEmailInfoAsync(smtpUser)),
                        EnableSsl = false
                    };
                    await fallback.SendMailAsync(msg);
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Message:" + ex.Message + "\nStackTrace:" + ex.StackTrace;
            }

            return result;
        }

        // Giải mã hóa để lấy thông tin gửi mail (lấy mật khẩu đã mã hóa từ AGENTDB)
        public static async Task<string> DecryptEmailInfoAsync(string mailADID)
        {
            // Kiểm tra xem EmailSender đã được khởi tạo chưa
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(mailADID))
                throw new ArgumentNullException(nameof(mailADID), "Email không được để trống");

            // Lấy mật khẩu đã mã hóa từ cơ sở dữ liệu AGENTDB
            using var connection = new SqlConnection(_agentContext);
            var sql = @"SELECT VCHR_USER_PASSWORD as password 
            FROM [AGENTDB].[dbo].[TM_USER_CONFIRM_SERVER] 
            WHERE VCHR_USER_ADID = @mailADID";
            var passWord = await connection.QueryFirstOrDefaultAsync<string>(sql, new { mailADID });

            if (string.IsNullOrEmpty(passWord))
                throw new ArgumentNullException(nameof(passWord), "Dữ liệu mail không tồn tại");

            // Giải mã mật khẩu đã mã hóa
            try
            {

                byte[] cipherBytes = Convert.FromBase64String(passWord);

                using (MD5 md5 = MD5.Create())
                {
                    byte[] keyBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(DecryptionKey));

                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = keyBytes;
                        aes.Mode = CipherMode.ECB;

                        aes.Padding = PaddingMode.PKCS7;

                        using (ICryptoTransform decryptor = aes.CreateDecryptor())
                        {
                            byte[] resultBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

                            return Encoding.UTF8.GetString(resultBytes);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi decrypt: " + ex.Message, ex);
            }
        }
    }
}
