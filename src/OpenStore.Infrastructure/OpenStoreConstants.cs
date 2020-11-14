using Microsoft.AspNetCore.Http;

namespace OpenStore.Infrastructure
{
    public static class OpenStoreConstants
    {
        public const string CorrelationIdKey = "CorrelationId";
        
        public static class Paths
        {
            public static readonly PathString AdminPath = new PathString("/admin");
            public static readonly PathString IdentityPath = new PathString("/identity");
            public static readonly PathString WebGwPath = new PathString("/web-gw");
            public static readonly PathString ListingPath = new PathString("/listing");
            public static readonly PathString PimPath = new PathString("/pim");
            public static readonly PathString LocalizationPath = new PathString("/localization");
            public static readonly PathString ReadModelPath = new PathString("/read");
            public static readonly PathString CmsPath = new PathString("/cms");
        }
    }
}