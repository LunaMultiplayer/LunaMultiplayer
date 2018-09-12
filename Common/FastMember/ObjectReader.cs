using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace LunaCommon.FastMember
{
    /// <summary>
    /// Provides a means of reading a sequence of objects as a data-reader, for example
    /// for use with SqlBulkCopy or other data-base oriented code
    /// </summary>
    public class ObjectReader : IDataReader
    {
        private IEnumerator _source;
        private readonly TypeAccessor _accessor;
        private readonly string[] _memberNames;
        private readonly Type[] _effectiveTypes;
        private readonly BitArray _allowNull;

        /// <summary>
        /// Creates a new ObjectReader instance for reading the supplied data
        /// </summary>
        /// <param name="source">The sequence of objects to represent</param>
        /// <param name="members">The members that should be exposed to the reader</param>
        public static ObjectReader Create<T>(IEnumerable<T> source, params string[] members)
        {
            return new ObjectReader(typeof(T), source, members);
        }

        /// <summary>
        /// Creates a new ObjectReader instance for reading the supplied data
        /// </summary>
        /// <param name="type">The expected Type of the information to be read</param>
        /// <param name="source">The sequence of objects to represent</param>
        /// <param name="members">The members that should be exposed to the reader</param>
        public ObjectReader(Type type, IEnumerable source, params string[] members)
        {
            if (source == null) throw new ArgumentOutOfRangeException(nameof(source));
            
            var allMembers = members == null || members.Length == 0;

            _accessor = TypeAccessor.Create(type);
            if (_accessor.GetMembersSupported)
            {
                var typeMembers = _accessor.GetMembers();

                if (allMembers)
                {
                    members = new string[typeMembers.Count];
                    for (var i = 0; i < members.Length; i++)
                    {
                        members[i] = typeMembers[i].Name;
                    }
                }

                _allowNull = new BitArray(members.Length);
                _effectiveTypes = new Type[members.Length];
                for (var i = 0; i < members.Length; i++)
                {
                    Type memberType = null;
                    var allowNull = true;
                    var hunt = members[i];
                    foreach (var member in typeMembers)
                    {
                        if (member.Name == hunt)
                        {
                            if (memberType == null)
                            {
                                var tmp = member.Type;
                                memberType = Nullable.GetUnderlyingType(tmp) ?? tmp;

                                allowNull = !(memberType.IsValueType && memberType == tmp);

                                // but keep checking, in case of duplicates
                            }
                            else
                            {
                                memberType = null; // duplicate found; say nothing
                                break;
                            }
                        }
                    }
                    _allowNull[i] = allowNull;
                    _effectiveTypes[i] = memberType ?? typeof(object);
                }
            }
            else if (allMembers)
            {
                throw new InvalidOperationException("Member information is not available for this type; the required members must be specified explicitly");
            }

            _current = null;
            _memberNames = (string[])members.Clone();

            _source = source.GetEnumerator();
        }

        private object _current;
        void IDataReader.Close()
        {
            Dispose();
        }

        int IDataReader.Depth => 0;

        DataTable IDataReader.GetSchemaTable()
        {
            // these are the columns used by DataTable load
            var table = new DataTable
            {
                Columns =
                {
                    {"ColumnOrdinal", typeof(int)},
                    {"ColumnName", typeof(string)},
                    {"DataType", typeof(Type)},
                    {"ColumnSize", typeof(int)},
                    {"AllowDBNull", typeof(bool)}
                }
            };
            var rowData = new object[5];
            for (var i = 0; i < _memberNames.Length; i++)
            {
                rowData[0] = i;
                rowData[1] = _memberNames[i];
                rowData[2] = _effectiveTypes == null ? typeof(object) : _effectiveTypes[i];
                rowData[3] = -1;
                rowData[4] = _allowNull == null ? true : _allowNull[i];
                table.Rows.Add(rowData);
            }
            return table;
        }

        bool IDataReader.IsClosed => _source == null;

        bool IDataReader.NextResult()
        {
            return false;
        }

        bool IDataReader.Read()
        {
            var tmp = _source;
            if (tmp != null && tmp.MoveNext())
            {
                _current = tmp.Current;
                return true;
            }
            _current = null;
            return false;
        }

        int IDataReader.RecordsAffected => 0;

        /// <summary>
        /// Releases all resources used by the ObjectReader
        /// </summary>
        public void Dispose()
        {
            _current = null;
            _source = null;
            if (_source is IDisposable tmp) tmp.Dispose();
        }

        int IDataRecord.FieldCount => _memberNames.Length;

        bool IDataRecord.GetBoolean(int i)
        {
            return (bool)this[i];
        }

        byte IDataRecord.GetByte(int i)
        {
            return (byte)this[i];
        }

        long IDataRecord.GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            var s = (byte[])this[i];
            var available = s.Length - (int)fieldOffset;
            if (available <= 0) return 0;

            var count = Math.Min(length, available);
            Buffer.BlockCopy(s, (int)fieldOffset, buffer, bufferoffset, count);
            return count;
        }

        char IDataRecord.GetChar(int i)
        {
            return (char)this[i];
        }

        long IDataRecord.GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            var s = (string)this[i];
            var available = s.Length - (int)fieldoffset;
            if (available <= 0) return 0;

            var count = Math.Min(length, available);
            s.CopyTo((int)fieldoffset, buffer, bufferoffset, count);
            return count;
        }

        IDataReader IDataRecord.GetData(int i)
        {
            throw new NotSupportedException();
        }

        string IDataRecord.GetDataTypeName(int i)
        {
            return (_effectiveTypes == null ? typeof(object) : _effectiveTypes[i]).Name;
        }

        DateTime IDataRecord.GetDateTime(int i)
        {
            return (DateTime)this[i];
        }

        decimal IDataRecord.GetDecimal(int i)
        {
            return (decimal)this[i];
        }

        double IDataRecord.GetDouble(int i)
        {
            return (double)this[i];
        }

        Type IDataRecord.GetFieldType(int i)
        {
            return _effectiveTypes == null ? typeof(object) : _effectiveTypes[i];
        }

        float IDataRecord.GetFloat(int i)
        {
            return (float)this[i];
        }

        Guid IDataRecord.GetGuid(int i)
        {
            return (Guid)this[i];
        }

        short IDataRecord.GetInt16(int i)
        {
            return (short)this[i];
        }

        int IDataRecord.GetInt32(int i)
        {
            return (int)this[i];
        }

        long IDataRecord.GetInt64(int i)
        {
            return (long)this[i];
        }

        string IDataRecord.GetName(int i)
        {
            return _memberNames[i];
        }

        int IDataRecord.GetOrdinal(string name)
        {
            return Array.IndexOf(_memberNames, name);
        }

        string IDataRecord.GetString(int i)
        {
            return (string)this[i];
        }

        object IDataRecord.GetValue(int i)
        {
            return this[i];
        }

        int IDataRecord.GetValues(object[] values)
        {
            // duplicate the key fields on the stack
            var members = _memberNames;
            var current = _current;
            var accessor = _accessor;

            var count = Math.Min(values.Length, members.Length);
            for (var i = 0; i < count; i++) values[i] = accessor[current, members[i]] ?? DBNull.Value;
            return count;
        }

        bool IDataRecord.IsDBNull(int i)
        {
            return this[i] is DBNull;
        }

        object IDataRecord.this[string name] => _accessor[_current, name] ?? DBNull.Value;

        /// <inheritdoc />
        /// <summary>
        /// Gets the value of the current object in the member specified
        /// </summary>
        public object this[int i] => _accessor[_current, _memberNames[i]] ?? DBNull.Value;
    }
}
