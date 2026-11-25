# Azure Blob Storage for Recam Media

## Local development with Azurite

| Setting | Value |
| --- | --- |
| Blob service endpoint | `http://127.0.0.1:10000/devstoreaccount1` |
| Container name | `recam-media` |
| Anonymous access | Disabled |
| Signed URL permissions | Read-only (`r`) |
| Signed URL lifetime | 15 minutes |

Both the listing details endpoint and the media list endpoint returned a signed
URL for the test image. The signed URL returned HTTP 200 with the `image/jpeg`
content type. Removing the SAS query string from the same URL returned HTTP 403.
Both API endpoints returned HTTP 401 when called without a bearer token.

## Azure access policy

Cloud deployment status: Pending verification in Azure.

1. Create the `recam-media` container with private access and disable anonymous
   blob access on the storage account.
2. Set `AzureBlobStorage__ServiceUri` to the storage account's HTTPS Blob service
   endpoint. Enable a managed identity for the App Service that runs the Recam API.
3. Assign the following Azure roles to that managed identity:

   | Role | Scope | Purpose |
   | --- | --- | --- |
   | Storage Blob Data Contributor | `recam-media` container | Upload, read, and delete media blobs. |
   | Storage Blob Delegator | Storage account | Request a user delegation key to sign read URLs. |

Recam checks the caller's access to the listing before issuing a read URL.
Each URL grants read access to one blob for 15 minutes. Anyone who receives
the URL can use it until it expires.

## Backup and recovery

Azure backup configuration and restore testing are pending.

### Before deployment

1. Enable blob versioning, blob soft delete, and container soft delete.
   Agree on retention periods with the project owner.
2. Configure vaulted backup for `recam-media` in Azure Backup. Record the
   backup schedule and retention period, then verify the first recovery point.

### Recover a media file

1. Find the affected `MediaAsset` record in SQL. Record its `Id`,
   `ListingCaseId`, and the blob name stored in `MediaUrl`.
2. If the blob was deleted or overwritten, locate its last known good version
   and copy that version back to the original blob name. If the container was
   deleted, restore the container first.
3. If the required version is unavailable, restore the container from a vaulted
   recovery point to a recovery storage account. Verify the recovered file,
   then copy it back to its original name in `recam-media`.
4. Request the listing as an authorized user to obtain a fresh signed URL.
   Verify that it returns HTTP 200, has the expected content type, and opens
   the correct file. Record the recovery point or version used and the result.