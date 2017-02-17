# BitUp

## What is this repository for?
* BitUp is an application to backup git repositories from Bitbucket.
* It is possible to store all repositories of specific Bitbucket Teams in Azure File Storage or to save them locally.

## Installation

### Bitbucket prerequisites

First you need to generate api keys for every team. To do that navigate to the team page and click on the button in the upper right corner "Manage team".
The keys are needed to configure the application.

![bitbucketapikey](https://cloud.githubusercontent.com/assets/25846873/23075455/d602652a-f53c-11e6-91a8-7d59fb7e4a15.jpg)

### Preparing Azure

The next step is to create an Azure Storage Account. Go to the [Azure Portal](https://portal.azure.com) and sign in.
Now you can navigate to "Storage accounts (classic)".

![storage](https://cloud.githubusercontent.com/assets/25846873/23075557/4608e420-f53d-11e6-9633-659b5da3279c.jpg)

Here click "Add" to add an storage account.

![addstorage](https://cloud.githubusercontent.com/assets/25846873/23075592/65121d00-f53d-11e6-8d65-a2234decce23.jpg)

Give it a unique name, change settings and click "Create".

![createstorageaccount](https://cloud.githubusercontent.com/assets/25846873/23075625/7f010f46-f53d-11e6-9033-c257fb2a8f55.jpg)

Now you can add a storage container clicking on the created storage account then "Overview" --> "Files" --> "+ File share". Choose a name for the storage container, set a sufficient quota (to limit max size) then click "Create".

![createstorage](https://cloud.githubusercontent.com/assets/25846873/23075687/bb51f2c6-f53d-11e6-89fd-493704f7df49.jpg)

You find the key needed for the config file at "Keys" --> "Primary Access Key".

![storagekeys](https://cloud.githubusercontent.com/assets/25846873/23075777/0364c08e-f53e-11e6-85b2-10da5cc7e01f.jpg)

### Configuration

Edit the _DataOne.BitUp.exe.config_ file.

![config](https://cloud.githubusercontent.com/assets/25846873/23075893/745adc92-f53e-11e6-9a8b-8aa1578ca7aa.jpg)

0. _BitbucketTeams:_ A collection of strings with all names of teams to backup the repositories of. A string consists of the team's ID and the API-Key of it, separated by a semicolon.

0. _AzureStorageContainer:_ The name of an Azure Storage Container. It has not to exist but has to fit the naming rules: [Azure Storage Naming Rules](https://blogs.msdn.microsoft.com/jmstall/2014/06/12/azure-storage-naming-rules/)

0. _AzureAccountName:_ The name of an already existing Azure Storage Account.

0. _AzureAccountKey:_ The primary Key of the specified Azure Storage Account.

0. _StoreInAzure:_ A boolean value on which it depends if the backups will be saved locally or in Azure. Possible values are: True or False.

0. _SmtpHost:_ The smpt host of the mail server you want to use to send the log mail after each run.

0. _EmailAccount:_ An email account with which the log mail will be sent.

0. _EmailPassword:_ The corresponding password of the stated email account.

0. _SendMailTo:_ The email account to receive the log mail.

0. _SmptPort:_ The used port of the smtp host. Default values are 25 or 587, but which one will work depends on the host.

0. _MailFrom:_ The sender of the log mail. It is not needed to change this value.

### Execution

#### Local

To run the application locally just execute the _DataOne.BitUp.exe_.

#### As Azure Web Job

If you want to use Azure to run it use the following steps to create a Web Job.

0. Create a zip file containing all files from the original _DataOne.BitUp.zip_ but where you changed the settings in the config file as you need them.

0. Navigate to "App Services" in the Azure Portal.
    * ![azureportal](https://cloud.githubusercontent.com/assets/25846873/23075920/9b195944-f53e-11e6-9f1d-c9c6433467af.jpg)

0. Add a new Web App (or use an existing one).
    * ![addwebapp](https://cloud.githubusercontent.com/assets/25846873/23075949/b7e23640-f53e-11e6-9e91-d96c1f10103a.jpg)

0. Choose an unique name for your App and click "Create".
    * ![createwebapp](https://cloud.githubusercontent.com/assets/25846873/23075984/dec138e2-f53e-11e6-9690-ee9c5621c609.jpg)

0. Navigate to "WebJobs" and click "Add".
    * ![addwebjob](https://cloud.githubusercontent.com/assets/25846873/23076043/10730e10-f53f-11e6-89e2-b2bcfa34090c.jpg)

0. Give it a name and upload the DataOne.BitUp.zip file containing the updated config file.
    * Choose type "Triggered" and decide when you want the application to run. 
    * To run it manually set "Triggers" to "Manual".
    * To run it at a specific time set "Triggers" to "Scheduled" and specify a valid [CRON expression](https://en.wikipedia.org/wiki/Cron#CRON_expression).
    * ![createwebjob](https://cloud.githubusercontent.com/assets/25846873/23076086/4b7581fa-f53f-11e6-85f3-e65e3608cac4.jpg)

0. If you set "Triggers" to "Manual" you can run the Web Job with a click on "Run" as shown in the next picture.
    * ![runwebjob](https://cloud.githubusercontent.com/assets/25846873/23076105/68136be2-f53f-11e6-810c-2c3cf7c55f4f.jpg)

### Done!

After a run you find your repository backups in the Azure Storage Container or in a local directory _./DataOne.BitUp.RepositoryBackups_ if you ran the application locally.

**Note that only repositories that changed since the last run (assumed that it was 24 hours ago) will get backed up!**
