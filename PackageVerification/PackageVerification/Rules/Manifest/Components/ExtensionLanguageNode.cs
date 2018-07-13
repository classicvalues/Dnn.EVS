﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using PackageVerification.Models;

namespace PackageVerification.Rules.Manifest.Components
{
    public class ExtensionLanguageNode : ComponentBase, IManifestRule
    {
        public List<VerificationMessage> ApplyRule(Package package, Models.Manifest manifest)
        {
            var r = new List<VerificationMessage>();

            try
            {
                if (manifest.ComponentNodes == null)
                    return r;

                foreach (XmlNode componentNode in manifest.ComponentNodes)
                {
                    if (componentNode.Attributes == null) continue;

                    var type = componentNode.Attributes["type"];
                    if (type == null || string.IsNullOrEmpty(type.Value)) continue;

                    if (type.Value != "ExtensionLanguage") continue;

                    var primaryNode = componentNode.SelectSingleNode("languageFiles");
                    if (primaryNode == null) continue;

                    var code = primaryNode.SelectSingleNode("code");
                    if (code == null || string.IsNullOrEmpty(code.InnerText))
                    {
                        r.Add(new VerificationMessage { Message = "ALL components of type Extension Language must have a code specified.", MessageType = MessageTypes.Error, MessageId = new Guid("5e4b873d-ed81-4df5-b908-8cf641516e60"), Rule = GetType().ToString() });
                    }

                    var packageNode = primaryNode.SelectSingleNode("package");
                    if (packageNode == null || string.IsNullOrEmpty(packageNode.InnerText))
                    {
                        r.Add(new VerificationMessage { Message = "ALL components of type Extension Language must have a package specified.", MessageType = MessageTypes.Error, MessageId = new Guid("ec025080-d5c1-41af-9d0d-f2a65ccf3b5a"), Rule = GetType().ToString() });
                    }

                    var basePathNode = primaryNode.SelectSingleNode("basePath");
                    if (basePathNode == null || string.IsNullOrEmpty(basePathNode.InnerText))
                    {
                        r.Add(new VerificationMessage { Message = "ALL components of type Extension Language must have a base path specified.", MessageType = MessageTypes.Error, MessageId = new Guid("30a9f28f-ca3c-4d66-bd7e-b3e67f861355"), Rule = GetType().ToString() });
                    }

                    ProcessComponentNode(r, package, manifest, primaryNode, "languageFile");
                }
            }
            catch (Exception exc)
            {
                Trace.WriteLine("There was an issue with the extension language node scanner", exc.Message);
                r.Add(new VerificationMessage { Message = "An Error occurred while processing Rules.Manifest.Components.ExtensionLanguageNode", MessageType = MessageTypes.SystemError, MessageId = new Guid("0ebfe7b6-16c4-41c9-9863-87ea1474d143"), Rule = GetType().ToString() });
            }

            return r;
        }
    }
}