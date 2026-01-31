using PRJ_WAREHOUSE_BIVN.Common.Enum;

namespace PRJ_WAREHOUSE_BIVN.Common.Helper
{

    public static class SqlOperatorHelper
    {
        public static string GetSqlOperator(OperatorType op)
        {
            return op switch
            {
                OperatorType.Equal => "=",
                OperatorType.NotEqual => "<>",
                OperatorType.Greater => ">",
                OperatorType.GreaterThanEqual => ">=",
                OperatorType.Lower => "<",
                OperatorType.LowerThanEqual => "<=",
                OperatorType.Contain => "LIKE", // dùng với '%value%'
                OperatorType.StartWith => "LIKE", // dùng với 'value%'
                OperatorType.EndWith => "LIKE", // dùng với '%value'
                OperatorType.In => "IN",
                OperatorType.NotIn => "NOT IN",
                OperatorType.IsNull => "IS NULL",
                OperatorType.IsNotNull => "IS NOT NULL",
                _ => string.Empty
            };
        }

    }
}
