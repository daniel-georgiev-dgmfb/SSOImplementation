﻿using System;
using System.Linq;
using Kernel.Cache;
using Kernel.Data.ORM;
using Kernel.Federation.FederationPartner;
using MemoryCacheProvider;
using ORMMetadataContextProvider.Models;

namespace ORMMetadataContextProvider.RelyingParty
{
    internal class FederationPartnerContextBuilder : IFederationPartnerContextBuilder
    {
        private readonly IDbContext _dbContext;
        private readonly ICacheProvider _cacheProvider;

        public FederationPartnerContextBuilder(IDbContext dbContext, ICacheProvider cacheProvider)
        {
            this._dbContext = dbContext;
            this._cacheProvider = cacheProvider;
        }
        public FederationPartnerContext BuildContext(string federationPartyId)
        {
            if (this._cacheProvider.Contains(federationPartyId))
                return this._cacheProvider.Get<FederationPartnerContext>(federationPartyId);

            var federationPartyContext = this._dbContext.Set<FederationPartySettings>()
                .FirstOrDefault(x => x.FederationPartyId == federationPartyId);

            var context = new FederationPartnerContext(federationPartyId, federationPartyContext.MetadataPath);
            context.RefreshInterval = TimeSpan.FromSeconds(federationPartyContext.RefreshInterval);
            context.AutomaticRefreshInterval = TimeSpan.FromDays(federationPartyContext.AutoRefreshInterval);
            this.BuildMetadataContext(context, federationPartyContext.MetadataSettings);
            object policy = new MemoryCacheItemPolicy();
            ((ICacheItemPolicy)policy).SlidingExpiration = TimeSpan.FromDays(1);
            this._cacheProvider.Put(federationPartyId, context,  (ICacheItemPolicy)policy);
            return context;
        }

        private void BuildMetadataContext(FederationPartnerContext federationPartyContext, MetadataSettings metadataSettings)
        {
            var metadataContextBuilder = new MetadataContextBuilder(this._dbContext, this._cacheProvider);
            federationPartyContext.MetadataContext = metadataContextBuilder.BuildFromDbSettings(metadataSettings);
        }

        public void Dispose()
        {
            if(this._dbContext != null)
                this._dbContext.Dispose();
        }
    }
}