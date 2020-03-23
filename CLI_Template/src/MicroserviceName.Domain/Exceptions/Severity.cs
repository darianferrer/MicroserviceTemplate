namespace MicroserviceName.Domain.Exceptions
{
    public enum Severity : byte
    {
        Correctable,
        Unrecoverable,
        Unexpected,
    }
}
