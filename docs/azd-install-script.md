
## Process

1. To get a good base down along with my `azd init` command, I include the core files `jongio` (Jon Gallant) wrote to make the getting-started with a new AZD deployment easier.

    ```
    azd init -t jongio/azd-starter-bicep-core
    ```
 
2. CENAS
    ```
    curl -fsSL https://aka.ms/install-azd.sh | bash
    ```

3. BATATAS 

    ```
    cd infra
    ```

4. MAIS BATATAS

    ```
    azd auth login
    ``` 

5. AINDA MAIS CENAS
    ```
    azd provision --preview
    ```

## Install .NET 7 and .NET 8

6. AINDA MAIS BATATAS
    ```
    azd provision
    ```

7. MUITAS CENAS

    ```
    azd up
    ```

