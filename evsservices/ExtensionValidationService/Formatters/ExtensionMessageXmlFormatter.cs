﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using EVSAppController.Models;

namespace ExtensionValidationService.Formatters
{
    public class ExtensionMessageXmlFormatter : BufferedMediaTypeFormatter 
    {
        private string _fileName;

        public ExtensionMessageXmlFormatter()
        {
            _fileName = "results.xml";
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xml"));
        }

        public ExtensionMessageXmlFormatter(string fileName)
        {
            _fileName = fileName;
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xml"));
        }

        public override bool CanWriteType(Type type)
        {
            if (type == typeof(ExtensionMessage))
            {
                return true;
            }

            var enumerableType = typeof(IEnumerable<ExtensionMessage>);
            return enumerableType.IsAssignableFrom(type);
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override void WriteToStream(Type type, object value, Stream stream, HttpContent content)
        {
            WriteToStream(value, stream);
        }

        public void WriteToStream(object value, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {

                writer.WriteLine(@"<ArrayOfExtensionMessage xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/EVSAppController.Models"">");

                var messages = value as IEnumerable<ExtensionMessage>;
                if (messages != null)
                {
                    foreach (var message in messages)
                    {
                        WriteItem(message, writer);
                    }
                }
                else
                {
                    var singleMessage = value as ExtensionMessage;
                    if (singleMessage == null)
                    {
                        throw new InvalidOperationException("Cannot serialize type");
                    }
                    WriteItem(singleMessage, writer);
                }

                writer.WriteLine("</ArrayOfExtensionMessage>");
            }
            stream.Close();
        }

        // Helper methods for serializing Products to CSV format. 
        private void WriteItem(ExtensionMessage message, StreamWriter writer)
        {
            writer.WriteLine("<ExtensionMessage><MessageID>{0}</MessageID><MessageType>{1}</MessageType><Rule>{2}</Rule><Message>{3}</Message></ExtensionMessage>", Escape(message.MessageID), Escape(message.MessageType), Escape(message.Rule), Escape(message.Message));
        }

        static readonly char[] SpecialChars = new[] { ',', '\n', '\r', '"' };

        private static string Escape(object o)
        {
            if (o == null)
            {
                return "";
            }

            var field = o.ToString();

            return field.IndexOfAny(SpecialChars) != -1 ? String.Format("\"{0}\"", field.Replace("\"", "\"\"")) : field;
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(type, headers, mediaType);
            headers.Add("Content-Disposition", "attachment; filename=" + _fileName);
        }
    }
}