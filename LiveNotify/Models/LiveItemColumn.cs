namespace LiveNotify.Models
{
    public class LiveItemColumn
    {
        public LiveItemColumn(string header, string dataMemberPath)
        {
            Header = header;
            DataMemberPath = dataMemberPath;
        }
        public string Header;
        public string DataMemberPath;
    }
}
