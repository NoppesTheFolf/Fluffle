namespace Noppes.Fluffle.Api.Mapping
{
    /// <summary>
    /// An interface that, when used, forces the the inheritor to implement a method that maps from
    /// <typeparamref name="TSrc"/> to <typeparamref name="TDest"/>. This method will automatically
    /// be called when <see cref="Mappers.MapTo{TDest}"/> is called on an object of type
    /// <typeparamref name="TSrc"/>. <see cref="Mappers.MapEnumerableTo{TDest}"/> should be used
    /// when an enumerable of instances needs to be mapped.
    /// </summary>
    public interface IMapper<in TSrc, in TDest>
    {
        public void MapFrom(TSrc src, TDest dest);
    }
}
