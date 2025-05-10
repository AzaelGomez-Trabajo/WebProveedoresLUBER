using WebProveedoresN.Models;

namespace WebProveedoresN.Repositories.Interfaces
{
    public interface IEmailRepository
    {
        bool SendEmail(EmailModel email, string name);
    }
}
