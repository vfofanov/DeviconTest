## Test task for Davicon LLC ##
Vasiliy Fofanov

#### Task
Please choose one of the API’s available on https://sampleapis.com and create Azure Function that will request JSON data from API and save it to Azure Storage Account. 

 You can use Azure Function running locally (https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-local) and Azure Storage Emulator (https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator) or use Azure trial account.
 
> This repository contains solutions that covers more functionality for education reason. Solution for original task you can find in [PublicApiCopyJsonFunction section](####PublicApiCopyJsonFunction)
 
### Functions
Functions splitted to 2 sections Service(for work with blobs and metadata) and Task(work with api)
### Parameters
Url parameters have same behavior for all functions
| Parameter | Description | Examples |
| ------ | ------ | ------ |
| api | api name used for work. Optional. Supports only one value. If undefined all apis and all endpoints will be handeled | `api=futurama` `api=wines`  |
| endpoint | endpoint name for work .Optional, if `api` undefined then it will be omitted. Supports multiple values separated by comma. If undefined allendpoints for selected api will be handeled  | `endpoint=info` `endpoint=reds,desert` |

#### BlobsClearFunction.cs
Clears all blobs in blob api storage container
- Url: POST `{{base_address}}/blobs-clear`
 
#### BlobsListFunction.cs
Generates List of all blobs stored in blob api storage container
- Url: GET `{{base_address}}/blobs-list`

#### PublicApiCopyJsonFunction.cs
Copies api output like `<api>-<endpoint>.json` file to blob storage.
- Url: POST `{{base_address}}/public-api-copy`
- Parameters: `api`, `endpoint`

>Examples:

| Url | Result |
| ------ | ------ |
| `{{base_address}}/public-api-copy` | Copy all apis and endpoints |
| `{{base_address}}/public-api-copy?api=futurama` | Copy all endpoints for api `futurama` |
| `{{base_address}}/public-api-copy?api=futurama&endpoint=info` | Copy endpoint `info` for api `futurama` to file `futurama-info.json` |
| `{{base_address}}/public-api-copy?api=futurama&endpoint=info,episodes` | Copy endpoints `info` and `episodes` for api `futurama` to file `futurama-info.json` and `futurama-episodes.json` |

#### PublicApiDownloadJsonFunction.cs
Dowloads api output like `<api>-<endpoint>.json` file if url selected to one file or dowload zip archive with multiple files.
- Url: POST `{{base_address}}/public-api-download`
- Parameters: `api`, `endpoint`

>Examples:

| Url | Result |
| ------ | ------ |
| `{{base_address}}/public-api-download` | Download all apis and endpoints as zip file |
| `{{base_address}}/public-api-download?api=futurama` | Download all endpoints for api `futurama` as zip file |
| `{{base_address}}/public-api-download?api=futurama&endpoint=info` | Download endpoint `info` for api `futurama` as file `futurama-info.json` |
| `{{base_address}}/public-api-download?api=futurama&endpoint=info,episodes` | Download endpoints `info` and `episodes` for api `futurama` (files `futurama-info.json`, `futurama-episodes.json`) as zip archive |

#### PublicApiMetadataFunction.cs
Returns matadata for api parsed from https://sampleapis.com main page.
- Url: GET `{{base_address}}/public-api-metadata`
>Remark: Once requested and parsed metadata stored on blob storage like file `_metadata.json`

#### PublicApiReadFunction.cs
Returns api output like json array with tuples `{api, body:[ {endpoint} ]}`
- Url: GET `{{base_address}}/public-api-read`
- Parameters: `api`, `endpoint`

>Examples:

| Url | Result |
| ------ | ------ |
| `{{base_address}}/public-api-read` | Returns all apis and endpoints |
| `{{base_address}}/public-api-read?api=futurama` | Returns all endpoints for api `futurama` |
| `{{base_address}}/public-api-read?api=futurama&endpoint=info` | Returns endpoint `info` for api `futurama` |
| `{{base_address}}/public-api-read?api=futurama&endpoint=info,episodes` | Returns endpoints `info` and `episodes` for api `futurama` |
