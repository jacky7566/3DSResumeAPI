//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;

//namespace LumMVC_WebAPI.Utils
//{
//    public static class Caculater
//    {
//        public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> source, int chunkSize)
//        {
//            return source.Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / chunkSize).Select(x => x.Select(v => v.Value));
//        }
//    }
//}