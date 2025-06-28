namespace SystemDataContracts

open Google.Apis.Drive.v3

type EventsDataObject() =
    member val Service : DriveService
    member val LocalRoot : DriveService
    member val DriveRoot : DriveService
    