using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace uhttpsharp.Handlers
{
    public class ClassRouter : IHttpRequestHandler
    {
        private static readonly object SyncRoot = new object();

        private static readonly HashSet<Type> LoadedRoutes = new HashSet<Type>();

        private static readonly IDictionary<Tuple<Type, string>, Func<IHttpRequestHandler, IHttpRequestHandler>>
            Routers = new Dictionary<Tuple<Type, string>, Func<IHttpRequestHandler, IHttpRequestHandler>>();

        private static readonly ConcurrentDictionary<Type, Func<IHttpContext, IHttpRequestHandler, string, Task<IHttpRequestHandler>>>
            IndexerRouters = new ConcurrentDictionary<Type, Func<IHttpContext, IHttpRequestHandler, string, Task<IHttpRequestHandler>>>();

        private readonly IHttpRequestHandler _root;

        public ClassRouter(IHttpRequestHandler root)
        {
            _root = root;
            LoadRoute(_root);
        }

        private void LoadRoute(IHttpRequestHandler root)
        {
            var rootType = root.GetType();

            if (LoadedRoutes.Contains(rootType))
            {
                return;
            }

            lock (SyncRoot)
            {
                if (LoadedRoutes.Add(rootType))
                {
                    var routes = GetRoutesOfHandler(rootType);
                    foreach (var route in routes)
                    {
                        var tuple = Tuple.Create(rootType, route.Name);
                        var value = CreateRoute(tuple);
                        Routers.Add(tuple, value);
                    }
                }
            }
        }
        private IEnumerable<PropertyInfo> GetRoutesOfHandler(Type type)
        {
            return type
                   .GetProperties()
                   .Where(p => typeof(IHttpRequestHandler).IsAssignableFrom(p.PropertyType));
        }

        public async Task Handle(IHttpContext context, Func<Task> next)
        {
            var handler = _root;

            foreach (var parameter in context.Request.RequestParameters)
            {
                Func<IHttpRequestHandler, IHttpRequestHandler> getNextHandler;

                LoadRoute(handler);

                if (Routers.TryGetValue(Tuple.Create(handler.GetType(), parameter), out getNextHandler))
                {
                    handler = getNextHandler(handler);
                }
                else
                {
                    var getNextByIndex = IndexerRouters.GetOrAdd(handler.GetType(), GetIndexerRouter);

                    if (getNextByIndex == null) //Indexer is not found
                    {

                        await next().ConfigureAwait(false);
                        return;
                    }

                    var returnedTask = getNextByIndex(context, handler, parameter);

                    if (returnedTask == null) //Indexer found, but returned null (for whatever reason)
                    {
                        await next().ConfigureAwait(false);
                        return;
                    }

                    handler = await returnedTask.ConfigureAwait(false);
                }

                // Incase that one of the methods returned null (Indexer / Getter)
                if (handler == null)
                {
                    await next().ConfigureAwait(false);
                    return;
                }
            }

            await handler.Handle(context, next).ConfigureAwait(false);
        }

        private Func<IHttpContext, IHttpRequestHandler, string, Task<IHttpRequestHandler>> GetIndexerRouter(Type arg)
        {
            var indexer = GetIndexer(arg);

            if (indexer == null)
            {
                return null;
            }
            return CreateIndexerFunction<IHttpRequestHandler>(arg, indexer);
        }
        internal static Func<IHttpContext, T, string, Task<T>> CreateIndexerFunction<T>(Type arg, MethodInfo indexer)
        {
            var parameterType = indexer.GetParameters()[1].ParameterType;

            var httpContext = Expression.Parameter(typeof(IHttpContext), "context");
            var inputHandler = Expression.Parameter(typeof(T), "instance");
            var inputObject = Expression.Parameter(typeof(string), "input");

            var tryParseMethod = parameterType.GetMethod("TryParse", new[] {typeof(string), parameterType.MakeByRefType()});

            Expression body;

            if (tryParseMethod == null)
            {
                var handlerConverted = Expression.Convert(inputHandler, arg);
                var objectConverted =
                    Expression.Convert(
                        Expression.Call(typeof(Convert).GetMethod("ChangeType", new[] {typeof(object), typeof(Type)}), inputObject,
                            Expression.Constant(parameterType)), parameterType);

                var indexerExpression = Expression.Call (handlerConverted, indexer, httpContext, objectConverted);
                var returnValue = Expression.Convert (indexerExpression, typeof(T));

                body = returnValue;
            }
            else
            {
                var inputConvertedVar = Expression.Variable(parameterType, "inputObjectConverted");

                var handlerConverted = Expression.Convert(inputHandler, arg);
                var objectConverted = inputConvertedVar;

                var indexerExpression = Expression.Call(handlerConverted, indexer, httpContext, objectConverted);
                var returnValue = Expression.Convert(indexerExpression, typeof(Task<T>));
                var returnTarget = Expression.Label(typeof(Task<T>));
                var returnLabel = Expression.Label(returnTarget,
                    Expression.Convert(Expression.Constant(null), typeof(Task<T>)));
                body =
                    Expression.Block(
                        new[] {inputConvertedVar},
                        Expression.IfThen(
                            Expression.Call(tryParseMethod, inputObject,
                                inputConvertedVar),
                            Expression.Return(returnTarget, returnValue)
                            ),
                        returnLabel);
            }

            return
                Expression.Lambda<Func<IHttpContext, T, string, Task<T>>>(body, httpContext,
                    inputHandler,
                    inputObject).Compile();
        }
        private MethodInfo GetIndexer(Type arg)
        {
            var indexer =
                arg.GetMethods().SingleOrDefault(m => Attribute.IsDefined(m, typeof(IndexerAttribute))
                                             && m.GetParameters().Length == 2
                                             && typeof(Task<IHttpRequestHandler>).IsAssignableFrom(m.ReturnType));

            return indexer;
        }
        private Func<IHttpRequestHandler, IHttpRequestHandler> CreateRoute(Tuple<Type, string> arg)
        {
            var parameter = Expression.Parameter(typeof(IHttpRequestHandler), "input");
            var converted = Expression.Convert(parameter, arg.Item1);

            var propertyInfo = arg.Item1.GetProperty(arg.Item2);

            if (propertyInfo == null)
            {
                return null;
            }

            var property = Expression.Property(converted, propertyInfo);
            var propertyConverted = Expression.Convert(property, typeof(IHttpRequestHandler));

            return Expression.Lambda<Func<IHttpRequestHandler, IHttpRequestHandler>>(propertyConverted, new[] { parameter }).Compile();
        }
    }

    public class IndexerAttribute : Attribute
    {
    }
}
