#
# GENERAL
#
variable "env" {
  type        = string
  description = "The deployment environment name (dev / test / prod)"
}

variable "deployment_index" {
  type        = string
  default     = "01"
  description = "Index counter of this instance, will be used on resource naming"
}

variable "component" {
  type        = string
  description = "Name of the application component, will be used on resource naming"
  validation {
    condition     = length(var.component) < 1 || length(var.deployment_index) + length(var.component) + (length(var.resource_name_infix) > 0 ? length(var.resource_name_infix) : 3) + length(var.env) + length(local.location_short) <= 24
    error_message = "The resulting resource name must not exceed 24 characters and may only containe lowercase alphanumeric characters. Adjust component, resource_name_infix and deployment_index accordingly"
  }
}

variable "resource_name_infix" {
  type        = string
  description = "Random resource name infix."

  validation {
    condition     = length(var.resource_name_infix) == 3
    error_message = "The random name infix must be quals 3 caracters"
  }
}

variable "location" {
  type        = string
  default     = "switzerlandnorth"
  description = "Deployment location"
}

variable "resource_group_name" {
  type        = string
  description = "Name of the Resource Group into which this Storage Account will be deployed"
}

variable "tags" {
  type        = map(string)
  default     = {}
  description = "Map of TAGs that will be added to the deployed resources"
}

variable "storage_types" {
  type        = set(string)
  default     = ["blob"]
  description = "Storage usage type. For each type the according Private DNS Zone name has to be provided in `dns_zone_names`. Allowed are `blob`, `table`, `queue` or `file`."

  validation {
    condition = alltrue([
      for type in var.storage_types :
      contains(["blob", "table", "queue", "file"], type)
    ])
    error_message = "Allowed are `blob`, `table`, `queue` or `file`."
  }
}

variable "account_tier" {
  type        = string
  default     = "Standard"
  description = "Defines the Tier to use for this storage account. Valid options are Standard and Premium. For BlockBlobStorage and FileStorage accounts only Premium is valid. Changing this forces a new resource to be created."
  validation {
    condition     = var.account_kind != "FileStorage" || var.account_tier == "Premium"
    error_message = "When using FileStorage, the account_tier must be `Premium`"
  }
  validation {
    condition     = var.account_kind != "BlockBlobStorage" || var.account_tier == "Premium"
    error_message = "When using BlockBlobStorage, the account_tier must be `Premium`"
  }
  validation {
    condition     = contains(["Standard", "Premium"], var.account_tier)
    error_message = "Account tier must be one of `Standard` or `Premium`"
  }
}

variable "account_replication_type" {
  type    = string
  default = "ZRS"

  validation {
    condition     = contains(["LRS", "GRS", "RAGRS", "ZRS", "GZRS", "RAGZRS"], var.account_replication_type)
    error_message = "Account replication type must be one of `LRS`, `GRS`, `RAGRS`, `ZRS`, `GZRS` or `RAGZRS`"
  }
  description = "The Replication type of the Storage Account. Must be one of `LRS`, `GRS`, `RAGRS`, `ZRS`, `GZRS` or `RAGZRS`."
}

variable "account_kind" {
  type    = string
  default = "StorageV2"

  validation {
    condition     = contains(["Storage", "StorageV2", "BlobStorage", "BlockBlobStorage", "FileStorage"], var.account_kind)
    error_message = "Account Kind must be one of `Storage`, `StorageV2`, `BlobStorage`, `BlockBlobStorage` or `FileStorage`"
  }
  description = "The account type. Must be one of `Storage`, `StorageV2`, `BlobStorage`, `BlockBlobStorage` or `FileStorage`"
}

variable "access_tier" {
  type        = string
  default     = "Hot"
  description = "The access type of the Storage Account. Must be one of `Hot`, `Cool`, `Cold` or `Premium`"
  validation {
    condition     = contains(["Hot", "Cool", "Cold", "Premium"], var.access_tier)
    error_message = "Access tier must be one of `Hot`, `Cool`, `Cold` or `Premium`"
  }
}

variable "sas_enabled" {
  type        = bool
  default     = true
  description = "Indicates whether the storage account permits requests to be authorized with the account access key via Shared Key. If false, then all requests, including shared access signatures, must be authorized with Azure Active Directory (Azure AD)."
  validation {
    condition     = length(var.storage_tables) <= 0 || var.sas_enabled == true
    error_message = "When using tables, shared access keys needs to be enabled, otherwise terraform can't create the tables. See the [provider configuration](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs#storage_use_azuread-1)."
  }
}

variable "diagnostics" {
  description = <<DESCRIPTION
Settings required to configure the log analytics integration. Specify the log_analytics_workspace_id property for the target Log Analytics Workspace.
To enable specific log category groups or categories, check the azure docs for specifics: https://learn.microsoft.com/en-us/azure/azure-monitor/platform/diagnostic-settings?WT.mc_id=Portal-Microsoft_Azure_Monitoring&tabs=portal#category-groups
DESCRIPTION
  type = object(
    {
      storage_account = optional(
        object({
          log_analytics_workspace_id = string
          log_metrics                = optional(list(string), ["Transaction"])
        })
      )
      blobs = optional(
        object({
          log_analytics_workspace_id = string
          log_metrics                = optional(list(string), ["Transaction"])
          log_category_groups        = optional(list(string), ["allLogs"])
          log_categories             = optional(list(string), [])
        })
      )
      files = optional(
        object({
          log_analytics_workspace_id = string
          log_metrics                = optional(list(string), ["Transaction"])
          log_category_groups        = optional(list(string), ["allLogs"])
          log_categories             = optional(list(string), [])
        })
      )
      tables = optional(
        object({
          log_analytics_workspace_id = string
          log_metrics                = optional(list(string), ["Transaction"])
          log_category_groups        = optional(list(string), ["allLogs"])
          log_categories             = optional(list(string), [])
      }))
      queues = optional(
        object({
          log_analytics_workspace_id = string
          log_metrics                = optional(list(string), ["Transaction"])
          log_category_groups        = optional(list(string), ["allLogs"])
          log_categories             = optional(list(string), [])
      }))
    }
  )
  default = null
}

#
# Network rules
#
variable "private_endpoint_subnet_id" {
  type        = string
  description = "Private endpoint will be deployed by default for all storageTypes configured. If set to null ipWhitelists need to be set. This will only work with Policy exemptions"
}

variable "network_access_mode" {
  type        = string
  default     = "OnlyPrivateEndpoints"
  description = "How network access is permitted. Must be one of `OnlyPrivateEndpoints`, `OnlyAllowed` or `Public`"
  validation {
    condition     = contains(["OnlyPrivateEndpoints", "OnlyAllowed", "Public"], var.network_access_mode)
    error_message = "Network access mode must be one of `OnlyPrivateEndpoints`, `OnlyAllowed` or `Public`"
  }
}

variable "network_ips_to_whitelist" {
  type        = list(string)
  default     = []
  nullable    = false
  description = "IPs to whitelist. WARNING! Setting this config will block the deployment due to azure policy setting. Policy exemption needs to be requested first"
  validation {
    condition     = length(var.network_ips_to_whitelist) <= 0 || var.network_access_mode == "OnlyAllowed"
    error_message = "To whitelist ips, network mode must be OnlyAllowed"
  }
}

variable "network_subnet_id_to_whitelist" {
  type        = list(string)
  default     = []
  nullable    = false
  description = "Subnets to whitelist. WARNING! Setting this config will block the deployment due to azure policy setting. Policy exemption needs to be requested first"
  validation {
    condition     = length(var.network_subnet_id_to_whitelist) <= 0 || var.network_access_mode == "OnlyAllowed"
    error_message = "To whitelist subnets, network mode must be OnlyAllowed"
  }
}

variable "network_bypass" {
  type        = set(string)
  default     = ["None"]
  nullable    = false
  description = "Specifies whether traffic is bypassed for Logging/Metrics/AzureServices. Valid options are any combination of `Logging`, `Metrics`, `AzureService`, or `None`"
  validation {
    condition     = var.network_bypass == toset(["None"]) || length(var.network_bypass) == 0 || var.network_access_mode == "OnlyAllowed"
    error_message = "To whitelist Azure Services, network mode must be OnlyAllowed"
  }
}

variable "central_dns_zone_resource_group_name" {
  type        = string
  description = "Name of the external private DNS zone resource group that should be used to register private endpoint A record"
}

variable "https_traffic_only_enabled" {
  type        = bool
  default     = true
  description = "Boolean flag which forces HTTPS if enabled."
}

#
# Storage Config
#
variable "storage_containers" {
  type = list(object({
    name        = string
    access_type = optional(string, "private")
    metadata    = optional(map(string))
  }))
  default     = []
  description = "List of Object of Storage Containers that should be created in this Storage Account"
  validation {
    condition = alltrue([
      for container in var.storage_containers :
      contains(["blob", "container", "private"], container.access_type)
    ])
    error_message = "Container access types must be one of `blob`, `container` oder `private`"
  }
}

variable "storage_file_shares" {
  type = list(object({
    name        = string
    metadata    = optional(map(string))
    access_tier = optional(string)
    quota_in_gb = number
    protocol    = string
    acl = optional(list(object({
      id = string
      access_policy = optional(object({
        permissions = string
        start       = string
        end         = string
      }))
    })), [])
  }))
  default     = []
  description = "List of Object of Storage File Shares that should be created in this Storage Account"
}

variable "storage_queues" {
  type = list(object({
    name     = string
    metadata = optional(map(string))
  }))
  default     = []
  description = "List of Object of Storage Queues that should be created in this Storage Account"
}

variable "storage_tables" {
  type = list(object({
    name     = string
    metadata = optional(map(string))

    acl = optional(list(object({
      id = string
      access_policy = optional(object({
        permissions = string
        start       = string
        end         = string
      }))
    })), [])
  }))
  default     = []
  description = "List of Object of Storage Tables that should be created in this Storage Account. Be sure to configure the storage account so that terraform has network access, otherwise creation will fail!"
}

variable "allow_nested_items_to_be_public" {
  type        = bool
  default     = false
  description = "Allow or disallow nested items within this Account to opt into being public"
}

variable "large_file_share_enabled" {
  type        = bool
  default     = null
  nullable    = true
  description = "Are Large File Shares Enabled? If unset, defaults to false, except for `account_kind` = `FileStorage` where it is true."
}

#
# Encryption
#

variable "storage_encryption_customer_managed_key" {
  type = object({
    user_assigned_identity_id = string
    // We use a uri instead of id to prevent a diff when the key vault is in a different subscription, see azurerm_storage_account_customer_managed_key docs
    key_vault_uri = string
    key_name      = string
  })
  nullable    = true
  default     = null
  description = "Config for using a Customer managed key for encryption"
}
