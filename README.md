# api-reac-android

This API will be used as a middleware between the Android Application and the Database/Logic of the server.
The API will be RESTful and their CRUD operations are defined below:

## General Documentation   

### [IPAddress](#ip_address_extra) 
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/ipaddress      | Returns the External IP Address of the Server |

### [Register Administrator](#register_administrator_extra) 
| Method | Url | Action |
| ------ | ------------------- | --- |
| POST    | /api/register      | Register a new Administrator when there isn't any (on first setup) |

### [Login Administrator](#login_administrator_extra) 
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/login          | Login for Administrator users |

### [Users List](#users_list)
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/users          | Retrieves a list with information about every member |

### [User Images List](#user_images_list)
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/user/:userid/images  | Retrieves a list with all the photo URIs from that user |

### [User Image](#user_image)
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/user/image/:image_id | Get the image |

### [User To Admin](#user_to_admin)
| Method | Url | Action |
| ------ | ------------------- | --- |
| POST   | /api/admin          | Returns a new session_id that will expire in 5 minutes and is required for registering the new admin |

### [User To Admin Confirm](#user_to_admin_confirm)
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/admin/confirm  | Returns a password of the new created admin (it should be done from the other android device) |

<!-- NOT DONE YET


## User
| Method | Url | Action |
| ------ | ------------------- | --- |
| POST   | /api/user               | Registers a new user into the database (an one-time password will be returned with a limited time of 5min for the member to add it to his mobile device) |
| PUT    | /api/user/:username     | Updates information about the user (name, profile photo, role, ... |
| DELETE | /api/user/:username     | Delete the instance of user |

### DataLog
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/log                | Retrieve all datalog from the house given an $USER_ID and $SESSION_ID of an owner (given a range of dates) |

### Open Door-Lock
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/door               | Ask the server to open the door |

### Camera streaming
| Method | Url | Action |
| ------ | ------------------- | --- |
| GET    | /api/camera             | Ask the server to give an url (that will be playable by VLC integrated API in Android) back from the video streaming camera |


-->




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

<a name="register_administrator_extra"/>

**Register Administrator**
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

  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "short_username_length",
      "content": 0
    }
    ```

  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "database_error",
      "content": 0
    }
    ```

  OR

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

  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "name_already_in_use",
      "content": 0
    }
    ```

  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "member_is_already_an_admin",
      "content": 0
    }
    ```

<a name="login_administrator_extra"/>

**Login Administrator**
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

  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "short_username_length",
      "content": 0
    }
    ```

  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "wrong_user_password",
      "content": 0
    }
    ```

<a name="users_list"/>

**Users List**
----
   Retrieves a list with information about every member. Returns an array of JObject on `content` field with information about all the users registered in the database (owners and members).

* ***URL***

  /api/users

* **Method:**

  `GET`
  
* **URL Params**

  **Required:**
 
   `session_id=[string]` the logged-in session_id.

* **Data Params**

  None

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
        "error": false,
        "errorMessage": "",
        "content": [
            {
                "isOwner": true,
                "userID": 1,
                "name": "ivan",
                "role": "ADMIN",
                "profilePhoto": "/api/user/image/0"
            },
            {
                "isOwner": true,
                "userID": 6,
                "name": "ivan2",
                "role": "ADMIN",
                "profilePhoto": "/api/user/image/0"
            },
            {
                "isOwner": false,
                "userID": 8,
                "name": "Test1",
                "role": "MEMBER",
                "profilePhoto": "/api/user/image/0"
            }
        ]
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

  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "expired_session_id",
      "content": 0
    }
    ```

  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "database_error",
      "content": 0
    }
    ```

<a name="user_images_list"/>

**User Images List**
----
   Retrieves a list with all the links of the user's images. Returns an array of uint on `content` field.

* ***URL***

  /api/user/:userid/images

* **Method:**

  `GET`
  
* **URL Params**

  **Required:**
 
   `:userid=[uint]` the name of the user. <br />
   `session_id=[string]` the logged-in session_id.

* **Data Params**

  None

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
        "error": false,
        "errorMessage": "",
        "content": [ "/api/user/image/1", "/api/user/image/2", "/api/user/image/3" ]
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

  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "expired_session_id",
      "content": 0
    }
    ```

  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "database_error",
      "content": 0
    }
    ```
    
<a name="user_image"/>

**User Image**
----
   It's used for downloading an image from the server. Returns a response with Content-Type set to "image/jpeg".

* ***URL***

  /api/user/image/:image_id

* **Method:**

  `GET`
  
* **URL Params**

  **Required:**
 
   `image_id=[uint]` the id of the photo. <br />
   `session_id=[string]` the logged-in session_id.

* **Data Params**

  None

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```
    Content-Type: "image/jpeg"
    ```
 
* **Error Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "expired_session_id",
      "content": 0
    }
    ```

<a name="user_to_admin"/>

**User To Admin**
----
   It's used for upgrading a member to administator/owner level. Returns a string on `content` field with the session_id needed for completing the upgrade. This session_id will expire within the next 5 minutes. A way to send the url from Android to Android has to be coded (via QR maybe??). The url sent should be: http://<SERVER_IP>/api/admin/confirm?session_id=<SESSION_ID>

* ***URL***

  /api/admin

* **Method:**

  `POST`
  
* **URL Params**

   None

* **Data Params**

  **Required:**

  `user_id=[uint]` the id of the user to upgrade ownership. <br />
  `session_id=[string]` the logged-in session_id.

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
    
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "expired_session_id",
      "content": 0
    }
    ```
        
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "member_is_already_an_admin",
      "content": 0
    }
    ```
        
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "database_error",
      "content": 0
    }
    ```
    
<a name="user_to_admin_confirm"/>

**User To Admin Confirm**
----
   It's used for upgrading a member to administator/owner level. Returns a string on `content` field with the password_id needed for completing the upgrade. 

* ***URL***

  /api/admin/confirm

* **Method:**

  `GET`
  
* **URL Params**

   **Required:**

  `session_id=[string]` the new one time session_id generated on the previous step.

* **Data Params**

  None

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
    
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "expired_session_id",
      "content": 0
    }
    ```
        
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "member_is_already_an_admin",
      "content": 0
    }
    ```
        
  OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "member_id_not_found",
      "content": 0
    }
    ```
    
     OR

  * **Code:** 200 <br />
    **Content:** 
    ```javascript
    {
      "error": true,
      "errorMessage": "database_error",
      "content": 0
    }
    ```
    
