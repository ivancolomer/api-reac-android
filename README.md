# api-reac-android

This API will be used as a middleware between the Android Application and the Database/Logic of the server.
The API will be RESTful and their CRUD operations are defined below:

## General Documentation   

### [IPAddress](#ip_address_extra) 
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/ipaddress      | Returns the External IP Address of the Server |

### [Register](#register_extra) 
| Method | Url | Action |
| ------ | ------------------- | --- |
| POST    | /api/register      | Register a new Administrator when there isn't any (on first setup) |

### [Login](#login_extra) 
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/login          | Login for Administrator users |

## User
| Method | Url | Action |
| ------ | ------------------- | --- |
| POST   | /api/user               | Registers a new user into the database by given an $SERIAL_ID |
| GET    | /api/user               | Retrieve all the information about the user given an $USER_ID and $SESSION_ID (logged-in) |
| PUT    | /api/user               | Updates information about the logged-in user (name, profile photo, ...) |

### DataLog
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/log                | Retrieve all datalog from the house given an $USER_ID and $SESSION_ID of an owner (given a range of dates) |

### Member
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/members            | Retrieve a list of all the members (with all information) the owner has |
| POST   | /api/member             | Send a request to add him/her as a member of the house, an one-time password will be returned with a limited time of 5min for the member to add it to his mobile device |
| PUT    | /api/member             | Modify member information and owner permissions (create temporary user for retrieving face and fingerprint) |
| DELETE | /api/member             | Modify member information and owner permissions |

### Open Door-Lock
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/door               | Ask the server to open the door |

### Camera streaming
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/camera             | Ask the server to give an url (that will be playable by VLC integrated API in Android) back from the video streaming camera |

## Installation

```bash
sudo apt update && sudo apt install curl

bash <(curl -s https://raw.githubusercontent.com/ivancolomer/api-reac-android/master/install.sh)
```

# RestAPI Documentation

**Packet Format**
----
  The packet format is in JSON. There is a basic struct for all the packets returned by the API:
```javascript
{
  "error": true,
  "errorMessage": "expired_session_id",
  "content": 0
}
```
   Where `error` is a `boolean` type which tells if the request succeed or not.<br />
   Where `errorMessage` is a `string` type which is `null` if there isn't any error, or the error's identifier if there was an error.<br />
   Where `content` is a JSONObject which it's used to display different things depending on the request. When there's an error, it's    always 0.<br />

<a name="ip_address_extra"/>

**IPAddress**
----
  Returns a String on `content` field with the External IPAddress of the Server.

* ***URL***

  /api/ipaddress

* **Method:**

  `GET`
  
* **URL Params**
 
  None

* **Data Params**

  None

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
```javascript
{
  "error": false,
  "errorMessage": "",
  "content": "5.186.124.216"
}
```
 
* **Error Response:**

  * **Code:** 200 <br />
    **Content:** 
```javascript
{
  "error": true,
  "errorMessage": "unable_get_ipaddress",
  "content": 0
}
```

<a name="register_extra"/>

**Register**
----
   Register a new Administrator when there isn't any (on first setup) and returns a String on `content` field with the new generated random password.

* ***URL***

  /api/register

* **Method:**

  `POST`
  
* **URL Params**

  None

* **Data Params**

  **Required:**
 
   `serial_id=[string]` the serial code of the server device (from factory).<br />
   `user_name=[string]` the name of the new administrator.

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
```javascript
{
    "error": false,
    "errorMessage": "",
    "content": "HnP0BkORqL08ocPtddb8HQJmx3MH0UXMLG7FoiRDQEA="
}
```
 
* **Error Response:**

  * **Code:** 200 <br />
    **Content:** 
```javascript
{
  "error": true,
  "errorMessage": "missing_request_parameters",
  "content": 0
}
```

  * **Code:** 200 <br />
    **Content:** 
```javascript
{
  "error": true,
  "errorMessage": "short_username_length",
  "content": 0
}
```

  * **Code:** 200 <br />
    **Content:** 
```javascript
{
  "error": true,
  "errorMessage": "database_error",
  "content": 0
}
```

  * **Code:** 200 <br />
    **Content:** 
```javascript
{
  "error": true,
  "errorMessage": "admin_already_exists",
  "content": 0
}
```

  * **Code:** 200 <br />
    **Content:** 
```javascript
{
  "error": true,
  "errorMessage": "wrong_serial_id",
  "content": 0
}
```

  * **Code:** 200 <br />
    **Content:** 
```javascript
{
  "error": true,
  "errorMessage": "name_already_in_use",
  "content": 0
}
```

  * **Code:** 200 <br />
    **Content:** 
```javascript
{
  "error": true,
  "errorMessage": "member_is_already_an_admin",
  "content": 0
}
```

<a name="login_extra"/>

**Login**
----
   Used for login with your admin account and returns a String on `content` field with the new generated random session_id. This session_id will be valid during the following 15 minutes. Once this time has passed, it will not be longer valid and all the calls using this session_id will get an expired_session_id error.

* ***URL***

  /api/login

* **Method:**

  `GET`
  
* **URL Params**

  **Required:**
 
   `user_name=[string]` the name of the administrator account.<br />
   `password=[string]` the password of the administrator account (in Base64).

* **Data Params**

  None

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
```javascript
{
    "error": false,
    "errorMessage": "",
    "content": "F9-20-F6-89-6E-3E-76-4D-A3-20-EB-5F-74-F9-99-50"
}
```
 
* **Error Response:**

  * **Code:** 200 <br />
    **Content:** 
```javascript
{
  "error": true,
  "errorMessage": "missing_request_parameters",
  "content": 0
}
```

  * **Code:** 200 <br />
    **Content:** 
```javascript
{
  "error": true,
  "errorMessage": "short_username_length",
  "content": 0
}
```

  * **Code:** 200 <br />
    **Content:** 
```javascript
{
  "error": true,
  "errorMessage": "wrong_user_password",
  "content": 0
}
```



