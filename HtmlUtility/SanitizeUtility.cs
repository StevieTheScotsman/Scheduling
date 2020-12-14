using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Web;
using HtmlAgilityPack;

namespace Scheduling.Html
{
    public class SanitizeUtility
    {
        public static bool VerifyChild (HtmlNode CurrentNode)
        {
            bool retBool = false;
            //If text node we are good
            if (CurrentNode.NodeType == HtmlNodeType.Text) retBool=true;
            
            if(CurrentNode.NodeType==HtmlNodeType.Element)
            {
                if(CurrentNode.Name=="em" || CurrentNode.Name=="strong")
                {
                    retBool = true;
                }


            }

            return retBool;
        }

        //The parents should all be p tags and the children should be bold ,italic or text..locked down for xss purposes
        //This jives with the configuration set in ckeditor/config.js file which is restricted.
        public static string SanitizeFromWhitelist(string html)
        {
            HtmlDocument CleanDoc = new HtmlDocument();
            HtmlDocument doc=new HtmlDocument();
            doc.LoadHtml(html);
            //doc.OptionOutputAsXml = true;
            HtmlNodeCollection coll=doc.DocumentNode.SelectNodes("*");
            foreach(HtmlNode node in coll)
            {    
                //editor should just show p tags
                string n = node.Name;
                if(n !="p")
                {
                    coll.Remove(node);
                    
                }

                if(node.HasChildNodes)
                {

                    foreach(HtmlNode child in node.ChildNodes)
                    {
                        if (!VerifyChild(child))
                        {
                            child.Remove();
                        }

                    }

                }
                

            }

            
           return doc.DocumentNode.OuterHtml;

        }
    }
}