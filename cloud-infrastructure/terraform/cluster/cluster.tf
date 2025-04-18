module "cluster_resource_group" {
  source              = "../modules/resource-group"
  tags                = local.tags
  resource_location   = var.resource_location
  resource_group_name = var.resource_group_name
}

module "virtual_network" {
  source              = "../modules/virtual-network"
  tags                = local.tags
  resource_location   = var.resource_location
  resource_group_name = var.resource_group_name

  depends_on = [
    module.cluster_resource_group
  ]
}

module "storage_account" {
  source                   = "../modules/storage-account"
  tags                     = local.tags
  resource_location        = var.resource_location
  resource_group_name      = var.resource_group_name
  unique_name              = "${var.cluster_unique_name}diagnostic"
  account_replication_type = "GRS"

  depends_on = [
    module.cluster_resource_group
  ]
}

module "key_vault" {
  source                       = "../modules/key-vault"
  tags                         = local.tags
  environment                  = var.environment
  resource_location            = var.resource_location
  resource_group_name          = var.resource_group_name
  unique_name                  = var.cluster_unique_name
  subnet_id                    = module.virtual_network.subnet_id_out
  dianostic_storage_account_id = module.storage_account.storage_account_id

  depends_on = [
    module.cluster_resource_group,
    module.storage_account
  ]
}

module "service_bus_namespace" {
  source                       = "../modules/service-bus-namespace"
  tags                         = local.tags
  environment                  = var.environment
  resource_location            = var.resource_location
  resource_group_name          = var.resource_group_name
  unique_name                  = var.cluster_unique_name
  dianostic_storage_account_id = module.storage_account.storage_account_id

  depends_on = [
    module.cluster_resource_group,
    module.storage_account
  ]
}

module "mssql-server" {
  source                                  = "../modules/mssql-server"
  tags                                    = local.tags
  resource_location                       = var.resource_location
  resource_group_name                     = var.resource_group_name
  sql_server_name                         = var.cluster_unique_name
  subnet_id                               = module.virtual_network.subnet_id_out
  dianostic_storage_account_id            = module.storage_account.storage_account_id
  dianostic_storage_account_blob_endpoint = module.storage_account.primary_blob_endpoint

  depends_on = [
    module.cluster_resource_group,
    module.storage_account
  ]
}

module "mssql-elasticpool" {
  source              = "../modules/mssql-elasticpool"
  count               = var.use_mssql_elasticpool ? 1 : 0
  tags                = local.tags
  resource_location   = var.resource_location
  resource_group_name = var.resource_group_name
  sql_server_name     = var.cluster_unique_name

  depends_on = [
    module.mssql-server,
    module.cluster_resource_group,
    module.storage_account
  ]
}

module "container_apps_environment" {
  source            = "../modules/container-apps-environment"
  tags              = local.tags
  environment       = var.environment
  resource_location = var.resource_location
  resource_group_id = module.cluster_resource_group.resource_group_id_out
  subnet_id         = module.virtual_network.subnet_id_out

  depends_on = [
    module.virtual_network
  ]
}

