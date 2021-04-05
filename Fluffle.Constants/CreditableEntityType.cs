namespace Noppes.Fluffle.Constants
{
    /// <summary>
    /// Types of creditable entities. A creditable entity is an entity, like a person or company,
    /// which owns the copyright of a work or the entity which has uploaded the work. Credits should
    /// be given primarily to <see cref="Artist"/>, but sometimes this information is not available
    /// and the best next thing is the entity which uploaded the work, which is the <see cref="Owner"/>.
    /// </summary>
    public enum CreditableEntityType
    {
        Artist = 1,
        Owner = 2
    }
}
