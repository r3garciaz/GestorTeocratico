using Microsoft.AspNetCore.Identity;
using Resend;
using GestorTeocratico.Data;

namespace GestorTeocratico.Services;

public class ResendEmailSender : IEmailSender<ApplicationUser>
{
    private readonly IResend _resend;
    private readonly ILogger<ResendEmailSender> _logger;
    private readonly IConfiguration _configuration;

    public ResendEmailSender(IResend resend, ILogger<ResendEmailSender> logger, IConfiguration configuration)
    {
        _resend = resend;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        var subject = "Confirma tu cuenta - Gestor Teocrático";
        var htmlBody = CreateConfirmationEmailHtml(confirmationLink);
        var textBody = CreateConfirmationEmailText(confirmationLink);

        await SendEmailAsync(email, subject, htmlBody, textBody);
    }

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        var subject = "Restablece tu contraseña - Gestor Teocrático";
        var htmlBody = CreatePasswordResetEmailHtml(resetLink);
        var textBody = CreatePasswordResetEmailText(resetLink);

        await SendEmailAsync(email, subject, htmlBody, textBody);
    }

    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        var subject = "Código de restablecimiento de contraseña - Gestor Teocrático";
        var htmlBody = CreatePasswordResetCodeEmailHtml(resetCode);
        var textBody = CreatePasswordResetCodeEmailText(resetCode);

        await SendEmailAsync(email, subject, htmlBody, textBody);
    }

    private async Task SendEmailAsync(string email, string subject, string htmlBody, string textBody)
    {
        try
        {
            var fromEmail = _configuration["Resend:FromEmail"] ?? "Gestor Teocrático <noreply@gestorteocratico.com>";
            
            var message = new EmailMessage
            {
                From = fromEmail,
                Subject = subject,
                HtmlBody = htmlBody,
                TextBody = textBody
            };
            
            message.To.Add(email);

            var result = await _resend.EmailSendAsync(message);
            
            _logger.LogInformation("Email sent successfully to {Email}. Status: {Status}", 
                email, result.Success ? "Success" : "Failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", email);
            throw;
        }
    }

    private static string CreateConfirmationEmailHtml(string confirmationLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Confirma tu cuenta</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .button {{ 
            display: inline-block; 
            background-color: #007bff; 
            color: white; 
            padding: 12px 24px; 
            text-decoration: none; 
            border-radius: 4px; 
            margin: 20px 0; 
        }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Gestor Teocrático</h1>
        </div>
        <div class='content'>
            <h2>Confirma tu cuenta</h2>
            <p>Hola,</p>
            <p>Gracias por registrarte en Gestor Teocrático. Para completar tu registro, necesitas confirmar tu dirección de email.</p>
            <p>Haz clic en el siguiente enlace para confirmar tu cuenta:</p>
            <a href='{confirmationLink}' class='button'>Confirmar cuenta</a>
            <p>Si no puedes hacer clic en el enlace, copia y pega la siguiente URL en tu navegador:</p>
            <p style='word-break: break-all;'>{confirmationLink}</p>
            <p>Si no creaste esta cuenta, puedes ignorar este email.</p>
        </div>
        <div class='footer'>
            <p>© 2024 Gestor Teocrático. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
    }

    private static string CreateConfirmationEmailText(string confirmationLink)
    {
        return $@"
Gestor Teocrático - Confirma tu cuenta

Hola,

Gracias por registrarte en Gestor Teocrático. Para completar tu registro, necesitas confirmar tu dirección de email.

Haz clic en el siguiente enlace para confirmar tu cuenta:
{confirmationLink}

Si no creaste esta cuenta, puedes ignorar este email.

© 2024 Gestor Teocrático. Todos los derechos reservados.
";
    }

    private static string CreatePasswordResetEmailHtml(string resetLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Restablece tu contraseña</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .button {{ 
            display: inline-block; 
            background-color: #dc3545; 
            color: white; 
            padding: 12px 24px; 
            text-decoration: none; 
            border-radius: 4px; 
            margin: 20px 0; 
        }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
        .warning {{ background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 10px; border-radius: 4px; margin: 10px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Gestor Teocrático</h1>
        </div>
        <div class='content'>
            <h2>Restablece tu contraseña</h2>
            <p>Hola,</p>
            <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta de Gestor Teocrático.</p>
            <p>Haz clic en el siguiente enlace para restablecer tu contraseña:</p>
            <a href='{resetLink}' class='button'>Restablecer contraseña</a>
            <p>Si no puedes hacer clic en el enlace, copia y pega la siguiente URL en tu navegador:</p>
            <p style='word-break: break-all;'>{resetLink}</p>
            <div class='warning'>
                <strong>Importante:</strong> Este enlace expirará en 24 horas por seguridad.
            </div>
            <p>Si no solicitaste restablecer tu contraseña, puedes ignorar este email de forma segura.</p>
        </div>
        <div class='footer'>
            <p>© 2024 Gestor Teocrático. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
    }

    private static string CreatePasswordResetEmailText(string resetLink)
    {
        return $@"
Gestor Teocrático - Restablece tu contraseña

Hola,

Recibimos una solicitud para restablecer la contraseña de tu cuenta de Gestor Teocrático.

Haz clic en el siguiente enlace para restablecer tu contraseña:
{resetLink}

IMPORTANTE: Este enlace expirará en 24 horas por seguridad.

Si no solicitaste restablecer tu contraseña, puedes ignorar este email de forma segura.

© 2024 Gestor Teocrático. Todos los derechos reservados.
";
    }

    private static string CreatePasswordResetCodeEmailHtml(string resetCode)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Código de restablecimiento</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .code {{ 
            background-color: #e9ecef; 
            padding: 20px; 
            text-align: center; 
            font-size: 24px; 
            font-weight: bold; 
            letter-spacing: 2px; 
            border-radius: 4px; 
            margin: 20px 0; 
        }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
        .warning {{ background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 10px; border-radius: 4px; margin: 10px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Gestor Teocrático</h1>
        </div>
        <div class='content'>
            <h2>Código de restablecimiento de contraseña</h2>
            <p>Hola,</p>
            <p>Tu código de restablecimiento de contraseña es:</p>
            <div class='code'>{resetCode}</div>
            <p>Ingresa este código en la aplicación para restablecer tu contraseña.</p>
            <div class='warning'>
                <strong>Importante:</strong> Este código expirará en 15 minutos por seguridad.
            </div>
            <p>Si no solicitaste restablecer tu contraseña, puedes ignorar este email de forma segura.</p>
        </div>
        <div class='footer'>
            <p>© 2024 Gestor Teocrático. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
    }

    private static string CreatePasswordResetCodeEmailText(string resetCode)
    {
        return $@"
Gestor Teocrático - Código de restablecimiento de contraseña

Hola,

Tu código de restablecimiento de contraseña es: {resetCode}

Ingresa este código en la aplicación para restablecer tu contraseña.

IMPORTANTE: Este código expirará en 15 minutos por seguridad.

Si no solicitaste restablecer tu contraseña, puedes ignorar este email de forma segura.

© 2024 Gestor Teocrático. Todos los derechos reservados.
";
    }
}
