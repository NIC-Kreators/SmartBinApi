namespace SmartBin.Domain.Shared;

// ReSharper disable once TypeParameterCanBeVariant
public interface IEventCase<TInput>
{
    Task Execute(TInput input);
}
