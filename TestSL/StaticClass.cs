namespace TestSL
{
    public class StaticClass
    {
        static StaticClass()
        {
        }

        private static object defaultInstanceLock = new object();
        private static StaticClass defaultInstance = null;

        public static StaticClass Default
        {
            get
            {
                if (defaultInstance == null)
                    lock (defaultInstanceLock)
                        if (defaultInstance == null)
                            defaultInstance = new StaticClass();
                return defaultInstance;
            }
        }

        public string TestString
        {
            get { return "This is a test...\r\nThe static extension works!"; }
        }
    }
}
