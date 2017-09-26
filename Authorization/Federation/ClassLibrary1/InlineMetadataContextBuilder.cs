﻿using System.IdentityModel.Tokens;
using Kernel.Federation.MetaData;
using Kernel.Federation.MetaData.Configuration;
using Kernel.Federation.MetaData.Configuration.Cryptography;

namespace InlineMetadataContextProvider
{
    internal class InlineMetadataContextBuilder : IMetadataContextBuilder
    {
        public MetadataContext BuildContext(MetadataGenerateRequest metadataGenerateContext)
        {
            var entityDescriptorConfiguration = MetadataHelper.BuildEntityDesriptorConfiguration();

            var keyDescriptorConfiguration = MetadataHelper.BuildKeyDescriptorConfiguration();
            
            var spDescriptorConfigurtion = MetadataHelper.BuildSPSSODescriptorConfiguration();
            entityDescriptorConfiguration.RoleDescriptors.Add(spDescriptorConfigurtion);
            
            var context = new MetadataContext
            {
                EntityDesriptorConfiguration = entityDescriptorConfiguration,
                SignMetadata = true
            };

            context.MetadataSigningContext = new MetadataSigningContext(SecurityAlgorithms.RsaSha1Signature, SecurityAlgorithms.Sha1Digest);
            context.MetadataSigningContext.KeyDescriptors.Add(keyDescriptorConfiguration);
            return context;
        }

        public void Dispose()
        {
        }
    }
}