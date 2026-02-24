using System;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        [HttpPost("send")]
        [AllowAnonymous]
        public IActionResult Send([FromBody] EmailRequest req)
        {
            if (req is null) return BadRequest(new { success = false, message = "request body required" });
            if (string.IsNullOrWhiteSpace(req.MailFrom)) return BadRequest(new { success = false, message = "mailFrom required" });

            bool ok = EmailSender.sendEmailNotify(
                req.Title ?? string.Empty,
                req.MailFrom,
                req.MailTo ?? string.Empty,
                req.MailCc ?? string.Empty,
                req.MailBcc ?? string.Empty,
                req.Body ?? string.Empty,
                req.Priority);

            if (ok) return Ok(new { success = true });
            return StatusCode(500, new { success = false, message = "failed to send email" });
        }

        public class EmailRequest
        {
            public string? Title { get; set; }
            public string MailFrom { get; set; } = string.Empty;
            public string? MailTo { get; set; }
            public string? MailCc { get; set; }
            public string? MailBcc { get; set; }
            public string? Body { get; set; }
            public int Priority { get; set; }
        }
    }

    public static class EmailSender
    {
        public static bool sendEmailNotify(string title, string mail_from, string mail_to, string mail_cc, string mail_bcc, string body, int priority)
        {
            bool blresult = true;

            try
            {
                MailMessage msg = new MailMessage();
                msg.From = new MailAddress(mail_from);

                if (!string.IsNullOrWhiteSpace(mail_to))
                {
                    var arrTo = mail_to.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string item_to in arrTo)
                    {
                        var t = item_to?.Trim();
                        if (!string.IsNullOrEmpty(t))
                            msg.To.Add(new MailAddress(t));
                    }
                }

                if (!string.IsNullOrWhiteSpace(mail_cc))
                {
                    var arrCc = mail_cc.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string item_cc in arrCc)
                    {
                        var c = item_cc?.Trim();
                        if (!string.IsNullOrEmpty(c))
                            msg.CC.Add(new MailAddress(c));
                    }
                }

                if (!string.IsNullOrWhiteSpace(mail_bcc))
                {
                    var arrBcc = mail_bcc.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string item_bcc in arrBcc)
                    {
                        var b = item_bcc?.Trim();
                        if (!string.IsNullOrEmpty(b))
                            msg.Bcc.Add(new MailAddress(b));
                    }
                }

                if (priority == 1)
                {
                    msg.Priority = MailPriority.High;
                }

                msg.Subject = title;
                msg.Body = body;
                msg.IsBodyHtml = true;

                using (SmtpClient emailClient = new SmtpClient("smtp.brother.co.jp", 25))
                {
                    emailClient.UseDefaultCredentials = true;
                    emailClient.Send(msg);
                }
            }
            catch (Exception)
            {
                blresult = false;
            }

            return blresult;
        }
    }
}
