namespace System
{
    /// <summary>
    /// Extensions for <see cref="Tuple"/>
    /// </summary>
    internal static class TupleExtensions
    {
        /// <summary>
        /// Writes Tuple data to a formatted string
        /// </summary>
        /// <typeparam name="T1">The Type of the first element of the Tuple</typeparam>
        /// <typeparam name="T2">The Type of the second element of the Tuple</typeparam>
        /// <param name="source">The Tuple source</param>
        /// <param name="Format">The string format</param>
        /// <returns></returns>
        public static string ToString<T1, T2>(this (T1, T2) source, string Format)
            => string.Format(Format, source.Item1, source.Item2);
    }
}
