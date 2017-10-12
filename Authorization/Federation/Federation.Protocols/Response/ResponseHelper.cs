﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Shared.Federtion.Constants;

namespace Federation.Protocols.Response
{
    internal class ResponseHelper
    {
        internal static void EnsureSuccess(string response)
        {
            using (var reader = new StringReader(response))
            {
                using (var xmlReader = XmlReader.Create(reader))
                {
                    ResponseHelper.ValidateResponseSuccess(xmlReader);
                }
            }
        }
        
        internal static void ValidateResponseSuccess(XmlReader reader)
        {
            while (!reader.IsStartElement("StatusCode", Saml20Constants.Protocol))
            {
                if (!reader.Read())
                    throw new InvalidOperationException("Can't find status code element.");
            }
            var status = reader.GetAttribute("Value");
            if (String.IsNullOrWhiteSpace(status) || !String.Equals(status, StatusCodes.Success))
                throw new Exception(status);
        }
    }
}
