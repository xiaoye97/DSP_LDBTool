using UnityExplorer;

namespace xiaoye97
{
    public class ShowItem
    {
        object proto;
        public ShowItem(object proto)
        {
            this.proto = proto;
        }

        public void Show()
        {
            InspectorManager.Inspect(proto);
        }
    }
}
