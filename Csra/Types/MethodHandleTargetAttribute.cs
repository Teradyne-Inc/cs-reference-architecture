using System;

namespace Csra {

    /// <summary>
    /// Indicates that the target method is intended to be referenced via a method handle, such as a delegate.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class MethodHandleTargetAttribute : Attribute { }
}
