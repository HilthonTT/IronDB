namespace IronDB.Common.Utils;

public interface IValidator<T>
{
    void Validate(T t);
}
