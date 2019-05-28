<#
.SYNOPSIS
  Enterprise Integration Account Artifacts Sync from one region to another.
.DESCRIPTION
  Syncs Integration Account Artifacts from one region to another, this feature will be helpful to perform Disaster Recovery of Integration Platform.
  - Create Azure Automation Run As Account which will intern creates new Service Principal with contributor access in the subscription.
        * Make sure Azure Automation SP account has read access to a primary region of Integration Account and Key-Vault "Keys" Read, List, Recover and Backup permissions.
        * Make sure Azure Automation SP account has write/contributor access to a secondary region of Integration Account and respective Key-Vault "Keys" Read, List, Recover and Backup permissions.
  - Create Automation Variable's with String data type as shown below; These variables read during execution of PowerShell
        AutomationAccountName                       -    <<Hosted Automation Account Name>>
        AutomationRG                                -    <<Hosted Automation Account Resource Group Name>>        
        IntegrationAccountNamePrimary               -    <<Integration Account Name Primary>> 
        IntegrationAccountNameSecondary             -    <<Integration Account Name Secondary>>
        IntegrationAccountResourceGroupNamePrimary  -    <<Integration Account Resource Group Name Primary>>
        IntegrationAccountResourceGroupNameSecondary-    <<Integration Account Resource Group Name Secondary>>
        IntegrationAccountKeyVaultNameSecondary     -    <<Secondary Region Key Vault Name>>        
        IntegrationAccountSyncStorageAccountName    -    <<Sync Azure Stroage Account Name>>
        IntegrationAccountSyncStorageAccKey         -    <<Sync Azure Stroage Account Key>>
        IntegrationAccountSyncStorageContainerName  -    <<Azure Storage Container Name>> Example: intsync-artifacts
        LastSyncDate                                -    <<Last Sync Date in UTC>> Example: 2019-05-26T12:01:55.6672968Z
        TenantId                                    -    <<Orgnization Tenant ID>> Example: microsoft.onmicrosoft.com 
.PARAMETER filterCondition
    Artifacts Filter Condition criteria; By default, it will search all artifacts in Integration Account as we have given wildcard(*) char. Change filter condition as per your need.
.PARAMETER IsSyncProvisionRequired    
    Enable or Disable Sync Provision feature. By default enabled as True. Change this param False if you want just comparison of artifacts between regions.
.PARAMETER IsSchemaSyncRequired
    Enable or Disable Integration Account Schema Sync Provision feature. By default enabled as True, change this param False if you want to disable.
.PARAMETER IsMapSyncRequired    
    Enable or Disable Integration Account Maps(XSLT and Liquid) Sync Provision feature. By default enabled as True, change this param False if you want to disable.
.PARAMETER IsCertSyncRequired
    Enable or Disable Integration Account Certificates(public and private certificates) Sync Provision feature. By default enabled as True, change this param False if you want to disable.
.PARAMETER IsPartnerSyncRequired    
    Enable or Disable Integration Account Partner Sync Provision feature. By default enabled as True, change this param False if you want to disable.
.PARAMETER IsBatchConfigSyncRequired
    Enable or Disable Integration Account Batch Configuration Sync Provision feature. By default enabled as True, change this param False if you want to disable.
.PARAMETER IsRosettaNetPIPSyncRequired    
    Enable or Disable Integration Account RosettaNet PIP Sync Provision feature. By default enabled as True, change this param False if you want to disable.
.PARAMETER IsAssemblySyncRequired
    Enable or Disable Integration Account Custom Assembly(DLL) Sync Provision feature. By default enabled as True, change this param False if you want to disable.
.PARAMETER IsAgreementSyncRequired    
    Enable or Disable Integration Account Agreement Sync Provision feature. By default enabled as True, change this param False if you want to disable.
.PARAMETER DeleteExtraSecondaryResources    
    Deletes extra resources in the secondary region if it's not there in the primary region. By default enabled as True, change this param False if you want to disable.
.PARAMETER IsBlobStoreRequired    
    Stores all Sync changes in Azure Storage Blob container for the run. By default enabled as True, change this param False if you want to disable.
.PARAMETER IsDateBasedSync    
    To avoid full comparison of all artifacts changes between region, the date based sync has been introduced to improve performance of Sync.
    By default enabled as True, change this param False if you want to disable and enable full artifacts comparison.
.PARAMETER IsReportNeeded    
    Generates a comparison report(CSV) of two Integration Accounts regions. By default enabled as True, change this param False if you want to enable.
.INPUTS
  No mandatory input params as all provided with default values. Schedule this PowerShell script as Azure Automation Job with custom parameters as per need.
.OUTPUTS
  Two regions Integration Account Artifacts Synced.
.NOTES
  Version:        1.0
  Author:         Dhanaekar Vellore(dhvellor@microsoft.com)
  Creation Date:  5/27/2019
#>
param
(       
    [Parameter(Mandatory = $false)]
    [string] $filterCondition = "*",

    [Parameter(Mandatory = $false)]
    [bool] $IsSyncProvisionRequired = $true,

    [Parameter(Mandatory = $false)]
    [bool] $IsSchemaSyncRequired = $true,

    [Parameter(Mandatory = $false)]
    [bool] $IsMapSyncRequired = $true,

    [Parameter(Mandatory = $false)]
    [bool] $IsCertSyncRequired = $true,

    [Parameter(Mandatory = $false)]
    [bool] $IsPartnerSyncRequired = $true,

    [Parameter(Mandatory = $false)]
    [bool] $IsBatchConfigSyncRequired = $true,

    [Parameter(Mandatory = $false)]
    [bool] $IsRosettaNetPIPSyncRequired = $true,

    [Parameter(Mandatory = $false)]
    [bool] $IsAssemblySyncRequired = $true,

    [Parameter(Mandatory = $false)]
    [bool] $IsAgreementSyncRequired = $true,

    [Parameter(Mandatory = $false)]
    [bool] $DeleteExtraSecondaryResources = $true,

    [Parameter(Mandatory = $false)]
    [bool] $IsBlobStoreRequired = $true,
    
    [Parameter(Mandatory = $false)]
    [bool] $IsDateBasedSync = $true,

    [Parameter(Mandatory = $false)]
    [bool] $IsReportNeeded = $true    
)

Try {
    $Error.Clear()
    
    #--------------Login------------
    $connection = Get-AutomationConnection -Name AzureRunAsConnection
    Connect-AzureRmAccount -ServicePrincipal -Tenant $connection.TenantID -ApplicationID $connection.ApplicationID -CertificateThumbprint $connection.CertificateThumbprint | out-null
    $AzureContext = Select-AzureRmSubscription -SubscriptionId $connection.SubscriptionID    
        
    #--------------Variables------------
    $scriptsMainPath = $env:TEMP    
    $filterCondition = "*{0}*" -f $filterCondition       

    # Get Last Sync Date from Automation Variable
    $LastSyncDate = (Get-AutomationVariable -Name 'LastSyncDate').ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")
    $DateInUTC = (Get-Date).ToUniversalTime()
    $CurrentSyncDate = $DateInUTC.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")
    
    $resourceGroupNamePrimary = Get-AutomationVariable -Name 'IntegrationAccountResourceGroupNamePrimary'
    $resourceGroupNameSecondary = Get-AutomationVariable -Name 'IntegrationAccountResourceGroupNameSecondary'
    $IntegrationAccountName = Get-AutomationVariable -Name 'IntegrationAccountNamePrimary'
    $IntegrationAccountNameSec = Get-AutomationVariable -Name 'IntegrationAccountNameSecondary'

    $AutomationRG = Get-AutomationVariable -Name 'AutomationRG'
    $AutomationAccountName = Get-AutomationVariable -Name 'AutomationAccountName'
    $TenantId = Get-AutomationVariable -Name 'TenantId'
    $KeyVaultSecActualName = Get-AutomationVariable -Name 'IntegrationAccountKeyVaultNameSecondary'
    
    # Create Azure Storage Context
    $StorageAccountName = Get-AutomationVariable -Name 'IntegrationAccountSyncStorageAccountName'
    $StorageContainerName = Get-AutomationVariable -Name 'IntegrationAccountSyncStorageContainerName'
    $StorageAccKey = Get-AutomationVariable -Name 'IntegrationAccountSyncStorageAccKey'

    $FolderName = "SyncRun-{0}" -f $DateInUTC.ToString("MM-dd-yyyy_HH_mm_ss")
    $filePathMainUnique = "{0}\{1}" -f $scriptsMainPath, $FolderName

    if (!(Test-Path $filePathMainUnique)) {
        New-Item -ItemType Directory $filePathMainUnique | Out-Null
    }

    $ReportFileName = "{0}\Report.csv" -f $filePathMainUnique

    #--------------Custom Functions------------
    
    Function New-AzureRestAuthorizationHeaderUsingCert {
        param(
            $TenantID,
            $CertificateThumbprint,        
            $ApplicationId
        )
        $AuthUri = "https://login.windows.net/$TenantID/oauth2/authorize"
        $clientCertificate = Get-Item -Path Cert:\CurrentUser\My\$CertificateThumbprint
        $authenticationContext = New-Object -TypeName Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext -ArgumentList $AuthUri
        $certificateCredential = New-Object -TypeName Microsoft.IdentityModel.Clients.ActiveDirectory.ClientAssertionCertificate -ArgumentList ($ApplicationId, $clientCertificate)
        $resource = "https://management.core.windows.net/" 
        $authToken = $authenticationContext.AcquireTokenAsync($resource, $certificateCredential).Result
        $authHeader = @{
            'Content-Type'  = 'application/json'
            'Authorization' = $authToken.CreateAuthorizationHeader()
        }
        $authHeader
    }    

    Function Download-AzureContent-File {
        Param 
        (
            [Parameter(Mandatory = $true)][String]$ContentUri, 
            [Parameter(Mandatory = $true)][String]$FilePath,
            [Parameter(Mandatory = $false)][String]$Type = ""
        )

        $req = [System.Net.WebRequest]::Create($ContentUri)        
        $resp = $req.GetResponse()
        $reader = new-object System.IO.StreamReader($resp.GetResponseStream())
        $content = $reader.ReadToEnd()
        [System.IO.File]::WriteAllText($FilePath, $content)      
    }

    Function Upload-FileToAzureStorageContainer { 
        param(
            $StorageAccountName,
            $StorageAccountKey,
            $ContainerName,
            $sourceFileRootDirectory,
            $Force
        )

        $ctx = New-AzureStorageContext -StorageAccountName $StorageAccountName -StorageAccountKey $StorageAccountKey
        $container = Get-AzureStorageContainer -Name $ContainerName -Context $ctx -ErrorAction Ignore

        if (!($container)) {
            New-AzureStorageContainer -Name $ContainerName -Context $ctx -Permission Off -ErrorAction Ignore | Out-Null
            $container = Get-AzureStorageContainer -Name $ContainerName -Context $ctx -ErrorAction Ignore
        }

        $filesToUpload = Get-ChildItem $sourceFileRootDirectory -Recurse -File

        foreach ($x in $filesToUpload) {
            $targetPath = ($x.fullname.Substring($sourceFileRootDirectory.Length + 1)).Replace("\", "/")

            Write-Verbose "Uploading $("\" + $x.fullname.Substring($sourceFileRootDirectory.Length + 1)) to $($container.CloudBlobContainer.Uri.AbsoluteUri + "/" + $targetPath)"
            Set-AzureStorageBlobContent -File $x.fullname -Container $ContainerName -Blob $targetPath -Context $ctx -Force:$Force -ErrorAction Ignore | Out-Null
        }
    
    }

    Function Delete-Resource-Api() {
        Param 
        ( 
            [Parameter(Mandatory = $true)][String]$ResourceName, 
            [Parameter(Mandatory = $true)][String]$ResourceType,         
            [Parameter(Mandatory = $true)][String]$IntegrationAccountName, 
            [Parameter(Mandatory = $true)][String]$resourceGroupName,
            [Parameter(Mandatory = $true)][Hashtable]$authHeader
        )   
        $IntegrationAccountResourceId = (Get-AzureRmIntegrationAccount -Name $IntegrationAccountName -ResourceGroupName $resourceGroupName).Id
                
        # Construct the Uri
        $RestEndpoint = 'https://management.azure.com' + $IntegrationAccountResourceId + '/' + $ResourceType + '/' + $ResourceName + '?api-version=2016-06-01'
    
        Write-Verbose '-------------------------------------------------------------------------------------'
        Write-Verbose "Attempting to delete '$ResourceName'..."

        # Invoke the REST method.
        try {
            $response = Invoke-WebRequest -Method Delete -Uri $RestEndpoint -Headers $authHeader -UseBasicParsing
            if ($response.StatusCode -notin ("200", "201", "202")) {                
                $response
            } 
            write-Verbose "Successfully deleted '$ResourceName'"
            Write-Output "$ResourceName $ResourceType deleted in secondary region as it's not found in primary region."
            Write-Verbose '-------------------------------------------------------------------------------------'
        }  
        catch {
            $_.Exception.Response
            $errorOccured = $true
            Write-Verbose '******************************************************************************'
            write-output "Failed to delete '$ResourceName'"
            Write-Verbose '******************************************************************************'
        }
    }

    Function New-Provision-Resource-Api() {
        Param 
        ( 
            [Parameter(Mandatory = $true)][String]$ResourceName, 
            [Parameter(Mandatory = $true)][String]$ResourceType, 
            [Parameter(Mandatory = $true)][String]$ResourceFilePath, 
            [Parameter(Mandatory = $true)][String]$IntegrationAccountName, 
            [Parameter(Mandatory = $true)][String]$resourceGroupName,
            [Parameter(Mandatory = $true)][Hashtable]$authHeader
        )
            
        $ResourceJson = Get-Content $ResourceFilePath             
        $IntegrationAccountResourceId = (Get-AzureRmIntegrationAccount -Name $IntegrationAccountName -ResourceGroupName $resourceGroupName).Id
                
        # Construct the Uri
        $RestEndpoint = 'https://management.azure.com' + $IntegrationAccountResourceId + '/' + $ResourceType + '/' + $ResourceName + '?api-version=2016-06-01'
    
        Write-Verbose '-------------------------------------------------------------------------------------'
        Write-Verbose "Attempting to Provision '$ResourceName'..."

        # Invoke the REST method.
        try {
            $response = Invoke-WebRequest -Method Put -Uri $RestEndpoint -Body $ResourceJson -Headers $authHeader -UseBasicParsing
            if ($response.StatusCode -notin ("200", "201", "202")) {                
                $response
            } 
            write-output "Successfully provisioned '$ResourceName'"
            Write-Verbose '-------------------------------------------------------------------------------------'
        }  
        catch {
            $_.Exception.Response
            $errorOccured = $true
            Write-Verbose '******************************************************************************'
            write-output "Failed to provision '$ResourceName'"
            Write-Verbose '******************************************************************************'
        }
    }

    Function New-Sync-Provision-Resource() {
        Param 
        (         
            [Parameter(Mandatory = $true)][String]$ResourceType,         		
            [Parameter(Mandatory = $true)][String]$IntegrationAccountName, 
            [Parameter(Mandatory = $true)][String]$IntegrationAccountNameSec, 
            [Parameter(Mandatory = $true)][String]$resourceGroupNamePrimary,        
            [Parameter(Mandatory = $true)][String]$resourceGroupNameSecondary,
            [Parameter(Mandatory = $true)][String]$filterCondition,
            [Parameter(Mandatory = $true)][String]$filePathMainUnique,
            [Parameter(Mandatory = $true)][Hashtable]$authHeader,
            [Parameter(Mandatory = $false)][bool]$IsSyncProvisionRequired = $true,
            [Parameter(Mandatory = $false)][bool]$DeleteExtraSecondaryResources = $true,
            [Parameter(Mandatory = $false)][bool]$IsDateBasedSync = $true,
            [Parameter(Mandatory = $false)][bool]$IsReportNeeded = $true,
            [Parameter(Mandatory = $false)][String]$KeyVaultNameSec = ""
        )
    
        if ($ResourceType -notin ("partners", "agreements", "batchConfigurations", "certificates","rosettaNetProcessConfigurations")) {
            write-output "Invalid Integration Resource Type : $ResourceType " 
            return;
        }

        $resouceTypeVal = "Microsoft.Logic/integrationAccounts/$ResourceType"

        $ItemsAll = Get-AzureRmResource -ResourceGroupName $resourceGroupNamePrimary -ResourceType $resouceTypeVal -ResourceName $IntegrationAccountName -ApiVersion 2016-06-01
        $ItemsAllSec = Get-AzureRmResource -ResourceGroupName $resourceGroupNameSecondary -ResourceType $resouceTypeVal -ResourceName $IntegrationAccountNameSec -ApiVersion 2016-06-01

        $resourceFilePathMain = "$filePathMainUnique\\$ResourceType"

        if (!(Test-Path $resourceFilePathMain)) {
            New-Item -ItemType Directory $resourceFilePathMain | Out-Null
        }

        $ItemsFilter = $ItemsAll | Where-Object {$_.Name -like $filterCondition}

        $IntegrationAccountResourceId = (Get-AzureRmIntegrationAccount -Name $IntegrationAccountName -ResourceGroupName $resourceGroupNamePrimary).Id
        $IntegrationAccountResourceIdSec = (Get-AzureRmIntegrationAccount -Name $IntegrationAccountNameSec -ResourceGroupName $resourceGroupNameSecondary).Id

        [int]$SyncCount = 0
        [int]$FilterCount = 0

        $ItemsFilter | ForEach-Object {
            $item = $_
            $ResourceName = $item.Name
            $FilterCount++

            $IsResourceChanged = $false
            $resourceExtn = "json"           

            $ResourceFilePath = "$resourceFilePathMain\\{0}.{1}" -f $ResourceName, $resourceExtn
            $ResourceFilePathSec = "$resourceFilePathMain\\{0}_Sec.{1}" -f $ResourceName, $resourceExtn
            
            $IsSecExist = $ItemsAllSec | Where-Object {$_.Name -eq $ResourceName}

            $CsvRow = [PSCustomObject]@{Type = $ResourceType; Name = $ResourceName; Compliant = 'True'; Primary = 'True'; Secondary = 'True'}            

            if ($IsSecExist) {

                if ($IsDateBasedSync -eq $false -or ($IsDateBasedSync -eq $true -and ($item.Properties.changedTime -gt $LastSyncDate -or $IsSecExist.Properties.changedTime -gt $LastSyncDate))) {
                                                                        
                    $RestEndpoint = 'https://management.azure.com' + $IntegrationAccountResourceId + '/' + $ResourceType + '/' + $ResourceName + '?api-version=2016-06-01'
                    $resourceItem = Invoke-WebRequest -Method Get -Uri $RestEndpoint -Headers $authHeader -UseBasicParsing

                    $ResourceJson = ConvertFrom-Json -InputObject $resourceItem.Content                        

                    $ResourceJson.id = "dummy"
                    $ResourceJson.properties.createdTime = "dummy"
                    $ResourceJson.properties.changedTime = "dummy"
                        
                    $RestEndpointSec = 'https://management.azure.com' + $IntegrationAccountResourceIdSec + '/' + $ResourceType + '/' + $ResourceName + '?api-version=2016-06-01'
                    $resourceItemSec = Invoke-WebRequest -Method Get -Uri $RestEndpointSec -Headers $authHeader -UseBasicParsing

                    $ResourceJsonSec = ConvertFrom-Json -InputObject $resourceItemSec.Content
                    $ResourceJsonSec.id = "dummy"
                    $ResourceJsonSec.properties.createdTime = "dummy"
                    $ResourceJsonSec.properties.changedTime = "dummy"

                    $IsPrivateCert = $false
                    $IsPrivateCertChanged = $false

                    # Private Certificates provision is disabled, to enable change to $IsPrivateCertChanged = $true below
                    if (($ResourceType -eq 'certificates') -and ($ResourceJson.properties.key.keyName)) {                        
                        $IsPrivateCert = $true                           

                        $KeyVaultName = $ResourceJson.properties.key.keyVault.name
                        $KeyVaultKeyName = $ResourceJson.properties.key.keyName                                              
                        
                        $itemPrimary = Get-AzureKeyVaultKey -VaultName $KeyVaultName -Name $KeyVaultKeyName
                        $itemSec = Get-AzureKeyVaultKey -VaultName $ResourceJsonSec.properties.key.keyVault.name -Name $ResourceJsonSec.properties.key.keyName                                               

                        if ($KeyVaultKeyName -ne $ResourceJsonSec.properties.key.keyName) {
                            $IsPrivateCertChanged = $false                            
                            write-output "Warning:: $ResourceName private certificate key-vault key name($KeyVaultKeyName) is different in secondary region."
                        }
                        elseif ($KeyVaultNameSec -ne $ResourceJsonSec.properties.key.keyVault.name) {
                            $IsPrivateCertChanged = $false
                            write-output "Warning:: $ResourceName private certificate key-vault in secondary region not using configured vault($KeyVaultNameSec)."
                        }
                        elseif (($itemPrimary.Expires -ne $itemSec.Expires) -or ($itemPrimary.Enabled -ne $itemSec.Enabled) -or ($itemPrimary.NotBefore -ne $itemSec.NotBefore) -or ($itemPrimary.PurgeDisabled -ne $itemSec.PurgeDisabled)) {                            
                            $IsPrivateCertChanged = $false
                            $itemPrimary
                            $itemSec
                            write-output "Warning:: $ResourceName private certificate properties differnt in secondary region as shown above."
                        }

                        #Upload primary cert if it's different                        
                        if (($IsSyncProvisionRequired -eq $true) -and ($IsPrivateCertChanged -eq $true)) {
                            $KeyVaultCertBackupFilePath = "$resourceFilePathMain\\{0}_{1}.txt" -f $ResourceName, $KeyVaultName
                                
                            Backup-AzureKeyVaultKey -VaultName $KeyVaultName -Name $KeyVaultKeyName -OutputFile $KeyVaultCertBackupFilePath -Force 

                            $VaultKey = Get-AzureKeyVaultKey -VaultName $KeyVaultNameSec -Name $KeyVaultKeyName -ErrorAction Ignore
                            if ($VaultKey) {
                                Remove-AzureKeyVaultKey -VaultName $KeyVaultNameSec -Name $KeyVaultKeyName -Force
                            }
                            Restore-AzureKeyVaultKey -VaultName $KeyVaultNameSec -InputFile $KeyVaultCertBackupFilePath -ErrorAction Ignore
                        }

                        $ResourceJson.properties.key.keyVault.name = "dummy"
                        $ResourceJsonSec.properties.key.keyVault.name = "dummy"

                        $ResourceJson.properties.key.keyVault.id = "dummy"                        
                        $ResourceJsonSec.properties.key.keyVault.id = "dummy"

                        if ($ResourceJson.properties.key.keyVersion) {
                            $ResourceJson.properties.key.keyVersion = "dummy"
                        }
                        if ($ResourceJsonSec.properties.key.keyVersion) {
                            $ResourceJsonSec.properties.key.keyVersion = "dummy"
                        }                        
                    }

                    $IsDiffFound = Compare-Object -ReferenceObject ($ResourceJson | ConvertTo-Json -Depth 10 -Compress) -DifferenceObject ($ResourceJsonSec | ConvertTo-Json -Depth 10 -Compress)

                    if ($IsDiffFound -or ($IsPrivateCertChanged -eq $true)) {
                        
                        if (!($IsPrivateCertChanged) -or ($IsPrivateCertChanged -eq $false)) {
                            write-output "$ResourceName $ResourceType is different in secondary region."
                        }
                        $CsvRow.Compliant = 'Different'

                        $JsonSec = ConvertFrom-Json -InputObject $resourceItemSec.Content

                        $ResourceJson.id = $JsonSec.id
                        $ResourceJson.properties.createdTime = $JsonSec.properties.createdTime
                        $ResourceJson.properties.changedTime = $JsonSec.properties.changedTime

                        if (($ResourceType -eq 'certificates') -and ($IsPrivateCert -eq $true)) {
                            $ResourceJson.properties.key.keyVault.name = $KeyVaultNameSec
                            $ResourceJson.properties.key.keyVault.id = (Get-AzureRmKeyVault -VaultName $KeyVaultNameSec).ResourceId
                            #Disabling private certificate update
                            $IsResourceChanged = $false
                        }
                        else {
                            $IsResourceChanged = $true
                        }

                        #Store both primary and secondary file
                        $ResourceJson | ConvertTo-Json -Depth 10 | Out-File -FilePath $ResourceFilePath -Force
                        $JsonSec | ConvertTo-Json -Depth 10 | Out-File -FilePath $ResourceFilePathSec -Force
                    }
                    else {
                        Write-Verbose "$ResourceName  $ResourceType in sync, no difference found."
                        $SyncCount++                           
                    }
                }
                else {
                    $SyncCount++
                }

            }
            else {
                $CsvRow.Secondary = 'False'
                $CsvRow.Compliant = 'Not Found'

                $IntegrationAccountResourceId = (Get-AzureRmIntegrationAccount -Name $IntegrationAccountName -ResourceGroupName $resourceGroupNamePrimary).Id
                $RestEndpoint = 'https://management.azure.com' + $IntegrationAccountResourceId + '/' + $ResourceType + '/' + $ResourceName + '?api-version=2016-06-01'
                $resourceItem = Invoke-WebRequest -Method Get -Uri $RestEndpoint -Headers $authHeader -UseBasicParsing

                $ResourceJson = ConvertFrom-Json -InputObject $resourceItem.Content
                    
                # Private Certificates provision is disabled, to enable change to $IsResourceChanged = $true below
                if (($ResourceType -eq 'certificates') -and ($ResourceJson.properties.key.keyName)) {

                    $KeyVaultName = $ResourceJson.properties.key.keyVault.name
                    $KeyVaultKeyName = $ResourceJson.properties.key.keyName

                    $ResourceJson.properties.key.keyVault.name = $KeyVaultNameSec
                    $ResourceJson.properties.key.keyVault.id = (Get-AzureRmKeyVault -VaultName $KeyVaultNameSec).ResourceId
                    $ResourceJson.properties.key.keyVault.type = "Microsoft.KeyVault/vaults"

                    #Disabling private certificate update
                    $IsResourceChanged = $false
                    write-output "$ResourceName $ResourceType private certificate create disabled."

                    if ($IsSyncProvisionRequired -eq $true -and $IsResourceChanged -eq $true) {

                        $KeyVaultCertBackupFilePath = "$resourceFilePathMain\\{0}_{1}.txt" -f $ResourceName, $KeyVaultName                            
                            
                        Backup-AzureKeyVaultKey -VaultName $KeyVaultName -Name $KeyVaultKeyName -OutputFile $KeyVaultCertBackupFilePath -Force 
                        $VaultKey = Get-AzureKeyVaultKey -VaultName $KeyVaultNameSec -Name $KeyVaultKeyName -ErrorAction Ignore
                        if ($VaultKey) {
                            Remove-AzureKeyVaultKey -VaultName $KeyVaultNameSec -Name $KeyVaultKeyName -Force
                        }
                        Restore-AzureKeyVaultKey -VaultName $KeyVaultNameSec -InputFile $KeyVaultCertBackupFilePath -ErrorAction Ignore                                               
                    }
                }
                else {
                    $IsResourceChanged = $true
                }
                                        
                $ResourceJson | ConvertTo-Json -Depth 10 | Out-File -FilePath $ResourceFilePath -Force                         
                write-output "$ResourceName $ResourceType Not Found in secondary region."
            }                

            if (($IsResourceChanged -eq $true) -and ($IsSyncProvisionRequired -eq $true)) {
                New-Provision-Resource-Api -ResourceType $ResourceType -ResourceName $ResourceName -ResourceFilePath $ResourceFilePath -IntegrationAccountName $IntegrationAccountNameSec -resourceGroupName $resourceGroupNameSecondary -authHeader $authHeader
                $CsvRow.Secondary = 'True'
                $CsvRow.Compliant = 'True'
            }
            

            if ($IsReportNeeded -eq $true) {
                $CsvRow | Export-Csv -Path $ReportFileName -NoTypeInformation -Append -Force
            }
        }

        [int]$DeleteExtraResourceCount = 0
        if ($DeleteExtraSecondaryResources -eq $true) {
            $ItemsAllSec | Where-Object {$_.Name -like $filterCondition} | ForEach-Object {
                $item = $_    
                $itemFileName = $item.Name

                $ResourceFilePathSec = "$resourceFilePathMain\\{0}_Sec.json" -f $itemFileName

                $IsExist = $ItemsAll | Where-Object {$_.Name -eq $itemFileName }

                if (!($IsExist)) {                    
                    #Download file for reference
                    $IntegrationAccountResourceIdSec = (Get-AzureRmIntegrationAccount -Name $IntegrationAccountNameSec -ResourceGroupName $resourceGroupNameSecondary).Id
                    $RestEndpointSec = 'https://management.azure.com' + $IntegrationAccountResourceIdSec + '/' + $ResourceType + '/' + $ResourceName + '?api-version=2016-06-01'
                    $resourceItemSec = Invoke-WebRequest -Method Get -Uri $RestEndpointSec -Headers $authHeader -UseBasicParsing
                    ConvertFrom-Json -InputObject $resourceItemSec.Content | ConvertTo-Json -Depth 10 | Out-File -FilePath $ResourceFilePathSec -Force
                    
                    $DeleteExtraResourceCount++
                    Delete-Resource-Api -ResourceType $ResourceType -ResourceName $itemFileName -IntegrationAccountName $IntegrationAccountNameSec -resourceGroupName $resourceGroupNameSecondary -authHeader $authHeader
                    if ($IsReportNeeded -eq $true) {
                        $CsvRow = [PSCustomObject]@{Type = $ResourceType; Name = $itemFileName; Compliant = 'Extra Resource Deleted'; Primary = 'False'; Secondary = 'True'}
                        $CsvRow | Export-Csv -Path $ReportFileName -NoTypeInformation -Append -Force
                    }
                }

            }
        }

        If (($FilterCount -eq 0) -and ($DeleteExtraResourceCount -eq 0)) {
            write-output "No $ResourceType found with search criteria($filterCondition)."
        }
        elseif ($FilterCount -eq $SyncCount) {
            write-output "All $FilterCount $ResourceType in sync,no action needed."
        }

    }

    Function New-Sync-Provision-Schemas() {
        Param 
        (
            [Parameter(Mandatory = $true)][String]$IntegrationAccountName, 
            [Parameter(Mandatory = $true)][String]$IntegrationAccountNameSec, 
            [Parameter(Mandatory = $true)][String]$resourceGroupNamePrimary,        
            [Parameter(Mandatory = $true)][String]$resourceGroupNameSecondary,
            [Parameter(Mandatory = $true)][String]$filterCondition,
            [Parameter(Mandatory = $true)][String]$filePathMainUnique,
            [Parameter(Mandatory = $true)][Hashtable]$authHeader,
            [Parameter(Mandatory = $false)][bool]$IsSyncProvisionRequired = $true,      
            [Parameter(Mandatory = $false)][bool]$IsDateBasedSync = $true,      
            [Parameter(Mandatory = $false)][bool]$IsReportNeeded = $true,
            [Parameter(Mandatory = $false)][bool]$DeleteExtraSecondaryResources = $true
        )

        $schemaAll = Get-AzureRmResource -ResourceGroupName $resourceGroupNamePrimary -ResourceType Microsoft.Logic/integrationAccounts/schemas -ResourceName $IntegrationAccountName -ApiVersion 2016-06-01
        $schemaAllSec = Get-AzureRmResource -ResourceGroupName $resourceGroupNameSecondary -ResourceType Microsoft.Logic/integrationAccounts/schemas -ResourceName $IntegrationAccountNameSec -ApiVersion 2016-06-01

        $schemaFilePathMain = "$filePathMainUnique\\Schemas"

        if (!(Test-Path $schemaFilePathMain)) {
            New-Item -ItemType Directory $schemaFilePathMain | Out-Null
        }

        $SchemaFilter = $schemaAll | Where-Object {$_.Name -like $filterCondition}
        
        [int]$SyncCount = 0
        [int]$FilterCount = 0

        $SchemaFilter | ForEach-Object {
            $item = $_
            $SchemaName = $item.Name
            $FilterCount++

            $SchemasFilepath = "$schemaFilePathMain\\{0}.xsd" -f $SchemaName
            $SchemasFilepathSec = "$schemaFilePathMain\\{0}_Sec.xsd" -f $SchemaName

            $CsvRow = [PSCustomObject]@{Type = 'Schema'; Name = $SchemaName; Compliant = 'True'; Primary = 'True'; Secondary = 'True'}

            $itemSec = $schemaAllSec | Where-Object {$_.Name -eq $SchemaName}

            if ($itemSec) {

                if ($IsDateBasedSync -eq $false -or ($IsDateBasedSync -eq $true -and ($item.Properties.changedTime -gt $LastSyncDate -or $itemSec.Properties.changedTime -gt $LastSyncDate))) {
		    
                    if ($item.Properties.ContentLink.ContentHash.Value -ne $itemSec.Properties.ContentLink.ContentHash.Value) {
                        write-output "$SchemaName Schema is different in secondary region."    
                        $CsvRow.Compliant = 'Different'                    

                        if ($IsSyncProvisionRequired -eq $true) {

                            #Download both files
                            Download-AzureContent-File -ContentUri $item.Properties.ContentLink.Uri -FilePath $SchemasFilepath
                            Download-AzureContent-File -ContentUri $itemSec.Properties.ContentLink.Uri -FilePath $SchemasFilepathSec

                            Write-Verbose '-------------------------------------------------------------------------------------'
                            Write-Verbose "Attempting to Provision '$SchemaName'..."
                            try {

                                $updateItem = Set-AzureRmIntegrationAccountSchema -Name $IntegrationAccountNameSec `
                                    -ResourceGroupName $resourceGroupNameSecondary `
                                    -SchemaName $SchemaName `
                                    -SchemaFilePath $SchemasFilepath `
                                    -SchemaType Xml `
                                    -Force `
                                    -Verbose

                                if ($item.Properties.ContentLink.ContentHash.Value -ne $updateItem.ContentLink.ContentHash.Value) {
                                    throw "$SchemaName Uploaded schema hash is different, please publish correct schema file."
                                }

                                $CsvRow.Compliant = 'True'

                                write-output "Successfully provisioned '$SchemaName'"
                                Write-Verbose '-------------------------------------------------------------------------------------'
                            }  
                            catch {
                                $CsvRow.Compliant = 'Different-Provision failed'    
                                Write-Verbose '******************************************************************************'
                                write-output "Failed to provision '$SchemaName'"
                                Write-Verbose '******************************************************************************'
                            }
                        }

                    }
                    else {                        
                        Write-Verbose "$SchemaName Schema in sync, no difference found."
                        $SyncCount++                       
                    }
		
                }
                else {                    
                    $SyncCount++	
                }                
            }
            else {
                write-output "$SchemaName Schema Not Found in secondary region."                
                $CsvRow.Secondary = 'False'
                $CsvRow.Compliant = 'Not Found'

                if ($IsSyncProvisionRequired -eq $true) {

                    Download-AzureContent-File -ContentUri $item.Properties.ContentLink.Uri -FilePath $SchemasFilepath

                    Write-Verbose '-------------------------------------------------------------------------------------'
                    Write-Verbose "Attempting to Provision '$SchemaName'..."
                    try {

                        $newItem = New-AzureRmIntegrationAccountSchema -Name $IntegrationAccountNameSec `
                            -ResourceGroupName $resourceGroupNameSecondary `
                            -SchemaName $SchemaName `
                            -SchemaFilePath $SchemasFilepath `
                            -SchemaType Xml `
                            -Verbose


                        if ($item.Properties.ContentLink.ContentHash.Value -ne $newItem.ContentLink.ContentHash.Value) {
                            throw "$SchemaName Uploaded schema hash is different, please publish correct schema file."
                        }

                        $CsvRow.Secondary = 'True'
                        $CsvRow.Compliant = 'True'

                        write-output "Successfully provisioned '$SchemaName'"
                        Write-Verbose '-------------------------------------------------------------------------------------'
                    }  
                    catch {
                        $CsvRow.Compliant = 'Not Found-Provision failed'    
                        Write-Verbose '******************************************************************************'
                        write-output "Failed to provision '$SchemaName'"
                        Write-Verbose '******************************************************************************'
                    }
                }

            }
            
            if ($IsReportNeeded -eq $true) {
                $CsvRow | Export-Csv -Path $ReportFileName -NoTypeInformation -Append -Force
            }
        }

        [int]$DeleteExtraResourceCount = 0
        if ($DeleteExtraSecondaryResources -eq $true) {
            $schemaAllSec | Where-Object {$_.Name -like $filterCondition} | ForEach-Object {
                $item = $_
                $schemaFileName = $item.Name
                $SchemasFilepathSec = "$schemaFilePathMain\\{0}_Sec.xsd" -f $schemaFileName

                $IsExist = $schemaAll | Where-Object {$_.Name -eq $schemaFileName }

                if (!($IsExist)) {
                    $DeleteExtraResourceCount++
                    Download-AzureContent-File -ContentUri $item.Properties.contentLink.uri -FilePath $SchemasFilepathSec
                    Remove-AzureRmIntegrationAccountSchema -ResourceGroupName $resourceGroupNameSecondary -SchemaName $schemaFileName -Name $IntegrationAccountNameSec -Verbose -Force
                    Write-Output "$schemaFileName schema deleted in secondary region as it's not found in primary region."

                    if ($IsReportNeeded -eq $true) {
                        $CsvRow = [PSCustomObject]@{Type = 'schema'; Name = $schemaFileName; Compliant = 'Extra Resource Deleted'; Primary = 'False'; Secondary = 'True'}
                        $CsvRow | Export-Csv -Path $ReportFileName -NoTypeInformation -Append -Force
                    }
                }

            }
        }

        If (($FilterCount -eq 0) -and ($DeleteExtraResourceCount -eq 0)) {
            write-output "No schema found with search criteria($filterCondition)."
        }
        elseif ($FilterCount -eq $SyncCount) {
            write-output "All $FilterCount schema's in sync,no action needed."
        }

    }

    Function New-Sync-Provision-Assembly() {
        Param 
        (
            [Parameter(Mandatory = $true)][String]$IntegrationAccountName, 
            [Parameter(Mandatory = $true)][String]$IntegrationAccountNameSec, 
            [Parameter(Mandatory = $true)][String]$resourceGroupNamePrimary,        
            [Parameter(Mandatory = $true)][String]$resourceGroupNameSecondary,
            [Parameter(Mandatory = $true)][String]$filterCondition,
            [Parameter(Mandatory = $true)][String]$filePathMainUnique,
            [Parameter(Mandatory = $true)][Hashtable]$authHeader,
            [Parameter(Mandatory = $false)][bool]$IsSyncProvisionRequired = $true,        
            [Parameter(Mandatory = $false)][bool]$IsDateBasedSync = $true,
            [Parameter(Mandatory = $false)][bool]$IsReportNeeded = $true,    
            [Parameter(Mandatory = $false)][bool]$DeleteExtraSecondaryResources = $true
        )
	
        $assemblyAll = Get-AzureRmResource -ResourceGroupName $resourceGroupNamePrimary -ResourceType Microsoft.Logic/integrationAccounts/assemblies -ResourceName $IntegrationAccountName -ApiVersion 2016-06-01
        $assemblyAllSec = Get-AzureRmResource -ResourceGroupName $resourceGroupNameSecondary -ResourceType Microsoft.Logic/integrationAccounts/assemblies -ResourceName $IntegrationAccountNameSec -ApiVersion 2016-06-01

        $assemblyFilePathMain = "$filePathMainUnique\\Assembly"

        if (!(Test-Path $assemblyFilePathMain)) {
            New-Item -ItemType Directory $assemblyFilePathMain | Out-Null
        }

        $AssemblyFilter = $assemblyAll | Where-Object {$_.Name -like $filterCondition}

        [int]$SyncCount = 0
        [int]$FilterCount = 0

        $AssemblyFilter | ForEach-Object {
            $item = $_
            $AssemblyName = $item.Name            
            $FilterCount++
            
            $AssemblyFilepath = "$assemblyFilePathMain\\{0}.dll" -f $AssemblyName
            $AssemblyFilepathSec = "$assemblyFilePathMain\\{0}_Sec.dll" -f $AssemblyName

            $itemSec = $assemblyAllSec | Where-Object {$_.Name -eq $AssemblyName}

            $CsvRow = [PSCustomObject]@{Type = 'Assembly'; Name = $AssemblyName; Compliant = 'True'; Primary = 'True'; Secondary = 'True'}

            if ($itemSec) {                
	    	
                if ($IsDateBasedSync -eq $false -or ($IsDateBasedSync -eq $true -and ($item.Properties.changedTime -gt $LastSyncDate -or $itemSec.Properties.changedTime -gt $LastSyncDate))) {
		                   
                    if ($item.properties.ContentLink.ContentHash.Value -ne $itemSec.properties.ContentLink.ContentHash.Value) {
                        $CsvRow.Compliant = 'Different'
                        write-output "$AssemblyName Assembly is different in secondary region."
                        if ($IsSyncProvisionRequired -eq $true) {
                            #Download both primary and secondary region Assembly DLLs
                            Invoke-webrequest -OutFile $AssemblyFilepath $item.Properties.contentLink.uri -UseBasicParsing
                            Invoke-webrequest -OutFile $AssemblyFilepathSec $itemSec.Properties.contentLink.uri -UseBasicParsing

                            $Resource = Get-AzureRmResource -ResourceGroupName $resourceGroupNameSecondary -ResourceType Microsoft.Logic/integrationAccounts/assemblies -ResourceName "$IntegrationAccountNameSec/$AssemblyName" -ApiVersion 2016-06-01                     
                            $PropertiesObject = @{
                                assemblyName    = $AssemblyName
                                content         = [System.Convert]::ToBase64String([System.IO.File]::ReadAllBytes($AssemblyFilepath))
                                assemblyVersion = "0.0.0.0"
                                contentType     = "application/octet-stream"
                            }                        
                            Set-AzureRmResource -PropertyObject $PropertiesObject -ResourceGroupName $Resource.ResourceGroupName -ResourceType $Resource.ResourceType -ResourceName $Resource.ResourceName -ApiVersion 2016-06-01 -Force
                            write-output "Successfully provisioned '$AssemblyName'"
                            $CsvRow.Compliant = 'True'
                        }
                    }
                    else {                        
                        Write-Verbose "$AssemblyName Assembly in sync, no difference found."
                        $SyncCount++
                    }
		
                }
                else {                    
                    $SyncCount++		
                }
            }
            else {
                write-output "$AssemblyName Assembly Not Found in secondary region."
                $CsvRow.Secondary = 'False'
                $CsvRow.Compliant = 'Not Found'

                if ($IsSyncProvisionRequired -eq $true) {
                    #Download-AzureContent-File -ContentUri $item.properties.ContentLink.Uri -FilePath $AssemblyFilepath
                    Invoke-webrequest -OutFile $AssemblyFilepath $item.Properties.contentLink.uri -UseBasicParsing

                    $PropertiesObject = @{
                        assemblyName    = $AssemblyName
                        content         = [System.Convert]::ToBase64String([System.IO.File]::ReadAllBytes($AssemblyFilepath))
                        assemblyVersion = "0.0.0.0"
                        contentType     = "application/octet-stream"
                    }         
                    New-AzureRmResource -Location (Get-AzureRmResourceGroup -Name $resourceGroupNameSecondary).location -PropertyObject $PropertiesObject -ResourceGroupName $resourceGroupNameSecondary -ResourceType Microsoft.Logic/integrationAccounts/assemblies -ResourceName "$IntegrationAccountNameSec/$AssemblyName" -ApiVersion 2016-06-01 -Force                
                    write-output "Successfully provisioned '$AssemblyName'"
                    $CsvRow.Secondary = 'True'
                    $CsvRow.Compliant = 'True'
                }
            }

            if ($IsReportNeeded -eq $true) {
                $CsvRow | Export-Csv -Path $ReportFileName -NoTypeInformation -Append -Force
            }
        }

        If ($FilterCount -eq 0) {
            write-output "No Assembly found with search criteria($filterCondition)."
        }
        elseif ($FilterCount -eq $SyncCount) {
            write-output "All $FilterCount aseembly's in sync,no action needed."
        }
    }

    Function New-Sync-Provision-Maps() {
        Param 
        (
            [Parameter(Mandatory = $true)][String]$IntegrationAccountName, 
            [Parameter(Mandatory = $true)][String]$IntegrationAccountNameSec, 
            [Parameter(Mandatory = $true)][String]$resourceGroupNamePrimary,        
            [Parameter(Mandatory = $true)][String]$resourceGroupNameSecondary,
            [Parameter(Mandatory = $true)][String]$filterCondition,
            [Parameter(Mandatory = $true)][String]$filePathMainUnique,
            [Parameter(Mandatory = $true)][Hashtable]$authHeader,    
            [Parameter(Mandatory = $false)][bool]$IsSyncProvisionRequired = $true,
            [Parameter(Mandatory = $false)][bool]$IsDateBasedSync = $true,
            [Parameter(Mandatory = $false)][bool]$IsReportNeeded = $true,
            [Parameter(Mandatory = $false)][bool]$DeleteExtraSecondaryResources = $true
        )
	
        $mapAll = Get-AzureRmResource -ResourceGroupName $resourceGroupNamePrimary -ResourceType Microsoft.Logic/integrationAccounts/maps -ResourceName $IntegrationAccountName -ApiVersion 2016-06-01
        $mapAllSec = Get-AzureRmResource -ResourceGroupName $resourceGroupNameSecondary -ResourceType Microsoft.Logic/integrationAccounts/maps -ResourceName $IntegrationAccountNameSec -ApiVersion 2016-06-01

        $mapFilePathMain = "$filePathMainUnique\\Maps"

        if (!(Test-Path $mapFilePathMain)) {
            New-Item -ItemType Directory $mapFilePathMain | Out-Null
        }

        $MapFilter = $mapAll | Where-Object {$_.Name -like $filterCondition}
        [int]$SyncCount = 0
        [int]$FilterCount = 0

        $MapFilter | ForEach-Object {
            $item = $_
            $MapName = $item.Name
            $MapType = $item.Properties.mapType
            $FilterCount++
            $MapsFilepath = "$mapFilePathMain\\{0}.{1}" -f $MapName, $MapType
            $MapsFilepathSec = "$mapFilePathMain\\{0}_Sec.{1}" -f $MapName, $MapType

            $CsvRow = [PSCustomObject]@{Type = "Map - $MapType"; Name = $MapName; Compliant = 'True'; Primary = 'True'; Secondary = 'True'}

            if ($MapType -eq "Xslt") {

                $itemSec = $mapAllSec | Where-Object {$_.Name -eq $MapName}                

                if ($itemSec) {                    
                    if ($IsDateBasedSync -eq $false -or ($IsDateBasedSync -eq $true -and ($item.Properties.changedTime -gt $LastSyncDate -or $itemSec.Properties.changedTime -gt $LastSyncDate))) {
		
                        if ($item.Properties.ContentLink.ContentHash.Value -ne $itemSec.Properties.ContentLink.ContentHash.Value) {
                            write-output "$MapName Map is different in secondary region."
                            $CsvRow.Compliant = 'Different'

                            if ($IsSyncProvisionRequired -eq $true) {

                                #Download both files
                                Download-AzureContent-File -ContentUri $item.Properties.ContentLink.Uri -FilePath $MapsFilepath
                                Download-AzureContent-File -ContentUri $itemSec.Properties.ContentLink.Uri -FilePath $MapsFilepathSec

                                Write-Verbose '-------------------------------------------------------------------------------------'
                                Write-Verbose "Attempting to Provision '$MapName'..."
                                try {
                                    $updateItem = Set-AzureRmIntegrationAccountMap -Name $IntegrationAccountNameSec `
                                        -ResourceGroupName $resourceGroupNameSecondary `
                                        -MapName $MapName `
                                        -MapFilePath $MapsFilepath `
                                        -MapType Xslt `
                                        -Force `
                                        -Verbose

                                    if ($item.Properties.ContentLink.ContentHash.Value -ne $updateItem.ContentLink.ContentHash.Value) {
                                        throw "$MapName Uploaded Map hash is different, please publish correct map file."
                                    }

                                    write-output "Successfully provisioned '$MapName'"
                                    Write-Verbose '-------------------------------------------------------------------------------------'
                                    $CsvRow.Compliant = 'True'
                                }  
                                catch {    
                                    $CsvRow.Compliant = 'Different-Provision failed'   
                                    Write-Verbose '******************************************************************************'
                                    write-output "Failed to provision '$MapName'"
                                    Write-Verbose '******************************************************************************'
                                }
                            }

                        }
                        else {
                            Write-Verbose "$MapName Map in sync, no difference found."
                            $SyncCount++
                            #Delete the both files as there is no differnce.
                            #Remove-Item -Path $MapsFilepath -Force
                            #Remove-Item -Path $MapsFilepathSec -Force
                        }
                    }
                    else {
                        $SyncCount++
                    }
		
                }
                else {
                    write-output "$MapName Map Not Found in secondary region."
                    $CsvRow.Secondary = 'False'
                    $CsvRow.Compliant = 'Not Found'

                    if ($IsSyncProvisionRequired -eq $true) {
                        #Download the file
                        Download-AzureContent-File -ContentUri $item.Properties.ContentLink.Uri -FilePath $MapsFilepath

                        Write-Verbose '-------------------------------------------------------------------------------------'
                        Write-Verbose "Attempting to Provision '$MapName'..."
                        try {
                            $newItem = New-AzureRmIntegrationAccountMap -Name $IntegrationAccountNameSec `
                                -ResourceGroupName $resourceGroupNameSecondary `
                                -MapName $MapName `
                                -MapFilePath $MapsFilepath `
                                -MapType Xslt `
                                -Verbose

                            if ($item.Properties.ContentLink.ContentHash.Value -ne $newItem.ContentLink.ContentHash.Value) {
                                throw "$MapName Uploaded Map hash is different, please publish correct map file."
                            }

                            write-output "Successfully provisioned '$MapName'"
                            Write-Verbose '-------------------------------------------------------------------------------------'
                            $CsvRow.Secondary = 'True'
                            $CsvRow.Compliant = 'True'
                        }  
                        catch {    
                            Write-Verbose '******************************************************************************'
                            write-output "Failed to provision '$MapName'"
                            Write-Verbose '******************************************************************************'
                            $CsvRow.Secondary = 'False'
                            $CsvRow.Compliant = 'Not Found-Provision failed'
                        }
                    }

                }

            }
            else {
                #Liquid maps                
                $itemSec = $mapAllSec | Where-Object {$_.Name -eq $MapName}
                if ($itemSec) {                    
                    if ($item.properties.ContentLink.ContentHash.Value -ne $itemSec.properties.ContentLink.ContentHash.Value) {                        
                        $CsvRow.Compliant = 'Different'

                        write-output "$MapName Map is different in secondary region."
                        if ($IsSyncProvisionRequired -eq $true) {
                            Download-AzureContent-File -ContentUri $item.properties.ContentLink.Uri -FilePath $MapsFilepath
                            $Content = Get-Content -Path $MapsFilepath | Out-String
                            $Resource = Get-AzureRmResource -ResourceGroupName $resourceGroupNameSecondary -ResourceType Microsoft.Logic/integrationAccounts/maps -ResourceName "$IntegrationAccountNameSec/$MapName" -ApiVersion 2016-06-01                     
                            $PropertiesObject = @{
                                mapType     = "liquid"
                                content     = "$Content"
                                contentType = "text/plain"
                            }                        
                            Set-AzureRmResource -PropertyObject $PropertiesObject -ResourceGroupName $Resource.ResourceGroupName -ResourceType $Resource.ResourceType -ResourceName $Resource.ResourceName -ApiVersion 2016-06-01 -Force | out-null
                            write-output "Successfully provisioned '$MapName'"
                            $CsvRow.Compliant = 'True'
                        }
                    }
                    else {
                        Write-Verbose "$MapName Map in sync, no difference found."
                        $SyncCount++
                    }
                }
                else {
                    write-output "$MapName Map Not Found in secondary region."
                    $CsvRow.Secondary = 'False'
                    $CsvRow.Compliant = 'Not Found'
                    if ($IsSyncProvisionRequired -eq $true) {
                        Download-AzureContent-File -ContentUri $item.properties.ContentLink.Uri -FilePath $MapsFilepath
                        $Content = Get-Content -Path $MapsFilepath | Out-String
                        #Write-Host $Content
                        $PropertiesObject = @{
                            mapType     = "liquid"
                            content     = "$Content"
                            contentType = "text/plain"
                        }
                        New-AzureRmResource -Location (Get-AzureRmResourceGroup -Name $resourceGroupNameSecondary).location -PropertyObject $PropertiesObject -ResourceGroupName $resourceGroupNameSecondary -ResourceType Microsoft.Logic/integrationAccounts/maps -ResourceName "$IntegrationAccountNameSec/$MapName" -ApiVersion 2016-06-01 -Force | out-null                
                        write-output "Successfully provisioned '$MapName'"
                        $CsvRow.Secondary = 'True'
                        $CsvRow.Compliant = 'True'
                    }
                }
            }

            if ($IsReportNeeded -eq $true) {
                $CsvRow | Export-Csv -Path $ReportFileName -NoTypeInformation -Append -Force
            }
        }

        [int]$DeleteExtraResourceCount = 0
        if ($DeleteExtraSecondaryResources -eq $true) {
            $mapAllSec | Where-Object {$_.Name -like $filterCondition} | ForEach-Object {
                $item = $_
                $mapFileName = $item.Name
                $MapType = $item.Properties.mapType
                $MapsFilepathSec = "$mapFilePathMain\\{0}_Sec.{1}" -f $mapFileName, $item.Properties.mapType

                $IsExist = $mapAll | Where-Object {$_.Name -eq $mapFileName }

                if (!($IsExist)) {
                    $DeleteExtraResourceCount++
                    #Download the file for refernce
                    Download-AzureContent-File -ContentUri $item.Properties.contentLink.uri -FilePath $MapsFilepathSec -Type "Map"

                    Remove-AzureRmIntegrationAccountMap -ResourceGroupName $resourceGroupNameSecondary -MapName $mapFileName -Name $IntegrationAccountNameSec -Force
                    Write-Output "$mapFileName map deleted in secondary region as it's not found in primary region."

                    if ($IsReportNeeded -eq $true) {
                        $CsvRow = [PSCustomObject]@{Type = "Map - $MapType"; Name = $mapFileName; Compliant = 'Extra Resource Deleted'; Primary = 'False'; Secondary = 'True'}
                        $CsvRow | Export-Csv -Path $ReportFileName -NoTypeInformation -Append -Force
                    }
                }

            }
        }

        If (($FilterCount -eq 0) -and ($DeleteExtraResourceCount -eq 0)) {
            write-output "No Map found with search criteria($filterCondition)."
        }
        elseif ($FilterCount -eq $SyncCount) {
            write-output "All $FilterCount maps in sync,no action needed."
        }

    }

    #--------------Artifacts Sync------------
    Write-Output "Integration Account Artifacts Sync"
    Write-Output "Filter Search Criteria : $filterCondition"
    if ($IsDateBasedSync -eq $true) {
        Write-Output "Artifacts Created between $LastSyncDate to $CurrentSyncDate (UTC)"
    }
    Write-Output "Log (Azure Blob) : $StorageAccountName\$StorageContainerName\$FolderName"
    Write-Output "Note:= Private Certificate sync disabled in code, please enable if needed."

    #Auth Header   
    $authHeader = New-AzureRestAuthorizationHeaderUsingCert -ApplicationId $connection.ApplicationID -TenantID $TenantId -CertificateThumbprint $connection.CertificateThumbprint 

    if ($IsSchemaSyncRequired -eq $true) {
        Write-Output "------------------------------------------------------Schemas Sync Start-------------------------------------------------------------"
        New-Sync-Provision-Schemas -IntegrationAccountName $IntegrationAccountName -IntegrationAccountNameSec $IntegrationAccountNameSec -resourceGroupNamePrimary $resourceGroupNamePrimary -resourceGroupNameSecondary $resourceGroupNameSecondary -filterCondition $filterCondition -filePathMainUnique $filePathMainUnique -authHeader $authHeader -IsSyncProvisionRequired $IsSyncProvisionRequired -DeleteExtraSecondaryResources $DeleteExtraSecondaryResources -IsDateBasedSync $IsDateBasedSync -IsReportNeeded $IsReportNeeded
        Write-Output "------------------------------------------------------Schemas Sync End---------------------------------------------------------------"
    }

    if ($IsMapSyncRequired -eq $true) {
        Write-Output "------------------------------------------------------Maps Sync Start----------------------------------------------------------------" 
        New-Sync-Provision-Maps -IntegrationAccountName $IntegrationAccountName -IntegrationAccountNameSec $IntegrationAccountNameSec -resourceGroupNamePrimary $resourceGroupNamePrimary -resourceGroupNameSecondary $resourceGroupNameSecondary -filterCondition $filterCondition -filePathMainUnique $filePathMainUnique -authHeader $authHeader -IsSyncProvisionRequired $IsSyncProvisionRequired -DeleteExtraSecondaryResources $DeleteExtraSecondaryResources -IsDateBasedSync $IsDateBasedSync -IsReportNeeded $IsReportNeeded
        Write-Output "------------------------------------------------------Maps Sync End------------------------------------------------------------------" 
    }
    if ($IsAssemblySyncRequired -eq $true) {
        Write-Output "------------------------------------------------------Assembly Sync Start------------------------------------------------------------" 
        New-Sync-Provision-Assembly -IntegrationAccountName $IntegrationAccountName -IntegrationAccountNameSec $IntegrationAccountNameSec -resourceGroupNamePrimary $resourceGroupNamePrimary -resourceGroupNameSecondary $resourceGroupNameSecondary -filterCondition $filterCondition -filePathMainUnique $filePathMainUnique -authHeader $authHeader -IsSyncProvisionRequired $IsSyncProvisionRequired -DeleteExtraSecondaryResources $DeleteExtraSecondaryResources -IsDateBasedSync $IsDateBasedSync -IsReportNeeded $IsReportNeeded
        Write-Output "------------------------------------------------------Assembly Sync End--------------------------------------------------------------" 
    }

    if ($IsCertSyncRequired -eq $true) {
        Write-Output "------------------------------------------------------Certficates Sync Start---------------------------------------------------------" 
        New-Sync-Provision-Resource -ResourceType "certificates" -IntegrationAccountName $IntegrationAccountName -IntegrationAccountNameSec $IntegrationAccountNameSec -resourceGroupNamePrimary $resourceGroupNamePrimary -resourceGroupNameSecondary $resourceGroupNameSecondary -filterCondition $filterCondition -filePathMainUnique $filePathMainUnique -authHeader $authHeader -IsSyncProvisionRequired $IsSyncProvisionRequired -DeleteExtraSecondaryResources $DeleteExtraSecondaryResources -KeyVaultNameSec $KeyVaultSecActualName -IsDateBasedSync $IsDateBasedSync -IsReportNeeded $IsReportNeeded       
        Write-Output "------------------------------------------------------Certficates Sync End-----------------------------------------------------------" 
    }

    if ($IsPartnerSyncRequired -eq $true) {
        Write-Output "------------------------------------------------------Partners Sync Start------------------------------------------------------------" 
        New-Sync-Provision-Resource -ResourceType "partners" -IntegrationAccountName $IntegrationAccountName -IntegrationAccountNameSec $IntegrationAccountNameSec -resourceGroupNamePrimary $resourceGroupNamePrimary -resourceGroupNameSecondary $resourceGroupNameSecondary -filterCondition $filterCondition -filePathMainUnique $filePathMainUnique -authHeader $authHeader -IsSyncProvisionRequired $IsSyncProvisionRequired -DeleteExtraSecondaryResources $DeleteExtraSecondaryResources -IsDateBasedSync $IsDateBasedSync -IsReportNeeded $IsReportNeeded
        Write-Output "------------------------------------------------------Partners Sync End--------------------------------------------------------------" 
    }

    if ($IsBatchConfigSyncRequired -eq $true) {
        Write-Output "------------------------------------------------------Batch Configurations Sync Start------------------------------------------------" 
        New-Sync-Provision-Resource -ResourceType "batchConfigurations" -IntegrationAccountName $IntegrationAccountName -IntegrationAccountNameSec $IntegrationAccountNameSec -resourceGroupNamePrimary $resourceGroupNamePrimary -resourceGroupNameSecondary $resourceGroupNameSecondary -filterCondition $filterCondition -filePathMainUnique $filePathMainUnique -authHeader $authHeader -IsSyncProvisionRequired $IsSyncProvisionRequired -DeleteExtraSecondaryResources $DeleteExtraSecondaryResources -IsDateBasedSync $IsDateBasedSync -IsReportNeeded $IsReportNeeded
        Write-Output "------------------------------------------------------Batch Configurations Sync End--------------------------------------------------" 
    }

    if ($IsRosettaNetPIPSyncRequired -eq $true) {
        Write-Output "------------------------------------------------------RosettaNet PIP Sync Start------------------------------------------------------" 
        New-Sync-Provision-Resource -ResourceType "rosettaNetProcessConfigurations" -IntegrationAccountName $IntegrationAccountName -IntegrationAccountNameSec $IntegrationAccountNameSec -resourceGroupNamePrimary $resourceGroupNamePrimary -resourceGroupNameSecondary $resourceGroupNameSecondary -filterCondition $filterCondition -filePathMainUnique $filePathMainUnique -authHeader $authHeader -IsSyncProvisionRequired $IsSyncProvisionRequired -DeleteExtraSecondaryResources $DeleteExtraSecondaryResources -IsDateBasedSync $IsDateBasedSync -IsReportNeeded $IsReportNeeded
        Write-Output "------------------------------------------------------RosettaNet PIP Sync End--------------------------------------------------------" 
    }    

    if ($IsAgreementSyncRequired -eq $true) {
        Write-Output "------------------------------------------------------Agreements Sync Start----------------------------------------------------------" 
        New-Sync-Provision-Resource -ResourceType "agreements" -IntegrationAccountName $IntegrationAccountName -IntegrationAccountNameSec $IntegrationAccountNameSec -resourceGroupNamePrimary $resourceGroupNamePrimary -resourceGroupNameSecondary $resourceGroupNameSecondary -filterCondition $filterCondition -filePathMainUnique $filePathMainUnique -authHeader $authHeader -IsSyncProvisionRequired $IsSyncProvisionRequired -DeleteExtraSecondaryResources $DeleteExtraSecondaryResources -IsDateBasedSync $IsDateBasedSync -IsReportNeeded $IsReportNeeded
        Write-Output "------------------------------------------------------Agreements Sync End------------------------------------------------------------" 
    }

    #Store All Changed Artifacts in Blob finally
    if ($IsBlobStoreRequired -eq $true) {
        Upload-FileToAzureStorageContainer -StorageAccountName $StorageAccountName -StorageAccountKey $StorageAccKey -ContainerName $StorageContainerName -sourceFileRootDirectory $scriptsMainPath 
    }

    #Update Last Sync Date to Automation Variable
    if ($IsDateBasedSync -eq $true -and $IsSyncProvisionRequired -eq $true) {
        Set-AzureRmAutomationVariable -ResourceGroupName $AutomationRG -AutomationAccountName $AutomationAccountName -Name "LastSyncDate" -Value $CurrentSyncDate -Encrypted $False | Out-Null
    }    
}
Catch {
}

# Exception Handling
If ($Error) {   
    RETURN $Error 
}
