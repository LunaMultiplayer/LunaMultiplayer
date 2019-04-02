using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace LmpCommon.FastMember
{
    /// <summary>
    /// Provides by-name member-access to objects of a given type
    /// </summary>
    public abstract class TypeAccessor
    {
        // hash-table has better read-without-locking semantics than dictionary
        private static readonly Hashtable PublicAccessorsOnly = new Hashtable(), NonPublicAccessors = new Hashtable();

        /// <summary>
        /// Does this type support new instances via a parameterless constructor?
        /// </summary>
        public virtual bool CreateNewSupported => false;

        /// <summary>
        /// Create a new instance of this type
        /// </summary>
        public virtual object CreateNew() { throw new NotSupportedException(); }

        /// <summary>
        /// Can this type be queried for member availability?
        /// </summary>
        public virtual bool GetMembersSupported => false;

        /// <summary>
        /// Query the members available for this type
        /// </summary>
        public virtual MemberSet GetMembers() { throw new NotSupportedException(); }

        /// <summary>
        /// Provides a type-specific accessor, allowing by-name access for all objects of that type
        /// </summary>
        /// <remarks>The accessor is cached internally; a pre-existing accessor may be returned</remarks>
        public static TypeAccessor Create(Type type)
        {
            return Create(type, false);
        }

        /// <summary>
        /// Provides a type-specific accessor, allowing by-name access for all objects of that type
        /// </summary>
        /// <remarks>The accessor is cached internally; a pre-existing accessor may be returned</remarks>
        public static TypeAccessor Create(Type type, bool allowNonPublicAccessors)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            var lookup = allowNonPublicAccessors ? NonPublicAccessors : PublicAccessorsOnly;
            var obj = (TypeAccessor)lookup[type];
            if (obj != null) return obj;

            lock (lookup)
            {
                // double-check
                obj = (TypeAccessor)lookup[type];
                if (obj != null) return obj;

                obj = CreateNew(type, allowNonPublicAccessors);

                lookup[type] = obj;
                return obj;
            }
        }

        private static AssemblyBuilder _assembly;
        private static ModuleBuilder _module;
        private static int _counter;

        private static readonly MethodInfo TryGetValue = typeof(Dictionary<string, int>).GetMethod("TryGetValue");
        private static void WriteMapImpl(ILGenerator il, Type type, List<MemberInfo> members, FieldBuilder mapField, bool allowNonPublicAccessors, bool isGet)
        {
            OpCode obj, index, value;

            var fail = il.DefineLabel();
            if (mapField == null)
            {
                index = OpCodes.Ldarg_0;
                obj = OpCodes.Ldarg_1;
                value = OpCodes.Ldarg_2;
            }
            else
            {
                il.DeclareLocal(typeof(int));
                index = OpCodes.Ldloc_0;
                obj = OpCodes.Ldarg_1;
                value = OpCodes.Ldarg_3;

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, mapField);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Ldloca_S, (byte)0);
                il.EmitCall(OpCodes.Callvirt, TryGetValue, null);
                il.Emit(OpCodes.Brfalse, fail);
            }
            var labels = new Label[members.Count];
            for (var i = 0; i < labels.Length; i++)
            {
                labels[i] = il.DefineLabel();
            }
            il.Emit(index);
            il.Emit(OpCodes.Switch, labels);
            il.MarkLabel(fail);
            il.Emit(OpCodes.Ldstr, "name");
            il.Emit(OpCodes.Newobj, typeof(ArgumentOutOfRangeException).GetConstructor(new Type[] { typeof(string) }));
            il.Emit(OpCodes.Throw);
            for (var i = 0; i < labels.Length; i++)
            {
                il.MarkLabel(labels[i]);
                var member = members[i];
                var isFail = true;
                switch (member.MemberType)
                {
                    case MemberTypes.Field:
                        var field = (FieldInfo)member;
                        il.Emit(obj);
                        Cast(il, type, true);
                        if (isGet)
                        {
                            il.Emit(OpCodes.Ldfld, field);
                            if (field.FieldType.IsValueType) il.Emit(OpCodes.Box, field.FieldType);
                        }
                        else
                        {
                            il.Emit(value);
                            Cast(il, field.FieldType, false);
                            il.Emit(OpCodes.Stfld, field);
                        }
                        il.Emit(OpCodes.Ret);
                        isFail = false;
                        break;
                    case MemberTypes.Property:
                        var prop = (PropertyInfo)member;
                        MethodInfo accessor;
                        if (prop.CanRead && (accessor = isGet ? prop.GetGetMethod(allowNonPublicAccessors) : prop.GetSetMethod(allowNonPublicAccessors)) != null)
                        {
                            il.Emit(obj);
                            Cast(il, type, true);
                            if (isGet)
                            {
                                il.EmitCall(type.IsValueType ? OpCodes.Call : OpCodes.Callvirt, accessor, null);
                                if (prop.PropertyType.IsValueType) il.Emit(OpCodes.Box, prop.PropertyType);
                            }
                            else
                            {
                                il.Emit(value);
                                Cast(il, prop.PropertyType, false);
                                il.EmitCall(type.IsValueType ? OpCodes.Call : OpCodes.Callvirt, accessor, null);
                            }
                            il.Emit(OpCodes.Ret);
                            isFail = false;
                        }
                        break;
                }
                if (isFail) il.Emit(OpCodes.Br, fail);
            }
        }

        private static readonly MethodInfo StrinqEquals = typeof(string).GetMethod("op_Equality", new Type[] { typeof(string), typeof(string) });

        /// <summary>
        /// A TypeAccessor based on a Type implementation, with available member metadata
        /// </summary>
        protected abstract class RuntimeTypeAccessor : TypeAccessor
        {
            /// <summary>
            /// Returns the Type represented by this accessor
            /// </summary>
            protected abstract Type Type { get; }

            /// <summary>
            /// Can this type be queried for member availability?
            /// </summary>
            public override bool GetMembersSupported => true;

            private MemberSet _members;
            /// <summary>
            /// Query the members available for this type
            /// </summary>
            public override MemberSet GetMembers()
            {
                return _members ?? (_members = new MemberSet(Type));
            }
        }

        private sealed class DelegateAccessor : RuntimeTypeAccessor
        {
            private readonly Dictionary<string, int> _map;
            private readonly Func<int, object, object> _getter;
            private readonly Action<int, object, object> _setter;
            private readonly Func<object> _ctor;
            private readonly Type _type;
            protected override Type Type => _type;

            public DelegateAccessor(Dictionary<string, int> map, Func<int, object, object> getter, Action<int, object, object> setter, Func<object> ctor, Type type)
            {
                _map = map;
                _getter = getter;
                _setter = setter;
                _ctor = ctor;
                _type = type;
            }
            public override bool CreateNewSupported => _ctor != null;

            public override object CreateNew()
            {
                return _ctor != null ? _ctor() : base.CreateNew();
            }
            public override object this[object target, string name]
            {
                get
                {
                    if (_map.TryGetValue(name, out var index)) return _getter(index, target);
                    throw new ArgumentOutOfRangeException(nameof(name));
                }
                set
                {
                    if (_map.TryGetValue(name, out var index)) _setter(index, target, value);
                    else throw new ArgumentOutOfRangeException(nameof(name));
                }
            }
        }
        private static bool IsFullyPublic(Type type, PropertyInfo[] props, bool allowNonPublicAccessors)
        {
            while (type.IsNestedPublic) type = type.DeclaringType;
            if (!type.IsPublic) return false;

            if (allowNonPublicAccessors)
            {
                for (var i = 0; i < props.Length; i++)
                {
                    if (props[i].GetGetMethod(true) != null && props[i].GetGetMethod(false) == null) return false; // non-public getter
                    if (props[i].GetSetMethod(true) != null && props[i].GetSetMethod(false) == null) return false; // non-public setter
                }
            }

            return true;
        }

        private static TypeAccessor CreateNew(Type type, bool allowNonPublicAccessors)
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var map = new Dictionary<string, int>(StringComparer.Ordinal);
            var members = new List<MemberInfo>(props.Length + fields.Length);
            var i = 0;
            foreach (var prop in props)
            {
                if (!map.ContainsKey(prop.Name) && prop.GetIndexParameters().Length == 0)
                {
                    map.Add(prop.Name, i++);
                    members.Add(prop);
                }
            }
            foreach (var field in fields) if (!map.ContainsKey(field.Name)) { map.Add(field.Name, i++); members.Add(field); }

            ConstructorInfo ctor = null;
            if (type.IsClass && !type.IsAbstract)
            {
                ctor = type.GetConstructor(Type.EmptyTypes);
            }
            ILGenerator il;
            if (!IsFullyPublic(type, props, allowNonPublicAccessors))
            {
                DynamicMethod dynGetter = new DynamicMethod(type.FullName + "_get", typeof(object), new Type[] { typeof(int), typeof(object) }, type, true),
                              dynSetter = new DynamicMethod(type.FullName + "_set", null, new Type[] { typeof(int), typeof(object), typeof(object) }, type, true);
                WriteMapImpl(dynGetter.GetILGenerator(), type, members, null, allowNonPublicAccessors, true);
                WriteMapImpl(dynSetter.GetILGenerator(), type, members, null, allowNonPublicAccessors, false);
                DynamicMethod dynCtor = null;
                if (ctor != null)
                {
                    dynCtor = new DynamicMethod(type.FullName + "_ctor", typeof(object), Type.EmptyTypes, type, true);
                    il = dynCtor.GetILGenerator();
                    il.Emit(OpCodes.Newobj, ctor);
                    il.Emit(OpCodes.Ret);
                }
                return new DelegateAccessor(
                    map,
                    (Func<int, object, object>)dynGetter.CreateDelegate(typeof(Func<int, object, object>)),
                    (Action<int, object, object>)dynSetter.CreateDelegate(typeof(Action<int, object, object>)),
                    dynCtor == null ? null : (Func<object>)dynCtor.CreateDelegate(typeof(Func<object>)), type);
            }

            // note this region is synchronized; only one is being created at a time so we don't need to stress about the builders
            if (_assembly == null)
            {
                var name = new AssemblyName("FastMember_dynamic");
                _assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
                _module = _assembly.DefineDynamicModule(name.Name);
            }
            var tb = _module.DefineType("FastMember_dynamic." + type.Name + "_" + Interlocked.Increment(ref _counter),
                (typeof(TypeAccessor).Attributes | TypeAttributes.Sealed | TypeAttributes.Public) & ~(TypeAttributes.Abstract | TypeAttributes.NotPublic), typeof(RuntimeTypeAccessor));

            il = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] {
                typeof(Dictionary<string,int>)
            }).GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            var mapField = tb.DefineField("_map", typeof(Dictionary<string, int>), FieldAttributes.InitOnly | FieldAttributes.Private);
            il.Emit(OpCodes.Stfld, mapField);
            il.Emit(OpCodes.Ret);


            var indexer = typeof(TypeAccessor).GetProperty("Item");
            MethodInfo baseGetter = indexer.GetGetMethod(), baseSetter = indexer.GetSetMethod();
            var body = tb.DefineMethod(baseGetter.Name, baseGetter.Attributes & ~MethodAttributes.Abstract, typeof(object), new Type[] { typeof(object), typeof(string) });
            il = body.GetILGenerator();
            WriteMapImpl(il, type, members, mapField, allowNonPublicAccessors, true);
            tb.DefineMethodOverride(body, baseGetter);

            body = tb.DefineMethod(baseSetter.Name, baseSetter.Attributes & ~MethodAttributes.Abstract, null, new Type[] { typeof(object), typeof(string), typeof(object) });
            il = body.GetILGenerator();
            WriteMapImpl(il, type, members, mapField, allowNonPublicAccessors, false);
            tb.DefineMethodOverride(body, baseSetter);

            MethodInfo baseMethod;
            if (ctor != null)
            {
                baseMethod = typeof(TypeAccessor).GetProperty("CreateNewSupported").GetGetMethod();
                body = tb.DefineMethod(baseMethod.Name, baseMethod.Attributes, baseMethod.ReturnType, Type.EmptyTypes);
                il = body.GetILGenerator();
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Ret);
                tb.DefineMethodOverride(body, baseMethod);

                baseMethod = typeof(TypeAccessor).GetMethod("CreateNew");
                body = tb.DefineMethod(baseMethod.Name, baseMethod.Attributes, baseMethod.ReturnType, Type.EmptyTypes);
                il = body.GetILGenerator();
                il.Emit(OpCodes.Newobj, ctor);
                il.Emit(OpCodes.Ret);
                tb.DefineMethodOverride(body, baseMethod);
            }

            baseMethod = typeof(RuntimeTypeAccessor).GetProperty("Type", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
            body = tb.DefineMethod(baseMethod.Name, baseMethod.Attributes & ~MethodAttributes.Abstract, baseMethod.ReturnType, Type.EmptyTypes);
            il = body.GetILGenerator();
            il.Emit(OpCodes.Ldtoken, type);
            il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
            il.Emit(OpCodes.Ret);
            tb.DefineMethodOverride(body, baseMethod);

            var accessor = (TypeAccessor)Activator.CreateInstance(tb.CreateType(), map);
            return accessor;
        }

        private static void Cast(ILGenerator il, Type type, bool valueAsPointer)
        {
            if (type == typeof(object)) { }
            else if (type.IsValueType)
            {
                if (valueAsPointer)
                {
                    il.Emit(OpCodes.Unbox, type);
                }
                else
                {
                    il.Emit(OpCodes.Unbox_Any, type);
                }
            }
            else
            {
                il.Emit(OpCodes.Castclass, type);
            }
        }

        /// <summary>
        /// Get or set the value of a named member on the target instance
        /// </summary>
        public abstract object this[object target, string name]
        {
            get;
            set;
        }
    }
}
