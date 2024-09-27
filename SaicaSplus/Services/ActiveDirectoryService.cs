using System.DirectoryServices.AccountManagement;

public class ActiveDirectoryService
{
    public bool ValidateUser(string domain, string username, string password)
    {
        using (var context = new PrincipalContext(ContextType.Domain, domain))
        {
            return context.ValidateCredentials(username, password);
        }
    }
}
