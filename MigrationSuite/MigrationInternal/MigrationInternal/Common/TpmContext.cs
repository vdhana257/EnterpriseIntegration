//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.ApplicationServer.Integration.PartnerManagement
{
    public enum RelationshipCardinality
    {
        OneToOne,
        OneToMany,
        ManyToOne,
        ManyToMany
    }

    partial class TpmContext
    {
        public void RelateEntities<T1, T2>(
            T1 entity1,
            T2 entity2,
            string entity1NavigationPropertyName,
            string entity2NavigationPropertyName,
            RelationshipCardinality cardinality)
        {
            switch (cardinality)
            {
                case RelationshipCardinality.OneToOne:
                    this.SetLink(entity1, entity1NavigationPropertyName, entity2);
                    this.SetLink(entity2, entity2NavigationPropertyName, entity1);
                    break;
                case RelationshipCardinality.OneToMany:
                    this.AddLink(entity1, entity1NavigationPropertyName, entity2);
                    this.SetLink(entity2, entity2NavigationPropertyName, entity1);
                    break;
                case RelationshipCardinality.ManyToOne:
                    this.SetLink(entity1, entity1NavigationPropertyName, entity2);
                    this.AddLink(entity2, entity2NavigationPropertyName, entity1);
                    break;
                case RelationshipCardinality.ManyToMany:
                    this.AddLink(entity1, entity1NavigationPropertyName, entity2);
                    this.AddLink(entity2, entity2NavigationPropertyName, entity1);
                    break;
            }
        }
    }
}
