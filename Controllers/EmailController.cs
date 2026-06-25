using System;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PRJ_WAREHOUSE_BIVN.Common;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly ISendMailService _sendMailService;

        public EmailController(ISendMailService sendMailService)
        {
            _sendMailService = sendMailService;
        }

        [HttpPost("send")]
        [AllowAnonymous]
        public async Task<IActionResult> Send([FromBody] EmailRequest req)
        {
            if (req is null) return BadRequest(new { success = false, message = "request body required" });
            if (string.IsNullOrWhiteSpace(req.MailFrom)) return BadRequest(new { success = false, message = "mailFrom required" });

            bool ok = await EmailSender.sendEmailNotifyAsync(
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

        [HttpGet]
        [HttpGet("SendMailSupplier")]
        [AllowAnonymous]
        public async Task<IActionResult> SendMailSupplier()
        {
            var res = await _sendMailService.SendMailToSupplierAsync();
            if (!res.Success)
            {
                return StatusCode(500, new { success = false, message = res.Message });
            }
            return Ok(new { success = true, message = "Email API is running" });
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
}
