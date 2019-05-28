# Integration Account Runtime Sync
LogicApps ARM Templates provided in folder (/IntegrationAccountSynchronization) for runtime sync(Control Numbers and MIC Algorithm) from primary to secondary region Integration account. Note these LogicApps scheduled to run every 3 mins, change the interval as needed.
  - AS2 Synchronization (MIC Algorithm) 
  - X12 Synchronization (Control Numbers Sync)
  - EDIFACT Synchronization (Control Numbers Sync)

# Integration Account Artifacts Sync
This PowerShell Azure Automation script (IntegrationAccountArtifactsSync.ps1) for sync artifacts mentioned below from primary to secondary region Integration Account. Make sure to schedule the Azure Automation Powershell Job as per your need to sync artifacts. More detailed instructions given in Powershell script notes, please check.
  - Schemas
  - Maps
  - Certificates
  - Partners
  - Agreements
  - Assembly
  - Batch Configurations
  - RosettaNet PIP
# 
