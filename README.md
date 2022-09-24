# SRXDCustomVisuals
SRXDCustomVisuals is a set of mods that enable custom visual elements to be imported into charts

### Importing New Backgrounds

- Add any asset bundle and manifest files to the AssetBundles folder in your customs folder
- Add the background definition .json file to the Backgrounds folder in your customs folder

### Assigning a Custom Background to a Chart

- Open the chart .srtb file in a text editor
- Add this entry to the end of the array of string values, near the end of the file:
```json
{"key":"CustomVisualsInfo","val":"{\"background\":\"<Background Definition File Name>\"}","loadedGenerationId":1}
```
