namespace Gem
{
    namespace Networking
    {
        /// <summary>
        /// A callback for network requests made.
        /// </summary>
        /// <param name="aData"></param>
        public delegate void RequestCallback(RequestData aData);

        /// <summary>
        /// Data returned back upon request complete.
        /// </summary>
        public struct RequestData
        {
            public object data { get; set; }
            public Request request { get; set; }
        }
    }
}