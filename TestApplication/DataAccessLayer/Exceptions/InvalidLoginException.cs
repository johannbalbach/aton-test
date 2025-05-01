namespace TestApplication.DataAccessLayer.Exceptions
{
    public class InvalidLoginException : Exception
    {
        public InvalidLoginException(string message) : base(message)
        {

        }
    }
}
