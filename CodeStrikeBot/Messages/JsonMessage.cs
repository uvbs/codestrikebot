﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
//using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;

namespace CodeStrikeBot.Messages
{
    public class JsonMessage : XmlMessage
    {
        public JsonMessage(Message message)
            : base(message)
        {
            //this.Document = new XmlDocument();
        }

        public JsonMessage(JsonMessage message)
            : base(message)
        {
            //this.Document = message.Document;

            //DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Messages.Data.TileUpdatedMessage));
            var serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new Utilities.DynamicJsonConverter() });

            //dynamic data = serializer.Deserialize(json, typeof(object));
        }

        public static JsonMessage Parse(XmlMessage message)
        {
            JsonMessage ret = new JsonMessage(message);

            try
            {
                ret.LoadXml();
                System.Xml.XmlNode node = ret.Document.DocumentElement.FirstChild.FirstChild;

                if (node != null && node.Name == "items")
                {
                    ret.Id = UInt64.Parse(node.FirstChild.Attributes["id"].Value, System.Globalization.NumberStyles.HexNumber);

                    switch (node.Attributes["node"].Value)
                    {
                        case "EVENT_WAR_RALLY_BEGAN": //rally defense
                            ret.LoadXml();
                            node = ret.Document.DocumentElement.SelectSingleNode("//*[local-name()='payload']");
                            string json = node.InnerText;
                            ret = new WarRallyBeginMessage(ret);
                            break;
                        case "EVENT_MARCH": //march
                            node = ret.Document.DocumentElement.SelectSingleNode("//*[local-name()='payload']");
                            break;
                    }
                }

                node = node;

                //if (node.)
            }
            catch (System.Xml.XmlException ex) { }

            return ret;
        }
    }
}
