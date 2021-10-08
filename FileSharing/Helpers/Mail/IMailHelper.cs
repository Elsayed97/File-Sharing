using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileSharing.Helpers.Mail
{
	public interface IMailHelper
	{
		void sendMessage(InputMessageHelper message);
	}
}
