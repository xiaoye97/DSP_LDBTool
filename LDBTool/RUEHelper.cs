namespace xiaoye97
{
    public static class RUEHelper
    {
        public static void ShowProto(Proto proto)
        {
            if (proto != null)
            {
                ShowItem item = new ShowItem(proto);
                item.Show();
            }
        }
    }
}
