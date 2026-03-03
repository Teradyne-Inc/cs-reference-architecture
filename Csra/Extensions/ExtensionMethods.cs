using System.Diagnostics;
using System.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra {

    public static class ExtensionMethods {

        /// <summary>
        /// Returns the single element of a sequence, or the element at the specified index if the sequence contains more than one element.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the source sequence.</typeparam>
        /// <param name="values">The sequence to return an element from.</param>
        /// <param name="index">The zero-based index of the element to retrieve if the sequence contains more than one element.</param>
        /// <returns>The single element of the sequence, or the element at the specified index if the sequence contains more than one element.</returns>
        
        public static T SingleOrAt<T>(this T[] values, int index) => values.Length == 1 ? values[0] : values[index];

        internal static bool IsUniform<T>(this Site<T> value, out T first) {
            T theFirst = value[TheExec.Sites.Selected.First()];
            first = theFirst; // can't use out parameter in lambda directly
            return value.All(s => s.Equals(theFirst));
        }
    }
}
