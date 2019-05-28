# EnterpriseIntegration

Enterprise Integration using Azure LogicApps and Integration Account.

# Integration Account Runtime Sync
LogicApps ARM Templates prvoided in folder (/IntegrationAccountSynchronization) to sync from primary to secondary region Integration account. These LogicApps scheduled to run every 3 mins, change interval if needed
  - AS2 Synchronization (MIC Algorithm) 
  - X12 Synchronization (Control Numbers Sync)
  - EDIFACT Synchronization (Control Numbers Sync)

# Integration Account Artifacts Sync
Integration Account Artifacts sync powershell Azure Automation script (IntegrationAccountArtifactsSync.ps1) used to sync from primary to secondary region Integration account with below artifacts. 
  - Schemas
  - Maps
  - Certificates
  - Partners
  - Agreements
  - Assembly
  - Batch Configurations
  - RosettaNet PIP
# 
