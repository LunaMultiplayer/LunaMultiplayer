using System;

namespace LunaCommon.FastMember
{
    /// <summary>
    /// Represents an individual object, allowing access to members by-name
    /// </summary>
    public abstract class ObjectAccessor
    {
        /// <summary>
        /// Get or Set the value of a named member for the underlying object
        /// </summary>
        public abstract object this[string name] { get; set; }
        /// <summary>
        /// The object represented by this instance
        /// </summary>
        public abstract object Target { get; }
        /// <summary>
        /// Use the target types definition of equality
        /// </summary>
        public override bool Equals(object obj)
        {
            return Target.Equals(obj);
        }
        /// <summary>
        /// Obtain the hash of the target object
        /// </summary>
        public override int GetHashCode()
        {
            return Target.GetHashCode();
        }
        /// <summary>
        /// Use the target's definition of a string representation
        /// </summary>
        public override string ToString()
        {
            return Target.ToString();
        }

        /// <summary>
        /// Wraps an individual object, allowing by-name access to that instance
        /// </summary>
        public static ObjectAccessor Create(object target)
        {
            return Create(target, false);
        }
        /// <summary>
        /// Wraps an individual object, allowing by-name access to that instance
        /// </summary>
        public static ObjectAccessor Create(object target, bool allowNonPublicAccessors)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            return new TypeAccessorWrapper(target, TypeAccessor.Create(target.GetType()));
        }

        private sealed class TypeAccessorWrapper : ObjectAccessor
        {
            private readonly object _target;
            private readonly TypeAccessor _accessor;
            public TypeAccessorWrapper(object target, TypeAccessor accessor)
            {
                _target = target;
                _accessor = accessor;
            }
            public override object this[string name]
            {
                get => _accessor[_target, name];
                set => _accessor[_target, name] = value;
            }
            public override object Target => _target;
        }
    }

}
