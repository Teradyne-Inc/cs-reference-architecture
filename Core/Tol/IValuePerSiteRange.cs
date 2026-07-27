using System;
using Teradyne.Igxl.Interfaces.Public;

namespace Tol {

    /// <summary>
    /// Range interface defining minimum and maximum values.
    /// </summary>
    public interface IRange {
        /// <summary>
        /// Gets the minimum value of the range.
        /// </summary>
        public double Min { get; }
        /// <summary>
        /// Gets the maximum value of the range.
        /// </summary>
        public double Max { get; }
    }

    /// <summary>
    /// Value interface with support for site-specific values.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IValuePerSite<T> {
        /// <summary>
        /// Sets the value, for all sites.
        /// </summary>
        public T Value { set; }

        /// <summary>
        /// Sets the value associated with each site.
        /// </summary>
        public Site<T> ValuePerSite { set; }
    }

    [Serializable]
    internal class ValuePerSiteType<T> : IValuePerSite<T> {
        [NonSerialized]
        private readonly Action<T> _setValue;
        [NonSerialized]
        private readonly Action<Site<T>> _setValuePerSite;
        public ValuePerSiteType(Action<T> setValue, Action<Site<T>> setValuePerSite) {
            _setValue = setValue;
            _setValuePerSite = setValuePerSite;
        }
        public T Value {
            set => _setValue(value);
        }
        public Site<T> ValuePerSite {
            set => _setValuePerSite(value);
        }
    }

    /// <summary>
    /// Auto range settings interface with support for site-specific values.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IAutoValuePerSiteRange<T> : IValuePerSite<T>, IRange {
        /// <summary>
        /// Sets the value, for all sites.
        /// </summary>
        public bool Auto { set; }

        /// <summary>
        /// Sets the value associated with each site.
        /// </summary>
        public Site<bool> AutoPerSite { set; }
    }

    /// <summary>
    /// Defines settings for a value with a specified range.
    /// </summary>
    /// <typeparam name="T">The type of the value the settings applies to.</typeparam>
    public interface IValuePerSiteRange<T> : IValuePerSite<T>, IRange { }

    [Serializable]
    internal class ValuePerSiteRangeType<T> : ValuePerSiteType<T>, IValuePerSiteRange<T> {
        [NonSerialized]
        private readonly Func<double> _getMin;
        [NonSerialized]
        private readonly Func<double> _getMax;
        public ValuePerSiteRangeType(
            Action<T> setValue,
            Action<Site<T>> setValuePerSite,
            Func<double> getMin,
            Func<double> getMax) : base(setValue, setValuePerSite) {
            _getMin = getMin;
            _getMax = getMax;
        }
        public double Min => _getMin();
        public double Max => _getMax();
    }
}
