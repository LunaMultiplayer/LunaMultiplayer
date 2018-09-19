using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LmpCommon.FastMember
{
    /// <summary>
    /// Represents an abstracted view of the members defined for a type
    /// </summary>
    public sealed class MemberSet : IEnumerable<Member>, IList<Member>
    {
        private readonly Member[] _members;

        internal MemberSet(Type type) => _members = type.GetProperties().Concat(type.GetFields().Cast<MemberInfo>()).OrderBy(x => x.Name, StringComparer.InvariantCulture)
                .Select(member => new Member(member)).ToArray();
        
        /// <inheritdoc />
        /// <summary>
        /// Return a sequence of all defined members
        /// </summary>
        public IEnumerator<Member> GetEnumerator()
        {
            foreach (var member in _members) yield return member;
        }
        /// <summary>
        /// Get a member by index
        /// </summary>
        public Member this[int index] => _members[index];

        /// <inheritdoc />
        /// <summary>
        /// The number of members defined for this type
        /// </summary>
        public int Count => _members.Length;

        Member IList<Member>.this[int index]
        {
            get => _members[index];
            set => throw new NotSupportedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
        bool ICollection<Member>.Remove(Member item) { throw new NotSupportedException(); }
        void ICollection<Member>.Add(Member item) { throw new NotSupportedException(); }
        void ICollection<Member>.Clear() { throw new NotSupportedException(); }
        void IList<Member>.RemoveAt(int index) { throw new NotSupportedException(); }
        void IList<Member>.Insert(int index, Member item) { throw new NotSupportedException(); }

        bool ICollection<Member>.Contains(Member item) { return _members.Contains(item); }
        void ICollection<Member>.CopyTo(Member[] array, int arrayIndex) { _members.CopyTo(array, arrayIndex); }
        bool ICollection<Member>.IsReadOnly => true;
        int IList<Member>.IndexOf(Member member) { return Array.IndexOf<Member>(_members, member); }
        
    }
    /// <summary>
    /// Represents an abstracted view of an individual member defined for a type
    /// </summary>
    public sealed class Member
    {
        private readonly MemberInfo _member;
        internal Member(MemberInfo member) => _member = member;

        /// <summary>
        /// The name of this member
        /// </summary>
        public string Name => _member.Name;

        /// <summary>
        /// The type of value stored in this member
        /// </summary>
        public Type Type
        {
            get
            {
                switch (_member.MemberType)
                {
                    case MemberTypes.Field: return ((FieldInfo)_member).FieldType;
                    case MemberTypes.Property: return ((PropertyInfo)_member).PropertyType;
                    default: throw new NotSupportedException(_member.MemberType.ToString());
                }
            }
        }

        /// <summary>
        /// Is the attribute specified defined on this type
        /// </summary>
        public bool IsDefined(Type attributeType)
        {
            if (attributeType == null) throw new ArgumentNullException(nameof(attributeType));
            return Attribute.IsDefined(_member, attributeType);
        }
    }
}
