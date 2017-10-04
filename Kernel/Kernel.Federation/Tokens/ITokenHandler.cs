﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens;
using System.Xml;

namespace Kernel.Federation.Tokens
{
    public interface ITokenHandler
    {
        Saml2Assertion GetAssertion(XmlReader reader, string partnerId);
        SecurityToken ReadToken(XmlReader reader, string partnerId);
        TokenHandlingResponse HandleToken(HandleTokenContext context);
        bool Validate(SecurityToken token, ICollection<ValidationResult> validationResult, string partnerId);
    }
}