﻿using Bit.Core.SecretsManager.Entities;

namespace Bit.Core.SecretsManager.Models.Data;

public class ApiKeyDetails : ApiKey
{
    public string ClientSecret { get; set; } // Deprecated as of 2023-05-17

    protected ApiKeyDetails() { }

    protected ApiKeyDetails(ApiKey apiKey)
    {
        Id = apiKey.Id;
        ServiceAccountId = apiKey.ServiceAccountId;
        Name = apiKey.Name;
        ClientSecretHash = apiKey.ClientSecretHash;
        Scope = apiKey.Scope;
        EncryptedPayload = apiKey.EncryptedPayload;
        Key = apiKey.Key;
        ExpireAt = apiKey.ExpireAt;
        CreationDate = apiKey.CreationDate;
        RevisionDate = apiKey.RevisionDate;
    }
}

public class ServiceAccountApiKeyDetails : ApiKeyDetails
{
    public ServiceAccountApiKeyDetails()
    {

    }

    public ServiceAccountApiKeyDetails(ApiKey apiKey, Guid organizationId) : base(apiKey)
    {
        ServiceAccountOrganizationId = organizationId;
    }

    public Guid ServiceAccountOrganizationId { get; set; }
}
