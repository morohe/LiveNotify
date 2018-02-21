using System;
using System.IO;
using System.Xml.Serialization;

namespace LicenseViewer.Models
{
    /// <summary>
    /// License information
    /// </summary>
    [XmlRoot("license")]
    public class License
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Title of license
        /// </summary>
        [XmlElement("title")]
        public string Title { get; set; }

        /// <summary>
        /// Author, Copyright.
        /// </summary>
        [XmlElement("author")]
        public string Author { get; set; }

        /// <summary>
        /// License detail text file
        /// </summary>
        [XmlElement("license_file")]
        public string LicenseFile { get; set; }

        /// <summary>
        /// Import from Xml File
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static License ImportFromXmlFile(string filename)
        {
            License ret = new License();
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(License));
                using (StreamReader wr = new StreamReader(filename))
                {
                    ret = (License)xs.Deserialize(wr);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return ret;
        }
    }
}
