using System;

namespace LunaCommon.Pointer
{
    public class Ptr<T>
    {
        private readonly Func<T> _getter;
        private readonly Action<T> _setter;

        public Ptr(Func<T> g, Action<T> s)
        {
            _getter = g;
            _setter = s;
        }

        public T Deref
        {
            get => _getter();
            set => _setter(value);
        }
    }

    //public class Example
    //{
    //    public void Main()
    //    {
    //        var x = 0;
    //        var xPtr = new Ptr<int>(() => x, v => x = v); // &x

    //        xPtr.Deref = 1;
    //        var a = xPtr.Deref;
    //    }
    //}
}
