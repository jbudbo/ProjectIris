namespace System
{
    internal static class TupleExtensions
    {
        public static string ToString<T1, T2>(this (T1, T2) source, string Format)
            => string.Format(Format, source.Item1, source.Item2);
    }
}
