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