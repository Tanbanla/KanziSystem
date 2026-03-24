using DocumentFormat.OpenXml.Wordprocessing;

namespace PRJ_WAREHOUSE_BIVN.View_Models.Master
{
    public class CatergoryVM
    {
    }
    public class SearchCatergory
    {
        public string Name { get; set; }
        public int? pageIndex { get; set; } = 1;

        public int? pageSize { get; set; } = 100;
    }
}
