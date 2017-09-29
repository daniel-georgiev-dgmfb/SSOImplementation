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
        public FederationPartnerContext BuildContext(string relyingPartyId)
        {
            if (this._cacheProvider.Contains(relyingPartyId))
                return this._cacheProvider.Get<FederationPartnerContext>(relyingPartyId);

            var relyingPartyContext = this._dbContext.Set<RelyingPartySettings>()
                .FirstOrDefault(x => x.RelyingPartyId == relyingPartyId);

            var context = new FederationPartnerContext(relyingPartyId, relyingPartyContext.MetadataPath);
            context.RefreshInterval = TimeSpan.FromSeconds(relyingPartyContext.RefreshInterval);
            context.AutomaticRefreshInterval = TimeSpan.FromDays(relyingPartyContext.AutoRefreshInterval);
            this.BuildMetadataContext(context, relyingPartyContext.MetadataSettings);
            object policy = new MemoryCacheItemPolicy();
            ((ICacheItemPolicy)policy).SlidingExpiration = TimeSpan.FromDays(1);
            this._cacheProvider.Put(relyingPartyId, context,  (ICacheItemPolicy)policy);
            return context;
        }

        private void BuildMetadataContext(FederationPartnerContext relyingPartyContext, MetadataSettings metadataSettings)
        {
            var metadataContextBuilder = new MetadataContextBuilder(this._dbContext, this._cacheProvider);
            relyingPartyContext.MetadataContext = metadataContextBuilder.BuildFromDbSettings(metadataSettings);
        }

        public void Dispose()
        {
            if(this._dbContext != null)
                this._dbContext.Dispose();
        }
    }
}