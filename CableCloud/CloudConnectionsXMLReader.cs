 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace CableCloud
{
    class CloudConnectionsXMLReader
    {
        public List<Link> ReadCloudConnections()
        {
            List<Link> result = new List<Link>();
            XmlDocument XmlDoc = new XmlDocument();
            try
            {
                XmlDoc.Load("ConfigFiles/CloudConnection.xml");
                Console.WriteLine("CloudConnection.xml Loaded!");
                int count = XmlDoc.GetElementsByTagName("Link").Count;
                for (int i = 0; i < count; i++)
                {
                    XmlAttributeCollection coll = XmlDoc.GetElementsByTagName("Link").Item(i).Attributes;
                    Link readLink = new Link(
                        Int32.Parse(XmlDoc.GetElementsByTagName("Link").Item(i).InnerText), // linkId
                        coll.Item(0).InnerText, //firstObjectId
                        coll.Item(2).InnerText, // secondObjectId
                        Int32.Parse(coll.Item(1).InnerText), //firstObjectPort
                        Int32.Parse(coll.Item(3).InnerText) //secondObjectPort
                        );
                    
                    result.Add(readLink);
                }
            }
            catch (XmlException exc)
            {
                Console.WriteLine(exc.Message);
            }

            return result;
        }

        public void UpdateTargetsWithIPs(List<TargetNetworkObject> targetsList)
        {
            List<Link> result = new List<Link>();
            XmlDocument XmlDoc = new XmlDocument();
            try
            {
                XmlDoc.Load("ConfigFiles/NetworkObjectsIP.xml");
                Console.WriteLine("NetworkObjectsIP.xml Loaded!");
                int count = XmlDoc.GetElementsByTagName("NodeIP").Count;
                for (int i = 0; i < count; i++)
                {
                    XmlAttributeCollection coll = XmlDoc.GetElementsByTagName("NodeIP").Item(i).Attributes;
                    targetsList.FindAll(x => x.TargetObjectId == coll.Item(0).InnerText).All(x => { x.TargetObjectAddress = coll.Item(1).InnerText; return true; });
                }              
            }
            catch (XmlException exc)
            {
                Console.WriteLine(exc.Message);
            }
        }
    }
}
