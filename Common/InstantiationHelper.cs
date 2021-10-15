using System;

namespace Filuet.Hardware.CashAcceptors.Common
{
    public static class InstantiationHelper
    {
        /// <summary>
        /// Creates an instance of type, invokes the given action on it and returns it.
        /// </summary>
        /// <typeparam name="T">The type of action argument. Must be a reference type and have
        /// a public default constructor.</typeparam>
        /// <param name="action">The action to invoke.</param>
        /// <returns>The created instance of <paramref name="T"/>.</returns>
        public static T CreateTargetAndInvoke<T>(this Action<T> action)
            where T : new()
        {
            T target = new T();

            action.Invoke(target);

            return target;
        }

        public static T CreateTargetAndInvoke<T>(this Func<T, T> action)
            where T : new()
        {
            T target = new T();

            target = action.Invoke(target);

            return target;
        }

        public static T CreateTargetAndInvoke<T, TImpl>(this Action<T> action)
            where TImpl : T, new()
        {
            T target = new TImpl();

            action.Invoke(target);

            return target;
        }

        public static TOutput CreateTargetAndInvoke<T, TInput, TOutput>(this Func<T, TOutput> func)
            where TInput : T, new()
        {
            T target = new TInput();

            return func.Invoke(target);
        }

        public static T InvokeAndReturn<T>(this Action<T> action, T target)
            where T : new()
        {
            action.Invoke(target);

            return target;
        }
    }
}
