using PRJ_WAREHOUSE_BIVN.Common.Enum;
using PRJ_WAREHOUSE_BIVN.Common;

namespace PRJ_WAREHOUSE_BIVN.Common.Helper
{
    public static class FormatSqlValueForSqlServerHelper
    {
        public static string FormatSqlValue(Filter filter)
        {
            string value = filter.Value?.Replace("'", "''") ?? string.Empty;

            return filter.Operator switch
            {
                OperatorType.Contain => $"'%{value}%'",
                OperatorType.StartWith => $"'{value}%'",
                OperatorType.EndWith => $"'%{value}'",
                OperatorType.In or OperatorType.NotIn => $"({value})", // đảm bảo value là danh sách đúng định dạng
                OperatorType.IsNull or OperatorType.IsNotNull => string.Empty,
                _ => $"'{value}'"
            };
        }

    }
}
