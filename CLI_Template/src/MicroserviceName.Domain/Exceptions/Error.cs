namespace MicroserviceName.Domain.Exceptions
{
    public class Error
    {
        public Error(Severity severity, string code, string detail)
        {
            Severity = severity;
            Code = code;
            Detail = detail;
        }

        public Severity Severity { get; }
        public string Code { get; }
        public string Detail { get; }
    }
}
