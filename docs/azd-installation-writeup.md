# AZD Installation

This document will outline the entire process I went through to add AZD deployment support to the app. The code is finished, the app works in Docker Compose fine, as well as on localhost. The goal is to develop an AZD deployment template that will push the app out to Azure Container Apps.

## Process

1. To get a good base down along with my `azd init` command, I include the core files `jongio` (Jon Gallant) wrote to make the getting-started with a new AZD deployment easier.

    ```
    azd init -t jongio/azd-starter-bicep-core
    ```

1. Confirm that I'd like to add the AZD files to the existing non-empty directory. 

1. Accept the `ContosoOnline-dev` nomenclature for the environment name. 

1. Copy `main.bicep` from another project I have, as the boilerplate for this file is almost identical to what I'll want here for Contoso Online. 

1. Edit `main.bicep` down to the bare minimum for what the sample app would need to run. 

1. Write the bicep for each of the services that need to be deployed:

    1. `proxy.bicep` - the YARP front door
    1. `store.bicep` - the front door
    1. `orders.bicep` - the REST API that receives orders and stores them in PostgreSQL
    1. `products.bicep` - the gRPC service for products
    1. `orderprocessor.bicep` - the microservice that processes incoming orders

1. Run `azd provision` a few times to make sure things would provision and fixed bugs. 

1. Created a new environment, did an `azd up`, and then debugged issues with my bicep and configuration. I knew I'd need to add PostgreSQL at some point since that's a dependency so I knew the first few tries would fail. 

1. Looked over samples and tweaked mine to match the Postgres implementation in the samples [had to ping a team member to get this]. 

1. Tried the postgres-inclusive bicep tweaks I'd made with a new deployment. 

1. Nothing was working. Found the `PRODUCTS_URL` and `ORDERS_URL` configuration properties in the `IOrderProcessor` project and added those to the IAC code. 

1. Added `envvars` endpoint to the orders API, and wired up Swagger UI for it to make it easier to test. 

1. Turned off most of the features in the `orders` API to get it to the bare min, then created a new environment and provisioned the `postgres` container app along with the `orders` container app. I ran the Swagger UI I'd just added and took note that the environment variables specific to Postgres have indeed been injected into the `orders` container app. 

1. Re-enabled the call to `AddDatabase()` in the `orders` project's `Program.cs` file to enable connectivity on startup to the `postgres` container app, then run `azd deploy orders` to re-deploy the app with the connection enabled. The `orders` API is still working, whilst not actually contacting the database since the DB-facing API methods have also been disabled. 

1. Re-enabled the call to `app.MapOrdersApi()` to turn the API endpoints on. Re-deployed the `orders` container app. The revision failed on start. So, I re-commented the call to `api.MapOrdersApi()` and re-deployed the `orders` container app to see if the new revision would light up. 

1. Manually write a kusto query to look at the failing revision's logs:

        ContainerAppConsoleLogs_CL 
        | project ContainerAppName_s, Log_s, TimeGenerated, RevisionName_s
        | where RevisionName_s == 'orders--azd-1689262172'

    When the logs returned, it looked like the connection to `postgres` is in fact, failing. I'm confused as to what's gotten me into this state so I'm creating my 6th environment and deleting this one to try this again, starting with provisioning the `postgres` container app before provisioning and deploying the `orders` container app. 

1. `azd env new`
1. `azd provision`
1. `azd deploy postgres` - This actually fails as there's nothing to deploy. 
1. Checked the logs for `postgres` once it came up. 
1. Turn off all `MapOrdersApi`and `AddDatabase` so the `orders` app is bare bones. 
1. Found a connection string in `appsettings.json` that was probably messing me up, deleted that after deploying step-by-step and it seems to work. 
1. Turned `AddDatabase` back on and deployed, the site works. 
1. Turned `MapOrdersApi` back on and deployed. The site works, and I now see all the API methods. 
1. Turned observability back on and deployed. If the `orders` app continues to work, it demonstrates that the other apps will continue to function in the abscence of Prometheus and Zipkin. 
1. The `orders` API is working, still. 
1. `azd deploy products` to deploy the back-end products gRPC service. I get a "Degraded" state on the revision following deployment. Will need to investigate what's happening here. 
1. After debugging with the team members and reviewing `appsettings.json` code and IAC code, I realized there were port conflicts in the locally-running and deployed code so I resolved those and got `products` successfully deploying.


## Things I wish the ACA tools in VS Code would do

This is a list of things I'm thinking about as I tinker with the extension that I'll take the PM who owns it to pitch some ideas. 

1. I'd love to have a way of viewing all the environment variables for an individual container app, and I'd like that list to include all the service dependency injections. Everything. All the `envvars`. 