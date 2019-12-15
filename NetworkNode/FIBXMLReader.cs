using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace NetworkNode
{
    public class FIBXMLReader
    {
        public RoutingInfo ReadFIB(string FileName, string routerName)
        {
            RoutingInfo result = new RoutingInfo();
            XmlDocument XmlDoc = new XmlDocument();
            try
            {
                XmlDoc.Load("ConfigFiles/" + FileName);
                Console.WriteLine(FileName + " Loaded!");
                XmlNodeList Node = XmlDoc.GetElementsByTagName(routerName);
                
                int countRouterLabels = Node.Item(0).ChildNodes[0].ChildNodes.Count; 
                //Console.WriteLine(count);
                for (int i = 0; i < countRouterLabels; i++)
                {
                    XmlAttributeCollection xmlRouterLabel = Node.Item(0).ChildNodes[0].ChildNodes[i].Attributes;
                    RouterLabel routerLabel = new RouterLabel(
                        Int32.Parse(xmlRouterLabel.Item(0).InnerText),
                        Int32.Parse(xmlRouterLabel.Item(1).InnerText),
                        Int32.Parse(xmlRouterLabel.Item(2).InnerText),
                        Int32.Parse(xmlRouterLabel.Item(3).InnerText)
                        );

                    result.routerLabels.Add(routerLabel);
                }

                int countRouterDeletedLabels = Node.Item(0).ChildNodes[1].ChildNodes.Count;
                //Console.WriteLine(count);
                for (int i = 0; i < countRouterDeletedLabels; i++)
                {
                    XmlAttributeCollection xmlRouterDeletedLabel = Node.Item(0).ChildNodes[1].ChildNodes[i].Attributes;
                    RouterDeletedLabel routerDeletedLabel = new RouterDeletedLabel(
                        Int32.Parse(xmlRouterDeletedLabel.Item(0).InnerText),
                        Int32.Parse(xmlRouterDeletedLabel.Item(1).InnerText)
                        );

                    result.routerDeletedLabels.Add(routerDeletedLabel);
                }

                int countRouterActions = Node.Item(0).ChildNodes[2].ChildNodes.Count;
                //Console.WriteLine(count);
                for (int i = 0; i < countRouterActions; i++)
                {
                    XmlAttributeCollection xmlRouterAction = Node.Item(0).ChildNodes[2].ChildNodes[i].Attributes;
                    RouterAction routerAction = new RouterAction(
                        Int32.Parse(xmlRouterAction.Item(0).InnerText),
                                    xmlRouterAction.Item(1).InnerText,
                                    xmlRouterAction.Item(2).InnerText,
                                    xmlRouterAction.Item(3).InnerText,
                                    xmlRouterAction.Item(4).InnerText
                        );

                    result.routerActions.Add(routerAction);
                }

            }
            catch (XmlException exc)
            {
                Console.WriteLine(exc.Message);
            }

            return result;
        }
    }
}

