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

