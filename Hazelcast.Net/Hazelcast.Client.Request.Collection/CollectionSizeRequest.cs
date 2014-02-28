using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class CollectionSizeRequest : CollectionRequest
    {
        public CollectionSizeRequest()
        {
        }

        public CollectionSizeRequest(string name) : base(name)
        {
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.CollectionSize;
        }
    }
}