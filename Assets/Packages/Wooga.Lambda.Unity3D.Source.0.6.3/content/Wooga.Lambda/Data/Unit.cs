namespace Wooga.Lambda.Data
{
    /// <summary>
    ///     Represents void as a value type
    /// </summary>
    public sealed class Unit
    {
        public static readonly Unit Default = new Unit();

        private Unit()
        {
        }
    }
}