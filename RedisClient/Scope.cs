namespace Redis
{
    class Scope : DynamicDataObject
    {
        readonly string absolutePath;
        readonly ServiceStack.Redis.RedisNativeClient dataStore;

        internal Scope(string absolutePath, ServiceStack.Redis.RedisNativeClient dataStore)
        {
            this.absolutePath = absolutePath;
            this.dataStore = dataStore;
        }

        protected override ServiceStack.Redis.RedisNativeClient GetDataStore(string memberAbsolutePath)
        {
            return dataStore;
        }

        protected override string GetMemberAbsolutePath(string name, bool ignoreCase)
        {
            return absolutePath + "." + (ignoreCase ? name.ToLowerInvariant() : name);
        }

        protected override Scope CreateScope(string memberAbsolutePath)
        {
            return new Scope(memberAbsolutePath , dataStore);
        }
    }
}
