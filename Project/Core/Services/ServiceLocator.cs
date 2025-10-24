using System;
using System.Collections.Generic;

namespace EtT.Services
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _map = new();
        public static void Register<T>(T instance) where T:class => _map[typeof(T)]=instance;
        public static T Get<T>() where T:class => (T)_map[typeof(T)];
        public static void Clear()=>_map.Clear();
    }
}