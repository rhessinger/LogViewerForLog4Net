using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using log4net;

namespace LogViewer
{
    /// <summary>
    /// Object Copier. Returns a clone of original (must be serializable).
    /// </summary>
    public static class ObjectCopier
    {
        /// <summary> 
        /// Perform a deep Copy of the object. 
        /// </summary> 
        /// <typeparam name="T">The type of object being copied.</typeparam> 
        /// <param name="source">The object instance to copy.</param> 
        /// <returns>The copied object.</returns> 
        /// 

        private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static T Clone<T>(this T source)
        {
            if (!typeof(T).IsSerializable)
            {
                log.Error("Type must be serializable");
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object 
            log.Info("Don't serialize a null object, simply return the default for that object ");
            if (ReferenceEquals(source, null))
            {
                return default(T);
            }

            log.Info("Initializing IFormatter objec based on BinaryFormatter");
            IFormatter formatter = new BinaryFormatter();
            log.Info("Initializing Memory Stream");
            Stream stream = new MemoryStream();
            log.Info("Using the stream object to format the serialized data");
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }

        }
    }
}
    
