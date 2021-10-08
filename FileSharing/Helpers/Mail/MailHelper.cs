using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace FileSharing.Helpers.Mail
{
	public class MailHelper : IMailHelper
	{
		private readonly IConfiguration config;

		public MailHelper(IConfiguration config)
		{
			this.config = config;
		}
		public void sendMessage(InputMessageHelper model)
		{
			using (SmtpClient client = new SmtpClient(config.GetValue<string>("Mail:Host"), config.GetValue<int>("Mail:Port")))
			{
				client.EnableSsl = true;
				client.UseDefaultCredentials = false;
				client.DeliveryMethod = SmtpDeliveryMethod.Network;
				client.Credentials = new NetworkCredential(config.GetValue<string>("Mail:From"), config.GetValue<string>("Mail:PWD"));
				var message = new MailMessage();
				message.To.Add(model.Email);
				message.Subject = model.Subject;
				message.Body = model.Body;
				message.From = new MailAddress(config.GetValue<string>("Mail:From"), config.GetValue<string>("Mail:Sender"), System.Text.Encoding.UTF8);
				message.IsBodyHtml = true;
				client.ServicePoint.MaxIdleTime = 1;
				client.Send(message);
			}
		}
	}
}
