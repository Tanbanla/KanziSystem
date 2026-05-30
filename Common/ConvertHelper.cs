namespace PRJ_WAREHOUSE_BIVN.Common
{
    public static class ConvertHelper
    {
        public static string? ParsePhanloai(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "No list";
            if (s != "A" && s != "B" && s != "C" && s != "E" && s != "I") return "No list";
            return s;
        }
        public static double? ParseDouble(string s)

        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (double.TryParse(s.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d)) return d;
            return null;
        }
        public static int? ParseInt(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (int.TryParse(s.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d)) return d;
            return null;
        }
        public static DateTime? ParseDate(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (DateTime.TryParse(s, out var dt)) return dt;
            return null;
        }
        public static bool? ParseBool(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            var v = s.Trim().ToLowerInvariant();
            return v.ToUpper().Contains("O") ? true : false;
        }
        public static string? ParseNameHQ(string Catergory, string Shape, string Material, string Composition, string Dimension, string UsedFor, string Purpose)
        {
            return Catergory + "có hình dáng " + Shape + " chất liệu " + Material + " thành phần hóa chất " + Composition + " có kích thước " + Dimension + " dùng để " + UsedFor + " cho " + Purpose;
        }
        // Đổi tiền VND sang USD or USD sang VND
        public static double? ParseVNDtoUSD(double input, bool isVNDToUSD, float exchangeRate)
        {
            double result = 0;
            if (input > 0)
            {
                if (isVNDToUSD)
                {
                    result = input / exchangeRate;
                }
                else
                {
                    result = input * exchangeRate;
                }
                return Math.Round(result, 4);
            }
            return null;
        }
    }
}
