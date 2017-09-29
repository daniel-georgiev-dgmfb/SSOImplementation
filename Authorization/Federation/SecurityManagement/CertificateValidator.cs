﻿using System;
using System.IdentityModel.Selectors;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
using System.Threading.Tasks;
using System.Web;
using Kernel.Cryptography.Validation;
using SecurityManagement.BackchannelCertificateValidationRules;
using SecurityManagement.CertificateValidationRules;

namespace SecurityManagement
{
    internal class CertificateValidator : X509CertificateValidator, ICertificateValidator
    {
        private CertificateValidationConfiguration _configuration;

        private readonly ICertificateValidationConfigurationProvider _configurationProvider;
        public CertificateValidator(ICertificateValidationConfigurationProvider configurationProvider)
        {
            if (configurationProvider == null)
                throw new ArgumentNullException("configurationProvider");

            this._configurationProvider = configurationProvider;
        }

        public string FederationPartyId { get; private set; }

        public X509CertificateValidationMode X509CertificateValidationMode
        {
            get
            {
                var configuration = this._configurationProvider.GetConfiguration(this.FederationPartyId);
                if (configuration == null)
                    throw new ArgumentNullException("certificateValidationConfiguration");

                return configuration.X509CertificateValidationMode;
            }
        }
        
        public void SetFederationPartyId(string federationPartyId)
        {
            this.FederationPartyId = federationPartyId;
        }

        public bool Validate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            this.FederationPartyId = FederationPartyIdentifierHelper.GetFederationPartyIdFromRequestOrDefault(sender as HttpWebRequest);
            var configiration = this.GetConfiguration();
            //ToDo: complete pinning validation. Moved to back log on 27/09/2017
            if(configiration.UsePinningValidation && configiration.BackchannelValidatorResolver != null)
            {
                try
                {
                    var type = configiration.BackchannelValidatorResolver.Type;
                    var instace = Activator.CreateInstance(type) as ICertificateValidatorResolver;
                    if(instace != null)
                    {
                        var validators = instace.Resolve();
                        
                        return true;
                    }
                }
                catch(Exception)
                {
                    return true;
                }
            }

            var context = new BackchannelCertificateValidationContext(certificate, chain, sslPolicyErrors);

            //default rule. No validation
            Func<BackchannelCertificateValidationContext, Task> seed = x =>
            {
                x.Validated();
                return Task.CompletedTask;
            };

            var rules = BackchannelCertificateValidationRulesFactory.GetRules(configiration);
            var validationDelegate = rules.Aggregate(seed, (f, next) => new Func<BackchannelCertificateValidationContext, Task>(c => next.Validate(c, f)));
            var task = validationDelegate(context);
            task.Wait();
            return context.IsValid;
        }
        
        public override void Validate(X509Certificate2 certificate)
        {
            var configiration = this.GetConfiguration();
            var context = new CertificateValidationContext(certificate);
            Func<CertificateValidationContext, Task> seed = x => Task.CompletedTask;

            var rules = CertificateValidationRulesFactory.GetRules(configiration);
            var validationDelegate = rules.Aggregate(seed, (f, next) => new Func<CertificateValidationContext, Task>(c => next.Validate(c, f)));
            var task = validationDelegate(context);
            task.Wait();
        }
        
        private CertificateValidationConfiguration GetConfiguration()
        {
            if (this._configuration == null)
            {
                this._configuration = this._configurationProvider.GetConfiguration(this.FederationPartyId);
            }
            if (this._configuration == null)
                throw new InvalidOperationException("CertificateValidationConfiguration is null!");

            return this._configuration;
        }
    }
}