﻿using Kernel.Federation.FederationPartner;
using Kernel.Federation.Protocols;
using Shared.Federtion.Models;

namespace Federation.Protocols.Request.ClauseBuilders
{
    internal abstract class ClauseBuilder : IAuthnRequestClauseBuilder<AuthnRequest>
    {
        public void Build(AuthnRequest request, FederationPartyContext federationParty)
        {
            this.BuildInternal(request, federationParty.RequestConfiguration);
        }

        protected abstract void BuildInternal(AuthnRequest request, AuthnRequestConfiguration configuration);
    }
}