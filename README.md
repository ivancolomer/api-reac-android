# api-reac-android

This API will be used as a middleware between the Android Application and the Database/Logic of the server.
The API will be RESTful and their CRUD operations are defined below:

## Login
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /login              | Checks if the given $PASSWORD and $USER_ID is good, and if so, returns a $SESSION_ID used for the following queries |

## User
| Method | Url | Action |
| ------ | ------------------- | --- |
| POST   | /user               | Registers a new user into the database by given an $SERIAL_ID |
| GET    | /user               | Retrieve all the information about the user given an $USER_ID and $SESSION_ID (logged-in) |
| PUT    | /user               | Updates information about the logged-in user (name, profile photo, ...) |

## DataLog
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /log                | Retrieve all datalog from the house given an $USER_ID and $SESSION_ID of an owner |

## Member
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /members            | Retrieve a list of all the members (with all information) the owner has |
| POST   | /member             | Send a request to add him/her as a member of the house, an one-time password will be returned with a limited time of 5min for the member to add it to his mobile device |
| PUT    | /member             | Modify member information and owner permissions |

## Open Door-Lock
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /door               | Ask the server to open the door |

## Camera streaming
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /camera             | Ask the server to give an url (that will be playable by VLC integrated API in Android) back from the video streaming camera |

