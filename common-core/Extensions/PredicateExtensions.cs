using System.Linq.Expressions;

namespace Core.Extensions
{
    public static class PredicateExtensions
    {
        /// <summary>
        /// 以And合并单个表达式
        /// 此处采用AndAlso实现“最短路径”，避免掉额外且不需要的比较运算式
        /// </summary> 
        public static Expression<Func<T, bool>> MergeAnd<T>(this Expression<Func<T, bool>> leftExpress, Expression<Func<T, bool>> rightExpress)
        {
            //声明传递参数（也就是表达式树里面的参数别名s）
            ParameterExpression parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "s");
            //统一管理参数，保证参数一致，否则会报错 
            var visitor = new PredicateExpressionVisitor(parameter);
            //表达式树内容
            System.Linq.Expressions.Expression left = visitor.Visit(leftExpress.Body);
            System.Linq.Expressions.Expression right = visitor.Visit(rightExpress.Body);
            //合并表达式
            return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(System.Linq.Expressions.Expression.AndAlso(left, right), parameter);
        }

        /// <summary>
        /// 以And合并多个表达式
        /// 此处采用AndAlso实现“最短路径”，避免掉额外且不需要的比较运算式
        /// </summary> 
        public static Expression<Func<T, bool>> MergeAnd<T>(this Expression<Func<T, bool>> express, params Expression<Func<T, bool>>[] arrayExpress)
        {
            if (!arrayExpress?.Any() ?? true) return express;
            //声明传递参数（也就是表达式树里面的参数别名s）
            ParameterExpression parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "s");
            //统一管理参数，保证参数一致，否则会报错 
            var visitor = new PredicateExpressionVisitor(parameter);
            Expression<Func<T, bool>> result = null;
            //合并表达式
            foreach (var curExpression in arrayExpress)
            {
                //表达式树内容
                if (result != null && curExpression != null)
                {
                    System.Linq.Expressions.Expression left = visitor.Visit(result.Body);
                    System.Linq.Expressions.Expression right = visitor.Visit(curExpression.Body);
                    result = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(System.Linq.Expressions.Expression.AndAlso(left, right), parameter);
                }
                if (result == null) result = curExpression;

            }
            return result;
        }

        /// <summary>
        /// 以Or合并表达式
        /// 此处采用OrElse实现“最短路径”，避免掉额外且不需要的比较运算式
        /// </summary> 
        public static Expression<Func<T, bool>> MergeOr<T>(this Expression<Func<T, bool>> leftExpress, Expression<Func<T, bool>> rightExpress)
        {
            //声明传递参数（也就是表达式树里面的参数别名s）
            ParameterExpression parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "s");
            //统一管理参数，保证参数一致，否则会报错 
            var visitor = new PredicateExpressionVisitor(parameter);
            //表达式树内容
            System.Linq.Expressions.Expression left = visitor.Visit(leftExpress.Body);
            System.Linq.Expressions.Expression right = visitor.Visit(rightExpress.Body);
            //合并表达式
            return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(System.Linq.Expressions.Expression.OrElse(left, right), parameter);
        }

        /// <summary>
        /// 以Or合并多个表达式
        /// 此处采用AndAlso实现“最短路径”，避免掉额外且不需要的比较运算式
        /// </summary> 
        public static Expression<Func<T, bool>> MergeOr<T>(this Expression<Func<T, bool>> express, params Expression<Func<T, bool>>[] arrayExpress)
        {
            if (!arrayExpress?.Any() ?? true) return express;
            //声明传递参数（也就是表达式树里面的参数别名s）
            ParameterExpression parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "s");
            //统一管理参数，保证参数一致，否则会报错 
            var visitor = new PredicateExpressionVisitor(parameter);
            Expression<Func<T, bool>> result = null;
            //合并表达式
            foreach (var curExpression in arrayExpress)
            {
                //表达式树内容
                System.Linq.Expressions.Expression left = visitor.Visit(result.Body);
                System.Linq.Expressions.Expression right = visitor.Visit(curExpression.Body);
                result = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(System.Linq.Expressions.Expression.OrElse(left, right), parameter);
            }
            return result;
        }
    }


    public class PredicateExpressionVisitor : ExpressionVisitor
    {
        public ParameterExpression _parameter { get; set; }

        public PredicateExpressionVisitor(ParameterExpression parameter)
        {
            _parameter = parameter;
        }
        protected override System.Linq.Expressions.Expression VisitParameter(ParameterExpression p)
        {
            return _parameter;
        }

        public override System.Linq.Expressions.Expression Visit(System.Linq.Expressions.Expression expression)
        {
            //Visit会根据VisitParameter()方法返回的Expression进行相关变量替换
            return base.Visit(expression);
        }
    }
}
