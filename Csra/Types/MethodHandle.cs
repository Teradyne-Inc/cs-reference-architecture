using System;
using System.Reflection;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using System.Linq;

namespace Csra {

    /// <summary>
    /// Provides a flexible and type-safe mechanism for dynamically resolving and invoking delegates
    /// based on the execution context (e.g., debug vs. production).
    /// </summary>
    /// <typeparam name="T">The delegate type. Must inherit from <see cref="Delegate"/>.</typeparam>
    [Serializable]
    public class MethodHandle<T> where T : Delegate {

        [NonSerialized]
        private T _inProcessDelegate = null;
        [NonSerialized]
        private T _outOfProcessDelegate = null;

        /// <summary>
        /// Gets the fully qualified name of the method to bind to the delegate.
        /// </summary>
        public string FullyQualifiedName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodHandle{T}"/> class.
        /// </summary>
        /// <param name="fullyQualifiedName"> The fully qualified name of the delegate to be created. ("Namespace.Class.Method")</param>
        public MethodHandle(string fullyQualifiedName) {
            FullyQualifiedName = fullyQualifiedName;
            _ = Execute; // touch the property to initialize the delegate
        }

        /// <summary>
        /// Gets the resolved delegate instance based on the current execution context.
        /// </summary>
        public T Execute => TheExec.Flow.RunOption[OptType.Debug] ? _outOfProcessDelegate ??= CreateDelegate() : _inProcessDelegate ??= CreateDelegate();

        private T CreateDelegate() {
            MethodInfo tInfo = typeof(T).GetMethod("Invoke");
            ParameterInfo[] tParams = tInfo.GetParameters();

            // Split fully qualified name into type and method
            int lastDot = FullyQualifiedName.LastIndexOf('.');
            if (lastDot < 0 || lastDot == FullyQualifiedName.Length - 1) {
                Api.Services.Alert.Error($"Invalid fully qualified name: '{FullyQualifiedName}'.");
                return default;
            }
            string typeName = FullyQualifiedName.Substring(0, lastDot);
            string methodName = FullyQualifiedName.Substring(lastDot + 1);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies) {
                Type type = assembly.GetType(typeName, false);
                if (type is null) continue;

                var candidateMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .Where(m => m.Name == methodName &&
                                m.IsDefined(typeof(MethodHandleTargetAttribute), false));

                foreach (var method in candidateMethods) {
                    if (method.ReturnType == tInfo.ReturnType &&
                        method.GetParameters().Select(p => p.ParameterType).SequenceEqual(tParams.Select(p => p.ParameterType))) {
                        return (T)(object)Delegate.CreateDelegate(typeof(T), method);
                    }
                }
            }

            Api.Services.Alert.Error($"Cannot find '{FullyQualifiedName}' with the correct prototype in the loaded assemblies.");
            return default;
        }
    }
}
