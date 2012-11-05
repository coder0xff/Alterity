namespace Redis
{
    internal class Scope : DynamicDataObject
    {
        readonly string absolutePath;
        readonly ServiceStack.Redis.RedisClient dataStore;

        internal Scope(string absolutePath, ServiceStack.Redis.RedisNativeClient dataStore)
        {
            this.absolutePath = absolutePath;
            this.dataStore = dataStore;
        }

        public override ServiceStack.Redis.RedisClient GetDataStore(string memberAbsolutePath)
        {
            return dataStore;
        }

        public override string GetMemberAbsolutePath(string name, bool ignoreCase)
        {
            return absolutePath + "." + (ignoreCase ? name.ToLowerInvariant() : name);
        }

        internal override Scope CreateScope(string memberAbsolutePath)
        {
            return new Scope(memberAbsolutePath , dataStore);
        }
    }
}
