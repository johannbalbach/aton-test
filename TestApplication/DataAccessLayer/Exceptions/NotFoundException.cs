namespace TestApplication.DataAccessLayer.Exceptions
{
    public class NotFoundException: Exception
    {
        public NotFoundException(string message): base(message) 
        {

        }
    }
}
