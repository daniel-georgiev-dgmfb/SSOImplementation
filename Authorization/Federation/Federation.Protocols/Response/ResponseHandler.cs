﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Kernel.Authentication.Claims;
using Kernel.Federation.Protocols;
using Kernel.Federation.Protocols.Bindings.HttpPostBinding;
using Kernel.Federation.Protocols.Response;
using Kernel.Federation.Tokens;

namespace Federation.Protocols.Response
{
    internal class ResponseHandler : IReponseHandler<ClaimsIdentity>
    {
        private readonly IRelayStateHandler _relayStateHandler;
        private readonly ITokenHandler _tokenHandler;
        private readonly IUserClaimsProvider<SecurityToken> _identityProvider;
        public ResponseHandler(IRelayStateHandler relayStateHandler, ITokenHandler tokenHandler, IUserClaimsProvider<SecurityToken> identityProvider)
        {
            this._relayStateHandler = relayStateHandler;
            this._tokenHandler = tokenHandler;
            this._identityProvider = identityProvider;
        }
        public async Task<ClaimsIdentity> Handle(HttpPostResponseContext context)
        {
            //ToDo handle this properly, response handling, token validation, claims generation etc
            var elements = context.Form;
            var responseBase64 = elements["SAMLResponse"];
            var responseBytes = Convert.FromBase64String(responseBase64);
            var responseText = Encoding.UTF8.GetString(responseBytes);
            
            var relayState = await this._relayStateHandler.GetRelayStateFromFormData(elements);
#if(DEBUG)
            this.SaveTemp(responseText);
#endif
            var xmlReader = XmlReader.Create(new StringReader(responseText));
            this.ValidateResponseSuccess(xmlReader);
            var token = _tokenHandler.ReadToken(xmlReader, relayState.ToString());
            //sort this out
            var issuer = ((Saml2SecurityToken)token).IssuerToken as X509SecurityToken;
            var validator = this._tokenHandler as ITokenValidator;
            var validationResult = new List<ValidationResult>();
            var isValid = validator.Validate(token, validationResult, relayState.ToString());
            if (!isValid)
                throw new InvalidOperationException(validationResult.ToArray()[0].ErrorMessage);
            //ToDo: Decide how to do it 03/10/17. Inject this one when you've decided what to do
            var foo = new ClaimsProvider();
            var identity = await foo.GenerateUserIdentitiesAsync((Federation.Protocols.Tokens.SecurityTokenHandler)this._tokenHandler, new[] { context.AuthenticationMethod });
            //var identity = await this._identityProvider.GenerateUserIdentitiesAsync(token, new[] { context.AuthenticationMethod });
            return identity[context.AuthenticationMethod];
           
        }

        //ToDo: sort this out clean up
        private void ValidateResponseSuccess(XmlReader reader)
        {
            while (!reader.IsStartElement("StatusCode", "urn:oasis:names:tc:SAML:2.0:protocol"))
            {
                if (!reader.Read())
                    throw new InvalidOperationException("Can't find status code element.");
            }
            var status = reader.GetAttribute("Value");
            if (String.IsNullOrWhiteSpace(status) || !String.Equals(status, "urn:oasis:names:tc:SAML:2.0:status:Success"))
                throw new Exception(status);
        }
        //ToDo clean up
        private void SaveTemp(string responseText)
        {
            try
            {
                var path = @"D:\Dan\Software\Apira\Assertions\";
                var now = DateTimeOffset.Now;
                var tag = String.Format("{0}{1}{2}{3}{4}", now.Year, now.Month, now.Day, now.Hour, now.Minute);
                var writer = XmlWriter.Create(String.Format("{0}{1}{2}", path, tag, ".xml"));
                var el = new XmlDocument();
                el.Load(new StringReader(responseText));
                el.DocumentElement.WriteTo(writer);
                writer.Flush();
                writer.Dispose();
            }
            catch (Exception)
            {
                //ignore
            }
        }
    }
}