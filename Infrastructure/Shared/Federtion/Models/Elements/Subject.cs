﻿using System;
using System.Xml.Serialization;

namespace Federation.Protocols.Request.Elements
{
    /// <summary>
    /// The optional &lt;Subject&gt; element specifies the principal that is the subject of all of the (zero or more)
    /// statements in the assertion.
    /// </summary>
    [Serializable]
    [XmlType(Namespace = Saml20Constants.Assertion)]
    [XmlRoot(ElementName, Namespace = Saml20Constants.Assertion, IsNullable = false)]
    public class Subject
    {
        /// <summary>
        /// The XML Element name of this class
        /// </summary>
        public const string ElementName = "Subject";

        #region Elements

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>The items.</value>
        [XmlElement(BaseIdAbstract.ElementName, typeof(BaseIdAbstract), Order = 1)]
        [XmlElement(EncryptedElement.ElementName, typeof(EncryptedElement), Order = 1)]
        [XmlElement(NameId.ElementName, typeof(NameId), Order = 1)]
        [XmlElement(SubjectConfirmation.ElementName, typeof(SubjectConfirmation), Order = 1)]
        public object[] Items { get; set; }

        #endregion
    }
}