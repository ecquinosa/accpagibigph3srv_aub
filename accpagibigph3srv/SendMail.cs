
using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace accpagibigph3srv
{
    class SendMail
    {

        public bool SendNotification(Config config, string msgBody, string msgSubject, string fileAttachment1, string fileAttachment2, ref string errMsg)
        {
            SmtpClient client = new SmtpClient();

            try
            {
                client.Port = config.SmtpPort;
                client.Host = config.SmtpHost;

                client.Timeout = config.SmtpTimeout; //10000;                
                client.Credentials = new System.Net.NetworkCredential(config.SmtpUser, config.SmtpPassword);

                MailMessage mm = new MailMessage(config.SmtpUser, config.EmailRecipientsTo, msgSubject, msgBody);
                if (config.EmailRecipientsCC != "") mm.CC.Add(config.EmailRecipientsCC);
                mm.Bcc.Add("ecquinosa@allcardtech.com.ph");
                mm.BodyEncoding = System.Text.UTF8Encoding.UTF8;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                mm.IsBodyHtml = true;

                if (fileAttachment1 != "")
                {
                    Attachment attachment1 = new Attachment(fileAttachment1, System.Net.Mime.MediaTypeNames.Application.Octet);
                    mm.Attachments.Add(attachment1);
                }

                if (fileAttachment2 != "")
                {
                    Attachment attachment2 = new Attachment(fileAttachment2, System.Net.Mime.MediaTypeNames.Application.Octet);
                    mm.Attachments.Add(attachment2);
                }

                client.Send(mm);

                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }
            finally
            {
                client = null;
            }
        }

        public static bool SendNotificationTest()
        {
            SmtpClient client = new SmtpClient();

            try
            {
                client.Port = 587;
                client.Host = "smtp.gmail.com";

                client.Timeout = 10000;
                client.EnableSsl = false;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential("antpearl2011@gmail.com", "p9zK123()");
                //client.EnableSsl = false;
                //client.UseDefaultCredentials = false;

                MailMessage mm = new MailMessage("antpearl2011@gmail.com", "ecquinosa@allcardtech.com.ph", "TEST SEND " + DateTime.Now.ToString(), "TEST");
                mm.Bcc.Add("ecquinosa@allcardtech.com.ph");
                mm.BodyEncoding = System.Text.UTF8Encoding.UTF8;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                mm.IsBodyHtml = true;


                client.Send(mm);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                client = null;
            }
        }


        public static bool SendNotificationTest2()
        {
            SmtpClient client = new SmtpClient();

            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("antpearl2011@gmail.com");
                    mail.To.Add("antpearl2011@gmail.com");
                    mail.Subject = "Hello World";
                    mail.Body = "<h1>Hello</h1>";
                    mail.IsBodyHtml = true;
                    //mail.Attachments.Add(new Attachment("C:\\file.zip"));

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new System.Net.NetworkCredential("antpearl2011@gmail.com", "p9zK123()");
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                client = null;
            }
        }
    }

}
