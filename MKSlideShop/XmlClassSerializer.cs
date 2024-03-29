﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml;
using System.Xml.Serialization;

namespace MKSlideShop
{
    internal static class XmlClassSerializer
    {
        /// <summary>
        /// Converts the given object to XML. Use <see cref="Xml2Object"/> to
        /// convert the XML back to a class.
        /// </summary>
        /// <param name="data">The object to convert.</param>
        /// <returns>The XML version of the object.</returns>
        internal static string Object2Xml(object data)
        {
            StringBuilder builder = new StringBuilder();

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true; 
            settings.NewLineOnAttributes = true;    

            using (XmlWriter writer = XmlWriter.Create(builder, settings))
            {
                XmlSerializer serializer = new XmlSerializer(data.GetType());
                serializer.Serialize(writer, data);
                writer.Flush();
            }
            return builder.ToString();
        }
        /// <summary>
        /// Converts the given XML to a class. The XML must be compatible with or generated by
        /// <see cref="Object2Xml"/>.
        /// </summary>
        /// <param name="xml">The XML serialized version of the object</param>
        /// <param name="objectType">The type of the object that will be deserialized.</param>
        /// <returns>The deserialized object.</returns>
        internal static object? Xml2Object(string xml, Type objectType)
        {
            XmlSerializer serializer = new XmlSerializer(objectType);
            try
            {
                using (StringReader reader = new StringReader(xml))
                {

                    return serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                string err = "Invalid XML.";
                throw new Exception(err, ex);
            }
        }
    }
}
