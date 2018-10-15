using System;
using System.Collections.ObjectModel;

namespace GitBack
{
    [Serializable]
    public class ConfigPropertiesKeyedCollection : KeyedCollection<string, ConfigProperties>
    {
        public ConfigPropertiesKeyedCollection() : base(StringComparer.InvariantCultureIgnoreCase)
        {
        }

        public bool AddOrUpdate(ConfigProperties item)
        {
            var key = GetKeyForItem(item);
            var containsKey = Contains(key);
            if (containsKey)
            {
                var itemToUpdate = this[key];
                var index = IndexOf(itemToUpdate);
                SetItem(index, item);
            }
            else
            {
                Add(item);
            }

            return !containsKey;
        }

        public ConfigProperties GetOrDefault(string key)
        {
            if (Contains(key))
            {
                return this[key];
            }

            var defaultProperty = new ConfigProperties(key);
            Add(defaultProperty);
            return defaultProperty;
        }

        protected override string GetKeyForItem(ConfigProperties item) => item.Username;
    }
}