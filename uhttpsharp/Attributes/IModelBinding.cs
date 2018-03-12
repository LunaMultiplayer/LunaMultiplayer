using System;
using System.Collections.Generic;
using uhttpsharp.ModelBinders;

namespace uhttpsharp.Attributes
{
    internal interface IModelBinding
    {
        T Get<T>(IHttpContext context, IModelBinder binder);
    }

    public class FromStateAttribute : Attribute, IModelBinding
    {
        private readonly string _propertyName;
        public FromStateAttribute(string propertyName)
        {
            _propertyName = propertyName;
        }
        public T Get<T>(IHttpContext context, IModelBinder binder)
        {
            // Expando object
            var state = (context.State as IDictionary<string,object>);
            object real;
            if (state != null && state.TryGetValue(_propertyName, out real) && real is T)
            {
                    return (T)real;
            }

            return default(T);
        }
    }

    public class FromBodyAttribute : PrefixAttribute
    {
        public FromBodyAttribute(string prefix = null) : base(prefix)
        {
            
        }

        public override T Get<T>(IHttpContext context, IModelBinder binder)
        {
            return binder.Get<T>(context.Request.Post.Raw, Prefix);
        }
    }

    public class FromPostAttribute : PrefixAttribute
    {
        public FromPostAttribute(string prefix = null)
            : base(prefix)
        {
        }
        public override T Get<T>(IHttpContext context, IModelBinder binder)
        {
            return binder.Get<T>(context.Request.Post.Parsed, Prefix);
        }
    }

    public class FromQueryAttribute : PrefixAttribute
    {
        public FromQueryAttribute(string prefix)
            : base(prefix)
        {
        }
        public override T Get<T>(IHttpContext context, IModelBinder binder)
        {
            return binder.Get<T>(context.Request.QueryString, Prefix);
        }
    }

    public class FromHeadersAttribute : PrefixAttribute
    {
        public FromHeadersAttribute(string prefix)
            : base(prefix)
        {
        }

        public override T Get<T>(IHttpContext context, IModelBinder binder)
        {
            return binder.Get<T>(context.Request.Headers, Prefix);
        }
    }

    public abstract class PrefixAttribute : Attribute, IModelBinding
    {
        private readonly string _prefix;

        public PrefixAttribute(string prefix)
        {
            _prefix = prefix;
        }

        public bool HasPrefix
        {
            get { return !string.IsNullOrEmpty(_prefix); }
        }

        public string Prefix
        {
            get { return _prefix; }
        }

        public abstract T Get<T>(IHttpContext context, IModelBinder binder);
    }
}
