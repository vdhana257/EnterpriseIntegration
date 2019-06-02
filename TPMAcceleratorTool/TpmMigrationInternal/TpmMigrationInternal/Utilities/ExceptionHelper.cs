//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    public static class ExceptionHelper
    {
        public static string GetExceptionMessage(Exception ex)
        {
            if (ex.InnerException != null)
            {
                if (ex.Message.Contains(ex.InnerException.Message))
                {
                    return ex.Message;
                }
                else
                {
                    return string.Concat(ex.Message, " : ", ex.InnerException.Message);
                }
            }
            else
            {
                return ex.Message;
            }
        }

        public static string GetExceptionStackTrace(Exception ex)
        {
            if (!string.IsNullOrEmpty(ex.InnerException.StackTrace))
            {
                return ex.InnerException.StackTrace;
            }
            else
            {
                if (ex.InnerException != null)
                {
                    if (ex.Message.Contains(ex.InnerException.Message))
                    {
                        return ex.Message;
                    }
                    else
                    {
                        return string.Concat(ex.Message, " : ", ex.InnerException.Message);
                    }
                }
                else
                {
                    return ex.Message;
                }
            }
        }

        private static XElement StripNamespaces(XElement rootElement)
        {
            foreach (var element in rootElement.DescendantsAndSelf())
            {
                if (element.Name.Namespace != XNamespace.None)
                {
                    element.Name = XNamespace.None.GetName(element.Name.LocalName);
                }

                bool hasDefinedNamespaces = element.Attributes().Any(attribute => attribute.IsNamespaceDeclaration ||
                        (attribute.Name.Namespace != XNamespace.None && attribute.Name.Namespace != XNamespace.Xml));
                if (hasDefinedNamespaces)
                {
                    var attributes = element.Attributes()
                                            .Where(attribute => !attribute.IsNamespaceDeclaration)
                                            .Select(attribute =>
                                                (attribute.Name.Namespace != XNamespace.None && attribute.Name.Namespace != XNamespace.Xml) ?
                                                    new XAttribute(XNamespace.None.GetName(attribute.Name.LocalName), attribute.Value) :
                                                    attribute);
                    element.ReplaceAttributes(attributes);
                }
            }

            return rootElement;
        }
    }
}
