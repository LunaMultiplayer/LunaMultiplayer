using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using uhttpsharp.Attributes;
using uhttpsharp.Controllers;
using uhttpsharp.ModelBinders;

namespace uhttpsharp.Handlers
{

    /// <summary>
    /// Need some kind of way to prevent default behavior of controller that inherits a base controller...
    /// since we are not using virtual methods 
    /// </summary>
    public class ControllerHandler : IHttpRequestHandler
    {
        sealed class ControllerMethod
        {
            private readonly Type _controllerType;
            private readonly HttpMethods _method;

            public ControllerMethod(Type controllerType, HttpMethods method)
            {
                _controllerType = controllerType;
                _method = method;
            }

            public Type ControllerType
            {
                get { return _controllerType; }
            }
            public HttpMethods Method
            {
                get { return _method; }
            }

            private bool Equals(ControllerMethod other)
            {
                return _controllerType == other._controllerType && _method == other._method;
            }
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is ControllerMethod && Equals((ControllerMethod)obj);
            }
            public override int GetHashCode()
            {
                unchecked
                {
                    return (_controllerType.GetHashCode() * 397) ^ (int)_method;
                }
            }
        }
        sealed class ControllerRoute
        {
            private readonly Type _controllerType;
            private readonly string _propertyName;
            public ControllerRoute(Type controllerType, string propertyName)
            {
                _controllerType = controllerType;
                _propertyName = propertyName;
            }
            private bool Equals(ControllerRoute other)
            {
                return _controllerType == other._controllerType && string.Equals(_propertyName, other._propertyName, StringComparison.InvariantCultureIgnoreCase);
            }
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is ControllerRoute && Equals((ControllerRoute)obj);
            }
            public override int GetHashCode()
            {
                unchecked
                {
                    return ((_controllerType != null ? _controllerType.GetHashCode() : 0) * 397) ^ (_propertyName != null ? _propertyName.ToLowerInvariant().GetHashCode() : 0);
                }
            }
        }

        private static readonly IDictionary<ControllerMethod, ControllerFunction> ControllerFunctions = new Dictionary<ControllerMethod, ControllerFunction>();

        private static readonly IDictionary<ControllerRoute, Func<IController, IController>> Routes = new Dictionary<ControllerRoute, Func<IController, IController>>();

        private static readonly IDictionary<Type, Func<IHttpContext, IController, string, Task<IController>>> IndexerRoutes = new Dictionary<Type, Func<IHttpContext, IController, string, Task<IController>>>();

        private static readonly ICollection<Type> LoadedControllerRoutes = new HashSet<Type>();

        private static readonly object SyncRoot = new object();

        public delegate Task<IControllerResponse> ControllerFunction(IHttpContext context, IModelBinder binder, IController controller);

        private readonly IController _controller;
        private readonly IModelBinder _modelBinder;
        private readonly IView _view;

        public ControllerHandler(IController controller, IModelBinder modelBinder, IView view)
        {
            _controller = controller;
            _modelBinder = modelBinder;
            _view = view;
        }
        protected virtual IModelBinder ModelBinder
        {
            get { return _modelBinder; }
        }

        private static Func<IController, IController> GenerateRouteFunction(MethodInfo getter)
        {
            var instance = Expression.Parameter(typeof(IController), "instance");
            return Expression.Lambda<Func<IController, IController>>(Expression.Call(Expression.Convert(instance, getter.DeclaringType), getter), instance).Compile();
        }
        private static void LoadRoutes(Type controllerType)
        {
            if (!LoadedControllerRoutes.Contains(controllerType))
            {
                lock (SyncRoot)
                {
                    if (!LoadedControllerRoutes.Contains(controllerType))
                    {
                        foreach (var prop in controllerType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.PropertyType == typeof(IController)))
                        {
                            Routes.Add(new ControllerRoute(controllerType, prop.Name),
                               GenerateRouteFunction(prop.GetMethod));
                        }
                        // Indexers
                        var method = controllerType.GetMethods().SingleOrDefault(m => Attribute.IsDefined(m, typeof(IndexerAttribute)));
                        if (method != null)
                        {
                            IndexerRoutes.Add(controllerType, ClassRouter.CreateIndexerFunction<IController>(controllerType, method));
                        }

                        LoadedControllerRoutes.Add(controllerType);
                    }
                }
            }
        }

        public async Task Handle(IHttpContext context, Func<Task> next)
        {
            // I/O Bound?
            var controller = await GetController(context.Request.RequestParameters, context).ConfigureAwait(false);

            if (controller == null)
            {
                await next().ConfigureAwait(false);
                return;
            }

            var response = await controller.Pipeline.Go(() => CallMethod(context, controller), context).ConfigureAwait(false);
            context.Response = await response.Respond(context, _view).ConfigureAwait(false);


        }
        private async Task<IController> GetController(IEnumerable<string> requestParameters, IHttpContext context)
        {
            var current = _controller;
            foreach (var parameter in requestParameters)
            {
                var controllerType = current.GetType();

                LoadRoutes(controllerType);

                var route = new ControllerRoute(controllerType, parameter);

                Func<IController, IController> routeFunction;
                if (Routes.TryGetValue(route, out routeFunction))
                {
                    current = routeFunction(current);
                    continue;
                }

                // Try find indexer.
                Func<IHttpContext, IController, string, Task<IController>> indexerFunction;
                if (IndexerRoutes.TryGetValue(controllerType, out indexerFunction))
                {
                    current = await indexerFunction(context, current, parameter).ConfigureAwait(false);
                    continue;
                }

                return null;
            }

            return current;
        }

        private Task<IControllerResponse> CallMethod(IHttpContext context, IController controller)
        {
            var controllerMethod = new ControllerMethod(controller.GetType(), context.Request.Method);

            ControllerFunction controllerFunction;
            if (!ControllerFunctions.TryGetValue(controllerMethod, out controllerFunction))
            {
                lock (SyncRoot)
                {
                    if (!ControllerFunctions.TryGetValue(controllerMethod, out controllerFunction))
                    {
                        ControllerFunctions[controllerMethod] = controllerFunction = CreateControllerFunction(controllerMethod);
                    }
                }
            }

            return controllerFunction(context, this.ModelBinder, controller);
            //context.Response = await controllerResponse.Respond(context, _view).ConfigureAwait(false);
        }

        private ControllerFunction CreateControllerFunction(ControllerMethod controllerMethod)
        {
            var httpContextArgument = Expression.Parameter(typeof(IHttpContext), "httpContext");
            var modelBinderArgument = Expression.Parameter(typeof(IModelBinder), "modelBinder");
            var controllerArgument = Expression.Parameter(typeof(object), "controller");

            var errorContainerVariable = Expression.Variable(typeof(IErrorContainer));

            var foundMethod =
                (from method in controllerMethod.ControllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                 let attribute = method.GetCustomAttribute<HttpMethodAttribute>()
                 where attribute != null && attribute.HttpMethod == controllerMethod.Method
                 select method).FirstOrDefault();

            if (foundMethod == null)
            {
                return MethodNotFoundControllerFunction;
            }

            var parameters = foundMethod.GetParameters();

            IList<ParameterExpression> variables = new List<ParameterExpression>(parameters.Length);

            IList<Expression> body = new List<Expression>(parameters.Length);

            var modelBindingGetMethod = typeof(IModelBinding).GetMethods()[0];

            foreach (var parameter in parameters)
            {
                var variable = Expression.Variable(parameter.ParameterType, parameter.Name);
                variables.Add(variable);

                var attributes = parameter.GetCustomAttributes().ToList();

                var modelBindingAttribute = attributes.OfType<IModelBinding>().Single();

                body.Add(
                    Expression.Assign(variable,
                        Expression.Call(Expression.Constant(modelBindingAttribute),
                            modelBindingGetMethod.MakeGenericMethod(parameter.ParameterType),
                            httpContextArgument, modelBinderArgument
                            )));

                if (!attributes.OfType<NullableAttribute>().Any())
                {
                    body.Add(Expression.IfThen(Expression.Equal(variable, Expression.Constant(null)),
                        Expression.Call(errorContainerVariable, "Log", Type.EmptyTypes,
                            Expression.Constant(parameter.Name + " Is not found (null) and not marked as nullable."))));
                }

                if (parameter.ParameterType.GetInterfaces().Contains(typeof(IValidate)))
                {
                    body.Add(Expression.IfThen(Expression.NotEqual(variable, Expression.Constant(null)),
                        Expression.Call(variable, "Validate", Type.EmptyTypes, errorContainerVariable)));
                }

            }

            var methodCallExp = Expression.Call(Expression.Convert(controllerArgument, controllerMethod.ControllerType), foundMethod, variables);

            var labelTarget = Expression.Label(typeof(Task<IControllerResponse>));

            var parameterBindingExpression = body.Count > 0 ? (Expression)Expression.Block(body) : Expression.Empty();

            var methodBody = Expression.Block(
                variables.Concat(new[] { errorContainerVariable }),
                Expression.Assign(errorContainerVariable, Expression.New(typeof(ErrorContainer))),
                parameterBindingExpression,
                Expression.IfThen(Expression.Not(Expression.Property(errorContainerVariable, "Any")),
                        Expression.Return(labelTarget, methodCallExp)),

                Expression.Label(labelTarget, Expression.Call(errorContainerVariable, "GetResponse", Type.EmptyTypes))
                );

            var parameterExpressions = new[] { httpContextArgument, modelBinderArgument, controllerArgument };
            var lambda = Expression.Lambda<ControllerFunction>(methodBody, parameterExpressions);

            return lambda.Compile();
        }
        private Task<IControllerResponse> MethodNotFoundControllerFunction(IHttpContext context, IModelBinder binder, object controller)
        {
            // TODO : MethodNotFound.
            return Task.FromResult<IControllerResponse>(new RenderResponse(HttpResponseCode.MethodNotAllowed, new { Message = "Not Allowed" }));
        }
    }


}
